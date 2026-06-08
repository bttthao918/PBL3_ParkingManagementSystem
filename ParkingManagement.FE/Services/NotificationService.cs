using System.Security.Claims;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface INotificationService
    {
        Task<NotificationCenterViewModel> GetNotificationsAsync(ClaimsPrincipal user);
    }

    public class NotificationService : INotificationService
    {
        private readonly IReportService _reportService;
        private readonly ITicketService _ticketService;
        private readonly IEmployeeMonthlyTicketService _employeeMonthlyTicketService;
        private readonly IReservationService _reservationService;
        private readonly ICustomerApiService _customerApiService;
        private readonly IShiftScheduleService _shiftScheduleService;
        private readonly IWorkLogService _workLogService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IReportService reportService,
            ITicketService ticketService,
            IEmployeeMonthlyTicketService employeeMonthlyTicketService,
            IReservationService reservationService,
            ICustomerApiService customerApiService,
            IShiftScheduleService shiftScheduleService,
            IWorkLogService workLogService,
            ILogger<NotificationService> logger)
        {
            _reportService = reportService;
            _ticketService = ticketService;
            _employeeMonthlyTicketService = employeeMonthlyTicketService;
            _reservationService = reservationService;
            _customerApiService = customerApiService;
            _shiftScheduleService = shiftScheduleService;
            _workLogService = workLogService;
            _logger = logger;
        }

        public async Task<NotificationCenterViewModel> GetNotificationsAsync(ClaimsPrincipal user)
        {
            var center = new NotificationCenterViewModel();
            if (user.Identity?.IsAuthenticated != true)
            {
                return center;
            }

            try
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
                if (IsAdminRole(role))
                {
                    await BuildManagerNotificationsAsync(center);
                }
                else if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    await BuildEmployeeNotificationsAsync(center);
                }
                else if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    await BuildCustomerNotificationsAsync(center);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build notification center");
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Chưa tải được thông báo",
                    Message = "Hệ thống chưa lấy được dữ liệu thông báo mới nhất.",
                    Icon = "fa-solid fa-triangle-exclamation",
                    Type = "warning"
                });
            }

            center.Items = center.Items
                .Take(6)
                .ToList();

            return center;
        }

        private async Task BuildManagerNotificationsAsync(NotificationCenterViewModel center)
        {
            var dashboardTask = _reportService.GetManagerDashboardAsync();
            var ticketSummaryTask = _ticketService.GetTicketSummaryAsync();
            var monthlyTicketsTask = _employeeMonthlyTicketService.GetAllAsync(null, null, null, 1, 1);

            await Task.WhenAll(dashboardTask, ticketSummaryTask, monthlyTicketsTask);

            var dashboard = await dashboardTask;
            var ticketSummary = await ticketSummaryTask;
            var monthlyTickets = await monthlyTicketsTask;

            if (dashboard != null)
            {
                if (dashboard.SlotUtilizationRate >= 90)
                {
                    center.Items.Add(new NotificationItemViewModel
                    {
                        Title = "Bãi đỗ gần đầy",
                        Message = $"{dashboard.OccupiedSlots:N0}/{dashboard.TotalSlots:N0} chỗ đang sử dụng ({dashboard.SlotUtilizationRate:0.0}%).",
                        Url = "/Admin/ParkingSlotManagement",
                        Icon = "fa-solid fa-square-parking",
                        Type = "danger"
                    });
                }
                else if (dashboard.SlotUtilizationRate >= 70)
                {
                    center.Items.Add(new NotificationItemViewModel
                    {
                        Title = "Lưu lượng bãi đang cao",
                        Message = $"{dashboard.SlotUtilizationRate:0.0}% sức chứa đang được dùng.",
                        Url = "/Admin/ParkingSlotManagement",
                        Icon = "fa-solid fa-square-parking",
                        Type = "warning"
                    });
                }

                if (dashboard.TotalActiveEmployees > 0 && dashboard.EmployeesOnline == 0)
                {
                    center.Items.Add(new NotificationItemViewModel
                    {
                        Title = "Chưa có nhân viên online",
                        Message = "Không có tài khoản nhân viên nào đang hoạt động trong hệ thống.",
                        Url = "/Admin/EmployeeManagement",
                        Icon = "fa-solid fa-user-clock",
                        Type = "warning"
                    });
                }

                if (dashboard.TodayTickets > 0)
                {
                    center.Items.Add(new NotificationItemViewModel
                    {
                        Title = "Có vé phát sinh hôm nay",
                        Message = $"{dashboard.TodayTickets:N0} vé, doanh thu {dashboard.TodayRevenue:N0} đ.",
                        Url = "/Admin/TicketManagement",
                        Icon = "fa-solid fa-ticket",
                        Type = "info"
                    });
                }
            }

            if (monthlyTickets?.Summary.ExpiringSoon > 0)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Vé tháng sắp hết hạn",
                    Message = $"{monthlyTickets.Summary.ExpiringSoon:N0} vé tháng cần theo dõi gia hạn.",
                    Url = "/Admin/MonthlyTicketManagement",
                    Icon = "fa-solid fa-id-card",
                    Type = "warning"
                });
            }

            if (ticketSummary?.ActiveTickets > 0)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Xe đang gửi trong bãi",
                    Message = $"{ticketSummary.ActiveTickets:N0} vé đang hoạt động.",
                    Url = "/Admin/TicketManagement",
                    Icon = "fa-solid fa-car-side",
                    Type = "info"
                });
            }
        }

        private async Task BuildEmployeeNotificationsAsync(NotificationCenterViewModel center)
        {
            var todayShiftTask = _shiftScheduleService.GetMyTodayShiftAsync();
            var workStatusTask = _workLogService.GetCurrentStatusAsync();
            var ticketSummaryTask = _ticketService.GetTicketSummaryAsync();
            var reservationsTask = _reservationService.GetForEmployeeAsync(status: "Chờ", pageNumber: 1, pageSize: 5);
            var monthlyTicketsTask = _employeeMonthlyTicketService.GetAllAsync(null, null, null, 1, 1);

            await Task.WhenAll(todayShiftTask, workStatusTask, ticketSummaryTask, reservationsTask, monthlyTicketsTask);

            var todayShift = await todayShiftTask;
            var workStatus = await workStatusTask;
            var ticketSummary = await ticketSummaryTask;
            var reservations = await reservationsTask;
            var monthlyTickets = await monthlyTicketsTask;

            if (workStatus?.IsWorking == true)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Bạn đang trong ca làm",
                    Message = $"Ca {workStatus.ShiftType ?? "hiện tại"} đã bắt đầu lúc {workStatus.StartTime:HH:mm}.",
                    Url = "/Employee/ShiftManagement",
                    Icon = "fa-solid fa-clock",
                    Type = "success"
                });
            }
            else if (todayShift?.HasShift == true && todayShift.Shift != null)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Ca làm hôm nay",
                    Message = $"{todayShift.Shift.ShiftType}: {todayShift.Shift.StartTime} - {todayShift.Shift.EndTime}.",
                    Url = "/Employee/ShiftManagement",
                    Icon = "fa-solid fa-calendar-day",
                    Type = "info"
                });
            }

            if (reservations?.Items.Count > 0)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Đặt chỗ đang chờ",
                    Message = $"{reservations.Items.Count:N0} đơn đặt chỗ cần kiểm tra.",
                    Url = "/Employee/ReservationManagement?StatusFilter=Chờ",
                    Icon = "fa-solid fa-calendar-check",
                    Type = "warning"
                });
            }

            if (monthlyTickets?.Summary.ExpiringSoon > 0)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Vé tháng sắp hết hạn",
                    Message = $"{monthlyTickets.Summary.ExpiringSoon:N0} vé tháng cần nhắc gia hạn.",
                    Url = "/Employee/MonthlyTicketManagement",
                    Icon = "fa-solid fa-id-card",
                    Type = "warning"
                });
            }

            if (ticketSummary?.ActiveTickets > 0)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Vé đang hoạt động",
                    Message = $"{ticketSummary.ActiveTickets:N0} xe đang gửi cần theo dõi.",
                    Url = "/Employee/ParkingOperation",
                    Icon = "fa-solid fa-car-side",
                    Type = "info"
                });
            }
        }

        private async Task BuildCustomerNotificationsAsync(NotificationCenterViewModel center)
        {
            var monthlyTicketsTask = _customerApiService.GetMonthlyTicketsAsync();
            var reservationsTask = _customerApiService.GetReservationsAsync(1, 5);
            var activeTicketsTask = _customerApiService.GetTicketsAsync(1, 5, "Đang gửi");

            await Task.WhenAll(monthlyTicketsTask, reservationsTask, activeTicketsTask);

            var monthlyTickets = await monthlyTicketsTask;
            var reservations = await reservationsTask;
            var activeTickets = await activeTicketsTask;

            var expiringTickets = monthlyTickets?.Items
                .Where(ticket => IsActive(ticket.Status) && ticket.DaysRemaining >= 0 && ticket.DaysRemaining <= 7)
                .OrderBy(ticket => ticket.DaysRemaining)
                .ToList() ?? new List<CustomerMonthlyTicketDto>();

            foreach (var ticket in expiringTickets.Take(2))
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Vé tháng sắp hết hạn",
                    Message = $"{ticket.VehiclePlate} còn {ticket.DaysRemaining:N0} ngày, hết hạn {ticket.EndDate:dd/MM/yyyy}.",
                    Url = "/Customer/MonthlyTicket",
                    Icon = "fa-solid fa-id-card",
                    Type = "warning"
                });
            }

            var pendingMonthlyTicket = monthlyTickets?.Items
                .FirstOrDefault(ticket => IsPending(ticket.Status));
            if (pendingMonthlyTicket != null)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Vé tháng chờ thanh toán",
                    Message = $"{pendingMonthlyTicket.VehiclePlate} đang chờ hoàn tất thanh toán.",
                    Url = "/Customer/MonthlyTicket",
                    Icon = "fa-solid fa-credit-card",
                    Type = "warning"
                });
            }

            var upcomingReservations = reservations?.Items
                .Where(item => !IsCancelled(item.Status) && item.ExpectedTime >= DateTime.Now)
                .OrderBy(item => item.ExpectedTime)
                .Take(2)
                .ToList() ?? new List<CustomerReservationDto>();

            foreach (var reservation in upcomingReservations)
            {
                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Lịch đặt chỗ sắp tới",
                    Message = $"{reservation.VehiclePlate} lúc {reservation.ExpectedTime:HH:mm dd/MM}.",
                    Url = "/Customer/Booking",
                    Icon = "fa-solid fa-calendar-days",
                    Type = "info"
                });
            }

            if (activeTickets?.Items.Count > 0)
            {
                var ticket = activeTickets.Items
                    .OrderByDescending(item => item.CheckInTime)
                    .First();

                center.Items.Add(new NotificationItemViewModel
                {
                    Title = "Xe đang gửi trong bãi",
                    Message = $"{ticket.VehiclePlate} check-in lúc {ticket.CheckInTime:HH:mm dd/MM}.",
                    Url = "/Customer/Ticket",
                    Icon = "fa-solid fa-car-side",
                    Type = "info"
                });
            }
        }

        private static bool IsAdminRole(string role)
        {
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Manager", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsActive(string? status)
        {
            return ContainsAny(status, "Hoạt", "Active");
        }

        private static bool IsPending(string? status)
        {
            return ContainsAny(status, "Chờ", "Pending");
        }

        private static bool IsCancelled(string? status)
        {
            return ContainsAny(status, "Hủy", "Huỷ", "Cancel");
        }

        private static bool ContainsAny(string? value, params string[] terms)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
    }
}
