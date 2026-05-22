using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.DAL.Interfaces;
using ParkingManagement.DAL.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class MonthlyTicketService : IMonthlyTicketService
    {
        private readonly IMonthlyTicketRepository _repo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPricingService _pricingService;
        private readonly IPayOsService _payOsService;

        public MonthlyTicketService(
            IMonthlyTicketRepository repo,
            ICustomerRepository customerRepo,
            IVehicleRepository vehicleRepo,
            IPaymentRepository paymentRepo,
            IPricingService pricingService,
            IPayOsService payOsService)
        {
            _repo = repo;
            _customerRepo = customerRepo;
            _vehicleRepo = vehicleRepo;
            _paymentRepo = paymentRepo;
            _pricingService = pricingService;
            _payOsService = payOsService;
        }

        public async Task<List<MonthlyTicketDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<List<MonthlyTicketDto>> GetByCustomerIdAsync(string customerId)
        {
            var list = await _repo.GetByCustomerIdAsync(customerId);
            foreach (var ticket in list.Where(IsPendingPaymentTicket))
            {
                await TrySyncPayOsPaymentAsync(ticket);
            }

            var customer = await _customerRepo.GetByIdAsync(customerId);
            if (customer != null)
            {
                await ReconcileCustomerVipFromMonthlyTicketsAsync(customer, list);
            }

            return list.Select(MapToDto).ToList();
        }

        public async Task<MonthlyTicketDto?> GetByIdAsync(string id)
        {
            var ticket = await _repo.GetByIdAsync(id);
            return ticket == null ? null : MapToDto(ticket);
        }

        public async Task<ServiceResult<MonthlyTicketDto>> RegisterAsync(RegisterMonthlyTicketDto dto)
        {
            dto.VehiclePlate = MonthlyTicketValidator.NormalizeVehiclePlate(dto.VehiclePlate);
            dto.VehicleType = dto.VehicleType?.Trim();
            dto.PackageType = dto.PackageType.Trim();
            dto.PaymentMethod = dto.PaymentMethod?.Trim();

            var (isValid, errorMessage) = MonthlyTicketValidator.Validate(dto);
            if (!isValid)
                return ServiceResult<MonthlyTicketDto>.Fail(errorMessage ?? "Dữ liệu không hợp lệ.");

            var customerId = dto.CustomerId!.Trim();
            var vehicleType = dto.VehicleType!;
            dto.CustomerId = customerId;

            var existing = (await _repo.GetAllAsync())
                .FirstOrDefault(ticket =>
                    ticket.VehiclePlate == dto.VehiclePlate &&
                    MonthlyTicketStatuses.BlocksNewRegistration(ticket.Status));
            if (existing != null)
                return ServiceResult<MonthlyTicketDto>.Fail("Biển số xe này đang có vé tháng hoạt động hoặc đang chờ thanh toán.");

            var customer = await _customerRepo.GetByIdAsync(customerId);
            if (customer == null)
                return ServiceResult<MonthlyTicketDto>.Fail("Không tìm thấy khách hàng.");

            await ReconcileCustomerVipFromMonthlyTicketsAsync(customer);

            var fee = await CalculateFeeAsync(vehicleType, dto.PackageType, customerId);
            if (fee == 0)
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không hợp lệ.");

            var start = DateTime.Today;
            var end = start.AddMonths(GetPackageMonths(dto.PackageType)).AddDays(-1);
            var orderCode = GeneratePayOsOrderCode();

            var vehicle = await _vehicleRepo.GetByPlateAsync(dto.VehiclePlate);
            if (vehicle == null)
            {
                await _vehicleRepo.AddAsync(new Vehicle
                {
                    VehiclePlate = dto.VehiclePlate,
                    VehicleType = vehicleType,
                    CustomerId = customerId
                });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(vehicle.CustomerId) &&
                    !string.Equals(vehicle.CustomerId, customerId, StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<MonthlyTicketDto>.Fail("Biển số xe này đã thuộc khách hàng khác.");
                }

                if (!string.Equals(vehicle.VehicleType, vehicleType, StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<MonthlyTicketDto>.Fail("Loại xe không khớp với biển số đã lưu.");
                }

                if (string.IsNullOrWhiteSpace(vehicle.CustomerId))
                {
                    vehicle.CustomerId = customerId;
                    await _vehicleRepo.UpdateAsync(vehicle);
                }
            }

            var id = await _repo.GenerateIdAsync();
            var monthly = new MonthlyTicket
            {
                MonthlyTicketId = id,
                CustomerId = customerId,
                VehiclePlate = dto.VehiclePlate,
                VehicleType = vehicleType,
                StartDate = start,
                EndDate = end,
                PackageType = dto.PackageType,
                TotalFee = fee,
                Status = MonthlyTicketStatuses.PENDING_PAYMENT,
                CreatedAt = DateTime.Now
            };

            var paymentLink = await _payOsService.CreatePaymentLinkAsync(new PayOsCreatePaymentLinkDto
            {
                OrderCode = orderCode,
                Amount = decimal.ToInt32(fee),
                Description = id,
                ItemName = $"Vé tháng {id}"
            });

            if (!paymentLink.Success || paymentLink.Data == null)
            {
                return ServiceResult<MonthlyTicketDto>.Fail(
                    paymentLink.Message
                    ?? "Không tạo được QR thanh toán PayOS. Vui lòng cấu hình PayOS để thanh toán online.");
            }

            try
            {
                await _repo.AddAsync(monthly);
                await AddPendingPaymentAsync(id, fee, dto.PaymentMethod, orderCode);
            }
            catch (DbUpdateException)
            {
                return ServiceResult<MonthlyTicketDto>.Fail("Không lưu được vé tháng chờ thanh toán. Database chưa cập nhật migration cho trạng thái 'Chờ thanh toán'. Hãy khởi động lại backend hoặc chạy cập nhật database rồi thử lại.");
            }

            var resultDto = MapToDto(monthly);
            resultDto.PayOsOrderCode = orderCode;
            resultDto.PayOsPaymentLinkId = paymentLink.Data.PaymentLinkId;
            resultDto.CheckoutUrl = paymentLink.Data.CheckoutUrl;
            resultDto.QrCode = paymentLink.Data.QrCode;

            return ServiceResult<MonthlyTicketDto>.Ok(resultDto, "Đã tạo QR thanh toán. Vé tháng sẽ hoạt động sau khi payOS xác nhận đã nhận tiền.");
        }

        private async Task<string?> SyncMonthlyTicketVehicleAsync(RegisterMonthlyTicketDto dto)
        {
            var vehicle = await _vehicleRepo.GetByPlateAsync(dto.VehiclePlate);
            if (vehicle == null)
            {
                await _vehicleRepo.AddAsync(new Vehicle
                {
                    VehiclePlate = dto.VehiclePlate,
                    VehicleType = dto.VehicleType!,
                    CustomerId = dto.CustomerId
                });

                return null;
            }

            if (!string.IsNullOrWhiteSpace(vehicle.CustomerId) && vehicle.CustomerId != dto.CustomerId)
                return "Biển số xe này đã thuộc khách hàng khác.";

            var changed = false;
            if (string.IsNullOrWhiteSpace(vehicle.CustomerId))
            {
                vehicle.CustomerId = dto.CustomerId;
                changed = true;
            }

            if (!string.Equals(vehicle.VehicleType, dto.VehicleType, StringComparison.OrdinalIgnoreCase))
            {
                vehicle.VehicleType = dto.VehicleType!;
                changed = true;
            }

            if (changed)
                await _vehicleRepo.UpdateAsync(vehicle);

            return null;
        }

        public async Task<ServiceResult<MonthlyTicketDto>> RenewAsync(string monthlyTicketId, RenewMonthlyTicketDto dto)
        {
            var ticket = await _repo.GetByIdAsync(monthlyTicketId);
            if (ticket == null)
                return ServiceResult<MonthlyTicketDto>.Fail("Không tìm thấy vé tháng.");

            if (ticket.Status == MonthlyTicketStatuses.CANCELLED)
                return ServiceResult<MonthlyTicketDto>.Fail("Vé tháng đã hủy không thể gia hạn.");

            if (ticket.Status == MonthlyTicketStatuses.PENDING_PAYMENT)
                return ServiceResult<MonthlyTicketDto>.Fail("Vé tháng đang chờ thanh toán nên chưa thể gia hạn.");

            if (string.IsNullOrWhiteSpace(dto.PackageType))
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không được để trống.");

            var fee = await CalculateFeeAsync(ticket.VehicleType, dto.PackageType, ticket.CustomerId);
            if (fee == 0)
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không hợp lệ.");

            var extensionStart = ticket.EndDate.Date >= DateTime.Today
                ? ticket.EndDate.Date.AddDays(1)
                : DateTime.Today;

            ticket.EndDate = extensionStart.AddMonths(GetPackageMonths(dto.PackageType)).AddDays(-1);
            ticket.PackageType = dto.PackageType;
            ticket.TotalFee += fee;
            ticket.Status = MonthlyTicketStatuses.ACTIVE;

            await _repo.UpdateAsync(ticket);
            await AddPaymentAsync(ticket.MonthlyTicketId, fee, dto.PaymentMethod);

            // Update VIP progress
            if (ticket.CustomerId != null)
            {
                var customer = await _customerRepo.GetByIdAsync(ticket.CustomerId);
                if (customer != null)
                {
                    customer.TotalSpent += fee;
                    customer.TotalTickets += 1;
                    customer.VipLevel = ParkingManagement.BLL.Helpers.VipHelper.DetermineVipLevel(customer.TotalSpent);
                    await _customerRepo.UpdateAsync(customer);
                }
            }

            return ServiceResult<MonthlyTicketDto>.Ok(MapToDto(ticket), "Gia hạn vé tháng thành công!");
        }

        public async Task<ServiceResult<string>> CancelAsync(string id)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null)
                return ServiceResult<string>.Fail("Không tìm thấy vé tháng.");

            if (ticket.Status != MonthlyTicketStatuses.ACTIVE)
                return ServiceResult<string>.Fail("Vé tháng đã không còn hoạt động.");

            ticket.Status = MonthlyTicketStatuses.CANCELLED;
            await _repo.UpdateAsync(ticket);

            return ServiceResult<string>.Ok(id, "Hủy vé tháng thành công.");
        }

        public async Task<ServiceResult<string>> ConfirmPayOsPaymentAsync(long orderCode, int amount, string? paymentLinkId, string? bankReference)
        {
            var payment = await _paymentRepo.GetByVnpTxnRefAsync(BuildPayOsTxnRef(orderCode));
            if (payment == null)
                return ServiceResult<string>.Fail("Không tìm thấy đơn thanh toán payOS.");

            if (PaymentStatuses.IsSuccessful(payment.Status))
                return ServiceResult<string>.Ok(payment.MonthlyTicketId ?? "", "Thanh toán đã được xử lý trước đó.");

            if (decimal.ToInt32(payment.Amount) != amount)
                return ServiceResult<string>.Fail("Số tiền payOS xác nhận không khớp với đơn vé tháng.");

            if (string.IsNullOrWhiteSpace(payment.MonthlyTicketId))
                return ServiceResult<string>.Fail("Payment payOS không gắn với vé tháng.");

            var ticket = await _repo.GetByIdAsync(payment.MonthlyTicketId);
            if (ticket == null)
                return ServiceResult<string>.Fail("Không tìm thấy vé tháng cần kích hoạt.");

            if (ticket.Status == MonthlyTicketStatuses.CANCELLED)
                return ServiceResult<string>.Fail("Vé tháng đã bị hủy nên không thể kích hoạt.");

            ticket.Status = MonthlyTicketStatuses.ACTIVE;
            payment.Status = PaymentStatuses.SUCCESS;
            payment.PaymentTime = DateTime.Now;

            await _repo.UpdateAsync(ticket);
            await _paymentRepo.UpdateAsync(payment);

            // Update VIP progress
            if (ticket.CustomerId != null)
            {
                var customer = await _customerRepo.GetByIdAsync(ticket.CustomerId);
                if (customer != null)
                {
                    customer.TotalSpent += payment.Amount;
                    customer.TotalTickets += 1; // Assuming buying a monthly ticket counts as a ticket interaction
                    customer.VipLevel = ParkingManagement.BLL.Helpers.VipHelper.DetermineVipLevel(customer.TotalSpent);
                    await _customerRepo.UpdateAsync(customer);
                }
            }

            return ServiceResult<string>.Ok(ticket.MonthlyTicketId, "Đã kích hoạt vé tháng sau khi payOS xác nhận thanh toán.");
        }

        public async Task<ServiceResult<string>> ConfirmPayOsReturnAsync(long orderCode)
        {
            var paymentInfo = await _payOsService.GetPaymentLinkInformationAsync(orderCode);
            if (!paymentInfo.Success || paymentInfo.Data == null)
            {
                return ServiceResult<string>.Fail(paymentInfo.Message ?? "Không kiểm tra được trạng thái thanh toán payOS.");
            }

            if (!string.Equals(paymentInfo.Data.Status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<string>.Fail($"payOS chưa xác nhận thanh toán. Trạng thái hiện tại: {paymentInfo.Data.Status}.");
            }

            return await ConfirmPayOsPaymentAsync(
                paymentInfo.Data.OrderCode,
                paymentInfo.Data.Amount,
                paymentInfo.Data.PaymentLinkId,
                null);
        }

        public async Task<ServiceResult<string>> ConfirmPayOsMonthlyTicketAsync(string monthlyTicketId, string? customerId)
        {
            if (string.IsNullOrWhiteSpace(monthlyTicketId))
            {
                return ServiceResult<string>.Fail("Khong xac dinh duoc ve thang can kiem tra thanh toan.");
            }

            var ticket = await _repo.GetByIdAsync(monthlyTicketId.Trim());
            if (ticket == null)
            {
                return ServiceResult<string>.Fail("Khong tim thay ve thang can kiem tra thanh toan.");
            }

            if (!string.IsNullOrWhiteSpace(customerId) &&
                !string.Equals(ticket.CustomerId, customerId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<string>.Fail("Ban khong co quyen xac nhan thanh toan cho ve thang nay.");
            }

            var payment = await FindPayOsPaymentAsync(ticket.MonthlyTicketId);
            if (payment == null)
            {
                return ServiceResult<string>.Fail("Khong tim thay don thanh toan payOS cua ve thang nay.");
            }

            if (PaymentStatuses.IsSuccessful(payment.Status))
            {
                if (!MonthlyTicketStatuses.IsActive(ticket.Status))
                {
                    ticket.Status = MonthlyTicketStatuses.ACTIVE;
                    await _repo.UpdateAsync(ticket);
                }

                return ServiceResult<string>.Ok(ticket.MonthlyTicketId, "Thanh toan da duoc xu ly truoc do.");
            }

            if (!TryGetPayOsOrderCode(payment.VnpTxnRef, out var orderCode))
            {
                return ServiceResult<string>.Fail("Don thanh toan nay khong co ma orderCode payOS hop le.");
            }

            return await ConfirmPayOsReturnAsync(orderCode);
        }

        public async Task<ServiceResult<MonthlyTicketDto>> CreatePendingPayOsPaymentAsync(string monthlyTicketId, string? customerId)
        {
            if (string.IsNullOrWhiteSpace(monthlyTicketId))
            {
                return ServiceResult<MonthlyTicketDto>.Fail("Không xác định được vé tháng cần in lại QR.");
            }

            var ticket = await _repo.GetByIdAsync(monthlyTicketId.Trim());
            if (ticket == null)
            {
                return ServiceResult<MonthlyTicketDto>.Fail("Không tìm thấy vé tháng cần in lại QR.");
            }

            if (!string.IsNullOrWhiteSpace(customerId) &&
                !string.Equals(ticket.CustomerId, customerId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<MonthlyTicketDto>.Fail("Bạn không có quyền in lại QR cho vé tháng này.");
            }

            if (!IsPendingPaymentTicket(ticket))
            {
                if (MonthlyTicketStatuses.IsActive(ticket.Status))
                {
                    return ServiceResult<MonthlyTicketDto>.Fail("Vé tháng này đã hoạt động nên không cần in lại QR.");
                }

                if (string.Equals(ticket.Status, MonthlyTicketStatuses.CANCELLED, StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<MonthlyTicketDto>.Fail("Vé tháng đã hủy. Vui lòng đặt lại vé mới.");
                }

                return ServiceResult<MonthlyTicketDto>.Fail("Chỉ có thể in lại QR cho vé đang chờ thanh toán.");
            }

            await TrySyncPayOsPaymentAsync(ticket);
            if (MonthlyTicketStatuses.IsActive(ticket.Status))
            {
                return ServiceResult<MonthlyTicketDto>.Ok(MapToDto(ticket), "Thanh toán đã được xác nhận. Vé tháng đã được kích hoạt.");
            }

            var orderCode = GeneratePayOsOrderCode();
            var paymentLink = await _payOsService.CreatePaymentLinkAsync(new PayOsCreatePaymentLinkDto
            {
                OrderCode = orderCode,
                Amount = decimal.ToInt32(ticket.TotalFee),
                Description = ticket.MonthlyTicketId,
                ItemName = $"Ve thang {ticket.MonthlyTicketId}"
            });

            if (!paymentLink.Success || paymentLink.Data == null)
            {
                return ServiceResult<MonthlyTicketDto>.Fail(
                    paymentLink.Message
                    ?? "Không tạo lại được QR thanh toán PayOS. Vui lòng cấu hình PayOS để thanh toán online.");
            }

            await AddPendingPaymentAsync(ticket.MonthlyTicketId, ticket.TotalFee, PaymentMethods.BANK_TRANSFER, orderCode);

            var dto = MapToDto(ticket);
            dto.PayOsOrderCode = orderCode;
            dto.PayOsPaymentLinkId = paymentLink.Data.PaymentLinkId;
            dto.CheckoutUrl = paymentLink.Data.CheckoutUrl;
            dto.QrCode = paymentLink.Data.QrCode;

            return ServiceResult<MonthlyTicketDto>.Ok(dto, "Đã tạo lại QR thanh toán cho vé tháng đang chờ thanh toán.");
        }

        public async Task<List<MonthlyTicketDto>> GetExpiringSoonAsync(int days = 7)
        {
            var list = await _repo.GetExpiringSoonAsync(days);
            return list.Select(MapToDto).ToList();
        }

        public async Task<decimal> CalculateFeeAsync(string vehicleType, string packageType, string? customerId = null)
        {
            var months = GetPackageMonths(packageType);
            var totalFee = months == 0
                ? 0m
                : await _pricingService.GetMonthlyTicketPriceAsync(vehicleType, months);

            if (customerId != null && totalFee > 0)
            {
                var customer = await _customerRepo.GetByIdAsync(customerId);
                if (customer != null)
                {
                    await ReconcileCustomerVipFromMonthlyTicketsAsync(customer);
                }

                if (customer != null && customer.VipLevel != ParkingManagement.BLL.Helpers.VipHelper.MEMBER)
                {
                    var discountPercent = ParkingManagement.BLL.Helpers.VipHelper.GetVipDiscountPercent(customer.VipLevel);
                    if (discountPercent > 0)
                    {
                        totalFee = totalFee - (totalFee * discountPercent / 100);
                    }
                }
            }

            return totalFee;
        }

        private async Task ReconcileCustomerVipFromMonthlyTicketsAsync(Customer customer)
        {
            var monthlyTickets = await _repo.GetByCustomerIdAsync(customer.CustomerId);
            await ReconcileCustomerVipFromMonthlyTicketsAsync(customer, monthlyTickets);
        }

        private async Task ReconcileCustomerVipFromMonthlyTicketsAsync(Customer customer, IEnumerable<MonthlyTicket> monthlyTickets)
        {
            var paidMonthlyTickets = monthlyTickets
                .Where(IsPaidMonthlyTicketForVip)
                .ToList();
            var actualSpent = Math.Max(customer.TotalSpent, paidMonthlyTickets.Sum(ticket => ticket.TotalFee));
            var actualTickets = Math.Max(customer.TotalTickets, paidMonthlyTickets.Count);
            var actualVipLevel = ParkingManagement.BLL.Helpers.VipHelper.DetermineVipLevel(actualSpent);

            if (customer.TotalSpent == actualSpent &&
                customer.TotalTickets == actualTickets &&
                string.Equals(customer.VipLevel, actualVipLevel, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            customer.TotalSpent = actualSpent;
            customer.TotalTickets = actualTickets;
            customer.VipLevel = actualVipLevel;

            await _customerRepo.UpdateAsync(customer);
        }

        private static bool IsPaidMonthlyTicketForVip(MonthlyTicket ticket)
        {
            if (ticket.TotalFee <= 0)
            {
                return false;
            }

            var status = ticket.Status?.Trim();
            return MonthlyTicketStatuses.IsActive(status)
                || string.Equals(status, MonthlyTicketStatuses.EXPIRED, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, MonthlyTicketStatuses.CANCELLED, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase);
        }

        private async Task AddPaymentAsync(string monthlyTicketId, decimal fee, string? paymentMethod)
        {
            var paymentId = await _paymentRepo.GenerateIdAsync();
            await _paymentRepo.AddAsync(new Payment
            {
                PaymentId = paymentId,
                TicketId = null,
                MonthlyTicketId = monthlyTicketId,
                Amount = fee,
                Method = PaymentMethods.Normalize(paymentMethod ?? PaymentMethods.BANK_TRANSFER),
                PaymentTime = DateTime.Now,
                Status = PaymentStatuses.SUCCESS
            });
        }

        private async Task<Payment> AddPendingPaymentAsync(string monthlyTicketId, decimal fee, string? paymentMethod, long orderCode)
        {
            var payment = new Payment
            {
                PaymentId = await _paymentRepo.GenerateIdAsync(),
                TicketId = null,
                MonthlyTicketId = monthlyTicketId,
                Amount = fee,
                Method = PaymentMethods.Normalize(paymentMethod ?? PaymentMethods.BANK_TRANSFER),
                PaymentTime = DateTime.Now,
                Status = PaymentStatuses.PENDING,
                VnpTxnRef = BuildPayOsTxnRef(orderCode)
            };

            await _paymentRepo.AddAsync(payment);
            return payment;
        }

        private async Task TrySyncPayOsPaymentAsync(MonthlyTicket ticket)
        {
            var payment = await FindPayOsPaymentAsync(ticket.MonthlyTicketId);
            if (payment == null || PaymentStatuses.IsSuccessful(payment.Status))
            {
                return;
            }

            if (!TryGetPayOsOrderCode(payment.VnpTxnRef, out var orderCode))
            {
                return;
            }

            var result = await ConfirmPayOsReturnAsync(orderCode);
            if (result.Success)
            {
                ticket.Status = MonthlyTicketStatuses.ACTIVE;
            }
        }

        private async Task<Payment?> FindPayOsPaymentAsync(string monthlyTicketId)
        {
            var payments = await _paymentRepo.GetAllAsync();
            return payments
                .Where(payment => string.Equals(payment.MonthlyTicketId, monthlyTicketId, StringComparison.OrdinalIgnoreCase))
                .Where(payment => TryGetPayOsOrderCode(payment.VnpTxnRef, out _))
                .OrderByDescending(payment => payment.PaymentTime)
                .FirstOrDefault();
        }

        private static bool IsPendingPaymentTicket(MonthlyTicket ticket) =>
            string.Equals(ticket.Status, MonthlyTicketStatuses.PENDING_PAYMENT, StringComparison.OrdinalIgnoreCase);

        private static bool TryGetPayOsOrderCode(string? txnRef, out long orderCode)
        {
            orderCode = 0;
            const string prefix = "PAYOS:";

            return !string.IsNullOrWhiteSpace(txnRef) &&
                txnRef.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                long.TryParse(txnRef[prefix.Length..], out orderCode);
        }

        private static long GeneratePayOsOrderCode()
        {
            var suffix = RandomNumberGenerator.GetInt32(10, 99);
            return long.Parse($"{DateTime.UtcNow:yyMMddHHmmss}{suffix}");
        }

        private static string BuildPayOsTxnRef(long orderCode) => $"PAYOS:{orderCode}";

        private static int GetPackageMonths(string packageType) => packageType switch
        {
            "1 tháng" => 1,
            "3 tháng" => 3,
            "6 tháng" => 6,
            _ => 0
        };

        private static MonthlyTicketDto MapToDto(MonthlyTicket ticket) => new()
        {
            MonthlyTicketId = ticket.MonthlyTicketId,
            CustomerName = ticket.Customer?.FullName ?? "",
            VehiclePlate = ticket.VehiclePlate,
            VehicleType = ticket.VehicleType,
            PackageType = ticket.PackageType,
            StartDate = ticket.StartDate,
            EndDate = ticket.EndDate,
            TotalFee = ticket.TotalFee,
            Status = ticket.Status,
            DaysRemaining = MonthlyTicketStatuses.IsActive(ticket.Status)
                ? Math.Max(0, (int)(ticket.EndDate.Date - DateTime.Today).TotalDays)
                : 0
        };
    }
}
