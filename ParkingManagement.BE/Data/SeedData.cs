using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParkingManagement.DAL.Data
{
    public static class ParkingManagementSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            const string managerPasswordHash = "$2a$12$kA4mFAV2vy8DBLtVX2pvMObG4nlikvEj9S4hGSLWE2JkignKN8uwS"; // Huong@4906
            const string employeePasswordHash = "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1."; // Huong4906@
            const string customerPasswordHash = "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1."; // Huong4906@

            // 1. ACCOUNTS
            var accounts = new List<Account>
            {
                new Account { AccountId = "ACC001", PasswordHash = managerPasswordHash, Role = "Manager", Email = "th04092006@gmail.com", CreatedAt = new DateTime(2026, 1, 1, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new Account { AccountId = "ACC002", PasswordHash = employeePasswordHash, Role = "Employee", Email = "thanh76555765@gmail.com", CreatedAt = new DateTime(2026, 1, 5, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new Account { AccountId = "ACC003", PasswordHash = employeePasswordHash, Role = "Employee", Email = "staff.hung@gmail.com", CreatedAt = new DateTime(2026, 1, 8, 8, 0, 0), IsActive = true, RequirePasswordChange = false },
                new Account { AccountId = "ACC004", PasswordHash = employeePasswordHash, Role = "Employee", Email = "staff.disabled@gmail.com", CreatedAt = new DateTime(2026, 2, 1, 8, 0, 0), IsActive = false, RequirePasswordChange = false }
            };

            var customers = new List<Customer>();
            var vehicles = new List<Vehicle>();
            var random = new Random(12345);

            for (int i = 1; i <= 30; i++)
            {
                string accId = $"ACC{100 + i}";
                string cusId = $"CUS{i:000}";
                accounts.Add(new Account { AccountId = accId, PasswordHash = customerPasswordHash, Role = "Customer", Email = $"customer{i}@gmail.com", CreatedAt = new DateTime(2026, 1, 1).AddDays(i), IsActive = true, RequirePasswordChange = false });
                customers.Add(new Customer { CustomerId = cusId, AccountId = accId, FullName = $"Khách hàng {i}", PhoneNumber = $"09{random.Next(10000000, 99999999)}", Gender = i % 2 == 0 ? "Female" : "Male", IsDeleted = false });
                
                vehicles.Add(new Vehicle { VehiclePlate = $"43A-{random.Next(100, 999)}.{random.Next(10, 99)}", VehicleType = "Xe máy", CustomerId = cusId });
                if (i % 3 == 0) vehicles.Add(new Vehicle { VehiclePlate = $"43B-{random.Next(100, 999)}.{random.Next(10, 99)}", VehicleType = "Ô tô nhỏ", CustomerId = cusId });
                if (i % 5 == 0) vehicles.Add(new Vehicle { VehiclePlate = $"43C-{random.Next(100, 999)}.{random.Next(10, 99)}", VehicleType = "Ô tô lớn", CustomerId = cusId });
            }

            // 1.5 GUEST VEHICLES
            for (int i = 1; i <= 50; i++)
            {
                vehicles.Add(new Vehicle { VehiclePlate = $"92C-{random.Next(100, 999)}.{random.Next(10, 99)}", VehicleType = "Xe máy", CustomerId = null });
                if (i % 2 == 0) vehicles.Add(new Vehicle { VehiclePlate = $"74A-{random.Next(100, 999)}.{random.Next(10, 99)}", VehicleType = "Ô tô nhỏ", CustomerId = null });
            }

            modelBuilder.Entity<Account>().HasData(accounts.ToArray());
            modelBuilder.Entity<Customer>().HasData(customers.ToArray());
            modelBuilder.Entity<Vehicle>().HasData(vehicles.ToArray());

            // 2. MANAGER & EMPLOYEES
            modelBuilder.Entity<Manager>().HasData(
                new Manager { ManagerId = "MGR001", AccountId = "ACC001", FullName = "Nguyễn Thị Hường", PhoneNumber = "0901234567", Gender = "Female", IsDeleted = false }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeId = "EMP001", EmployeeCode = "EMP001", AccountId = "ACC002", FullName = "Nguyễn Thanh", PhoneNumber = "0912345678", Gender = "Male", Shift = "Sáng", ManagerId = "MGR001", IsDeleted = false },
                new Employee { EmployeeId = "EMP002", EmployeeCode = "EMP002", AccountId = "ACC003", FullName = "Lê Văn Hùng", PhoneNumber = "0923456789", Gender = "Male", Shift = "Chiều", ManagerId = "MGR001", IsDeleted = false },
                new Employee { EmployeeId = "EMP003", EmployeeCode = "EMP003", AccountId = "ACC004", FullName = "Phan Quốc Nam", PhoneNumber = "0987654321", Gender = "Male", Shift = null, ManagerId = "MGR001", IsDeleted = true }
            );

            modelBuilder.Entity<EmployeeInvite>().HasData(
                new EmployeeInvite { InviteToken = "INVITE-EMP004-2026", EmployeeCode = "EMP004", Email = "staff.invited@gmail.com", FullName = "Ngô Minh An", PhoneNumber = "0977000111", Shift = "Tối", CreatedAt = new DateTime(2026, 5, 8, 8, 0, 0), ExpiryTime = new DateTime(2030, 12, 31, 23, 59, 59), IsUsed = false },
                new EmployeeInvite { InviteToken = "INVITE-USED-EMP005", EmployeeCode = "EMP005", Email = "staff.usedinvite@gmail.com", FullName = "Đỗ Thanh Bình", PhoneNumber = "0977000222", Shift = "Sáng", CreatedAt = new DateTime(2026, 4, 20, 8, 0, 0), ExpiryTime = new DateTime(2026, 4, 21, 8, 0, 0), IsUsed = true }
            );

            // 3. PARKING SLOTS
            var slots = new List<ParkingSlot>();
            for (int i = 1; i <= 50; i++)
            {
                slots.Add(new ParkingSlot { SlotId = $"A{i:00}", Location = $"Khu A - Ô {i:00}", VehicleType = "Xe máy", Status = "Trống", LastUpdated = new DateTime(2026, 4, 1, 0, 0, 0) });
            }
            for (int i = 1; i <= 50; i++)
            {
                slots.Add(new ParkingSlot { SlotId = $"B{i:00}", Location = $"Khu B - Ô {i:00}", VehicleType = "Ô tô nhỏ", Status = "Trống", LastUpdated = new DateTime(2026, 4, 1, 0, 0, 0) });
            }
            for (int i = 1; i <= 20; i++)
            {
                slots.Add(new ParkingSlot { SlotId = $"C{i:00}", Location = $"Khu C - Ô {i:00}", VehicleType = "Ô tô lớn", Status = "Trống", LastUpdated = new DateTime(2026, 4, 1, 0, 0, 0) });
            }

            // 4. PRICING CONFIGURATIONS
            modelBuilder.Entity<PricingConfiguration>().HasData(
                new PricingConfiguration { PricingId = "PRICE-XM-HOUR", VehicleType = "Xe máy", RateType = "HourlyRate", Amount = 3000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-XM-DAY", VehicleType = "Xe máy", RateType = "MaxDailyFee", Amount = 30000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-XM-M1", VehicleType = "Xe máy", RateType = "Monthly1M", Amount = 150000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-XM-M3", VehicleType = "Xe máy", RateType = "Monthly3M", Amount = 400000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-XM-M6", VehicleType = "Xe máy", RateType = "Monthly6M", Amount = 750000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTON-HOUR", VehicleType = "Ô tô nhỏ", RateType = "HourlyRate", Amount = 5000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTON-DAY", VehicleType = "Ô tô nhỏ", RateType = "MaxDailyFee", Amount = 50000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTON-M1", VehicleType = "Ô tô nhỏ", RateType = "Monthly1M", Amount = 300000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTON-M3", VehicleType = "Ô tô nhỏ", RateType = "Monthly3M", Amount = 800000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTON-M6", VehicleType = "Ô tô nhỏ", RateType = "Monthly6M", Amount = 1500000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTOL-HOUR", VehicleType = "Ô tô lớn", RateType = "HourlyRate", Amount = 8000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTOL-DAY", VehicleType = "Ô tô lớn", RateType = "MaxDailyFee", Amount = 80000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTOL-M1", VehicleType = "Ô tô lớn", RateType = "Monthly1M", Amount = 500000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTOL-M3", VehicleType = "Ô tô lớn", RateType = "Monthly3M", Amount = 1300000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" },
                new PricingConfiguration { PricingId = "PRICE-OTOL-M6", VehicleType = "Ô tô lớn", RateType = "Monthly6M", Amount = 2500000, UpdatedAt = new DateTime(2026, 5, 1, 8, 0, 0), UpdatedBy = "MGR001" }
            );

            // 5. MONTHLY TICKETS
            var monthlyTickets = new List<MonthlyTicket>();
            for (int i = 1; i <= 15; i++)
            {
                var vehicle = vehicles[i];
                var mtkStart = new DateTime(2026, 4, 1).AddDays(random.Next(0, 30));
                var mtkEnd = mtkStart.AddMonths(i % 3 == 0 ? 3 : 1);
                var isExpired = mtkEnd < new DateTime(2026, 5, 12);
                monthlyTickets.Add(new MonthlyTicket
                {
                    MonthlyTicketId = $"MTK{i:000}",
                    CustomerId = vehicle.CustomerId,
                    VehiclePlate = vehicle.VehiclePlate,
                    VehicleType = vehicle.VehicleType,
                    StartDate = mtkStart,
                    EndDate = mtkEnd,
                    PackageType = i % 3 == 0 ? "3 tháng" : "1 tháng",
                    TotalFee = vehicle.VehicleType == "Xe máy" ? 150000 : (vehicle.VehicleType == "Ô tô nhỏ" ? 300000 : 500000),
                    Status = isExpired ? "Hết hạn" : "Hoạt động",
                    CreatedAt = mtkStart
                });
            }
            modelBuilder.Entity<MonthlyTicket>().HasData(monthlyTickets.ToArray());

            // 6. TICKETS & PAYMENTS
            var tickets = new List<Ticket>();
            var payments = new List<Payment>();
            int ticketCounter = 1;
            int paymentCounter = 1;
            var currentDate = new DateTime(2026, 4, 1);
            var endDate = new DateTime(2026, 5, 12); // Today

            while (currentDate <= endDate)
            {
                // Generate 5-15 tickets per day
                int ticketsToday = random.Next(5, 16);
                for (int i = 0; i < ticketsToday; i++)
                {
                    var randVehicle = vehicles[random.Next(0, vehicles.Count)];
                    bool isMotorbike = randVehicle.VehicleType == "Xe máy";
                    string vType = randVehicle.VehicleType;
                    string vPlate = randVehicle.VehiclePlate;
                    
                    var checkIn = currentDate.AddHours(random.Next(6, 20)).AddMinutes(random.Next(0, 60));
                    var checkOut = checkIn.AddHours(random.Next(1, 10)).AddMinutes(random.Next(0, 60));
                    
                    if (checkOut > endDate) continue;

                    decimal fee = isMotorbike ? 5000 : (vType == "Ô tô nhỏ" ? 15000 : 25000);
                    
                    string slotId = isMotorbike ? $"A{random.Next(1, 51):00}" : (vType == "Ô tô nhỏ" ? $"B{random.Next(1, 51):00}" : $"C{random.Next(1, 21):00}");
                    
                    string tktId = $"TKT{ticketCounter:0000}";
                    tickets.Add(new Ticket
                    {
                        TicketId = tktId,
                        VehiclePlate = vPlate,
                        VehicleType = vType,
                        SlotId = slotId,
                        CheckInTime = checkIn,
                        CheckOutTime = checkOut,
                        Fee = fee,
                        Status = "Đã ra",
                        CustomerId = randVehicle.CustomerId
                    });
                    
                    payments.Add(new Payment
                    {
                        PaymentId = $"PAY{paymentCounter:0000}",
                        TicketId = tktId,
                        Amount = fee,
                        Method = random.Next(0, 3) == 0 ? "Chuyển khoản" : "Tiền mặt",
                        PaymentTime = checkOut,
                        Status = "Thành công"
                    });

                    ticketCounter++;
                    paymentCounter++;
                }
                currentDate = currentDate.AddDays(1);
            }
            
            // Generate some currently parking tickets (đang trong bãi)
            for (int i = 0; i < 15; i++)
            {
                var randVehicle = vehicles[i];
                string vType = randVehicle.VehicleType;
                string vPlate = randVehicle.VehiclePlate;
                string slotId = vType == "Xe máy" ? $"A{i+1:00}" : (vType == "Ô tô nhỏ" ? $"B{i+1:00}" : $"C{i+1:00}");
                string tktId = $"TKT{ticketCounter:0000}";
                tickets.Add(new Ticket
                {
                    TicketId = tktId,
                    VehiclePlate = vPlate,
                    VehicleType = vType,
                    SlotId = slotId,
                    CheckInTime = new DateTime(2026, 5, 12, 10, 0, 0).AddHours(-random.Next(1, 5)),
                    CheckOutTime = null,
                    Fee = 0,
                    Status = "Đang trong bãi",
                    CustomerId = randVehicle.CustomerId
                });
                ticketCounter++;
                
                // Keep the slots consistent
                var slot = slots.FirstOrDefault(s => s.SlotId == slotId);
                if (slot != null) {
                    slot.Status = "Đang sử dụng";
                    slot.LastUpdated = new DateTime(2026, 5, 12, 10, 0, 0).AddHours(-1);
                }
            }

            modelBuilder.Entity<ParkingSlot>().HasData(slots.ToArray());
            modelBuilder.Entity<Ticket>().HasData(tickets.ToArray());
            modelBuilder.Entity<Payment>().HasData(payments.ToArray());

            // 7. OTPs
            modelBuilder.Entity<Otp>().HasData(
                new Otp { OtpId = "OTP001", Email = "customer.pending@gmail.com", Code = "123456", CreatedAt = new DateTime(2026, 5, 8, 8, 0, 0), ExpiresAt = new DateTime(2030, 12, 31, 23, 59, 59), IsVerified = false, VerifiedAt = null }
            );
        }
    }
}
