using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequirePasswordChange = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeInvites",
                columns: table => new
                {
                    InviteToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Shift = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeInvites", x => x.InviteToken);
                });

            migrationBuilder.CreateTable(
                name: "Otps",
                columns: table => new
                {
                    OtpId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otps", x => x.OtpId);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSlots",
                columns: table => new
                {
                    SlotId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSlots", x => x.SlotId);
                    table.CheckConstraint("CK_ParkingSlot_Status", "Status IN (N'Trống', N'Đang sử dụng', N'Đã đặt', N'Bảo trì')");
                });

            migrationBuilder.CreateTable(
                name: "PricingConfigurations",
                columns: table => new
                {
                    PricingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingConfigurations", x => x.PricingId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    ManagerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.ManagerId);
                    table.ForeignKey(
                        name: "FK_Managers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehiclePlate);
                    table.CheckConstraint("CK_Vehicle_Type", "VehicleType IN (N'Xe máy', N'Ô tô nhỏ', N'Ô tô lớn')");
                    table.ForeignKey(
                        name: "FK_Vehicles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Shift = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ManagerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "ManagerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyTickets",
                columns: table => new
                {
                    MonthlyTicketId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PackageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalFee = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyTickets", x => x.MonthlyTicketId);
                    table.CheckConstraint("CK_MonthlyTicket_Status", "Status IN (N'Hoạt động', N'Hết hạn', N'Đã hủy')");
                    table.ForeignKey(
                        name: "FK_MonthlyTickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonthlyTickets_Vehicles_VehiclePlate",
                        column: x => x.VehiclePlate,
                        principalTable: "Vehicles",
                        principalColumn: "VehiclePlate",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ReservationId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SlotId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpectedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ReservationId);
                    table.CheckConstraint("CK_Reservation_Status", "Status IN (N'Chờ', N'Đã nhận', N'Hủy', N'Hết hạn')");
                    table.ForeignKey(
                        name: "FK_Reservations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_ParkingSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "ParkingSlots",
                        principalColumn: "SlotId");
                    table.ForeignKey(
                        name: "FK_Reservations_Vehicles_VehiclePlate",
                        column: x => x.VehiclePlate,
                        principalTable: "Vehicles",
                        principalColumn: "VehiclePlate");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SlotId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fee = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.CheckConstraint("CK_Ticket_Status", "Status IN (N'Đang trong bãi', N'Đã ra')");
                    table.ForeignKey(
                        name: "FK_Tickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_Tickets_ParkingSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "ParkingSlots",
                        principalColumn: "SlotId");
                    table.ForeignKey(
                        name: "FK_Tickets_Vehicles_VehiclePlate",
                        column: x => x.VehiclePlate,
                        principalTable: "Vehicles",
                        principalColumn: "VehiclePlate",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSlotAuditLogs",
                columns: table => new
                {
                    LogId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SlotId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSlotAuditLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_ParkingSlotAuditLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParkingSlotAuditLogs_ParkingSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "ParkingSlots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TicketId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MonthlyTicketId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.CheckConstraint("CK_Payment_TicketOrMonthly", "(TicketId IS NOT NULL AND MonthlyTicketId IS NULL) OR (TicketId IS NULL AND MonthlyTicketId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Payments_MonthlyTickets_MonthlyTicketId",
                        column: x => x.MonthlyTicketId,
                        principalTable: "MonthlyTickets",
                        principalColumn: "MonthlyTicketId");
                    table.ForeignKey(
                        name: "FK_Payments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "TicketId");
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "AccountId", "CreatedAt", "Email", "IsActive", "PasswordHash", "RequirePasswordChange", "Role" },
                values: new object[,]
                {
                    { "ACC001", new DateTime(2026, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "th04092006@gmail.com", true, "$2a$12$kA4mFAV2vy8DBLtVX2pvMObG4nlikvEj9S4hGSLWE2JkignKN8uwS", false, "Manager" },
                    { "ACC002", new DateTime(2026, 1, 5, 8, 0, 0, 0, DateTimeKind.Unspecified), "thanh76555765@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC003", new DateTime(2026, 1, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "staff.hung@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC004", new DateTime(2026, 2, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "staff.disabled@gmail.com", false, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC005", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "th04092006.customer@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC006", new DateTime(2026, 2, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), "customer.lan@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC007", new DateTime(2026, 2, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), "customer.khoa@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC008", new DateTime(2026, 3, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "customer.huong@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC009", new DateTime(2026, 3, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), "customer.minh@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" }
                });

            migrationBuilder.InsertData(
                table: "EmployeeInvites",
                columns: new[] { "InviteToken", "CreatedAt", "Email", "EmployeeCode", "ExpiryTime", "FullName", "IsUsed", "PhoneNumber", "Shift" },
                values: new object[,]
                {
                    { "INVITE-EMP004-2026", new DateTime(2026, 5, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "staff.invited@gmail.com", "EMP004", new DateTime(2030, 12, 31, 23, 59, 59, 0, DateTimeKind.Unspecified), "Ngô Minh An", false, "0977000111", "Tối" },
                    { "INVITE-USED-EMP005", new DateTime(2026, 4, 20, 8, 0, 0, 0, DateTimeKind.Unspecified), "staff.usedinvite@gmail.com", "EMP005", new DateTime(2026, 4, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), "Đỗ Thanh Bình", true, "0977000222", "Sáng" }
                });

            migrationBuilder.InsertData(
                table: "Otps",
                columns: new[] { "OtpId", "Code", "CreatedAt", "Email", "ExpiresAt", "IsVerified", "VerifiedAt" },
                values: new object[,]
                {
                    { "OTP001", "123456", new DateTime(2026, 5, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "customer.pending@gmail.com", new DateTime(2030, 12, 31, 23, 59, 59, 0, DateTimeKind.Unspecified), false, null },
                    { "OTP002", "654321", new DateTime(2026, 5, 8, 8, 5, 0, 0, DateTimeKind.Unspecified), "th04092006.customer@gmail.com", new DateTime(2026, 5, 8, 8, 10, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2026, 5, 8, 8, 6, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "ParkingSlots",
                columns: new[] { "SlotId", "LastUpdated", "Location", "Status", "VehicleType" },
                values: new object[,]
                {
                    { "A01", new DateTime(2026, 5, 8, 8, 23, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 01", "Đang sử dụng", "Xe máy" },
                    { "A02", new DateTime(2026, 5, 8, 7, 30, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 02", "Trống", "Xe máy" },
                    { "A03", new DateTime(2026, 5, 8, 7, 30, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 03", "Trống", "Xe máy" },
                    { "A04", new DateTime(2026, 5, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 04", "Đã đặt", "Xe máy" },
                    { "A05", new DateTime(2026, 5, 8, 10, 15, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 05", "Đang sử dụng", "Xe máy" },
                    { "A06", new DateTime(2026, 5, 7, 16, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 06", "Bảo trì", "Xe máy" },
                    { "B01", new DateTime(2026, 5, 8, 9, 5, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 01", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B02", new DateTime(2026, 5, 8, 7, 30, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 02", "Trống", "Ô tô nhỏ" },
                    { "B03", new DateTime(2026, 5, 8, 7, 30, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 03", "Trống", "Ô tô nhỏ" },
                    { "C01", new DateTime(2026, 5, 8, 8, 40, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 01", "Đang sử dụng", "Ô tô lớn" },
                    { "C02", new DateTime(2026, 5, 8, 7, 30, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 02", "Trống", "Ô tô lớn" },
                    { "C03", new DateTime(2026, 5, 6, 15, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 03", "Bảo trì", "Ô tô lớn" }
                });

            migrationBuilder.InsertData(
                table: "PricingConfigurations",
                columns: new[] { "PricingId", "Amount", "RateType", "UpdatedAt", "UpdatedBy", "VehicleType" },
                values: new object[,]
                {
                    { "PRICE-OTOL-DAY", 80000m, "MaxDailyFee", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-HOUR", 8000m, "HourlyRate", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-M1", 500000m, "Monthly1M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-M3", 1300000m, "Monthly3M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-M6", 2500000m, "Monthly6M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTON-DAY", 50000m, "MaxDailyFee", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-HOUR", 5000m, "HourlyRate", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-M1", 300000m, "Monthly1M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-M3", 800000m, "Monthly3M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-M6", 1500000m, "Monthly6M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-XM-DAY", 30000m, "MaxDailyFee", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-HOUR", 3000m, "HourlyRate", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-M1", 150000m, "Monthly1M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-M3", 400000m, "Monthly3M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-M6", 750000m, "Monthly6M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehiclePlate", "CustomerId", "VehicleType" },
                values: new object[,]
                {
                    { "74A-567.89", null, "Ô tô nhỏ" },
                    { "92C-111.22", null, "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "AccountId", "FullName", "Gender", "IsDeleted", "PhoneNumber" },
                values: new object[,]
                {
                    { "CUS001", "ACC005", "Phạm Quốc Bảo", "Male", false, "0931122334" },
                    { "CUS002", "ACC006", "Hồ Thị Lan", "Female", false, "0942233445" },
                    { "CUS003", "ACC007", "Đặng Minh Khoa", "Male", false, "0953344556" },
                    { "CUS004", "ACC008", "Nguyễn Thị Thu Hương", "Female", false, "0964455667" },
                    { "CUS005", "ACC009", "Võ Nhật Minh", "Male", false, "0975566778" }
                });

            migrationBuilder.InsertData(
                table: "Managers",
                columns: new[] { "ManagerId", "AccountId", "FullName", "Gender", "IsDeleted", "PhoneNumber" },
                values: new object[] { "MGR001", "ACC001", "Nguyễn Thị Hường", "Female", false, "0901234567" });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "CheckInTime", "CheckOutTime", "CustomerId", "Fee", "SlotId", "Status", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "TKT003", new DateTime(2026, 5, 8, 10, 15, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A05", "Đang trong bãi", "92C-111.22", "Xe máy" },
                    { "TKT008", new DateTime(2026, 5, 4, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 12, 30, 0, 0, DateTimeKind.Unspecified), null, 24000m, "B03", "Đã ra", "74A-567.89", "Ô tô nhỏ" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "AccountId", "EmployeeCode", "FullName", "Gender", "IsDeleted", "ManagerId", "PhoneNumber", "Shift" },
                values: new object[,]
                {
                    { "EMP001", "ACC002", "EMP001", "Nguyễn Thanh", "Male", false, "MGR001", "0912345678", "Sáng" },
                    { "EMP002", "ACC003", "EMP002", "Lê Văn Hùng", "Male", false, "MGR001", "0923456789", "Chiều" },
                    { "EMP003", "ACC004", "EMP003", "Phan Quốc Nam", "Male", true, "MGR001", "0987654321", null }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Method", "MonthlyTicketId", "PaymentTime", "Status", "TicketId" },
                values: new object[] { "PAY004", 24000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 12, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT008" });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehiclePlate", "CustomerId", "VehicleType" },
                values: new object[,]
                {
                    { "43A-123.45", "CUS001", "Xe máy" },
                    { "43B-456.78", "CUS002", "Ô tô nhỏ" },
                    { "43C-789.01", "CUS003", "Xe máy" },
                    { "43D-234.56", "CUS004", "Ô tô lớn" },
                    { "43E-222.33", "CUS005", "Xe máy" },
                    { "51G-888.88", "CUS001", "Ô tô nhỏ" }
                });

            migrationBuilder.InsertData(
                table: "MonthlyTickets",
                columns: new[] { "MonthlyTicketId", "CreatedAt", "CustomerId", "EndDate", "PackageType", "StartDate", "Status", "TotalFee", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "MTK001", new DateTime(2026, 5, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "CUS004", new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 500000m, "43D-234.56", "Ô tô lớn" },
                    { "MTK002", new DateTime(2026, 4, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "CUS001", new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 800000m, "51G-888.88", "Ô tô nhỏ" },
                    { "MTK003", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43C-789.01", "Xe máy" },
                    { "MTK004", new DateTime(2026, 4, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã hủy", 150000m, "43E-222.33", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "ParkingSlotAuditLogs",
                columns: new[] { "LogId", "ChangedAt", "EmployeeId", "NewStatus", "Note", "OldStatus", "Reason", "SlotId" },
                values: new object[,]
                {
                    { "LOG001", new DateTime(2026, 5, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đã đặt", "Khách đặt chỗ trước", "Trống", "Reservation RES001", "A04" },
                    { "LOG002", new DateTime(2026, 5, 7, 16, 0, 0, 0, DateTimeKind.Unspecified), "EMP002", "Bảo trì", "Khóa ô để sửa cảm biến", "Trống", "Maintenance", "A06" },
                    { "LOG003", new DateTime(2026, 5, 8, 9, 5, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT002", "Trống", "Check-in", "B01" }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "ReservationId", "CreatedAt", "CustomerId", "ExpectedTime", "SlotId", "Status", "VehiclePlate" },
                values: new object[,]
                {
                    { "RES001", new DateTime(2026, 5, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 5, 8, 14, 0, 0, 0, DateTimeKind.Unspecified), "A04", "Chờ", "43C-789.01" },
                    { "RES002", new DateTime(2026, 5, 6, 20, 0, 0, 0, DateTimeKind.Unspecified), "CUS002", new DateTime(2026, 5, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), "B03", "Đã nhận", "43B-456.78" },
                    { "RES003", new DateTime(2026, 5, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 5, 6, 18, 0, 0, 0, DateTimeKind.Unspecified), "A03", "Hủy", "43E-222.33" },
                    { "RES004", new DateTime(2026, 4, 30, 9, 0, 0, 0, DateTimeKind.Unspecified), "CUS001", new DateTime(2026, 5, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "C02", "Hết hạn", "51G-888.88" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "CheckInTime", "CheckOutTime", "CustomerId", "Fee", "SlotId", "Status", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "TKT001", new DateTime(2026, 5, 8, 8, 23, 0, 0, DateTimeKind.Unspecified), null, "CUS001", 0m, "A01", "Đang trong bãi", "43A-123.45", "Xe máy" },
                    { "TKT002", new DateTime(2026, 5, 8, 9, 5, 0, 0, DateTimeKind.Unspecified), null, "CUS002", 0m, "B01", "Đang trong bãi", "43B-456.78", "Ô tô nhỏ" },
                    { "TKT004", new DateTime(2026, 5, 8, 8, 40, 0, 0, DateTimeKind.Unspecified), null, "CUS004", 0m, "C01", "Đang trong bãi", "43D-234.56", "Ô tô lớn" },
                    { "TKT005", new DateTime(2026, 5, 7, 14, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 17, 30, 0, 0, DateTimeKind.Unspecified), "CUS003", 10500m, "A02", "Đã ra", "43C-789.01", "Xe máy" },
                    { "TKT006", new DateTime(2026, 5, 6, 7, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 12, 0, 0, 0, DateTimeKind.Unspecified), "CUS001", 13500m, "A03", "Đã ra", "43A-123.45", "Xe máy" },
                    { "TKT007", new DateTime(2026, 5, 5, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 11, 0, 0, 0, DateTimeKind.Unspecified), "CUS002", 24000m, "B02", "Đã ra", "43B-456.78", "Ô tô nhỏ" }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Method", "MonthlyTicketId", "PaymentTime", "Status", "TicketId" },
                values: new object[,]
                {
                    { "PAY001", 10500m, "Tiền mặt", null, new DateTime(2026, 5, 7, 17, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT005" },
                    { "PAY002", 13500m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 12, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT006" },
                    { "PAY003", 24000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 11, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT007" },
                    { "PAY005", 500000m, "Ví điện tử", "MTK001", new DateTime(2026, 5, 1, 9, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY006", 800000m, "Chuyển khoản", "MTK002", new DateTime(2026, 4, 1, 10, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY007", 150000m, "Tiền mặt", "MTK003", new DateTime(2026, 3, 1, 8, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY008", 150000m, "Tiền mặt", "MTK004", new DateTime(2026, 4, 1, 8, 5, 0, 0, DateTimeKind.Unspecified), "Thất bại", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccountId",
                table: "Customers",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_AccountId",
                table: "Employees",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_AccountId",
                table: "Managers",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyTickets_CustomerId",
                table: "MonthlyTickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyTickets_VehiclePlate",
                table: "MonthlyTickets",
                column: "VehiclePlate");

            migrationBuilder.CreateIndex(
                name: "IX_Otps_Email",
                table: "Otps",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSlotAuditLogs_EmployeeId",
                table: "ParkingSlotAuditLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSlotAuditLogs_SlotId",
                table: "ParkingSlotAuditLogs",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MonthlyTicketId",
                table: "Payments",
                column: "MonthlyTicketId",
                unique: true,
                filter: "[MonthlyTicketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TicketId",
                table: "Payments",
                column: "TicketId",
                unique: true,
                filter: "[TicketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SlotId",
                table: "Reservations",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VehiclePlate",
                table: "Reservations",
                column: "VehiclePlate");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId",
                table: "Tickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SlotId",
                table: "Tickets",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_VehiclePlate",
                table: "Tickets",
                column: "VehiclePlate");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeInvites");

            migrationBuilder.DropTable(
                name: "Otps");

            migrationBuilder.DropTable(
                name: "ParkingSlotAuditLogs");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PricingConfigurations");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "MonthlyTickets");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "ParkingSlots");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
