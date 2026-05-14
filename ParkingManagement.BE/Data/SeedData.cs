using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.DAL.Data
{
    public static class ParkingManagementSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            const string managerPasswordHash = "$2a$12$kA4mFAV2vy8DBLtVX2pvMObG4nlikvEj9S4hGSLWE2JkignKN8uwS"; // Huong@4906
            const string employeePasswordHash = "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1."; // Huong4906@
            const string customerPasswordHash = "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1."; // Huong4906@

            const string Motorcycle = "Xe máy";
            const string SmallCar = "Ô tô nhỏ";
            const string LargeCar = "Ô tô lớn";
            const string Empty = "Trống";
            const string Occupied = "Đang sử dụng";
            const string Reserved = "Đã đặt";
            const string Maintenance = "Bảo trì";
            const string ActiveTicket = "Đang trong bãi";
            const string CheckedOutTicket = "Đã ra";
            const string ActiveMonthly = "Hoạt động";
            const string ExpiredMonthly = "Hết hạn";
            const string CancelledMonthly = "Đã hủy";
            const string WaitingReservation = "Chờ";
            const string ReceivedReservation = "Đã nhận";
            const string CancelledReservation = "Hủy";
            const string ExpiredReservation = "Hết hạn";
            const string PaymentSuccess = "Thành công";

            var baseTime = new DateTime(2026, 5, 13, 10, 0, 0);

            var accounts = new List<Account>
            {
                new() { AccountId = "ACC001", PasswordHash = managerPasswordHash, Role = "Manager", Email = "th04092006@gmail.com", CreatedAt = new DateTime(2026, 1, 1, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new() { AccountId = "ACC002", PasswordHash = employeePasswordHash, Role = "Employee", Email = "thanh76555765@gmail.com", CreatedAt = new DateTime(2026, 1, 5, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new() { AccountId = "ACC003", PasswordHash = employeePasswordHash, Role = "Employee", Email = "hung.levan@parking.local", CreatedAt = new DateTime(2026, 1, 8, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new() { AccountId = "ACC004", PasswordHash = employeePasswordHash, Role = "Employee", Email = "linh.tranmai@parking.local", CreatedAt = new DateTime(2026, 2, 2, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new() { AccountId = "ACC005", PasswordHash = employeePasswordHash, Role = "Employee", Email = "bao.doquoc@parking.local", CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new() { AccountId = "ACC006", PasswordHash = employeePasswordHash, Role = "Employee", Email = "nam.phanquoc@parking.local", CreatedAt = new DateTime(2026, 2, 20, 8, 0, 0), IsActive = false, RequirePasswordChange = false }
            };

            var customerProfiles = new[]
            {
                new { AccountId = "ACC101", CustomerId = "CUS001", Email = "minhanh.nguyen@example.com", CreatedAt = new DateTime(2026, 1, 12, 8, 15, 0), FullName = "Nguyễn Minh Anh", PhoneNumber = "0905123456", Gender = "Female" },
                new { AccountId = "ACC102", CustomerId = "CUS002", Email = "quocbao.tran@example.com", CreatedAt = new DateTime(2026, 1, 14, 9, 20, 0), FullName = "Trần Quốc Bảo", PhoneNumber = "0916234578", Gender = "Male" },
                new { AccountId = "ACC103", CustomerId = "CUS003", Email = "hoangnam.le@example.com", CreatedAt = new DateTime(2026, 1, 20, 10, 10, 0), FullName = "Lê Hoàng Nam", PhoneNumber = "0935129087", Gender = "Male" },
                new { AccountId = "ACC104", CustomerId = "CUS004", Email = "thuha.pham@example.com", CreatedAt = new DateTime(2026, 1, 25, 15, 0, 0), FullName = "Phạm Thu Hà", PhoneNumber = "0974306125", Gender = "Female" },
                new { AccountId = "ACC105", CustomerId = "CUS005", Email = "thanhtung.vo@example.com", CreatedAt = new DateTime(2026, 2, 1, 8, 45, 0), FullName = "Võ Thanh Tùng", PhoneNumber = "0982187345", Gender = "Male" },
                new { AccountId = "ACC106", CustomerId = "CUS006", Email = "ngocmai.dang@example.com", CreatedAt = new DateTime(2026, 2, 4, 11, 25, 0), FullName = "Đặng Ngọc Mai", PhoneNumber = "0946025178", Gender = "Female" },
                new { AccountId = "ACC107", CustomerId = "CUS007", Email = "giahuy.hoang@example.com", CreatedAt = new DateTime(2026, 2, 7, 14, 30, 0), FullName = "Hoàng Gia Huy", PhoneNumber = "0967852143", Gender = "Male" },
                new { AccountId = "ACC108", CustomerId = "CUS008", Email = "khanhlinh.bui@example.com", CreatedAt = new DateTime(2026, 2, 10, 9, 5, 0), FullName = "Bùi Khánh Linh", PhoneNumber = "0926017845", Gender = "Female" },
                new { AccountId = "ACC109", CustomerId = "CUS009", Email = "duclong.nguyen@example.com", CreatedAt = new DateTime(2026, 2, 15, 16, 15, 0), FullName = "Nguyễn Đức Long", PhoneNumber = "0907485126", Gender = "Male" },
                new { AccountId = "ACC110", CustomerId = "CUS010", Email = "myduyen.truong@example.com", CreatedAt = new DateTime(2026, 2, 18, 8, 35, 0), FullName = "Trương Mỹ Duyên", PhoneNumber = "0938741206", Gender = "Female" },
                new { AccountId = "ACC111", CustomerId = "CUS011", Email = "anhkhoa.phan@example.com", CreatedAt = new DateTime(2026, 2, 21, 13, 10, 0), FullName = "Phan Anh Khoa", PhoneNumber = "0919082746", Gender = "Male" },
                new { AccountId = "ACC112", CustomerId = "CUS012", Email = "quynhchi.lam@example.com", CreatedAt = new DateTime(2026, 2, 24, 10, 40, 0), FullName = "Lâm Quỳnh Chi", PhoneNumber = "0976012498", Gender = "Female" },
                new { AccountId = "ACC113", CustomerId = "CUS013", Email = "nhatminh.do@example.com", CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0), FullName = "Đỗ Nhật Minh", PhoneNumber = "0948127603", Gender = "Male" },
                new { AccountId = "ACC114", CustomerId = "CUS014", Email = "thaovy.huynh@example.com", CreatedAt = new DateTime(2026, 3, 4, 14, 20, 0), FullName = "Huỳnh Thảo Vy", PhoneNumber = "0965425006", Gender = "Female" },
                new { AccountId = "ACC115", CustomerId = "CUS015", Email = "congthanh.vu@example.com", CreatedAt = new DateTime(2026, 3, 8, 9, 15, 0), FullName = "Vũ Công Thành", PhoneNumber = "0906712485", Gender = "Male" },
                new { AccountId = "ACC116", CustomerId = "CUS016", Email = "hongphuc.mai@example.com", CreatedAt = new DateTime(2026, 3, 12, 17, 10, 0), FullName = "Mai Hồng Phúc", PhoneNumber = "0928174506", Gender = "Female" },
                new { AccountId = "ACC117", CustomerId = "CUS017", Email = "minhduc.cao@example.com", CreatedAt = new DateTime(2026, 3, 16, 8, 30, 0), FullName = "Cao Minh Đức", PhoneNumber = "0986401725", Gender = "Male" },
                new { AccountId = "ACC118", CustomerId = "CUS018", Email = "phuongnhi.nguyen@example.com", CreatedAt = new DateTime(2026, 3, 20, 12, 50, 0), FullName = "Nguyễn Phương Nhi", PhoneNumber = "0937084512", Gender = "Female" },
                new { AccountId = "ACC119", CustomerId = "CUS019", Email = "giabao.dinh@example.com", CreatedAt = new DateTime(2026, 3, 24, 9, 45, 0), FullName = "Đinh Gia Bảo", PhoneNumber = "0975306184", Gender = "Male" },
                new { AccountId = "ACC120", CustomerId = "CUS020", Email = "tuankiet.ha@example.com", CreatedAt = new DateTime(2026, 3, 28, 15, 30, 0), FullName = "Hà Tuấn Kiệt", PhoneNumber = "0962748031", Gender = "Male" },
                new { AccountId = "ACC121", CustomerId = "CUS021", Email = "baongoc.ly@example.com", CreatedAt = new DateTime(2026, 4, 2, 8, 25, 0), FullName = "Lý Bảo Ngọc", PhoneNumber = "0914057826", Gender = "Female" },
                new { AccountId = "ACC122", CustomerId = "CUS022", Email = "minhchau.ta@example.com", CreatedAt = new DateTime(2026, 4, 5, 11, 5, 0), FullName = "Tạ Minh Châu", PhoneNumber = "0947081625", Gender = "Female" },
                new { AccountId = "ACC123", CustomerId = "CUS023", Email = "vietanh.ho@example.com", CreatedAt = new DateTime(2026, 4, 9, 13, 40, 0), FullName = "Hồ Việt Anh", PhoneNumber = "0902476813", Gender = "Male" },
                new { AccountId = "ACC124", CustomerId = "CUS024", Email = "quanghung.nguyen@example.com", CreatedAt = new DateTime(2026, 4, 13, 16, 0, 0), FullName = "Nguyễn Quang Hưng", PhoneNumber = "0981276405", Gender = "Male" }
            };

            accounts.AddRange(customerProfiles.Select(c => new Account
            {
                AccountId = c.AccountId,
                PasswordHash = customerPasswordHash,
                Role = "Customer",
                Email = c.Email,
                CreatedAt = c.CreatedAt,
                IsActive = true,
                RequirePasswordChange = false
            }));

            var customers = customerProfiles.Select(c => new Customer
            {
                CustomerId = c.CustomerId,
                AccountId = c.AccountId,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Gender = c.Gender,
                IsDeleted = false
            }).ToList();

            var vehicles = new List<Vehicle>
            {
                new() { VehiclePlate = "43D1-256.31", VehicleType = Motorcycle, CustomerId = "CUS001" },
                new() { VehiclePlate = "43A-918.42", VehicleType = SmallCar, CustomerId = "CUS001" },
                new() { VehiclePlate = "43D1-344.88", VehicleType = Motorcycle, CustomerId = "CUS002" },
                new() { VehiclePlate = "43A-657.20", VehicleType = SmallCar, CustomerId = "CUS003" },
                new() { VehiclePlate = "43D1-490.12", VehicleType = Motorcycle, CustomerId = "CUS004" },
                new() { VehiclePlate = "43C-112.67", VehicleType = LargeCar, CustomerId = "CUS005" },
                new() { VehiclePlate = "43D1-628.09", VehicleType = Motorcycle, CustomerId = "CUS006" },
                new() { VehiclePlate = "43A-735.18", VehicleType = SmallCar, CustomerId = "CUS007" },
                new() { VehiclePlate = "43D1-812.43", VehicleType = Motorcycle, CustomerId = "CUS008" },
                new() { VehiclePlate = "43D1-921.54", VehicleType = Motorcycle, CustomerId = "CUS009" },
                new() { VehiclePlate = "43A-246.80", VehicleType = SmallCar, CustomerId = "CUS010" },
                new() { VehiclePlate = "43D2-105.77", VehicleType = Motorcycle, CustomerId = "CUS011" },
                new() { VehiclePlate = "43A-332.16", VehicleType = SmallCar, CustomerId = "CUS011" },
                new() { VehiclePlate = "43D2-218.90", VehicleType = Motorcycle, CustomerId = "CUS012" },
                new() { VehiclePlate = "43C-245.19", VehicleType = LargeCar, CustomerId = "CUS013" },
                new() { VehiclePlate = "43D2-387.66", VehicleType = Motorcycle, CustomerId = "CUS014" },
                new() { VehiclePlate = "43A-509.34", VehicleType = SmallCar, CustomerId = "CUS015" },
                new() { VehiclePlate = "43D2-474.21", VehicleType = Motorcycle, CustomerId = "CUS016" },
                new() { VehiclePlate = "43D2-588.64", VehicleType = Motorcycle, CustomerId = "CUS017" },
                new() { VehiclePlate = "43A-694.15", VehicleType = SmallCar, CustomerId = "CUS018" },
                new() { VehiclePlate = "43D2-730.08", VehicleType = Motorcycle, CustomerId = "CUS019" },
                new() { VehiclePlate = "43C-318.72", VehicleType = LargeCar, CustomerId = "CUS020" },
                new() { VehiclePlate = "43D3-044.39", VehicleType = Motorcycle, CustomerId = "CUS021" },
                new() { VehiclePlate = "43D3-115.84", VehicleType = Motorcycle, CustomerId = "CUS022" },
                new() { VehiclePlate = "43A-807.51", VehicleType = SmallCar, CustomerId = "CUS023" },
                new() { VehiclePlate = "43D3-236.97", VehicleType = Motorcycle, CustomerId = "CUS024" },
                new() { VehiclePlate = "92D1-222.11", VehicleType = Motorcycle, CustomerId = null },
                new() { VehiclePlate = "92A-518.26", VehicleType = SmallCar, CustomerId = null },
                new() { VehiclePlate = "74D1-704.33", VehicleType = Motorcycle, CustomerId = null },
                new() { VehiclePlate = "75A-663.40", VehicleType = SmallCar, CustomerId = null },
                new() { VehiclePlate = "76C-219.05", VehicleType = LargeCar, CustomerId = null },
                new() { VehiclePlate = "92D1-445.18", VehicleType = Motorcycle, CustomerId = null },
                new() { VehiclePlate = "77A-904.52", VehicleType = SmallCar, CustomerId = null }
            };

            var managers = new List<Manager>
            {
                new() { ManagerId = "MGR001", AccountId = "ACC001", FullName = "Nguyễn Thị Hường", PhoneNumber = "0901234567", Gender = "Female", IsDeleted = false }
            };

            var employees = new List<Employee>
            {
                new() { EmployeeId = "EMP001", EmployeeCode = "EMP001", AccountId = "ACC002", FullName = "Nguyễn Thanh", PhoneNumber = "0912345678", Gender = "Male", Shift = "Sáng", ManagerId = "MGR001", IsDeleted = false },
                new() { EmployeeId = "EMP002", EmployeeCode = "EMP002", AccountId = "ACC003", FullName = "Lê Văn Hùng", PhoneNumber = "0923456789", Gender = "Male", Shift = "Chiều", ManagerId = "MGR001", IsDeleted = false },
                new() { EmployeeId = "EMP003", EmployeeCode = "EMP003", AccountId = "ACC004", FullName = "Trần Mai Linh", PhoneNumber = "0934567890", Gender = "Female", Shift = "Tối", ManagerId = "MGR001", IsDeleted = false },
                new() { EmployeeId = "EMP004", EmployeeCode = "EMP004", AccountId = "ACC005", FullName = "Đỗ Quốc Bảo", PhoneNumber = "0977000111", Gender = "Male", Shift = "Sáng", ManagerId = "MGR001", IsDeleted = false },
                new() { EmployeeId = "EMP005", EmployeeCode = "EMP005", AccountId = "ACC006", FullName = "Phan Quốc Nam", PhoneNumber = "0987654321", Gender = "Male", Shift = null, ManagerId = "MGR001", IsDeleted = true }
            };

            var employeeInvites = new List<EmployeeInvite>
            {
                new() { InviteToken = "INVITE-EMP006-2026", EmployeeCode = "EMP006", Email = "an.ngominh@parking.local", FullName = "Ngô Minh An", PhoneNumber = "0977000222", Shift = "Chiều", CreatedAt = new DateTime(2026, 5, 8, 8, 0, 0), ExpiryTime = new DateTime(2030, 12, 31, 23, 59, 59), IsUsed = false },
                new() { InviteToken = "INVITE-USED-EMP007", EmployeeCode = "EMP007", Email = "binh.dothanh@parking.local", FullName = "Đỗ Thanh Bình", PhoneNumber = "0977000333", Shift = "Tối", CreatedAt = new DateTime(2026, 4, 20, 8, 0, 0), ExpiryTime = new DateTime(2026, 4, 21, 8, 0, 0), IsUsed = true }
            };

            var slots = BuildParkingSlots(Motorcycle, SmallCar, LargeCar, Empty, new DateTime(2026, 5, 1, 6, 0, 0));

            var pricingConfigurations = new List<PricingConfiguration>
            {
                // Xe máy: giờ đầu 5k, từ giờ 2: 2k/h, qua đêm 10k. Vé tháng 400k/1.1tr
                new() { PricingId = "PRICE-XM-FIRST", VehicleType = Motorcycle, RateType = "FirstHour", Amount = 5000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-XM-NEXT", VehicleType = Motorcycle, RateType = "PerHourAfter", Amount = 2000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-XM-NIGHT", VehicleType = Motorcycle, RateType = "Overnight", Amount = 10000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-XM-M1", VehicleType = Motorcycle, RateType = "Monthly1M", Amount = 400000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-XM-M3", VehicleType = Motorcycle, RateType = "Monthly3M", Amount = 1100000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                // Ô tô nhỏ: giờ đầu 15k, từ giờ 2: 5k/h, qua đêm 40k. Vé tháng 1.2tr/3.2tr
                new() { PricingId = "PRICE-OTON-FIRST", VehicleType = SmallCar, RateType = "FirstHour", Amount = 15000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTON-NEXT", VehicleType = SmallCar, RateType = "PerHourAfter", Amount = 5000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTON-NIGHT", VehicleType = SmallCar, RateType = "Overnight", Amount = 40000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTON-M1", VehicleType = SmallCar, RateType = "Monthly1M", Amount = 1200000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTON-M3", VehicleType = SmallCar, RateType = "Monthly3M", Amount = 3200000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                // Ô tô lớn: giờ đầu 25k, từ giờ 2: 8k/h, qua đêm 60k. Vé tháng 2tr/5.5tr
                new() { PricingId = "PRICE-OTOL-FIRST", VehicleType = LargeCar, RateType = "FirstHour", Amount = 25000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTOL-NEXT", VehicleType = LargeCar, RateType = "PerHourAfter", Amount = 8000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTOL-NIGHT", VehicleType = LargeCar, RateType = "Overnight", Amount = 60000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTOL-M1", VehicleType = LargeCar, RateType = "Monthly1M", Amount = 2000000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new() { PricingId = "PRICE-OTOL-M3", VehicleType = LargeCar, RateType = "Monthly3M", Amount = 5500000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" }
            };

            var monthlyTickets = new List<MonthlyTicket>();
            var tickets = new List<Ticket>();
            var payments = new List<Payment>();
            var reservations = new List<Reservation>();
            var auditLogs = new List<ParkingSlotAuditLog>();
            var paymentCounter = 1;
            var ticketCounter = 1;

            void AddPayment(string? ticketId, string? monthlyTicketId, decimal amount, string method, DateTime paymentTime)
            {
                payments.Add(new Payment
                {
                    PaymentId = $"PAY{paymentCounter:0000}",
                    TicketId = ticketId,
                    MonthlyTicketId = monthlyTicketId,
                    Amount = amount,
                    Method = method,
                    PaymentTime = paymentTime,
                    Status = PaymentSuccess
                });
                paymentCounter++;
            }

            void AddMonthlyTicket(
                string id,
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
                monthlyTickets.Add(new MonthlyTicket
                {
                    MonthlyTicketId = id,
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

                AddPayment(null, id, totalFee, paymentMethod, startDate.AddHours(8).AddMinutes(10));
            }

            AddMonthlyTicket("MTK001", "CUS001", "43D1-256.31", Motorcycle, new DateTime(2026, 4, 20), new DateTime(2026, 5, 20), "1 tháng", 400000, ActiveMonthly, "Chuyển khoản");
            AddMonthlyTicket("MTK002", "CUS002", "43D1-344.88", Motorcycle, new DateTime(2026, 3, 15), new DateTime(2026, 6, 15), "3 tháng", 1100000, ActiveMonthly, "Ví điện tử");
            AddMonthlyTicket("MTK003", "CUS003", "43A-657.20", SmallCar, new DateTime(2026, 5, 1), new DateTime(2026, 8, 1), "3 tháng", 3200000, ActiveMonthly, "Chuyển khoản");
            AddMonthlyTicket("MTK004", "CUS005", "43C-112.67", LargeCar, new DateTime(2026, 1, 20), new DateTime(2026, 4, 20), "3 tháng", 5500000, ExpiredMonthly, "Chuyển khoản");
            AddMonthlyTicket("MTK005", "CUS006", "43D1-628.09", Motorcycle, new DateTime(2026, 3, 1), new DateTime(2026, 4, 1), "1 tháng", 400000, ExpiredMonthly, "Tiền mặt");
            AddMonthlyTicket("MTK006", "CUS007", "43A-735.18", SmallCar, new DateTime(2026, 2, 10), new DateTime(2026, 5, 10), "3 tháng", 3200000, ActiveMonthly, "Chuyển khoản");
            AddMonthlyTicket("MTK007", "CUS010", "43A-246.80", SmallCar, new DateTime(2026, 4, 1), new DateTime(2026, 5, 1), "1 tháng", 1200000, CancelledMonthly, "Tiền mặt");
            AddMonthlyTicket("MTK008", "CUS011", "43A-332.16", SmallCar, new DateTime(2026, 4, 15), new DateTime(2026, 5, 15), "1 tháng", 1200000, ActiveMonthly, "Ví điện tử");
            AddMonthlyTicket("MTK009", "CUS013", "43C-245.19", LargeCar, new DateTime(2026, 1, 5), new DateTime(2026, 4, 5), "3 tháng", 5500000, ExpiredMonthly, "Chuyển khoản");
            AddMonthlyTicket("MTK010", "CUS018", "43A-694.15", SmallCar, new DateTime(2026, 4, 25), new DateTime(2026, 7, 25), "3 tháng", 3200000, ActiveMonthly, "Chuyển khoản");
            AddMonthlyTicket("MTK011", "CUS020", "43C-318.72", LargeCar, new DateTime(2026, 5, 5), new DateTime(2026, 6, 5), "1 tháng", 2000000, ActiveMonthly, "Ví điện tử");
            AddMonthlyTicket("MTK012", "CUS021", "43D3-044.39", Motorcycle, new DateTime(2026, 5, 10), new DateTime(2026, 6, 10), "1 tháng", 400000, ActiveMonthly, "Tiền mặt");

            void AddCompletedTicket(string? customerId, string plate, string type, string slotId, DateTime checkIn, DateTime checkOut, string paymentMethod)
            {
                var ticketId = $"TKT{ticketCounter:0000}";
                var fee = HasMonthlyAccess(monthlyTickets, customerId, plate, checkIn) ? 0 : CalculateParkingFee(type, checkIn, checkOut, Motorcycle, SmallCar, LargeCar);

                tickets.Add(new Ticket
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

                if (fee > 0)
                {
                    AddPayment(ticketId, null, fee, paymentMethod, checkOut);
                }

                ticketCounter++;
            }

            void AddActiveParkingTicket(string? customerId, string plate, string type, string slotId, DateTime checkIn)
            {
                var ticketId = $"TKT{ticketCounter:0000}";
                tickets.Add(new Ticket
                {
                    TicketId = ticketId,
                    CustomerId = customerId,
                    VehiclePlate = plate,
                    VehicleType = type,
                    SlotId = slotId,
                    CheckInTime = checkIn,
                    CheckOutTime = null,
                    Fee = 0,
                    Status = ActiveTicket
                });
                ticketCounter++;

                SetSlotStatus(slots, slotId, Occupied, checkIn, $"Check-in {ticketId}");
                AddAudit(auditLogs, $"LOG{auditLogs.Count + 1:000}", slotId, "EMP001", Empty, Occupied, checkIn, $"Check-in {ticketId}", null);
            }

            AddCompletedTicket("CUS001", "43D1-256.31", Motorcycle, "A09", new DateTime(2026, 4, 22, 7, 35, 0), new DateTime(2026, 4, 22, 17, 20, 0), "Chuyển khoản");
            AddCompletedTicket("CUS001", "43A-918.42", SmallCar, "B07", new DateTime(2026, 4, 24, 18, 5, 0), new DateTime(2026, 4, 24, 20, 10, 0), "Ví điện tử");
            AddCompletedTicket("CUS001", "43D1-256.31", Motorcycle, "A10", new DateTime(2026, 5, 6, 7, 50, 0), new DateTime(2026, 5, 6, 17, 15, 0), "Chuyển khoản");
            AddCompletedTicket("CUS002", "43D1-344.88", Motorcycle, "A11", new DateTime(2026, 4, 2, 8, 0, 0), new DateTime(2026, 4, 2, 17, 0, 0), "Ví điện tử");
            AddCompletedTicket("CUS002", "43D1-344.88", Motorcycle, "A12", new DateTime(2026, 5, 10, 8, 10, 0), new DateTime(2026, 5, 10, 16, 45, 0), "Ví điện tử");
            AddCompletedTicket("CUS003", "43A-657.20", SmallCar, "B08", new DateTime(2026, 5, 2, 8, 20, 0), new DateTime(2026, 5, 2, 18, 0, 0), "Chuyển khoản");
            AddCompletedTicket("CUS003", "43A-657.20", SmallCar, "B09", new DateTime(2026, 4, 25, 9, 5, 0), new DateTime(2026, 4, 25, 15, 40, 0), "Tiền mặt");
            AddCompletedTicket("CUS004", "43D1-490.12", Motorcycle, "A13", new DateTime(2026, 4, 12, 9, 0, 0), new DateTime(2026, 4, 12, 11, 0, 0), "Tiền mặt");
            AddCompletedTicket("CUS004", "43D1-490.12", Motorcycle, "A14", new DateTime(2026, 5, 8, 13, 15, 0), new DateTime(2026, 5, 8, 17, 20, 0), "Ví điện tử");
            AddCompletedTicket("CUS005", "43C-112.67", LargeCar, "C04", new DateTime(2026, 4, 3, 7, 45, 0), new DateTime(2026, 4, 3, 18, 30, 0), "Chuyển khoản");
            AddCompletedTicket("CUS005", "43C-112.67", LargeCar, "C05", new DateTime(2026, 5, 9, 8, 15, 0), new DateTime(2026, 5, 9, 19, 10, 0), "Chuyển khoản");
            AddCompletedTicket("CUS006", "43D1-628.09", Motorcycle, "A15", new DateTime(2026, 3, 20, 8, 5, 0), new DateTime(2026, 3, 20, 17, 5, 0), "Tiền mặt");
            AddCompletedTicket("CUS006", "43D1-628.09", Motorcycle, "A15", new DateTime(2026, 4, 28, 8, 15, 0), new DateTime(2026, 4, 28, 16, 30, 0), "Tiền mặt");
            AddCompletedTicket("CUS007", "43A-735.18", SmallCar, "B10", new DateTime(2026, 4, 16, 9, 0, 0), new DateTime(2026, 4, 16, 18, 30, 0), "Chuyển khoản");
            AddCompletedTicket("CUS007", "43A-735.18", SmallCar, "B10", new DateTime(2026, 5, 11, 8, 30, 0), new DateTime(2026, 5, 11, 17, 20, 0), "Chuyển khoản");
            AddCompletedTicket("CUS008", "43D1-812.43", Motorcycle, "A16", new DateTime(2026, 4, 18, 9, 35, 0), new DateTime(2026, 4, 18, 13, 30, 0), "Ví điện tử");
            AddCompletedTicket("CUS009", "43D1-921.54", Motorcycle, "A17", new DateTime(2026, 4, 27, 7, 40, 0), new DateTime(2026, 4, 27, 12, 15, 0), "Tiền mặt");
            AddCompletedTicket("CUS010", "43A-246.80", SmallCar, "B11", new DateTime(2026, 4, 4, 10, 0, 0), new DateTime(2026, 4, 4, 14, 30, 0), "Tiền mặt");
            AddCompletedTicket("CUS010", "43A-246.80", SmallCar, "B11", new DateTime(2026, 5, 4, 8, 20, 0), new DateTime(2026, 5, 4, 13, 50, 0), "Chuyển khoản");
            AddCompletedTicket("CUS011", "43D2-105.77", Motorcycle, "A18", new DateTime(2026, 4, 13, 12, 5, 0), new DateTime(2026, 4, 13, 14, 45, 0), "Tiền mặt");
            AddCompletedTicket("CUS011", "43A-332.16", SmallCar, "B12", new DateTime(2026, 4, 20, 7, 50, 0), new DateTime(2026, 4, 20, 17, 25, 0), "Ví điện tử");
            AddCompletedTicket("CUS012", "43D2-218.90", Motorcycle, "A19", new DateTime(2026, 5, 1, 8, 30, 0), new DateTime(2026, 5, 1, 14, 5, 0), "Tiền mặt");
            AddCompletedTicket("CUS013", "43C-245.19", LargeCar, "C06", new DateTime(2026, 3, 12, 9, 10, 0), new DateTime(2026, 3, 12, 17, 30, 0), "Chuyển khoản");
            AddCompletedTicket("CUS013", "43C-245.19", LargeCar, "C06", new DateTime(2026, 4, 20, 8, 15, 0), new DateTime(2026, 4, 20, 15, 5, 0), "Chuyển khoản");
            AddCompletedTicket("CUS014", "43D2-387.66", Motorcycle, "A20", new DateTime(2026, 4, 29, 14, 0, 0), new DateTime(2026, 4, 29, 16, 10, 0), "Ví điện tử");
            AddCompletedTicket("CUS015", "43A-509.34", SmallCar, "B13", new DateTime(2026, 5, 3, 9, 0, 0), new DateTime(2026, 5, 3, 14, 20, 0), "Tiền mặt");
            AddCompletedTicket("CUS016", "43D2-474.21", Motorcycle, "A21", new DateTime(2026, 5, 7, 10, 30, 0), new DateTime(2026, 5, 7, 14, 0, 0), "Tiền mặt");
            AddCompletedTicket("CUS017", "43D2-588.64", Motorcycle, "A22", new DateTime(2026, 5, 10, 15, 10, 0), new DateTime(2026, 5, 10, 16, 0, 0), "Ví điện tử");
            AddCompletedTicket("CUS018", "43A-694.15", SmallCar, "B14", new DateTime(2026, 4, 28, 8, 45, 0), new DateTime(2026, 4, 28, 17, 45, 0), "Chuyển khoản");
            AddCompletedTicket("CUS019", "43D2-730.08", Motorcycle, "A23", new DateTime(2026, 5, 2, 8, 10, 0), new DateTime(2026, 5, 2, 14, 40, 0), "Tiền mặt");
            AddCompletedTicket("CUS020", "43C-318.72", LargeCar, "C07", new DateTime(2026, 5, 7, 8, 0, 0), new DateTime(2026, 5, 7, 18, 10, 0), "Ví điện tử");
            AddCompletedTicket("CUS021", "43D3-044.39", Motorcycle, "A24", new DateTime(2026, 5, 11, 9, 0, 0), new DateTime(2026, 5, 11, 17, 0, 0), "Tiền mặt");
            AddCompletedTicket("CUS022", "43D3-115.84", Motorcycle, "A25", new DateTime(2026, 5, 5, 9, 40, 0), new DateTime(2026, 5, 5, 12, 5, 0), "Ví điện tử");
            AddCompletedTicket("CUS023", "43A-807.51", SmallCar, "B15", new DateTime(2026, 5, 6, 10, 0, 0), new DateTime(2026, 5, 6, 14, 55, 0), "Tiền mặt");
            AddCompletedTicket("CUS024", "43D3-236.97", Motorcycle, "A26", new DateTime(2026, 5, 12, 8, 15, 0), new DateTime(2026, 5, 12, 12, 45, 0), "Tiền mặt");
            AddCompletedTicket(null, "92D1-222.11", Motorcycle, "A27", new DateTime(2026, 5, 1, 10, 0, 0), new DateTime(2026, 5, 1, 11, 20, 0), "Tiền mặt");
            AddCompletedTicket(null, "92A-518.26", SmallCar, "B16", new DateTime(2026, 5, 2, 13, 20, 0), new DateTime(2026, 5, 2, 16, 10, 0), "Chuyển khoản");
            AddCompletedTicket(null, "74D1-704.33", Motorcycle, "A28", new DateTime(2026, 5, 3, 18, 0, 0), new DateTime(2026, 5, 3, 20, 30, 0), "Tiền mặt");
            AddCompletedTicket(null, "75A-663.40", SmallCar, "B17", new DateTime(2026, 5, 4, 7, 50, 0), new DateTime(2026, 5, 4, 12, 0, 0), "Ví điện tử");
            AddCompletedTicket(null, "76C-219.05", LargeCar, "C08", new DateTime(2026, 5, 5, 9, 10, 0), new DateTime(2026, 5, 5, 13, 45, 0), "Chuyển khoản");
            AddCompletedTicket(null, "92D1-445.18", Motorcycle, "A29", new DateTime(2026, 5, 6, 8, 30, 0), new DateTime(2026, 5, 6, 10, 0, 0), "Tiền mặt");
            AddCompletedTicket(null, "77A-904.52", SmallCar, "B18", new DateTime(2026, 5, 7, 16, 40, 0), new DateTime(2026, 5, 7, 21, 0, 0), "Chuyển khoản");

            AddActiveParkingTicket("CUS001", "43D1-256.31", Motorcycle, "A01", new DateTime(2026, 5, 13, 7, 35, 0));
            AddActiveParkingTicket("CUS003", "43A-657.20", SmallCar, "B01", new DateTime(2026, 5, 13, 8, 10, 0));
            AddActiveParkingTicket("CUS004", "43D1-490.12", Motorcycle, "A02", new DateTime(2026, 5, 13, 8, 25, 0));
            AddActiveParkingTicket("CUS005", "43C-112.67", LargeCar, "C01", new DateTime(2026, 5, 13, 7, 50, 0));
            AddActiveParkingTicket("CUS008", "43D1-812.43", Motorcycle, "A03", new DateTime(2026, 5, 13, 9, 5, 0));
            AddActiveParkingTicket("CUS011", "43A-332.16", SmallCar, "B02", new DateTime(2026, 5, 13, 9, 20, 0));
            AddActiveParkingTicket("CUS015", "43A-509.34", SmallCar, "B03", new DateTime(2026, 5, 13, 9, 45, 0));
            AddActiveParkingTicket("CUS019", "43D2-730.08", Motorcycle, "A04", new DateTime(2026, 5, 13, 8, 55, 0));
            AddActiveParkingTicket(null, "92D1-222.11", Motorcycle, "A05", new DateTime(2026, 5, 13, 9, 15, 0));
            AddActiveParkingTicket(null, "92A-518.26", SmallCar, "B04", new DateTime(2026, 5, 13, 8, 40, 0));
            AddActiveParkingTicket(null, "76C-219.05", LargeCar, "C02", new DateTime(2026, 5, 13, 9, 30, 0));

            void AddReservation(string id, string customerId, string plate, string slotId, DateTime expectedTime, DateTime createdAt, string status)
            {
                reservations.Add(new Reservation
                {
                    ReservationId = id,
                    CustomerId = customerId,
                    VehiclePlate = plate,
                    SlotId = slotId,
                    ExpectedTime = expectedTime,
                    CreatedAt = createdAt,
                    Status = status
                });

                if (status == WaitingReservation)
                {
                    SetSlotStatus(slots, slotId, Reserved, createdAt, $"Reservation {id}");
                    AddAudit(auditLogs, $"LOG{auditLogs.Count + 1:000}", slotId, "EMP002", Empty, Reserved, createdAt, $"Giữ chỗ cho {id}", null);
                }
            }

            AddReservation("RES001", "CUS006", "43D1-628.09", "A06", new DateTime(2026, 5, 20, 18, 15, 0), new DateTime(2026, 5, 14, 9, 0, 0), WaitingReservation);
            AddReservation("RES002", "CUS010", "43A-246.80", "B05", new DateTime(2026, 5, 21, 8, 0, 0), new DateTime(2026, 5, 14, 9, 10, 0), WaitingReservation);
            AddReservation("RES003", "CUS020", "43C-318.72", "C03", new DateTime(2026, 5, 22, 10, 0, 0), new DateTime(2026, 5, 14, 9, 20, 0), WaitingReservation);
            AddReservation("RES004", "CUS012", "43D2-218.90", "A07", new DateTime(2026, 5, 11, 9, 0, 0), new DateTime(2026, 5, 10, 17, 30, 0), ExpiredReservation);
            AddReservation("RES005", "CUS014", "43D2-387.66", "A08", new DateTime(2026, 5, 12, 16, 0, 0), new DateTime(2026, 5, 12, 8, 0, 0), CancelledReservation);
            AddReservation("RES006", "CUS001", "43A-918.42", "B07", new DateTime(2026, 4, 24, 18, 0, 0), new DateTime(2026, 4, 24, 12, 0, 0), ReceivedReservation);

            SetSlotStatus(slots, "A35", Maintenance, baseTime.AddDays(-2), "Bảo trì cảm biến khu A");
            SetSlotStatus(slots, "B20", Maintenance, baseTime.AddDays(-1), "Sơn lại vạch khu B");
            SetSlotStatus(slots, "C10", Maintenance, baseTime.AddDays(-3), "Kiểm tra camera khu C");
            AddAudit(auditLogs, $"LOG{auditLogs.Count + 1:000}", "A35", "EMP004", Empty, Maintenance, baseTime.AddDays(-2), "Bảo trì cảm biến khu A", "Thiết bị báo chiếm chỗ chập chờn");
            AddAudit(auditLogs, $"LOG{auditLogs.Count + 1:000}", "B20", "EMP004", Empty, Maintenance, baseTime.AddDays(-1), "Sơn lại vạch khu B", "Vạch đỗ bị mờ");
            AddAudit(auditLogs, $"LOG{auditLogs.Count + 1:000}", "C10", "EMP004", Empty, Maintenance, baseTime.AddDays(-3), "Kiểm tra camera khu C", "Camera góc khuất mất tín hiệu");

            var otps = new List<Otp>
            {
                new() { OtpId = "OTP001", Email = "dangky.moi@example.com", Code = "123456", CreatedAt = new DateTime(2026, 5, 13, 8, 0, 0), ExpiresAt = new DateTime(2030, 12, 31, 23, 59, 59), IsVerified = false, VerifiedAt = null }
            };

            ValidateSeedData(customers, vehicles, slots, monthlyTickets, tickets, payments, reservations);

            modelBuilder.Entity<Account>().HasData(accounts);
            modelBuilder.Entity<Customer>().HasData(customers);
            modelBuilder.Entity<Manager>().HasData(managers);
            modelBuilder.Entity<Employee>().HasData(employees);
            modelBuilder.Entity<EmployeeInvite>().HasData(employeeInvites);
            modelBuilder.Entity<Vehicle>().HasData(vehicles);
            modelBuilder.Entity<ParkingSlot>().HasData(slots);
            modelBuilder.Entity<PricingConfiguration>().HasData(pricingConfigurations);
            modelBuilder.Entity<MonthlyTicket>().HasData(monthlyTickets);
            modelBuilder.Entity<Ticket>().HasData(tickets);
            modelBuilder.Entity<Payment>().HasData(payments);
            modelBuilder.Entity<Reservation>().HasData(reservations);
            modelBuilder.Entity<ParkingSlotAuditLog>().HasData(auditLogs);
            modelBuilder.Entity<Otp>().HasData(otps);
        }

        private static List<ParkingSlot> BuildParkingSlots(string motorcycle, string smallCar, string largeCar, string empty, DateTime updatedAt)
        {
            var slots = new List<ParkingSlot>();

            for (var i = 1; i <= 50; i++)
            {
                slots.Add(new ParkingSlot { SlotId = $"A{i:00}", Location = $"Khu A - Ô {i:00}", VehicleType = motorcycle, Status = empty, LastUpdated = updatedAt });
            }

            for (var i = 1; i <= 50; i++)
            {
                slots.Add(new ParkingSlot { SlotId = $"B{i:00}", Location = $"Khu B - Ô {i:00}", VehicleType = smallCar, Status = empty, LastUpdated = updatedAt });
            }

            for (var i = 1; i <= 20; i++)
            {
                slots.Add(new ParkingSlot { SlotId = $"C{i:00}", Location = $"Khu C - Ô {i:00}", VehicleType = largeCar, Status = empty, LastUpdated = updatedAt });
            }

            return slots;
        }

        private static void SetSlotStatus(List<ParkingSlot> slots, string slotId, string status, DateTime updatedAt, string context)
        {
            var slot = slots.FirstOrDefault(s => s.SlotId == slotId)
                ?? throw new InvalidOperationException($"Không tìm thấy ô đỗ {slotId} khi seed data cho {context}.");

            slot.Status = status;
            slot.LastUpdated = updatedAt;
        }

        private static void AddAudit(
            List<ParkingSlotAuditLog> auditLogs,
            string logId,
            string slotId,
            string employeeId,
            string oldStatus,
            string newStatus,
            DateTime changedAt,
            string note,
            string? reason)
        {
            auditLogs.Add(new ParkingSlotAuditLog
            {
                LogId = logId,
                SlotId = slotId,
                EmployeeId = employeeId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = changedAt,
                Note = note,
                Reason = reason
            });
        }

        private static bool HasMonthlyAccess(List<MonthlyTicket> monthlyTickets, string? customerId, string plate, DateTime checkIn)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return false;
            }

            return monthlyTickets.Any(m =>
                m.CustomerId == customerId &&
                m.VehiclePlate == plate &&
                m.Status != "Đã hủy" &&
                m.StartDate.Date <= checkIn.Date &&
                m.EndDate.Date >= checkIn.Date);
        }

        private static decimal CalculateParkingFee(string vehicleType, DateTime checkIn, DateTime checkOut, string motorcycle, string smallCar, string largeCar)
        {
            var totalMinutes = (checkOut - checkIn).TotalMinutes;
            var hours = (int)Math.Ceiling(totalMinutes / 60.0);

            decimal firstHourFee, perHourFee;
            if (vehicleType == motorcycle) { firstHourFee = 5_000; perHourFee = 2_000; }
            else if (vehicleType == smallCar) { firstHourFee = 15_000; perHourFee = 5_000; }
            else if (vehicleType == largeCar) { firstHourFee = 25_000; perHourFee = 8_000; }
            else throw new InvalidOperationException($"Loại xe không hợp lệ: {vehicleType}");

            if (hours <= 1) return firstHourFee;
            return firstHourFee + ((hours - 1) * perHourFee);
        }

        private static void ValidateSeedData(
            List<Customer> customers,
            List<Vehicle> vehicles,
            List<ParkingSlot> slots,
            List<MonthlyTicket> monthlyTickets,
            List<Ticket> tickets,
            List<Payment> payments,
            List<Reservation> reservations)
        {
            EnsureUnique(vehicles.Select(v => v.VehiclePlate), "biển số xe");
            EnsureUnique(tickets.Select(t => t.TicketId), "mã vé lượt");
            EnsureUnique(monthlyTickets.Select(t => t.MonthlyTicketId), "mã vé tháng");
            EnsureUnique(payments.Select(p => p.PaymentId), "mã thanh toán");
            EnsureUnique(reservations.Select(r => r.ReservationId), "mã đặt chỗ");

            var customerIds = customers.Select(c => c.CustomerId).ToHashSet();
            var vehicleByPlate = vehicles.ToDictionary(v => v.VehiclePlate);
            var slotById = slots.ToDictionary(s => s.SlotId);

            foreach (var vehicle in vehicles.Where(v => v.CustomerId != null))
            {
                if (!customerIds.Contains(vehicle.CustomerId!))
                {
                    throw new InvalidOperationException($"Xe {vehicle.VehiclePlate} tham chiếu khách hàng không tồn tại: {vehicle.CustomerId}");
                }
            }

            foreach (var monthlyTicket in monthlyTickets)
            {
                if (!customerIds.Contains(monthlyTicket.CustomerId))
                {
                    throw new InvalidOperationException($"Vé tháng {monthlyTicket.MonthlyTicketId} tham chiếu khách hàng không tồn tại.");
                }

                if (!vehicleByPlate.TryGetValue(monthlyTicket.VehiclePlate, out var vehicle))
                {
                    throw new InvalidOperationException($"Vé tháng {monthlyTicket.MonthlyTicketId} tham chiếu xe không tồn tại.");
                }

                if (vehicle.CustomerId != monthlyTicket.CustomerId || vehicle.VehicleType != monthlyTicket.VehicleType)
                {
                    throw new InvalidOperationException($"Vé tháng {monthlyTicket.MonthlyTicketId} không khớp khách hàng hoặc loại xe.");
                }
            }

            foreach (var ticket in tickets)
            {
                if (!vehicleByPlate.TryGetValue(ticket.VehiclePlate, out var vehicle))
                {
                    throw new InvalidOperationException($"Vé {ticket.TicketId} tham chiếu xe không tồn tại.");
                }

                if (vehicle.VehicleType != ticket.VehicleType)
                {
                    throw new InvalidOperationException($"Vé {ticket.TicketId} không khớp loại xe với biển số {ticket.VehiclePlate}.");
                }

                if (ticket.CustomerId != null && vehicle.CustomerId != ticket.CustomerId)
                {
                    throw new InvalidOperationException($"Vé {ticket.TicketId} không khớp chủ xe {ticket.VehiclePlate}.");
                }

                if (ticket.SlotId != null)
                {
                    if (!slotById.TryGetValue(ticket.SlotId, out var slot))
                    {
                        throw new InvalidOperationException($"Vé {ticket.TicketId} tham chiếu ô đỗ không tồn tại.");
                    }

                    if (slot.VehicleType != ticket.VehicleType)
                    {
                        throw new InvalidOperationException($"Vé {ticket.TicketId} không khớp loại xe của ô đỗ {ticket.SlotId}.");
                    }
                }
            }

            var activeTickets = tickets.Where(t => t.Status == "Đang trong bãi").ToList();
            EnsureUnique(activeTickets.Select(t => t.VehiclePlate), "xe đang trong bãi");
            EnsureUnique(activeTickets.Select(t => t.SlotId ?? ""), "ô đỗ đang sử dụng");

            foreach (var ticket in activeTickets)
            {
                var slot = slotById[ticket.SlotId!];
                if (slot.Status != "Đang sử dụng")
                {
                    throw new InvalidOperationException($"Ô {slot.SlotId} phải ở trạng thái Đang sử dụng vì có vé {ticket.TicketId}.");
                }
            }

            foreach (var slot in slots.Where(s => s.Status == "Đang sử dụng"))
            {
                if (!activeTickets.Any(t => t.SlotId == slot.SlotId))
                {
                    throw new InvalidOperationException($"Ô {slot.SlotId} đang sử dụng nhưng không có vé đang trong bãi.");
                }
            }

            foreach (var reservation in reservations)
            {
                if (!customerIds.Contains(reservation.CustomerId))
                {
                    throw new InvalidOperationException($"Đặt chỗ {reservation.ReservationId} tham chiếu khách hàng không tồn tại.");
                }

                if (reservation.VehiclePlate != null && !vehicleByPlate.ContainsKey(reservation.VehiclePlate))
                {
                    throw new InvalidOperationException($"Đặt chỗ {reservation.ReservationId} tham chiếu xe không tồn tại.");
                }

                if (reservation.SlotId != null && !slotById.ContainsKey(reservation.SlotId))
                {
                    throw new InvalidOperationException($"Đặt chỗ {reservation.ReservationId} tham chiếu ô đỗ không tồn tại.");
                }
            }

            foreach (var slot in slots.Where(s => s.Status == "Đã đặt"))
            {
                if (!reservations.Any(r => r.SlotId == slot.SlotId && r.Status == "Chờ"))
                {
                    throw new InvalidOperationException($"Ô {slot.SlotId} đã đặt nhưng không có reservation đang chờ.");
                }
            }

            var ticketIds = tickets.Select(t => t.TicketId).ToHashSet();
            var monthlyTicketIds = monthlyTickets.Select(t => t.MonthlyTicketId).ToHashSet();
            foreach (var payment in payments)
            {
                var hasTicket = payment.TicketId != null;
                var hasMonthlyTicket = payment.MonthlyTicketId != null;
                if (hasTicket == hasMonthlyTicket)
                {
                    throw new InvalidOperationException($"Thanh toán {payment.PaymentId} phải tham chiếu đúng một loại vé.");
                }

                if (payment.TicketId != null && !ticketIds.Contains(payment.TicketId))
                {
                    throw new InvalidOperationException($"Thanh toán {payment.PaymentId} tham chiếu vé lượt không tồn tại.");
                }

                if (payment.MonthlyTicketId != null && !monthlyTicketIds.Contains(payment.MonthlyTicketId))
                {
                    throw new InvalidOperationException($"Thanh toán {payment.PaymentId} tham chiếu vé tháng không tồn tại.");
                }
            }
        }

        private static void EnsureUnique(IEnumerable<string> values, string label)
        {
            var duplicates = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .GroupBy(v => v)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                throw new InvalidOperationException($"Seed data bị trùng {label}: {string.Join(", ", duplicates)}");
            }
        }
    }
}
