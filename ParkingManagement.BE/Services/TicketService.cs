using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.BLL.Strategies;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using System.Globalization;

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
        private readonly IPricingService _pricingService;

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
            IParkingSlotStrategy slotStrategy,
            IPricingService pricingService)
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
            _pricingService = pricingService;
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

            if (!string.IsNullOrEmpty(filter.AreaFilter))
                filtered = filtered.Where(t => !string.IsNullOrEmpty(t.SlotId) &&
                                               t.SlotId.StartsWith(filter.AreaFilter, StringComparison.OrdinalIgnoreCase));

            if (filter.FromDate.HasValue)
                filtered = filtered.Where(t => t.CheckInTime.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                filtered = filtered.Where(t => t.CheckInTime.Date <= filter.ToDate.Value.Date);

            Dictionary<string, Customer>? customerLookup = null;
            if (!string.IsNullOrEmpty(filter.SearchKeyword))
            {
                var customers = await _customerRepository.GetAllAsync();
                customerLookup = customers.ToDictionary(c => c.CustomerId, c => c);
                var keyword = filter.SearchKeyword.Trim();
                filtered = filtered.Where(t =>
                    t.VehiclePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    t.TicketId.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (t.CustomerId != null &&
                     customerLookup.TryGetValue(t.CustomerId, out var customer) &&
                     (customer.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                      (customer.PhoneNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))));
            }

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
                var customerName = await ResolveCustomerNameAsync(ticket.CustomerId, customerLookup);

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

        private async Task<string?> ResolveCustomerNameAsync(string? customerId, Dictionary<string, Customer>? customerLookup = null)
        {
            if (string.IsNullOrEmpty(customerId))
                return null;

            if (customerLookup != null && customerLookup.TryGetValue(customerId, out var cachedCustomer))
                return cachedCustomer.FullName;

            return (await _customerRepository.GetByIdAsync(customerId))?.FullName;
        }

        public async Task<TicketSummaryDto> GetTicketSummaryAsync()
        {
            var tickets = await _ticketRepository.GetAllAsync();

            return new TicketSummaryDto
            {
                TotalTickets = tickets.Count,
                ActiveTickets = tickets.Count(t => t.Status == "Đang trong bãi"),
                CheckedOutTickets = tickets.Count(t => t.Status == "Đã ra"),
                TotalRevenue = tickets.Sum(t => t.Fee)
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

        public async Task<TicketDetailDto> UpdateTicketAsync(string ticketId, UpdateTicketDto input)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException($"Không tìm thấy vé: {ticketId}");

            var vehiclePlate = input.VehiclePlate.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(vehiclePlate))
                throw new ArgumentException("Biển số xe không được để trống.");

            if (input.Status != "Đang trong bãi" && input.Status != "Đã ra")
                throw new ArgumentException("Trạng thái vé không hợp lệ.");

            var vehicle = await _vehicleRepo.GetByPlateAsync(vehiclePlate);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    VehiclePlate = vehiclePlate,
                    VehicleType = input.VehicleType,
                    CustomerId = ticket.CustomerId
                };
                await _vehicleRepo.AddAsync(vehicle);
            }
            else if (vehicle.VehicleType != input.VehicleType)
            {
                vehicle.VehicleType = input.VehicleType;
                await _vehicleRepo.UpdateAsync(vehicle);
            }

            var previousSlotId = ticket.SlotId;
            var previousStatus = ticket.Status;
            var newSlotId = string.IsNullOrWhiteSpace(input.SlotId) ? null : input.SlotId.Trim();

            if (!string.IsNullOrEmpty(newSlotId) && await _slotRepo.GetByIdAsync(newSlotId) == null)
                throw new ArgumentException("Vị trí đỗ không tồn tại.");

            ticket.VehiclePlate = vehiclePlate;
            ticket.VehicleType = input.VehicleType;
            ticket.CheckInTime = input.CheckInTime;
            ticket.Status = input.Status;
            ticket.Fee = input.Fee < 0 ? 0 : input.Fee;
            ticket.SlotId = newSlotId;
            ticket.CheckOutTime = input.Status == "Đang trong bãi"
                ? null
                : input.CheckOutTime ?? ticket.CheckOutTime ?? DateTime.Now;

            await _ticketRepository.UpdateAsync(ticket);

            if (!string.IsNullOrEmpty(previousSlotId) &&
                (previousSlotId != ticket.SlotId || previousStatus != ticket.Status) &&
                (ticket.Status == "Đã ra" || previousSlotId != ticket.SlotId))
            {
                await _slotRepo.UpdateStatusAsync(previousSlotId, "Trống");
            }

            if (ticket.Status == "Đang trong bãi" && !string.IsNullOrEmpty(ticket.SlotId))
            {
                await _slotRepo.UpdateStatusAsync(ticket.SlotId, "Đang sử dụng");
            }

            return await GetTicketDetailAsync(ticketId);
        }

        public async Task<bool> DeleteTicketAsync(string ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                return false;

            if (ticket.Status == "Đang trong bãi" && !string.IsNullOrEmpty(ticket.SlotId))
            {
                await _slotRepo.UpdateStatusAsync(ticket.SlotId, "Trống");
            }

            await _ticketRepository.DeleteAsync(ticketId);
            return true;
        }

        public async Task<ListEmployeeTicketDto> SearchTicketsAsync(EmployeeTicketSearchDto search)
        {
            try
            {
                var allTickets = await _ticketRepository.GetAllAsync();
                var searched = allTickets.AsEnumerable();

                // Nếu có keyword thì filter, không thì lấy tất cả
                if (!string.IsNullOrWhiteSpace(search.SearchKeyword))
                {
                    var keyword = search.SearchKeyword.Trim();
                    searched = searched.Where(t =>
                        t.VehiclePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        t.TicketId.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                }

                var sorted = searched.OrderByDescending(t => t.CheckInTime).ToList();

                var totalItems = sorted.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)search.PageSize);
                var items = sorted
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
                var monthlyTickets = await _monthlyTicketRepository.GetByCustomerIdAsync(customerId);
                var monthlyTicketIds = monthlyTickets.Select(t => t.MonthlyTicketId).ToList();

                var allPayments = await _paymentRepo.GetAllAsync();
                var customerPayments = allPayments
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.TicketId) && ticketIds.Contains(p.TicketId)) ||
                        (!string.IsNullOrEmpty(p.MonthlyTicketId) && monthlyTicketIds.Contains(p.MonthlyTicketId)))
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
                    var monthlyTicket = monthlyTickets.FirstOrDefault(t => t.MonthlyTicketId == payment.MonthlyTicketId);
                    paymentDtos.Add(new CustomerPaymentListDto
                    {
                        PaymentId = payment.PaymentId,
                        TicketId = payment.TicketId ?? payment.MonthlyTicketId ?? "",
                        VehiclePlate = ticket?.VehiclePlate ?? monthlyTicket?.VehiclePlate,
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
                calculatedFee = await CalculateFeeAsync(durationMinutes, ticket.VehicleType);

            string? customerName = null;
            if (ticket.CustomerId != null)
            {
                var customer = await _customerRepository.GetByIdAsync(ticket.CustomerId);
                customerName = customer?.FullName;
            }

            var ticketType = isFreeTicket ? "Vé tháng" : "Vé lượt";
            var message = BuildCheckOutMessage(ticketType, durationMinutes, calculatedFee, isFreeTicket);
            var bankTransferContent = BuildBankTransferContent(ticket.TicketId);

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
                BankName = BidvQrInfo.BANK_NAME,
                BankAccount = BidvQrInfo.BANK_ACCOUNT,
                BankAccountHolder = BidvQrInfo.ACCOUNT_HOLDER,
                BankTransferContent = bankTransferContent,
                BankTransferQrUrl = calculatedFee > 0 ? BuildVietQrUrl(calculatedFee, bankTransferContent) : null,
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

            var validation = await ValidateAndPrepareCheckOutAsync(new CheckOutInputDto
            {
                VehiclePlateOrTicketId = input.TicketId,
                PaymentMethod = input.PaymentMethod
            });

            if (!validation.Success)
                return new CheckOutResultDto { Success = false, Message = validation.Message };

            var finalFee = input.Fee > 0 ? input.Fee : validation.CalculatedFee;
            var normalizedPaymentMethod = PaymentMethods.Normalize(input.PaymentMethod);

            if (finalFee > 0 &&
                RequiresReceivedConfirmation(input.PaymentMethod) &&
                !input.PaymentReceivedConfirmed)
            {
                return new CheckOutResultDto
                {
                    Success = false,
                    Message = "QR Pay chua duoc xac nhan da nhan tien vao tai khoan BIDV. Vui long kiem tra giao dich truoc khi check-out."
                };
            }

            var currentTime = DateTime.Now;
            ticket.CheckOutTime = currentTime;
            ticket.Fee = finalFee;
            ticket.Status = "Đã ra";
            await _ticketRepository.UpdateAsync(ticket);

            if (!string.IsNullOrEmpty(ticket.SlotId))
                await _slotRepo.UpdateStatusAsync(ticket.SlotId, "Trống");

            string? paymentId = null;
            if (finalFee > 0)
            {
                var payment = new Payment
                {
                    PaymentId = await _paymentRepo.GenerateIdAsync(),
                    TicketId = ticket.TicketId,
                    Amount = finalFee,
                    Method = normalizedPaymentMethod,
                    PaymentTime = currentTime,
<<<<<<< HEAD
                    Status = PaymentStatuses.SUCCESS,
                    CollectedByEmployeeId = input.CollectedByEmployeeId
=======
                    Status = "Thành công"
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
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
                Fee = finalFee,
                IsFree = finalFee == 0,
                PaymentId = paymentId
            };
        }

        private static bool RequiresReceivedConfirmation(string? paymentMethod)
        {
            return string.Equals(PaymentMethods.Normalize(paymentMethod), PaymentMethods.BANK_TRANSFER, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildBankTransferContent(string ticketId)
        {
            return $"PARKING-{ticketId}";
        }

        private static string BuildVietQrUrl(decimal amount, string transferContent)
        {
            var roundedAmount = Math.Round(amount, 0, MidpointRounding.AwayFromZero)
                .ToString("0", CultureInfo.InvariantCulture);
            var addInfo = Uri.EscapeDataString(transferContent);
            var accountName = Uri.EscapeDataString(BidvQrInfo.ACCOUNT_HOLDER);

            return $"https://img.vietqr.io/image/{BidvQrInfo.BANK_ID}-{BidvQrInfo.BANK_ACCOUNT}-{BidvQrInfo.QR_TEMPLATE}.png?amount={roundedAmount}&addInfo={addInfo}&accountName={accountName}";
        }

        /// <summary>
        /// Tinh phi gui xe theo bang gia hien tai, co gioi han toi da theo ngay.
        /// </summary>
        private async Task<decimal> CalculateFeeAsync(int durationMinutes, string vehicleType)
        {
            if (durationMinutes < MIN_CHARGE_MINUTES)
                durationMinutes = MIN_CHARGE_MINUTES;

            var pricing = await _pricingService.GetCurrentPricingAsync();
            var hourlyRate = GetPricingValue(pricing.HourlyRate, vehicleType, GetFallbackHourlyRate(vehicleType));
            var maxDailyFee = GetPricingValue(pricing.MaxDailyFee, vehicleType, GetFallbackMaxDailyFee(vehicleType));

            var fullDays = durationMinutes / (24 * 60);
            var remainingMinutes = durationMinutes % (24 * 60);
            var totalFee = fullDays * maxDailyFee;

            if (remainingMinutes > 0)
            {
                var remainingHours = Math.Ceiling(remainingMinutes / 60.0);
                totalFee += Math.Min((decimal)remainingHours * hourlyRate, maxDailyFee);
            }

            return totalFee;
        }

        private static decimal GetPricingValue(Dictionary<string, decimal> pricing, string vehicleType, decimal fallback)
        {
            if (pricing.TryGetValue(vehicleType, out var value) && value > 0)
                return value;

            var matchedValue = pricing
                .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase))
                .Value;

            return matchedValue > 0 ? matchedValue : fallback;
        }

        private static decimal GetFallbackHourlyRate(string vehicleType)
        {
            return vehicleType switch
            {
                "Ô tô nhỏ" => 5_000,
                "Ô tô lớn" => 8_000,
                _ => 3_000
            };
        }

        private static decimal GetFallbackMaxDailyFee(string vehicleType)
        {
            return vehicleType switch
            {
                "Ô tô nhỏ" => 50_000,
                "Ô tô lớn" => 80_000,
                _ => 30_000
            };
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
