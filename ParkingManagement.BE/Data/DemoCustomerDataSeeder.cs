using ParkingManagement.DAL.Models;

namespace ParkingManagement.DAL.Data;

public static class DemoCustomerDataSeeder
{
    private const string CustomerName = "Bùi Thị Thu Thảo";
    private const string DemoEmail = "buithuthuthao.demo@example.com";
    private const string DemoPasswordHash = "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1."; // Huong4906@

    private const string Motorcycle = "Xe máy";
    private const string SmallCar = "Ô tô nhỏ";
    private const string LargeCar = "Ô tô lớn";
    private const string Empty = "Trống";
    private const string Occupied = "Đang sử dụng";
    private const string Reserved = "Đã đặt";
    private const string ActiveTicket = "Đang trong bãi";
    private const string CheckedOutTicket = "Đã ra";
    private const string ActiveMonthly = "Hoạt động";
    private const string ExpiredMonthly = "Hết hạn";
    private const string CancelledMonthly = "Đã hủy";
    private const string WaitingReservation = "Chờ";
    private const string ReceivedReservation = "Đã nhận";
    private const string CancelledReservation = "Hủy";
    private const string ExpiredReservation = "Hết hạn";
    private const string PaymentSuccess = "Thành công";

    public static void Seed(AppDbContext db)
    {
        var customer = db.Customers.FirstOrDefault(c => c.FullName == CustomerName)
            ?? CreateDemoCustomer(db);

        var customerId = customer.CustomerId;

        EnsureVehicle(db, "43A-123.54", SmallCar, customerId);
        EnsureVehicle(db, "43D3-456.78", Motorcycle, customerId);
        EnsureVehicle(db, "43C-987.65", LargeCar, customerId);

        EnsureMonthlyTicket(db, "THAO-MTK001", "THAO-PAY001", customerId, "43A-123.54", SmallCar,
            new DateTime(2026, 4, 16), new DateTime(2026, 7, 16), "3 tháng", 3200000, ActiveMonthly, "VNPay");
        EnsureMonthlyTicket(db, "THAO-MTK002", "THAO-PAY002", customerId, "43D3-456.78", Motorcycle,
            new DateTime(2026, 2, 1), new DateTime(2026, 3, 1), "1 tháng", 400000, ExpiredMonthly, "Chuyển khoản");
        EnsureMonthlyTicket(db, "THAO-MTK003", "THAO-PAY003", customerId, "43C-987.65", LargeCar,
            new DateTime(2026, 3, 10), new DateTime(2026, 6, 10), "3 tháng", 5500000, CancelledMonthly, "Tiền mặt");

        EnsureCompletedTicket(db, "THAO-TKT001", "THAO-PAY004", customerId, "43A-123.54", SmallCar, "B36",
            new DateTime(2026, 4, 10, 8, 15, 0), new DateTime(2026, 4, 10, 16, 40, 0), 55000, "Tiền mặt");
        EnsureCompletedTicket(db, "THAO-TKT002", "THAO-PAY005", customerId, "43D3-456.78", Motorcycle, "A36",
            new DateTime(2026, 5, 3, 7, 45, 0), new DateTime(2026, 5, 3, 18, 10, 0), 23000, "Chuyển khoản");
        EnsureCompletedTicket(db, "THAO-TKT003", "THAO-PAY006", customerId, "43C-987.65", LargeCar, "C14",
            new DateTime(2026, 5, 6, 9, 20, 0), new DateTime(2026, 5, 6, 15, 35, 0), 65000, "VNPay");
        EnsureCompletedTicket(db, "THAO-TKT004", "THAO-PAY007", customerId, "43A-123.54", SmallCar, "B37",
            new DateTime(2026, 5, 9, 10, 0, 0), new DateTime(2026, 5, 9, 12, 30, 0), 0, "Vé tháng");
        EnsureCompletedTicket(db, "THAO-TKT005", "THAO-PAY008", customerId, "43D3-456.78", Motorcycle, "A37",
            new DateTime(2026, 5, 12, 17, 20, 0), new DateTime(2026, 5, 12, 21, 5, 0), 13000, "Ví điện tử");
        EnsureCompletedTicket(db, "THAO-TKT006", "THAO-PAY009", customerId, "43C-987.65", LargeCar, "C15",
            new DateTime(2026, 5, 14, 8, 5, 0), new DateTime(2026, 5, 14, 18, 15, 0), 97000, "Chuyển khoản");
        EnsureActiveTicket(db, "THAO-TKT007", customerId, "43A-123.54", SmallCar, "B33",
            new DateTime(2026, 5, 16, 7, 35, 0));

        EnsureReservation(db, "THAO-RES001", customerId, "43A-123.54", "B34",
            new DateTime(2026, 5, 17, 17, 30, 0), new DateTime(2026, 5, 16, 8, 20, 0), WaitingReservation);
        EnsureReservation(db, "THAO-RES002", customerId, "43A-123.54", "B35",
            new DateTime(2026, 5, 15, 9, 0, 0), new DateTime(2026, 5, 14, 19, 10, 0), ReceivedReservation);
        EnsureReservation(db, "THAO-RES003", customerId, "43D3-456.78", "A38",
            new DateTime(2026, 5, 13, 18, 0, 0), new DateTime(2026, 5, 12, 20, 0, 0), CancelledReservation);
        EnsureReservation(db, "THAO-RES004", customerId, "43C-987.65", "C16",
            new DateTime(2026, 5, 11, 10, 0, 0), new DateTime(2026, 5, 10, 14, 30, 0), ExpiredReservation);

        db.SaveChanges();
    }

    private static Customer CreateDemoCustomer(AppDbContext db)
    {
        var account = db.Accounts.FirstOrDefault(a => a.Email == DemoEmail);
        var accountId = account?.AccountId ?? NextId(db.Accounts.Select(a => a.AccountId), "ACC", 125);
        var customerId = NextId(db.Customers.Select(c => c.CustomerId), "CUS", 25);

        if (account == null)
        {
            account = new Account
            {
                AccountId = accountId,
                PasswordHash = DemoPasswordHash,
                Role = "Customer",
                Email = DemoEmail,
                CreatedAt = new DateTime(2026, 5, 1, 9, 0, 0),
                IsActive = true,
                RequirePasswordChange = false
            };

            db.Accounts.Add(account);
        }

        var customer = new Customer
        {
            CustomerId = customerId,
            AccountId = accountId,
            FullName = CustomerName,
            PhoneNumber = "0918234567",
            Gender = "Female",
            IsDeleted = false
        };

        db.Customers.Add(customer);
        return customer;
    }

    private static void EnsureVehicle(AppDbContext db, string plate, string type, string customerId)
    {
        var vehicle = db.Vehicles.FirstOrDefault(v => v.VehiclePlate == plate);
        if (vehicle == null)
        {
            db.Vehicles.Add(new Vehicle
            {
                VehiclePlate = plate,
                VehicleType = type,
                CustomerId = customerId
            });
            return;
        }

        if (vehicle.CustomerId == null || vehicle.CustomerId == customerId)
        {
            vehicle.CustomerId = customerId;
            vehicle.VehicleType = type;
        }
    }

    private static void EnsureMonthlyTicket(
        AppDbContext db,
        string monthlyTicketId,
        string paymentId,
        string customerId,
        string plate,
        string type,
        DateTime startDate,
        DateTime endDate,
        string packageType,
        decimal totalFee,
        string status,
        string paymentMethod)
    {
        if (!db.MonthlyTickets.Any(t => t.MonthlyTicketId == monthlyTicketId))
        {
            db.MonthlyTickets.Add(new MonthlyTicket
            {
                MonthlyTicketId = monthlyTicketId,
                CustomerId = customerId,
                VehiclePlate = plate,
                VehicleType = type,
                StartDate = startDate,
                EndDate = endDate,
                PackageType = packageType,
                TotalFee = totalFee,
                Status = status,
                CreatedAt = startDate.AddHours(8)
            });
        }

        EnsurePayment(db, paymentId, null, monthlyTicketId, totalFee, paymentMethod, startDate.AddHours(8).AddMinutes(12));
    }

    private static void EnsureCompletedTicket(
        AppDbContext db,
        string ticketId,
        string paymentId,
        string customerId,
        string plate,
        string type,
        string slotId,
        DateTime checkIn,
        DateTime checkOut,
        decimal fee,
        string paymentMethod)
    {
        if (!db.Tickets.Any(t => t.TicketId == ticketId))
        {
            db.Tickets.Add(new Ticket
            {
                TicketId = ticketId,
                CustomerId = customerId,
                VehiclePlate = plate,
                VehicleType = type,
                SlotId = slotId,
                CheckInTime = checkIn,
                CheckOutTime = checkOut,
                Fee = fee,
                Status = CheckedOutTicket
            });
        }

        if (fee > 0)
        {
            EnsurePayment(db, paymentId, ticketId, null, fee, paymentMethod, checkOut);
        }
    }

    private static void EnsureActiveTicket(
        AppDbContext db,
        string ticketId,
        string customerId,
        string plate,
        string type,
        string slotId,
        DateTime checkIn)
    {
        if (!db.Tickets.Any(t => t.TicketId == ticketId))
        {
            db.Tickets.Add(new Ticket
            {
                TicketId = ticketId,
                CustomerId = customerId,
                VehiclePlate = plate,
                VehicleType = type,
                SlotId = slotId,
                CheckInTime = checkIn,
                Fee = 0,
                Status = ActiveTicket
            });
        }

        SetSlotStatusIfPossible(db, slotId, Occupied, checkIn);
    }

    private static void EnsureReservation(
        AppDbContext db,
        string reservationId,
        string customerId,
        string plate,
        string slotId,
        DateTime expectedTime,
        DateTime createdAt,
        string status)
    {
        if (!db.Reservations.Any(r => r.ReservationId == reservationId))
        {
            db.Reservations.Add(new Reservation
            {
                ReservationId = reservationId,
                CustomerId = customerId,
                VehiclePlate = plate,
                SlotId = slotId,
                ExpectedTime = expectedTime,
                CreatedAt = createdAt,
                Status = status
            });
        }

        if (status == WaitingReservation)
        {
            SetSlotStatusIfPossible(db, slotId, Reserved, createdAt);
        }
    }

    private static void EnsurePayment(
        AppDbContext db,
        string paymentId,
        string? ticketId,
        string? monthlyTicketId,
        decimal amount,
        string method,
        DateTime paymentTime)
    {
        if (db.Payments.Any(p => p.PaymentId == paymentId))
        {
            return;
        }

        db.Payments.Add(new Payment
        {
            PaymentId = paymentId,
            TicketId = ticketId,
            MonthlyTicketId = monthlyTicketId,
            Amount = amount,
            Method = method,
            PaymentTime = paymentTime,
            Status = PaymentSuccess
        });
    }

    private static void SetSlotStatusIfPossible(AppDbContext db, string slotId, string status, DateTime updatedAt)
    {
        var slot = db.ParkingSlots.FirstOrDefault(s => s.SlotId == slotId);
        if (slot == null || (slot.Status != Empty && slot.Status != status))
        {
            return;
        }

        slot.Status = status;
        slot.LastUpdated = updatedAt;
    }

    private static string NextId(IEnumerable<string> existingIds, string prefix, int start)
    {
        var usedIds = existingIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var i = start; i < 1000; i++)
        {
            var id = $"{prefix}{i:000}";
            if (!usedIds.Contains(id))
            {
                return id;
            }
        }

        throw new InvalidOperationException($"Không tìm được mã trống cho prefix {prefix}.");
    }
}
