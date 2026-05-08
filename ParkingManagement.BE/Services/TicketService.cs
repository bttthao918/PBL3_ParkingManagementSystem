using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.BLL.Strategies;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IMonthlyTicketRepository _monthlyTicketRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IParkingSlotRepository _slotRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ICheckInValidator _validator;
        private readonly IParkingSlotStrategy _slotStrategy;

        private const decimal HOURLY_RATE = 5000;
        private const decimal DAILY_RATE = 50000;
        private const int MIN_CHARGE_MINUTES = 15;

        public TicketService(
            ITicketRepository ticketRepository,
            IMonthlyTicketRepository monthlyTicketRepository,
            IReservationRepository reservationRepository,
            IParkingSlotRepository slotRepo,
            IVehicleRepository vehicleRepo,
            ICustomerRepository customerRepository,
            IPaymentRepository paymentRepo,
            ICheckInValidator validator,
            IParkingSlotStrategy slotStrategy)
        {
            _ticketRepository = ticketRepository;
            _monthlyTicketRepository = monthlyTicketRepository;
            _reservationRepository = reservationRepository;
            _slotRepo = slotRepo;
            _vehicleRepo = vehicleRepo;
            _customerRepository = customerRepository;
            _paymentRepo = paymentRepo;
            _validator = validator;
            _slotStrategy = slotStrategy;
        }

        // ── 1. General Ticket Management ──
        public async Task<ListTicketDto> GetTicketsAsync(TicketFilterDto filter)
        {
            var allTickets = await _ticketRepository.GetAllAsync();
            var filtered = allTickets.AsEnumerable();

            if (!string.IsNullOrEmpty(filter.Status))
                filtered = filtered.Where(t => t.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.VehicleType))
                filtered = filtered.Where(t => t.VehicleType == filter.VehicleType);

            if (filter.FromDate.HasValue)
                filtered = filtered.Where(t => t.CheckInTime.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                filtered = filtered.Where(t => t.CheckInTime.Date <= filter.ToDate.Value.Date);

            if (!string.IsNullOrEmpty(filter.SearchKeyword))
                filtered = filtered.Where(t =>
                    t.VehiclePlate.Contains(filter.SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                    t.TicketId.Contains(filter.SearchKeyword, StringComparison.OrdinalIgnoreCase));

            var sorted = filtered.OrderByDescending(t => t.CheckInTime).ToList();

            var totalItems = sorted.Count;
            var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
            var items = sorted
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var ticketListDtos = new List<TicketListDto>();
            foreach (var ticket in items)
            {
                var customerName = ticket.CustomerId != null
                    ? (await _customerRepository.GetByIdAsync(ticket.CustomerId))?.FullName
                    : null;

                ticketListDtos.Add(new TicketListDto
                {
                    TicketId = ticket.TicketId,
                    VehiclePlate = ticket.VehiclePlate,
                    VehicleType = ticket.VehicleType,
                    CheckInTime = ticket.CheckInTime,
                    CheckOutTime = ticket.CheckOutTime,
                    Status = ticket.Status,
                    Fee = ticket.Fee,
                    SlotId = ticket.SlotId,
                    CustomerName = customerName
                });
            }

            return new ListTicketDto
            {
                Items = ticketListDtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<TicketDetailDto> GetTicketDetailAsync(string ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new Exception($"Không tìm thấy vé: {ticketId}");

            string? customerName = null;
            string? customerPhone = null;

            if (ticket.CustomerId != null)
            {
                var customer = await _customerRepository.GetByIdAsync(ticket.CustomerId);
                if (customer != null)
                {
                    customerName = customer.FullName;
                    customerPhone = customer.PhoneNumber;
                }
            }

            string? monthlyTicketId = null;
            bool hasActiveMonthlyTicket = false;

            if (ticket.CustomerId != null)
            {
                var monthlyTicket = await _monthlyTicketRepository.GetActiveByPlateAsync(ticket.VehiclePlate);
                if (monthlyTicket != null)
                {
                    monthlyTicketId = monthlyTicket.MonthlyTicketId;
                    hasActiveMonthlyTicket = true;
                }
            }

            int? durationMinutes = null;
            if (ticket.CheckOutTime.HasValue)
                durationMinutes = (int)(ticket.CheckOutTime.Value - ticket.CheckInTime).TotalMinutes;

            return new TicketDetailDto
            {
                TicketId = ticket.TicketId,
                VehiclePlate = ticket.VehiclePlate,
                VehicleType = ticket.VehicleType,
                CheckInTime = ticket.CheckInTime,
                CheckOutTime = ticket.CheckOutTime,
                Status = ticket.Status,
                Fee = ticket.Fee,
                SlotId = ticket.SlotId,
                DurationMinutes = durationMinutes,
                CustomerId = ticket.CustomerId,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                MonthlyTicketId = monthlyTicketId,
                HasActiveMonthlyTicket = hasActiveMonthlyTicket
            };
        }

        public async Task<ListEmployeeTicketDto> SearchTicketsAsync(EmployeeTicketSearchDto search)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(search.SearchKeyword))
                    return new ListEmployeeTicketDto
                    {
                        Items = new(),
                        PageNumber = search.PageNumber,
                        PageSize = search.PageSize,
                        TotalItems = 0,
                        TotalPages = 0
                    };

                var allTickets = await _ticketRepository.GetAllAsync();
                var keyword = search.SearchKeyword.Trim();
                var searched = allTickets
                    .Where(t =>
                        t.VehiclePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        t.TicketId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.CheckInTime)
                    .ToList();

                var totalItems = searched.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)search.PageSize);
                var items = searched
                    .Skip((search.PageNumber - 1) * search.PageSize)
                    .Take(search.PageSize)
                    .ToList();

                var ticketDtos = new List<EmployeeTicketListDto>();
                foreach (var ticket in items)
                {
                    var customerName = ticket.CustomerId != null
                        ? (await _customerRepository.GetByIdAsync(ticket.CustomerId))?.FullName
                        : null;

                    ticketDtos.Add(new EmployeeTicketListDto
                    {
                        TicketId = ticket.TicketId,
                        VehiclePlate = ticket.VehiclePlate,
                        VehicleType = ticket.VehicleType,
                        CheckInTime = ticket.CheckInTime,
                        CheckOutTime = ticket.CheckOutTime,
                        Status = ticket.Status,
                        Fee = ticket.Fee,
                        SlotId = ticket.SlotId,
                        CustomerName = customerName
                    });
                }

                return new ListEmployeeTicketDto
                {
                    Items = ticketDtos,
                    PageNumber = search.PageNumber,
                    PageSize = search.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new ListEmployeeTicketDto
                {
                    Items = new(),
                    PageNumber = search.PageNumber,
                    PageSize = search.PageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
        }

        // ── 2. Customer Specific ──
        public async Task<ListCustomerTicketDto> GetMyTicketsAsync(string customerId, CustomerTicketFilterDto filter)
        {
            try
            {
                var allTickets = await _ticketRepository.GetByCustomerIdAsync(customerId);
                var filtered = allTickets.AsEnumerable();

                if (!string.IsNullOrEmpty(filter.Status))
                    filtered = filtered.Where(t => t.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.VehicleType))
                    filtered = filtered.Where(t => t.VehicleType == filter.VehicleType);

                var sorted = filtered.OrderByDescending(t => t.CheckInTime).ToList();

                var totalItems = sorted.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var ticketDtos = items.Select(t => new CustomerTicketListDto
                {
                    TicketId = t.TicketId,
                    VehiclePlate = t.VehiclePlate,
                    VehicleType = t.VehicleType,
                    CheckInTime = t.CheckInTime,
                    CheckOutTime = t.CheckOutTime,
                    Status = t.Status,
                    Fee = t.Fee,
                    SlotId = t.SlotId
                }).ToList();

                return new ListCustomerTicketDto
                {
                    Items = ticketDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new ListCustomerTicketDto
                {
                    Items = new(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
        }

        public async Task<CustomerTicketDetailDto> GetCustomerTicketDetailAsync(string customerId, string ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null || ticket.CustomerId != customerId)
                    throw new Exception("Vé không tồn tại hoặc không thuộc về bạn");

                int? durationMinutes = null;
                if (ticket.CheckOutTime.HasValue)
                    durationMinutes = (int)(ticket.CheckOutTime.Value - ticket.CheckInTime).TotalMinutes;

                var payment = await _paymentRepo.GetByTicketIdAsync(ticketId);
                var monthlyTicket = await _monthlyTicketRepository.GetActiveByPlateAsync(ticket.VehiclePlate);

                return new CustomerTicketDetailDto
                {
                    TicketId = ticket.TicketId,
                    VehiclePlate = ticket.VehiclePlate,
                    VehicleType = ticket.VehicleType,
                    CheckInTime = ticket.CheckInTime,
                    CheckOutTime = ticket.CheckOutTime,
                    Status = ticket.Status,
                    Fee = ticket.Fee,
                    SlotId = ticket.SlotId,
                    DurationMinutes = durationMinutes,
                    PaymentId = payment?.PaymentId,
                    PaymentMethod = payment?.Method,
                    PaymentStatus = payment?.Status,
                    PaymentTime = payment?.PaymentTime,
                    MonthlyTicketId = monthlyTicket?.MonthlyTicketId,
                    HasMonthlyTicket = monthlyTicket != null && monthlyTicket.Status == "Hoạt động"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết vé: {ex.Message}");
            }
        }

        public async Task<ListCustomerPaymentDto> GetPaymentHistoryAsync(string customerId, CustomerPaymentFilterDto filter)
        {
            try
            {
                var tickets = await _ticketRepository.GetByCustomerIdAsync(customerId);
                var ticketIds = tickets.Select(t => t.TicketId).ToList();

                var allPayments = await _paymentRepo.GetAllAsync();
                var customerPayments = allPayments
                    .Where(p => ticketIds.Contains(p.TicketId ?? "") || 
                               (p.MonthlyTicketId != null && 
                                tickets.Any(t => t.CustomerId == customerId)))
                    .ToList();

                var filtered = customerPayments.AsEnumerable();
                if (!string.IsNullOrEmpty(filter.Status))
                    filtered = filtered.Where(p => p.Status == filter.Status);

                var sorted = filtered.OrderByDescending(p => p.PaymentTime).ToList();

                var totalItems = sorted.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var paymentDtos = new List<CustomerPaymentListDto>();
                foreach (var payment in items)
                {
                    var ticket = tickets.FirstOrDefault(t => t.TicketId == payment.TicketId);
                    paymentDtos.Add(new CustomerPaymentListDto
                    {
                        PaymentId = payment.PaymentId,
                        TicketId = payment.TicketId ?? payment.MonthlyTicketId ?? "",
                        VehiclePlate = ticket?.VehiclePlate,
                        Amount = payment.Amount,
                        PaymentMethod = payment.Method,
                        Status = payment.Status,
                        CreatedAt = payment.PaymentTime,
                        ConfirmedAt = payment.PaymentTime
                    });
                }

                return new ListCustomerPaymentDto
                {
                    Items = paymentDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new ListCustomerPaymentDto
                {
                    Items = new(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
        }

        // ── 3. Check-in ──
        public async Task<CheckInValidationDto> ValidateAndPrepareCheckInAsync(CheckInInputDto input)
        {
            var validationResult = _validator.ValidateInput(input);
            if (!validationResult.IsValid)
            {
                return new CheckInValidationDto { HasVehicleRecord = false, Message = validationResult.ErrorMessage };
            }

            var vehiclePlate = input.VehiclePlate.Trim().ToUpper();

            var activeTicket = await _ticketRepository.GetActiveByPlateAsync(vehiclePlate);
            if (activeTicket != null)
            {
                return new CheckInValidationDto { HasVehicleRecord = true, Message = "Xe này đang trong bãi rồi. Không thể check-in lại." };
            }

            var vehicle = await _vehicleRepo.GetByPlateAsync(vehiclePlate);
            string? foundCustomerId = null;
            string? foundCustomerName = null;

            if (vehicle?.CustomerId != null)
            {
                foundCustomerId = vehicle.CustomerId;
                var customer = await _customerRepository.GetByIdAsync(foundCustomerId);
                foundCustomerName = customer?.FullName;
            }

            var monthlyTicket = await _monthlyTicketRepository.GetActiveByPlateAsync(vehiclePlate);
            bool hasMonthlyTicket = monthlyTicket != null;

            var reservation = await _reservationRepository.GetActiveByPlateAsync(vehiclePlate);
            bool hasReservation = reservation != null;

            List<AvailableSlotDto> availableSlots = new();

            if (hasReservation && reservation?.SlotId != null)
            {
                var reservedSlot = await _slotRepo.GetByIdAsync(reservation.SlotId);
                if (reservedSlot != null && reservedSlot.Status == "Đã đặt")
                {
                    availableSlots.Add(new AvailableSlotDto
                    {
                        SlotId = reservedSlot.SlotId,
                        Location = reservedSlot.Location,
                        VehicleType = reservedSlot.VehicleType
                    });
                }
            }

            if (!availableSlots.Any())
            {
                availableSlots = await _slotStrategy.FindAvailableSlotsAsync(input.VehicleType);
            }

            if (!availableSlots.Any())
            {
                return new CheckInValidationDto
                {
                    HasVehicleRecord = vehicle != null,
                    CustomerId = foundCustomerId,
                    CustomerName = foundCustomerName,
                    HasMonthlyTicket = hasMonthlyTicket,
                    MonthlyTicketId = monthlyTicket?.MonthlyTicketId,
                    MonthlyTicketExpiryDate = monthlyTicket?.EndDate,
                    HasReservation = hasReservation,
                    ReservationId = reservation?.ReservationId,
                    PreferredSlotId = reservation?.SlotId,
                    Message = "Bãi xe hiện đã hết chỗ trống cho loại xe này."
                };
            }

            var message = BuildCheckInMessage(hasMonthlyTicket, hasReservation, foundCustomerName);

            return new CheckInValidationDto
            {
                HasVehicleRecord = vehicle != null,
                CustomerId = foundCustomerId ?? input.CustomerId,
                CustomerName = foundCustomerName,
                HasMonthlyTicket = hasMonthlyTicket,
                MonthlyTicketId = monthlyTicket?.MonthlyTicketId,
                MonthlyTicketExpiryDate = monthlyTicket?.EndDate,
                HasReservation = hasReservation,
                ReservationId = reservation?.ReservationId,
                PreferredSlotId = reservation?.SlotId,
                AvailableSlots = availableSlots,
                Message = message
            };
        }

        public async Task<CheckInResultDto> ConfirmCheckInAsync(ConfirmCheckInDto input)
        {
            var validationResult = _validator.ValidateInput(
                new CheckInInputDto { VehiclePlate = input.VehiclePlate, VehicleType = input.VehicleType });
            if (!validationResult.IsValid)
                return new CheckInResultDto { Success = false, Message = validationResult.ErrorMessage };

            var vehiclePlate = input.VehiclePlate.Trim().ToUpper();

            var activeTicket = await _ticketRepository.GetActiveByPlateAsync(vehiclePlate);
            if (activeTicket != null)
                return new CheckInResultDto { Success = false, Message = "Xe này đang trong bãi rồi." };

            var slot = await _slotRepo.GetByIdAsync(input.SlotId);
            if (slot == null || (slot.Status != "Trống" && slot.Status != "Đã đặt"))
                return new CheckInResultDto { Success = false, Message = "Chỗ đỗ không còn trống hoặc không hợp lệ." };

            var vehicle = await _vehicleRepo.GetByPlateAsync(vehiclePlate);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    VehiclePlate = vehiclePlate,
                    VehicleType = input.VehicleType,
                    CustomerId = input.CustomerId
                };
                await _vehicleRepo.AddAsync(vehicle);
            }

            var reservation = await _reservationRepository.GetActiveByPlateAsync(vehiclePlate);
            if (reservation != null)
            {
                reservation.Status = "Đã nhận";
                await _reservationRepository.UpdateAsync(reservation);
            }

            var ticketId = await _ticketRepository.GenerateIdAsync();
            var ticket = new Ticket
            {
                TicketId = ticketId,
                CustomerId = input.CustomerId,
                VehiclePlate = vehiclePlate,
                VehicleType = input.VehicleType,
                SlotId = input.SlotId,
                CheckInTime = DateTime.Now,
                CheckOutTime = null,
                Fee = 0,
                Status = "Đang trong bãi"
            };
            await _ticketRepository.AddAsync(ticket);

            await _slotRepo.UpdateStatusAsync(input.SlotId, "Đang sử dụng");

            return new CheckInResultDto
            {
                Success = true,
                Message = "Check-in thành công!",
                TicketId = ticketId,
                SlotId = input.SlotId,
                CheckInTime = ticket.CheckInTime
            };
        }

        private string BuildCheckInMessage(bool hasMonthlyTicket, bool hasReservation, string? customerName)
        {
            var messages = new List<string>();
            if (hasMonthlyTicket) messages.Add("✓ Xe có vé tháng còn hạn - Miễn phí check-in");
            if (hasReservation) messages.Add("✓ Xe có đặt chỗ - Chỗ ưu tiên sẵn sàng");
            if (!string.IsNullOrEmpty(customerName)) messages.Add($"✓ Khách hàng: {customerName}");
            if (!messages.Any()) messages.Add("• Khách hàng mới - Sẽ tạo record");
            return string.Join(" | ", messages);
        }

        // ── 4. Check-out ──
        public async Task<CheckOutValidationDto> ValidateAndPrepareCheckOutAsync(CheckOutInputDto input)
        {
            if (string.IsNullOrWhiteSpace(input.VehiclePlateOrTicketId))
                return new CheckOutValidationDto { Success = false, Message = "Vui lòng nhập mã vé hoặc biển số xe." };

            var identifier = input.VehiclePlateOrTicketId.Trim().ToUpper();

            Ticket? ticket = null;
            if (identifier.StartsWith("TKT"))
                ticket = await _ticketRepository.GetByIdAsync(identifier);
            else
                ticket = await _ticketRepository.GetActiveByPlateAsync(identifier);

            if (ticket == null)
                return new CheckOutValidationDto { Success = false, Message = "Không tìm thấy vé lượt. Vui lòng kiểm tra lại mã vé hoặc biển số." };

            if (ticket.CheckOutTime != null)
                return new CheckOutValidationDto { Success = false, Message = $"Vé này đã được check-out vào lúc {ticket.CheckOutTime:dd/MM/yyyy HH:mm}." };

            var monthlyTicket = ticket.CustomerId != null 
                ? await _monthlyTicketRepository.GetActiveByPlateAsync(ticket.VehiclePlate) 
                : null;
            bool isFreeTicket = monthlyTicket != null;

            var currentTime = DateTime.Now;
            var duration = currentTime - ticket.CheckInTime;
            int durationMinutes = (int)duration.TotalMinutes;

            decimal calculatedFee = 0;
            if (!isFreeTicket)
                calculatedFee = CalculateFee(durationMinutes);

            string? customerName = null;
            if (ticket.CustomerId != null)
            {
                var customer = await _customerRepository.GetByIdAsync(ticket.CustomerId);
                customerName = customer?.FullName;
            }

            var ticketType = isFreeTicket ? "Vé tháng" : "Vé lượt";
            var message = BuildCheckOutMessage(ticketType, durationMinutes, calculatedFee, isFreeTicket);

            return new CheckOutValidationDto
            {
                Success = true,
                TicketId = ticket.TicketId,
                VehiclePlate = ticket.VehiclePlate,
                VehicleType = ticket.VehicleType,
                CustomerName = customerName,
                CheckInTime = ticket.CheckInTime,
                CurrentTime = currentTime,
                DurationMinutes = durationMinutes,
                TicketType = ticketType,
                IsFreeTicket = isFreeTicket,
                CalculatedFee = calculatedFee,
                Message = message
            };
        }

        public async Task<CheckOutResultDto> ConfirmCheckOutAsync(ConfirmCheckOutDto input)
        {
            var ticket = await _ticketRepository.GetByIdAsync(input.TicketId);
            if (ticket == null)
                return new CheckOutResultDto { Success = false, Message = "Không tìm thấy vé." };

            if (ticket.CheckOutTime != null)
                return new CheckOutResultDto { Success = false, Message = "Vé này đã được check-out rồi." };

            var currentTime = DateTime.Now;
            ticket.CheckOutTime = currentTime;
            ticket.Fee = input.Fee;
            ticket.Status = "Đã ra";
            await _ticketRepository.UpdateAsync(ticket);

            if (!string.IsNullOrEmpty(ticket.SlotId))
                await _slotRepo.UpdateStatusAsync(ticket.SlotId, "Trống");

            string? paymentId = null;
            if (input.Fee > 0)
            {
                var payment = new Payment
                {
                    PaymentId = await _paymentRepo.GenerateIdAsync(),
                    TicketId = ticket.TicketId,
                    Amount = input.Fee,
                    PaymentTime = currentTime,
                    Status = "Hoàn tất"
                };
                await _paymentRepo.AddAsync(payment);
                paymentId = payment.PaymentId;
            }

            int durationMinutes = (int)(currentTime - ticket.CheckInTime).TotalMinutes;

            return new CheckOutResultDto
            {
                Success = true,
                Message = "Check-out thành công! Xe ra khỏi bãi.",
                TicketId = ticket.TicketId,
                VehiclePlate = ticket.VehiclePlate,
                CheckInTime = ticket.CheckInTime,
                CheckOutTime = currentTime,
                DurationMinutes = durationMinutes,
                Fee = input.Fee,
                IsFree = input.Fee == 0,
                PaymentId = paymentId
            };
        }

        private decimal CalculateFee(int durationMinutes)
        {
            int chargeMinutes = durationMinutes < MIN_CHARGE_MINUTES ? MIN_CHARGE_MINUTES : durationMinutes;

            if (durationMinutes >= 1440)
            {
                int days = durationMinutes / 1440;
                int remainingMinutes = durationMinutes % 1440;
                decimal fee = days * DAILY_RATE;
                if (remainingMinutes > 0)
                    fee += ((decimal)remainingMinutes / 60) * HOURLY_RATE;
                return fee;
            }

            decimal hours = (decimal)chargeMinutes / 60;
            return hours * HOURLY_RATE;
        }

        private string BuildCheckOutMessage(string ticketType, int durationMinutes, decimal fee, bool isFree)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;
            string durationText = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";

            if (isFree) return $"✓ {ticketType} - Miễn phí | Thời gian giữ: {durationText}";
            return $"✓ {ticketType} | Thời gian giữ: {durationText} | Phí: {fee:N0} VND";
        }
    }
}
