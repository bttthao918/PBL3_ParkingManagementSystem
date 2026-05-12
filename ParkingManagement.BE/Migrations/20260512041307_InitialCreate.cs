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
                    { "ACC101", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer1@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC102", new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer2@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC103", new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer3@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC104", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer4@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC105", new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer5@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC106", new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer6@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC107", new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer7@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC108", new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer8@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC109", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer9@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC110", new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer10@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC111", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer11@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC112", new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer12@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC113", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer13@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC114", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer14@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC115", new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer15@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC116", new DateTime(2026, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer16@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC117", new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer17@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC118", new DateTime(2026, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer18@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC119", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer19@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC120", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer20@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC121", new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer21@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC122", new DateTime(2026, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer22@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC123", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer23@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC124", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer24@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC125", new DateTime(2026, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer25@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC126", new DateTime(2026, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer26@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC127", new DateTime(2026, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer27@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC128", new DateTime(2026, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer28@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC129", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer29@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC130", new DateTime(2026, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "customer30@gmail.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" }
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
                values: new object[] { "OTP001", "123456", new DateTime(2026, 5, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "customer.pending@gmail.com", new DateTime(2030, 12, 31, 23, 59, 59, 0, DateTimeKind.Unspecified), false, null });

            migrationBuilder.InsertData(
                table: "ParkingSlots",
                columns: new[] { "SlotId", "LastUpdated", "Location", "Status", "VehicleType" },
                values: new object[,]
                {
                    { "A01", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 01", "Đang sử dụng", "Xe máy" },
                    { "A02", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 02", "Đang sử dụng", "Xe máy" },
                    { "A03", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 03", "Đang sử dụng", "Xe máy" },
                    { "A04", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 04", "Đang sử dụng", "Xe máy" },
                    { "A05", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 05", "Đang sử dụng", "Xe máy" },
                    { "A06", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 06", "Đang sử dụng", "Xe máy" },
                    { "A07", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 07", "Đang sử dụng", "Xe máy" },
                    { "A08", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 08", "Đang sử dụng", "Xe máy" },
                    { "A09", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 09", "Đang sử dụng", "Xe máy" },
                    { "A10", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 10", "Đang sử dụng", "Xe máy" },
                    { "A11", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 11", "Đang sử dụng", "Xe máy" },
                    { "A12", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 12", "Đang sử dụng", "Xe máy" },
                    { "A13", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 13", "Đang sử dụng", "Xe máy" },
                    { "A14", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 14", "Đang sử dụng", "Xe máy" },
                    { "A15", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 15", "Đang sử dụng", "Xe máy" },
                    { "A16", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 16", "Trống", "Xe máy" },
                    { "A17", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 17", "Trống", "Xe máy" },
                    { "A18", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 18", "Trống", "Xe máy" },
                    { "A19", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 19", "Trống", "Xe máy" },
                    { "A20", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 20", "Trống", "Xe máy" },
                    { "A21", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 21", "Trống", "Xe máy" },
                    { "A22", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 22", "Trống", "Xe máy" },
                    { "A23", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 23", "Trống", "Xe máy" },
                    { "A24", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 24", "Trống", "Xe máy" },
                    { "A25", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 25", "Trống", "Xe máy" },
                    { "A26", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 26", "Trống", "Xe máy" },
                    { "A27", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 27", "Trống", "Xe máy" },
                    { "A28", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 28", "Trống", "Xe máy" },
                    { "A29", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 29", "Trống", "Xe máy" },
                    { "A30", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 30", "Trống", "Xe máy" },
                    { "A31", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 31", "Trống", "Xe máy" },
                    { "A32", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 32", "Trống", "Xe máy" },
                    { "A33", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 33", "Trống", "Xe máy" },
                    { "A34", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 34", "Trống", "Xe máy" },
                    { "A35", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 35", "Trống", "Xe máy" },
                    { "A36", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 36", "Trống", "Xe máy" },
                    { "A37", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 37", "Trống", "Xe máy" },
                    { "A38", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 38", "Trống", "Xe máy" },
                    { "A39", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 39", "Trống", "Xe máy" },
                    { "A40", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 40", "Trống", "Xe máy" },
                    { "A41", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 41", "Trống", "Xe máy" },
                    { "A42", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 42", "Trống", "Xe máy" },
                    { "A43", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 43", "Trống", "Xe máy" },
                    { "A44", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 44", "Trống", "Xe máy" },
                    { "A45", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 45", "Trống", "Xe máy" },
                    { "A46", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 46", "Trống", "Xe máy" },
                    { "A47", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 47", "Trống", "Xe máy" },
                    { "A48", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 48", "Trống", "Xe máy" },
                    { "A49", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 49", "Trống", "Xe máy" },
                    { "A50", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 50", "Trống", "Xe máy" },
                    { "B01", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 01", "Trống", "Ô tô nhỏ" },
                    { "B02", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 02", "Trống", "Ô tô nhỏ" },
                    { "B03", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 03", "Trống", "Ô tô nhỏ" },
                    { "B04", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 04", "Trống", "Ô tô nhỏ" },
                    { "B05", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 05", "Trống", "Ô tô nhỏ" },
                    { "B06", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 06", "Trống", "Ô tô nhỏ" },
                    { "B07", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 07", "Trống", "Ô tô nhỏ" },
                    { "B08", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 08", "Trống", "Ô tô nhỏ" },
                    { "B09", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 09", "Trống", "Ô tô nhỏ" },
                    { "B10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 10", "Trống", "Ô tô nhỏ" },
                    { "B11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 11", "Trống", "Ô tô nhỏ" },
                    { "B12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 12", "Trống", "Ô tô nhỏ" },
                    { "B13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 13", "Trống", "Ô tô nhỏ" },
                    { "B14", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 14", "Trống", "Ô tô nhỏ" },
                    { "B15", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 15", "Trống", "Ô tô nhỏ" },
                    { "B16", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 16", "Trống", "Ô tô nhỏ" },
                    { "B17", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 17", "Trống", "Ô tô nhỏ" },
                    { "B18", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 18", "Trống", "Ô tô nhỏ" },
                    { "B19", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 19", "Trống", "Ô tô nhỏ" },
                    { "B20", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 20", "Trống", "Ô tô nhỏ" },
                    { "B21", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 21", "Trống", "Ô tô nhỏ" },
                    { "B22", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 22", "Trống", "Ô tô nhỏ" },
                    { "B23", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 23", "Trống", "Ô tô nhỏ" },
                    { "B24", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 24", "Trống", "Ô tô nhỏ" },
                    { "B25", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 25", "Trống", "Ô tô nhỏ" },
                    { "B26", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 26", "Trống", "Ô tô nhỏ" },
                    { "B27", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 27", "Trống", "Ô tô nhỏ" },
                    { "B28", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 28", "Trống", "Ô tô nhỏ" },
                    { "B29", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 29", "Trống", "Ô tô nhỏ" },
                    { "B30", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 30", "Trống", "Ô tô nhỏ" },
                    { "B31", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 31", "Trống", "Ô tô nhỏ" },
                    { "B32", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 32", "Trống", "Ô tô nhỏ" },
                    { "B33", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 33", "Trống", "Ô tô nhỏ" },
                    { "B34", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 34", "Trống", "Ô tô nhỏ" },
                    { "B35", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 35", "Trống", "Ô tô nhỏ" },
                    { "B36", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 36", "Trống", "Ô tô nhỏ" },
                    { "B37", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 37", "Trống", "Ô tô nhỏ" },
                    { "B38", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 38", "Trống", "Ô tô nhỏ" },
                    { "B39", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 39", "Trống", "Ô tô nhỏ" },
                    { "B40", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 40", "Trống", "Ô tô nhỏ" },
                    { "B41", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 41", "Trống", "Ô tô nhỏ" },
                    { "B42", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 42", "Trống", "Ô tô nhỏ" },
                    { "B43", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 43", "Trống", "Ô tô nhỏ" },
                    { "B44", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 44", "Trống", "Ô tô nhỏ" },
                    { "B45", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 45", "Trống", "Ô tô nhỏ" },
                    { "B46", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 46", "Trống", "Ô tô nhỏ" },
                    { "B47", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 47", "Trống", "Ô tô nhỏ" },
                    { "B48", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 48", "Trống", "Ô tô nhỏ" },
                    { "B49", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 49", "Trống", "Ô tô nhỏ" },
                    { "B50", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 50", "Trống", "Ô tô nhỏ" },
                    { "C01", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 01", "Trống", "Ô tô lớn" },
                    { "C02", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 02", "Trống", "Ô tô lớn" },
                    { "C03", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 03", "Trống", "Ô tô lớn" },
                    { "C04", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 04", "Trống", "Ô tô lớn" },
                    { "C05", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 05", "Trống", "Ô tô lớn" },
                    { "C06", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 06", "Trống", "Ô tô lớn" },
                    { "C07", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 07", "Trống", "Ô tô lớn" },
                    { "C08", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 08", "Trống", "Ô tô lớn" },
                    { "C09", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 09", "Trống", "Ô tô lớn" },
                    { "C10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 10", "Trống", "Ô tô lớn" },
                    { "C11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 11", "Trống", "Ô tô lớn" },
                    { "C12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 12", "Trống", "Ô tô lớn" },
                    { "C13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 13", "Trống", "Ô tô lớn" },
                    { "C14", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 14", "Trống", "Ô tô lớn" },
                    { "C15", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 15", "Trống", "Ô tô lớn" },
                    { "C16", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 16", "Trống", "Ô tô lớn" },
                    { "C17", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 17", "Trống", "Ô tô lớn" },
                    { "C18", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 18", "Trống", "Ô tô lớn" },
                    { "C19", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 19", "Trống", "Ô tô lớn" },
                    { "C20", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 20", "Trống", "Ô tô lớn" }
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
                table: "Customers",
                columns: new[] { "CustomerId", "AccountId", "FullName", "Gender", "IsDeleted", "PhoneNumber" },
                values: new object[,]
                {
                    { "CUS001", "ACC101", "Khách hàng 1", "Male", false, "0916007224" },
                    { "CUS002", "ACC102", "Khách hàng 2", "Female", false, "0956002533" },
                    { "CUS003", "ACC103", "Khách hàng 3", "Male", false, "0924936291" },
                    { "CUS004", "ACC104", "Khách hàng 4", "Female", false, "0944842639" },
                    { "CUS005", "ACC105", "Khách hàng 5", "Male", false, "0922135024" },
                    { "CUS006", "ACC106", "Khách hàng 6", "Female", false, "0942297855" },
                    { "CUS007", "ACC107", "Khách hàng 7", "Male", false, "0962242734" },
                    { "CUS008", "ACC108", "Khách hàng 8", "Female", false, "0977309445" },
                    { "CUS009", "ACC109", "Khách hàng 9", "Male", false, "0971771055" },
                    { "CUS010", "ACC110", "Khách hàng 10", "Female", false, "0979370095" },
                    { "CUS011", "ACC111", "Khách hàng 11", "Male", false, "0974041964" },
                    { "CUS012", "ACC112", "Khách hàng 12", "Female", false, "0935473645" },
                    { "CUS013", "ACC113", "Khách hàng 13", "Male", false, "0925809900" },
                    { "CUS014", "ACC114", "Khách hàng 14", "Female", false, "0965425006" },
                    { "CUS015", "ACC115", "Khách hàng 15", "Male", false, "0912709318" },
                    { "CUS016", "ACC116", "Khách hàng 16", "Female", false, "0947626844" },
                    { "CUS017", "ACC117", "Khách hàng 17", "Male", false, "0983769379" },
                    { "CUS018", "ACC118", "Khách hàng 18", "Female", false, "0975753067" },
                    { "CUS019", "ACC119", "Khách hàng 19", "Male", false, "0949517349" },
                    { "CUS020", "ACC120", "Khách hàng 20", "Female", false, "0968720907" },
                    { "CUS021", "ACC121", "Khách hàng 21", "Male", false, "0932591325" },
                    { "CUS022", "ACC122", "Khách hàng 22", "Female", false, "0916346049" },
                    { "CUS023", "ACC123", "Khách hàng 23", "Male", false, "0950975868" },
                    { "CUS024", "ACC124", "Khách hàng 24", "Female", false, "0994178734" },
                    { "CUS025", "ACC125", "Khách hàng 25", "Male", false, "0920023134" },
                    { "CUS026", "ACC126", "Khách hàng 26", "Female", false, "0967504954" },
                    { "CUS027", "ACC127", "Khách hàng 27", "Male", false, "0920255298" },
                    { "CUS028", "ACC128", "Khách hàng 28", "Female", false, "0943988409" },
                    { "CUS029", "ACC129", "Khách hàng 29", "Male", false, "0999549599" },
                    { "CUS030", "ACC130", "Khách hàng 30", "Female", false, "0929349613" }
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
                    { "TKT0001", new DateTime(2026, 4, 1, 15, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 16, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A22", "Đã ra", "43A-700.89", "Xe máy" },
                    { "TKT0002", new DateTime(2026, 4, 1, 10, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 18, 11, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B29", "Đã ra", "43B-258.49", "Ô tô nhỏ" },
                    { "TKT0003", new DateTime(2026, 4, 1, 16, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 17, 29, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "43A-555.37", "Xe máy" },
                    { "TKT0004", new DateTime(2026, 4, 1, 10, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 11, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-583.79", "Xe máy" },
                    { "TKT0005", new DateTime(2026, 4, 1, 18, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 22, 40, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C17", "Đã ra", "43B-277.64", "Ô tô lớn" },
                    { "TKT0006", new DateTime(2026, 4, 1, 7, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 11, 2, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "43A-247.89", "Xe máy" },
                    { "TKT0007", new DateTime(2026, 4, 1, 9, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 13, 33, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C03", "Đã ra", "43B-655.70", "Ô tô lớn" },
                    { "TKT0008", new DateTime(2026, 4, 1, 18, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 3, 57, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "43B-953.18", "Ô tô nhỏ" },
                    { "TKT0009", new DateTime(2026, 4, 1, 9, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 15, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "43A-572.26", "Xe máy" },
                    { "TKT0010", new DateTime(2026, 4, 1, 9, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 19, 46, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "43B-690.84", "Ô tô nhỏ" },
                    { "TKT0011", new DateTime(2026, 4, 1, 7, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 9, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "43A-532.50", "Xe máy" },
                    { "TKT0012", new DateTime(2026, 4, 1, 7, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 14, 0, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "43A-347.44", "Xe máy" },
                    { "TKT0013", new DateTime(2026, 4, 1, 6, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 15, 53, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-177.66", "Xe máy" },
                    { "TKT0014", new DateTime(2026, 4, 2, 18, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 3, 20, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B50", "Đã ra", "43B-898.88", "Ô tô nhỏ" },
                    { "TKT0015", new DateTime(2026, 4, 2, 10, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 11, 12, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "43A-304.14", "Xe máy" },
                    { "TKT0016", new DateTime(2026, 4, 2, 12, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 19, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-283.61", "Xe máy" },
                    { "TKT0017", new DateTime(2026, 4, 2, 17, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 19, 30, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "43A-553.54", "Xe máy" },
                    { "TKT0018", new DateTime(2026, 4, 2, 11, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 19, 26, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "43A-538.87", "Xe máy" },
                    { "TKT0019", new DateTime(2026, 4, 2, 8, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 14, 45, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "43A-365.11", "Xe máy" },
                    { "TKT0020", new DateTime(2026, 4, 2, 10, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 19, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-777.64", "Xe máy" },
                    { "TKT0021", new DateTime(2026, 4, 2, 16, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 1, 58, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C13", "Đã ra", "43B-350.12", "Ô tô lớn" },
                    { "TKT0022", new DateTime(2026, 4, 2, 19, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 3, 29, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C08", "Đã ra", "43B-902.56", "Ô tô lớn" },
                    { "TKT0023", new DateTime(2026, 4, 2, 15, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 16, 57, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C02", "Đã ra", "43B-857.24", "Ô tô lớn" },
                    { "TKT0024", new DateTime(2026, 4, 3, 16, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 18, 21, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C17", "Đã ra", "43B-279.16", "Ô tô lớn" },
                    { "TKT0025", new DateTime(2026, 4, 3, 17, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 20, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "43A-891.87", "Xe máy" },
                    { "TKT0026", new DateTime(2026, 4, 3, 18, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 4, 28, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "43A-865.27", "Xe máy" },
                    { "TKT0027", new DateTime(2026, 4, 3, 15, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 17, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A09", "Đã ra", "43A-223.45", "Xe máy" },
                    { "TKT0028", new DateTime(2026, 4, 3, 18, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 1, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A36", "Đã ra", "43A-305.67", "Xe máy" },
                    { "TKT0029", new DateTime(2026, 4, 3, 17, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 21, 15, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B07", "Đã ra", "43B-172.28", "Ô tô nhỏ" },
                    { "TKT0030", new DateTime(2026, 4, 3, 16, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 19, 19, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B43", "Đã ra", "43B-249.11", "Ô tô nhỏ" },
                    { "TKT0031", new DateTime(2026, 4, 3, 13, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 17, 57, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-742.48", "Ô tô lớn" },
                    { "TKT0032", new DateTime(2026, 4, 3, 16, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 19, 54, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B15", "Đã ra", "43B-143.80", "Ô tô nhỏ" },
                    { "TKT0033", new DateTime(2026, 4, 3, 13, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 16, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-752.97", "Xe máy" },
                    { "TKT0034", new DateTime(2026, 4, 3, 18, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 22, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "43A-356.90", "Xe máy" },
                    { "TKT0035", new DateTime(2026, 4, 3, 8, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 18, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-511.18", "Ô tô nhỏ" },
                    { "TKT0036", new DateTime(2026, 4, 3, 7, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 11, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "43A-921.45", "Xe máy" },
                    { "TKT0037", new DateTime(2026, 4, 3, 13, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 16, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "43A-880.35", "Xe máy" },
                    { "TKT0038", new DateTime(2026, 4, 3, 13, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 19, 38, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "43A-683.48", "Xe máy" },
                    { "TKT0039", new DateTime(2026, 4, 4, 18, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 3, 54, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B41", "Đã ra", "43B-604.18", "Ô tô nhỏ" },
                    { "TKT0040", new DateTime(2026, 4, 4, 12, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 20, 48, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-545.38", "Ô tô lớn" },
                    { "TKT0041", new DateTime(2026, 4, 4, 8, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 18, 27, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B36", "Đã ra", "43B-748.32", "Ô tô nhỏ" },
                    { "TKT0042", new DateTime(2026, 4, 4, 12, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 19, 20, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "43B-339.62", "Ô tô nhỏ" },
                    { "TKT0043", new DateTime(2026, 4, 4, 18, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 1, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "43A-318.29", "Xe máy" },
                    { "TKT0044", new DateTime(2026, 4, 4, 17, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 22, 56, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C07", "Đã ra", "43B-143.70", "Ô tô lớn" },
                    { "TKT0045", new DateTime(2026, 4, 4, 10, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 15, 25, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "43A-965.31", "Xe máy" },
                    { "TKT0046", new DateTime(2026, 4, 4, 19, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 5, 33, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-819.11", "Ô tô lớn" },
                    { "TKT0047", new DateTime(2026, 4, 5, 18, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 1, 23, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C10", "Đã ra", "43B-850.65", "Ô tô lớn" },
                    { "TKT0048", new DateTime(2026, 4, 5, 18, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 0, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-196.37", "Xe máy" },
                    { "TKT0049", new DateTime(2026, 4, 5, 9, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 13, 13, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A41", "Đã ra", "43A-799.41", "Xe máy" },
                    { "TKT0050", new DateTime(2026, 4, 5, 14, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 18, 46, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C03", "Đã ra", "43B-883.59", "Ô tô lớn" },
                    { "TKT0051", new DateTime(2026, 4, 5, 19, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 4, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-168.88", "Xe máy" },
                    { "TKT0052", new DateTime(2026, 4, 6, 15, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 18, 37, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B17", "Đã ra", "43B-986.69", "Ô tô nhỏ" },
                    { "TKT0053", new DateTime(2026, 4, 6, 13, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 19, 57, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A24", "Đã ra", "43A-934.22", "Xe máy" },
                    { "TKT0054", new DateTime(2026, 4, 6, 13, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 18, 31, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "43A-576.75", "Xe máy" },
                    { "TKT0055", new DateTime(2026, 4, 6, 12, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 19, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A20", "Đã ra", "43A-504.45", "Xe máy" },
                    { "TKT0056", new DateTime(2026, 4, 6, 19, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 21, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-862.27", "Xe máy" },
                    { "TKT0057", new DateTime(2026, 4, 6, 13, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 16, 46, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B31", "Đã ra", "43B-307.30", "Ô tô nhỏ" },
                    { "TKT0058", new DateTime(2026, 4, 7, 12, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 21, 52, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B47", "Đã ra", "43B-626.93", "Ô tô nhỏ" },
                    { "TKT0059", new DateTime(2026, 4, 7, 17, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 23, 18, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "43A-247.40", "Xe máy" },
                    { "TKT0060", new DateTime(2026, 4, 7, 8, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 14, 26, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-269.17", "Ô tô lớn" },
                    { "TKT0061", new DateTime(2026, 4, 7, 12, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 15, 38, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "43A-179.15", "Xe máy" },
                    { "TKT0062", new DateTime(2026, 4, 7, 10, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 11, 44, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A09", "Đã ra", "43A-524.38", "Xe máy" },
                    { "TKT0063", new DateTime(2026, 4, 7, 6, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 9, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "43A-566.78", "Xe máy" },
                    { "TKT0064", new DateTime(2026, 4, 7, 18, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 3, 35, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-462.77", "Xe máy" },
                    { "TKT0065", new DateTime(2026, 4, 7, 13, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 17, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "43A-757.79", "Xe máy" },
                    { "TKT0066", new DateTime(2026, 4, 7, 6, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 8, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "43A-644.90", "Xe máy" },
                    { "TKT0067", new DateTime(2026, 4, 7, 17, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 0, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "43A-247.82", "Xe máy" },
                    { "TKT0068", new DateTime(2026, 4, 7, 8, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 15, 35, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C09", "Đã ra", "43B-144.38", "Ô tô lớn" },
                    { "TKT0069", new DateTime(2026, 4, 7, 14, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 16, 35, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C10", "Đã ra", "43B-471.10", "Ô tô lớn" },
                    { "TKT0070", new DateTime(2026, 4, 7, 14, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 21, 25, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-265.61", "Ô tô lớn" },
                    { "TKT0071", new DateTime(2026, 4, 7, 12, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 14, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "43A-739.87", "Xe máy" },
                    { "TKT0072", new DateTime(2026, 4, 8, 14, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 23, 49, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A08", "Đã ra", "43A-687.61", "Xe máy" },
                    { "TKT0073", new DateTime(2026, 4, 8, 11, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "43A-819.47", "Xe máy" },
                    { "TKT0074", new DateTime(2026, 4, 8, 12, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 50, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B36", "Đã ra", "43B-962.24", "Ô tô nhỏ" },
                    { "TKT0075", new DateTime(2026, 4, 8, 10, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 26, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "43A-578.54", "Xe máy" },
                    { "TKT0076", new DateTime(2026, 4, 8, 11, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 14, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C14", "Đã ra", "43B-834.16", "Ô tô lớn" },
                    { "TKT0077", new DateTime(2026, 4, 8, 11, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 34, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "43B-778.70", "Ô tô nhỏ" },
                    { "TKT0078", new DateTime(2026, 4, 8, 18, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 2, 37, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C10", "Đã ra", "43B-857.17", "Ô tô lớn" },
                    { "TKT0079", new DateTime(2026, 4, 8, 14, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 0, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "43A-553.22", "Xe máy" },
                    { "TKT0080", new DateTime(2026, 4, 9, 17, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 0, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "43A-814.38", "Xe máy" },
                    { "TKT0081", new DateTime(2026, 4, 9, 11, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 15, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-734.74", "Xe máy" },
                    { "TKT0082", new DateTime(2026, 4, 9, 7, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 9, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "43A-779.68", "Xe máy" },
                    { "TKT0083", new DateTime(2026, 4, 9, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 18, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "43A-874.52", "Xe máy" },
                    { "TKT0084", new DateTime(2026, 4, 9, 10, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 19, 44, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-143.21", "Xe máy" },
                    { "TKT0085", new DateTime(2026, 4, 9, 17, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 19, 11, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "43B-349.36", "Ô tô nhỏ" },
                    { "TKT0086", new DateTime(2026, 4, 9, 15, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 20, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A50", "Đã ra", "43A-417.28", "Xe máy" },
                    { "TKT0087", new DateTime(2026, 4, 9, 6, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 13, 54, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C13", "Đã ra", "43B-870.13", "Ô tô lớn" },
                    { "TKT0088", new DateTime(2026, 4, 9, 9, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 12, 31, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-376.19", "Ô tô lớn" },
                    { "TKT0089", new DateTime(2026, 4, 9, 7, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 13, 54, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "43A-211.90", "Xe máy" },
                    { "TKT0090", new DateTime(2026, 4, 9, 17, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 22, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "43A-911.41", "Xe máy" },
                    { "TKT0091", new DateTime(2026, 4, 9, 12, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 15, 30, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-162.19", "Xe máy" },
                    { "TKT0092", new DateTime(2026, 4, 9, 13, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 21, 27, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-957.14", "Xe máy" },
                    { "TKT0093", new DateTime(2026, 4, 9, 13, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 16, 48, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-866.54", "Ô tô lớn" },
                    { "TKT0094", new DateTime(2026, 4, 10, 19, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 2, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-679.22", "Xe máy" },
                    { "TKT0095", new DateTime(2026, 4, 10, 14, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 16, 47, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-217.85", "Ô tô lớn" },
                    { "TKT0096", new DateTime(2026, 4, 10, 9, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 12, 34, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-852.74", "Ô tô lớn" },
                    { "TKT0097", new DateTime(2026, 4, 10, 12, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 20, 53, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A41", "Đã ra", "43A-433.33", "Xe máy" },
                    { "TKT0098", new DateTime(2026, 4, 10, 6, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 8, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "43A-839.91", "Xe máy" },
                    { "TKT0099", new DateTime(2026, 4, 11, 10, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 18, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-566.97", "Xe máy" },
                    { "TKT0100", new DateTime(2026, 4, 11, 9, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 16, 39, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B23", "Đã ra", "43B-884.96", "Ô tô nhỏ" },
                    { "TKT0101", new DateTime(2026, 4, 11, 9, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 16, 8, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C17", "Đã ra", "43B-887.37", "Ô tô lớn" },
                    { "TKT0102", new DateTime(2026, 4, 11, 17, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 2, 40, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "43B-308.28", "Ô tô nhỏ" },
                    { "TKT0103", new DateTime(2026, 4, 11, 18, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 1, 48, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B34", "Đã ra", "43B-543.70", "Ô tô nhỏ" },
                    { "TKT0104", new DateTime(2026, 4, 11, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 14, 12, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C15", "Đã ra", "43B-723.98", "Ô tô lớn" },
                    { "TKT0105", new DateTime(2026, 4, 11, 11, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 20, 25, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B06", "Đã ra", "43B-387.92", "Ô tô nhỏ" },
                    { "TKT0106", new DateTime(2026, 4, 11, 6, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 10, 34, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C08", "Đã ra", "43B-218.75", "Ô tô lớn" },
                    { "TKT0107", new DateTime(2026, 4, 11, 14, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 18, 49, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-141.35", "Ô tô lớn" },
                    { "TKT0108", new DateTime(2026, 4, 11, 8, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 14, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A29", "Đã ra", "43A-766.48", "Xe máy" },
                    { "TKT0109", new DateTime(2026, 4, 11, 16, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 18, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "43A-203.92", "Xe máy" },
                    { "TKT0110", new DateTime(2026, 4, 11, 17, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 18, 42, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "43B-694.95", "Ô tô nhỏ" },
                    { "TKT0111", new DateTime(2026, 4, 11, 14, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 18, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "43A-790.41", "Xe máy" },
                    { "TKT0112", new DateTime(2026, 4, 11, 17, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 21, 1, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-529.60", "Ô tô lớn" },
                    { "TKT0113", new DateTime(2026, 4, 12, 12, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 19, 25, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "43A-472.77", "Xe máy" },
                    { "TKT0114", new DateTime(2026, 4, 12, 17, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 23, 1, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B25", "Đã ra", "43B-530.51", "Ô tô nhỏ" },
                    { "TKT0115", new DateTime(2026, 4, 12, 6, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 16, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A25", "Đã ra", "43A-664.33", "Xe máy" },
                    { "TKT0116", new DateTime(2026, 4, 12, 8, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 18, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-603.37", "Xe máy" },
                    { "TKT0117", new DateTime(2026, 4, 12, 19, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 22, 38, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B26", "Đã ra", "43B-295.18", "Ô tô nhỏ" },
                    { "TKT0118", new DateTime(2026, 4, 13, 18, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 0, 30, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "43B-516.68", "Ô tô nhỏ" },
                    { "TKT0119", new DateTime(2026, 4, 13, 10, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 14, 57, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-782.27", "Xe máy" },
                    { "TKT0120", new DateTime(2026, 4, 13, 13, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 16, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "43A-988.39", "Xe máy" },
                    { "TKT0121", new DateTime(2026, 4, 13, 12, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 19, 48, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-912.16", "Xe máy" },
                    { "TKT0122", new DateTime(2026, 4, 13, 17, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 19, 57, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B31", "Đã ra", "43B-758.57", "Ô tô nhỏ" },
                    { "TKT0123", new DateTime(2026, 4, 13, 17, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 23, 5, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-529.56", "Ô tô lớn" },
                    { "TKT0124", new DateTime(2026, 4, 13, 16, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 21, 20, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C01", "Đã ra", "43B-165.26", "Ô tô lớn" },
                    { "TKT0125", new DateTime(2026, 4, 13, 9, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 17, 27, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "43A-988.60", "Xe máy" },
                    { "TKT0126", new DateTime(2026, 4, 13, 16, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 18, 29, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-538.57", "Ô tô nhỏ" },
                    { "TKT0127", new DateTime(2026, 4, 13, 7, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 13, 5, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-964.91", "Xe máy" },
                    { "TKT0128", new DateTime(2026, 4, 13, 11, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 17, 8, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B03", "Đã ra", "43B-209.40", "Ô tô nhỏ" },
                    { "TKT0129", new DateTime(2026, 4, 13, 14, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 18, 30, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "43B-844.62", "Ô tô nhỏ" },
                    { "TKT0130", new DateTime(2026, 4, 13, 13, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 17, 26, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-647.14", "Ô tô lớn" },
                    { "TKT0131", new DateTime(2026, 4, 14, 18, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 19, 45, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B10", "Đã ra", "43B-934.24", "Ô tô nhỏ" },
                    { "TKT0132", new DateTime(2026, 4, 14, 6, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 10, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B34", "Đã ra", "43B-761.86", "Ô tô nhỏ" },
                    { "TKT0133", new DateTime(2026, 4, 14, 18, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 0, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "43A-207.91", "Xe máy" },
                    { "TKT0134", new DateTime(2026, 4, 14, 12, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 19, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "43A-968.92", "Xe máy" },
                    { "TKT0135", new DateTime(2026, 4, 14, 17, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 1, 40, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-391.95", "Ô tô lớn" },
                    { "TKT0136", new DateTime(2026, 4, 14, 9, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 10, 29, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-599.92", "Xe máy" },
                    { "TKT0137", new DateTime(2026, 4, 14, 10, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 12, 54, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B43", "Đã ra", "43B-297.61", "Ô tô nhỏ" },
                    { "TKT0138", new DateTime(2026, 4, 14, 11, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 17, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "43A-297.44", "Xe máy" },
                    { "TKT0139", new DateTime(2026, 4, 14, 12, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 21, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-891.16", "Xe máy" },
                    { "TKT0140", new DateTime(2026, 4, 14, 18, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 3, 28, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C11", "Đã ra", "43B-485.24", "Ô tô lớn" },
                    { "TKT0141", new DateTime(2026, 4, 14, 16, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 17, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-407.67", "Xe máy" },
                    { "TKT0142", new DateTime(2026, 4, 14, 10, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 19, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "43A-977.75", "Xe máy" },
                    { "TKT0143", new DateTime(2026, 4, 14, 15, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 21, 10, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B09", "Đã ra", "43B-732.11", "Ô tô nhỏ" },
                    { "TKT0144", new DateTime(2026, 4, 15, 6, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 9, 49, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "43A-571.62", "Xe máy" },
                    { "TKT0145", new DateTime(2026, 4, 15, 18, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 2, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "43A-479.26", "Xe máy" },
                    { "TKT0146", new DateTime(2026, 4, 15, 19, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 21, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "43B-501.50", "Ô tô nhỏ" },
                    { "TKT0147", new DateTime(2026, 4, 15, 12, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 21, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-899.88", "Ô tô nhỏ" },
                    { "TKT0148", new DateTime(2026, 4, 15, 13, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 16, 6, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-707.79", "Ô tô lớn" },
                    { "TKT0149", new DateTime(2026, 4, 15, 12, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 21, 33, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-683.75", "Ô tô lớn" },
                    { "TKT0150", new DateTime(2026, 4, 15, 7, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 10, 30, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B35", "Đã ra", "43B-277.53", "Ô tô nhỏ" },
                    { "TKT0151", new DateTime(2026, 4, 15, 9, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 12, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "43A-278.75", "Xe máy" },
                    { "TKT0152", new DateTime(2026, 4, 16, 16, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 21, 46, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C08", "Đã ra", "43B-734.25", "Ô tô lớn" },
                    { "TKT0153", new DateTime(2026, 4, 16, 17, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 21, 38, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B35", "Đã ra", "43B-147.59", "Ô tô nhỏ" },
                    { "TKT0154", new DateTime(2026, 4, 16, 13, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 16, 5, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-592.56", "Ô tô lớn" },
                    { "TKT0155", new DateTime(2026, 4, 16, 12, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 13, 42, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "43A-294.89", "Xe máy" },
                    { "TKT0156", new DateTime(2026, 4, 16, 8, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 14, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A08", "Đã ra", "43A-512.11", "Xe máy" },
                    { "TKT0157", new DateTime(2026, 4, 16, 12, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 16, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-365.81", "Xe máy" },
                    { "TKT0158", new DateTime(2026, 4, 17, 8, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 15, 34, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B08", "Đã ra", "43B-501.17", "Ô tô nhỏ" },
                    { "TKT0159", new DateTime(2026, 4, 17, 10, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 13, 35, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A36", "Đã ra", "43A-762.96", "Xe máy" },
                    { "TKT0160", new DateTime(2026, 4, 17, 6, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 14, 31, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B30", "Đã ra", "43B-359.57", "Ô tô nhỏ" },
                    { "TKT0161", new DateTime(2026, 4, 17, 10, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 17, 22, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B19", "Đã ra", "43B-344.76", "Ô tô nhỏ" },
                    { "TKT0162", new DateTime(2026, 4, 17, 7, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 16, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-340.96", "Xe máy" },
                    { "TKT0163", new DateTime(2026, 4, 17, 15, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 22, 39, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "43A-731.27", "Xe máy" },
                    { "TKT0164", new DateTime(2026, 4, 17, 12, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 15, 39, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "43A-191.51", "Xe máy" },
                    { "TKT0165", new DateTime(2026, 4, 18, 16, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 21, 10, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C01", "Đã ra", "43B-868.34", "Ô tô lớn" },
                    { "TKT0166", new DateTime(2026, 4, 18, 6, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 11, 56, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "43B-363.43", "Ô tô nhỏ" },
                    { "TKT0167", new DateTime(2026, 4, 18, 6, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 9, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "43A-204.14", "Xe máy" },
                    { "TKT0168", new DateTime(2026, 4, 18, 15, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 21, 47, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C09", "Đã ra", "43B-134.83", "Ô tô lớn" },
                    { "TKT0169", new DateTime(2026, 4, 18, 8, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 12, 21, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B05", "Đã ra", "43B-431.22", "Ô tô nhỏ" },
                    { "TKT0170", new DateTime(2026, 4, 18, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 16, 48, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-284.19", "Ô tô lớn" },
                    { "TKT0171", new DateTime(2026, 4, 18, 8, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 16, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-512.76", "Xe máy" },
                    { "TKT0172", new DateTime(2026, 4, 18, 18, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 1, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B49", "Đã ra", "43B-479.20", "Ô tô nhỏ" },
                    { "TKT0173", new DateTime(2026, 4, 18, 8, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 17, 57, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B47", "Đã ra", "43B-512.36", "Ô tô nhỏ" },
                    { "TKT0174", new DateTime(2026, 4, 18, 15, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 20, 19, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "43B-724.42", "Ô tô nhỏ" },
                    { "TKT0175", new DateTime(2026, 4, 18, 9, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 18, 26, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A32", "Đã ra", "43A-656.95", "Xe máy" },
                    { "TKT0176", new DateTime(2026, 4, 18, 19, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 0, 17, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C04", "Đã ra", "43B-339.62", "Ô tô lớn" },
                    { "TKT0177", new DateTime(2026, 4, 18, 6, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 15, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "43A-471.42", "Xe máy" },
                    { "TKT0178", new DateTime(2026, 4, 18, 9, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 11, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "43A-969.28", "Xe máy" },
                    { "TKT0179", new DateTime(2026, 4, 18, 14, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 15, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "43A-946.28", "Xe máy" },
                    { "TKT0180", new DateTime(2026, 4, 19, 8, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 16, 10, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B07", "Đã ra", "43B-325.93", "Ô tô nhỏ" },
                    { "TKT0181", new DateTime(2026, 4, 19, 8, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 10, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-464.72", "Xe máy" },
                    { "TKT0182", new DateTime(2026, 4, 19, 8, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 11, 8, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B47", "Đã ra", "43B-609.44", "Ô tô nhỏ" },
                    { "TKT0183", new DateTime(2026, 4, 19, 10, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 12, 6, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A09", "Đã ra", "43A-664.81", "Xe máy" },
                    { "TKT0184", new DateTime(2026, 4, 19, 10, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 12, 6, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B12", "Đã ra", "43B-779.80", "Ô tô nhỏ" },
                    { "TKT0185", new DateTime(2026, 4, 19, 14, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 15, 50, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B35", "Đã ra", "43B-752.15", "Ô tô nhỏ" },
                    { "TKT0186", new DateTime(2026, 4, 19, 17, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 2, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A20", "Đã ra", "43A-994.49", "Xe máy" },
                    { "TKT0187", new DateTime(2026, 4, 19, 9, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 17, 22, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B31", "Đã ra", "43B-173.13", "Ô tô nhỏ" },
                    { "TKT0188", new DateTime(2026, 4, 19, 19, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 0, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "43A-139.95", "Xe máy" },
                    { "TKT0189", new DateTime(2026, 4, 19, 18, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 20, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A50", "Đã ra", "43A-657.17", "Xe máy" },
                    { "TKT0190", new DateTime(2026, 4, 20, 16, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 2, 7, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "43B-381.73", "Ô tô nhỏ" },
                    { "TKT0191", new DateTime(2026, 4, 20, 18, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 19, 46, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C01", "Đã ra", "43B-690.51", "Ô tô lớn" },
                    { "TKT0192", new DateTime(2026, 4, 20, 11, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 16, 39, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "43A-364.27", "Xe máy" },
                    { "TKT0193", new DateTime(2026, 4, 20, 7, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 14, 53, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B17", "Đã ra", "43B-409.83", "Ô tô nhỏ" },
                    { "TKT0194", new DateTime(2026, 4, 20, 18, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 2, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-472.94", "Xe máy" },
                    { "TKT0195", new DateTime(2026, 4, 20, 15, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 1, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "43A-698.10", "Xe máy" },
                    { "TKT0196", new DateTime(2026, 4, 20, 16, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 17, 40, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C08", "Đã ra", "43B-351.30", "Ô tô lớn" },
                    { "TKT0197", new DateTime(2026, 4, 20, 16, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 0, 39, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B23", "Đã ra", "43B-191.81", "Ô tô nhỏ" },
                    { "TKT0198", new DateTime(2026, 4, 20, 13, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 15, 59, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-593.83", "Xe máy" },
                    { "TKT0199", new DateTime(2026, 4, 20, 18, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 3, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-923.54", "Xe máy" },
                    { "TKT0200", new DateTime(2026, 4, 20, 12, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 16, 41, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B48", "Đã ra", "43B-852.18", "Ô tô nhỏ" },
                    { "TKT0201", new DateTime(2026, 4, 20, 9, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 17, 22, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "43A-758.33", "Xe máy" },
                    { "TKT0202", new DateTime(2026, 4, 20, 7, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 8, 23, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B05", "Đã ra", "43B-648.51", "Ô tô nhỏ" },
                    { "TKT0203", new DateTime(2026, 4, 20, 15, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 17, 33, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-911.13", "Ô tô lớn" },
                    { "TKT0204", new DateTime(2026, 4, 20, 8, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 16, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "43A-210.61", "Xe máy" },
                    { "TKT0205", new DateTime(2026, 4, 21, 8, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 15, 12, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "43A-295.56", "Xe máy" },
                    { "TKT0206", new DateTime(2026, 4, 21, 16, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 1, 41, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B35", "Đã ra", "43B-470.36", "Ô tô nhỏ" },
                    { "TKT0207", new DateTime(2026, 4, 21, 10, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 13, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A41", "Đã ra", "43A-997.38", "Xe máy" },
                    { "TKT0208", new DateTime(2026, 4, 21, 18, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 1, 47, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "43A-478.46", "Xe máy" },
                    { "TKT0209", new DateTime(2026, 4, 21, 9, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 13, 23, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-995.95", "Ô tô lớn" },
                    { "TKT0210", new DateTime(2026, 4, 21, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 14, 52, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C15", "Đã ra", "43B-403.38", "Ô tô lớn" },
                    { "TKT0211", new DateTime(2026, 4, 21, 18, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 1, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "43A-857.54", "Xe máy" },
                    { "TKT0212", new DateTime(2026, 4, 21, 18, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 1, 22, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "43A-620.38", "Xe máy" },
                    { "TKT0213", new DateTime(2026, 4, 21, 10, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 12, 23, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C17", "Đã ra", "43B-165.72", "Ô tô lớn" },
                    { "TKT0214", new DateTime(2026, 4, 22, 16, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 19, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "43A-855.40", "Xe máy" },
                    { "TKT0215", new DateTime(2026, 4, 22, 18, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 3, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A25", "Đã ra", "43A-649.55", "Xe máy" },
                    { "TKT0216", new DateTime(2026, 4, 22, 9, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 12, 29, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C11", "Đã ra", "43B-494.90", "Ô tô lớn" },
                    { "TKT0217", new DateTime(2026, 4, 22, 8, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 10, 23, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-159.83", "Ô tô lớn" },
                    { "TKT0218", new DateTime(2026, 4, 22, 12, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 21, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A50", "Đã ra", "43A-452.72", "Xe máy" },
                    { "TKT0219", new DateTime(2026, 4, 22, 14, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 18, 45, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B26", "Đã ra", "43B-618.55", "Ô tô nhỏ" },
                    { "TKT0220", new DateTime(2026, 4, 23, 15, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 19, 3, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C07", "Đã ra", "43B-678.46", "Ô tô lớn" },
                    { "TKT0221", new DateTime(2026, 4, 23, 13, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 23, 22, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C09", "Đã ra", "43B-111.90", "Ô tô lớn" },
                    { "TKT0222", new DateTime(2026, 4, 23, 6, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 10, 32, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B39", "Đã ra", "43B-715.85", "Ô tô nhỏ" },
                    { "TKT0223", new DateTime(2026, 4, 23, 13, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 20, 47, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-685.67", "Xe máy" },
                    { "TKT0224", new DateTime(2026, 4, 23, 15, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 0, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A29", "Đã ra", "43A-694.38", "Xe máy" },
                    { "TKT0225", new DateTime(2026, 4, 23, 16, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 19, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "43A-710.46", "Xe máy" },
                    { "TKT0226", new DateTime(2026, 4, 23, 17, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 22, 19, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B12", "Đã ra", "43B-640.92", "Ô tô nhỏ" },
                    { "TKT0227", new DateTime(2026, 4, 23, 14, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 17, 57, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "43A-857.80", "Xe máy" },
                    { "TKT0228", new DateTime(2026, 4, 23, 13, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 17, 48, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-152.32", "Ô tô nhỏ" },
                    { "TKT0229", new DateTime(2026, 4, 23, 17, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 3, 5, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B13", "Đã ra", "43B-469.76", "Ô tô nhỏ" },
                    { "TKT0230", new DateTime(2026, 4, 23, 13, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 15, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-246.18", "Xe máy" },
                    { "TKT0231", new DateTime(2026, 4, 23, 18, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 2, 29, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B06", "Đã ra", "43B-827.60", "Ô tô nhỏ" },
                    { "TKT0232", new DateTime(2026, 4, 24, 14, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 20, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A05", "Đã ra", "43A-948.21", "Xe máy" },
                    { "TKT0233", new DateTime(2026, 4, 24, 17, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 22, 6, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C10", "Đã ra", "43B-199.98", "Ô tô lớn" },
                    { "TKT0234", new DateTime(2026, 4, 24, 6, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 10, 26, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "43A-966.19", "Xe máy" },
                    { "TKT0235", new DateTime(2026, 4, 24, 7, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 11, 24, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "43B-301.83", "Ô tô nhỏ" },
                    { "TKT0236", new DateTime(2026, 4, 24, 9, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 18, 42, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B41", "Đã ra", "43B-141.70", "Ô tô nhỏ" },
                    { "TKT0237", new DateTime(2026, 4, 24, 15, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 18, 56, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B46", "Đã ra", "43B-548.31", "Ô tô nhỏ" },
                    { "TKT0238", new DateTime(2026, 4, 24, 13, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 22, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "43A-871.78", "Xe máy" },
                    { "TKT0239", new DateTime(2026, 4, 25, 15, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 18, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-620.40", "Xe máy" },
                    { "TKT0240", new DateTime(2026, 4, 25, 7, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 14, 7, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-911.96", "Ô tô lớn" },
                    { "TKT0241", new DateTime(2026, 4, 25, 12, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 14, 38, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A22", "Đã ra", "43A-838.20", "Xe máy" },
                    { "TKT0242", new DateTime(2026, 4, 25, 10, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 15, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B03", "Đã ra", "43B-917.97", "Ô tô nhỏ" },
                    { "TKT0243", new DateTime(2026, 4, 25, 12, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 19, 38, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B45", "Đã ra", "43B-313.94", "Ô tô nhỏ" },
                    { "TKT0244", new DateTime(2026, 4, 25, 12, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 18, 53, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "43A-580.36", "Xe máy" },
                    { "TKT0245", new DateTime(2026, 4, 25, 18, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 23, 28, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "43B-535.84", "Ô tô nhỏ" },
                    { "TKT0246", new DateTime(2026, 4, 25, 13, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 17, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-745.20", "Xe máy" },
                    { "TKT0247", new DateTime(2026, 4, 25, 8, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 13, 45, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "43B-868.77", "Ô tô nhỏ" },
                    { "TKT0248", new DateTime(2026, 4, 25, 7, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 10, 35, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-760.76", "Xe máy" },
                    { "TKT0249", new DateTime(2026, 4, 25, 12, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 21, 44, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-302.15", "Xe máy" },
                    { "TKT0250", new DateTime(2026, 4, 25, 19, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 23, 1, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-566.26", "Xe máy" },
                    { "TKT0251", new DateTime(2026, 4, 25, 15, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 18, 53, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B43", "Đã ra", "43B-176.48", "Ô tô nhỏ" },
                    { "TKT0252", new DateTime(2026, 4, 25, 7, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 15, 52, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-317.51", "Ô tô lớn" },
                    { "TKT0253", new DateTime(2026, 4, 25, 13, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 19, 16, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-457.80", "Ô tô lớn" },
                    { "TKT0254", new DateTime(2026, 4, 26, 14, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 20, 48, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A27", "Đã ra", "43A-417.42", "Xe máy" },
                    { "TKT0255", new DateTime(2026, 4, 26, 11, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 13, 6, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A03", "Đã ra", "43A-483.98", "Xe máy" },
                    { "TKT0256", new DateTime(2026, 4, 26, 8, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 16, 55, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C06", "Đã ra", "43B-638.18", "Ô tô lớn" },
                    { "TKT0257", new DateTime(2026, 4, 26, 12, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 15, 21, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-166.36", "Ô tô lớn" },
                    { "TKT0258", new DateTime(2026, 4, 26, 7, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 14, 18, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-805.95", "Xe máy" },
                    { "TKT0259", new DateTime(2026, 4, 26, 13, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 18, 59, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "43A-551.32", "Xe máy" },
                    { "TKT0260", new DateTime(2026, 4, 26, 6, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 8, 0, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "43A-404.77", "Xe máy" },
                    { "TKT0261", new DateTime(2026, 4, 26, 8, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 17, 42, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-553.60", "Xe máy" },
                    { "TKT0262", new DateTime(2026, 4, 26, 13, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 23, 44, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B19", "Đã ra", "43B-313.58", "Ô tô nhỏ" },
                    { "TKT0263", new DateTime(2026, 4, 26, 14, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 23, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A27", "Đã ra", "43A-917.79", "Xe máy" },
                    { "TKT0264", new DateTime(2026, 4, 27, 13, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 17, 44, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C06", "Đã ra", "43B-331.95", "Ô tô lớn" },
                    { "TKT0265", new DateTime(2026, 4, 27, 13, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 17, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "43A-393.33", "Xe máy" },
                    { "TKT0266", new DateTime(2026, 4, 27, 12, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 14, 56, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B44", "Đã ra", "43B-380.67", "Ô tô nhỏ" },
                    { "TKT0267", new DateTime(2026, 4, 27, 6, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 10, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "43A-312.41", "Xe máy" },
                    { "TKT0268", new DateTime(2026, 4, 27, 14, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 16, 23, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B33", "Đã ra", "43B-526.56", "Ô tô nhỏ" },
                    { "TKT0269", new DateTime(2026, 4, 27, 15, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 18, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "43A-595.72", "Xe máy" },
                    { "TKT0270", new DateTime(2026, 4, 27, 13, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 23, 30, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "43B-459.97", "Ô tô nhỏ" },
                    { "TKT0271", new DateTime(2026, 4, 28, 17, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 23, 1, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "43A-732.26", "Xe máy" },
                    { "TKT0272", new DateTime(2026, 4, 28, 13, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 18, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A32", "Đã ra", "43A-438.66", "Xe máy" },
                    { "TKT0273", new DateTime(2026, 4, 28, 10, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 19, 59, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-641.11", "Ô tô lớn" },
                    { "TKT0274", new DateTime(2026, 4, 28, 15, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 0, 43, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C08", "Đã ra", "43B-739.14", "Ô tô lớn" },
                    { "TKT0275", new DateTime(2026, 4, 28, 13, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 15, 0, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A37", "Đã ra", "43A-287.85", "Xe máy" },
                    { "TKT0276", new DateTime(2026, 4, 28, 8, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 14, 21, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B12", "Đã ra", "43B-784.36", "Ô tô nhỏ" },
                    { "TKT0277", new DateTime(2026, 4, 28, 12, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 14, 19, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C07", "Đã ra", "43B-148.37", "Ô tô lớn" },
                    { "TKT0278", new DateTime(2026, 4, 28, 15, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 21, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-239.90", "Xe máy" },
                    { "TKT0279", new DateTime(2026, 4, 28, 8, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 13, 34, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "43A-302.95", "Xe máy" },
                    { "TKT0280", new DateTime(2026, 4, 28, 16, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 23, 10, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-329.24", "Ô tô lớn" },
                    { "TKT0281", new DateTime(2026, 4, 29, 17, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 0, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "43B-616.20", "Ô tô nhỏ" },
                    { "TKT0282", new DateTime(2026, 4, 29, 16, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 0, 52, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "43B-538.90", "Ô tô nhỏ" },
                    { "TKT0283", new DateTime(2026, 4, 29, 15, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 19, 4, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C14", "Đã ra", "43B-767.94", "Ô tô lớn" },
                    { "TKT0284", new DateTime(2026, 4, 29, 10, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 17, 47, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C09", "Đã ra", "43B-536.62", "Ô tô lớn" },
                    { "TKT0285", new DateTime(2026, 4, 29, 19, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 2, 51, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-888.83", "Ô tô lớn" },
                    { "TKT0286", new DateTime(2026, 4, 29, 17, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 1, 28, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B22", "Đã ra", "43B-649.96", "Ô tô nhỏ" },
                    { "TKT0287", new DateTime(2026, 4, 29, 19, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 21, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-363.49", "Xe máy" },
                    { "TKT0288", new DateTime(2026, 4, 29, 8, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 17, 13, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-982.36", "Ô tô lớn" },
                    { "TKT0289", new DateTime(2026, 4, 29, 12, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 17, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-914.97", "Xe máy" },
                    { "TKT0290", new DateTime(2026, 4, 30, 11, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 40, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "43A-304.11", "Xe máy" },
                    { "TKT0291", new DateTime(2026, 4, 30, 18, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 21, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "43A-938.14", "Xe máy" },
                    { "TKT0292", new DateTime(2026, 4, 30, 18, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 1, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-745.11", "Xe máy" },
                    { "TKT0293", new DateTime(2026, 4, 30, 6, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 8, 12, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-654.24", "Ô tô lớn" },
                    { "TKT0294", new DateTime(2026, 4, 30, 12, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 8, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "43B-294.86", "Ô tô nhỏ" },
                    { "TKT0295", new DateTime(2026, 4, 30, 14, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "43A-375.17", "Xe máy" },
                    { "TKT0296", new DateTime(2026, 4, 30, 7, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 13, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-438.15", "Xe máy" },
                    { "TKT0297", new DateTime(2026, 4, 30, 13, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 15, 57, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B12", "Đã ra", "43B-767.50", "Ô tô nhỏ" },
                    { "TKT0298", new DateTime(2026, 4, 30, 10, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 19, 42, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C15", "Đã ra", "43B-103.62", "Ô tô lớn" },
                    { "TKT0299", new DateTime(2026, 4, 30, 14, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 16, 41, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-306.41", "Ô tô lớn" },
                    { "TKT0300", new DateTime(2026, 4, 30, 18, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 19, 27, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "43A-942.81", "Xe máy" },
                    { "TKT0301", new DateTime(2026, 4, 30, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 12, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "43A-443.68", "Xe máy" },
                    { "TKT0302", new DateTime(2026, 4, 30, 16, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 34, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-737.10", "Ô tô lớn" },
                    { "TKT0303", new DateTime(2026, 5, 1, 6, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 12, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "43A-913.71", "Xe máy" },
                    { "TKT0304", new DateTime(2026, 5, 1, 6, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 14, 25, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-364.87", "Xe máy" },
                    { "TKT0305", new DateTime(2026, 5, 1, 16, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 23, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "43A-231.84", "Xe máy" },
                    { "TKT0306", new DateTime(2026, 5, 1, 17, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 2, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "43A-587.53", "Xe máy" },
                    { "TKT0307", new DateTime(2026, 5, 1, 14, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 18, 10, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C10", "Đã ra", "43B-632.46", "Ô tô lớn" },
                    { "TKT0308", new DateTime(2026, 5, 1, 12, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 17, 16, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "43B-174.63", "Ô tô nhỏ" },
                    { "TKT0309", new DateTime(2026, 5, 1, 9, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 14, 5, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-108.83", "Ô tô lớn" },
                    { "TKT0310", new DateTime(2026, 5, 1, 19, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 3, 8, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B29", "Đã ra", "43B-439.58", "Ô tô nhỏ" },
                    { "TKT0311", new DateTime(2026, 5, 1, 8, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 10, 1, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C01", "Đã ra", "43B-368.96", "Ô tô lớn" },
                    { "TKT0312", new DateTime(2026, 5, 1, 14, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 21, 30, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C04", "Đã ra", "43B-128.83", "Ô tô lớn" },
                    { "TKT0313", new DateTime(2026, 5, 1, 11, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 17, 54, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "43A-670.83", "Xe máy" },
                    { "TKT0314", new DateTime(2026, 5, 1, 15, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 17, 34, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A32", "Đã ra", "43A-427.65", "Xe máy" },
                    { "TKT0315", new DateTime(2026, 5, 1, 14, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 19, 1, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "43B-438.54", "Ô tô nhỏ" },
                    { "TKT0316", new DateTime(2026, 5, 1, 15, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 22, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B07", "Đã ra", "43B-690.91", "Ô tô nhỏ" },
                    { "TKT0317", new DateTime(2026, 5, 2, 17, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 19, 25, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B03", "Đã ra", "43B-586.36", "Ô tô nhỏ" },
                    { "TKT0318", new DateTime(2026, 5, 2, 6, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 14, 45, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "43B-626.23", "Ô tô nhỏ" },
                    { "TKT0319", new DateTime(2026, 5, 2, 12, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 18, 27, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-357.61", "Ô tô lớn" },
                    { "TKT0320", new DateTime(2026, 5, 2, 7, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 15, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-517.74", "Xe máy" },
                    { "TKT0321", new DateTime(2026, 5, 2, 19, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 4, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "43A-329.36", "Xe máy" },
                    { "TKT0322", new DateTime(2026, 5, 2, 6, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 7, 52, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-800.86", "Ô tô lớn" },
                    { "TKT0323", new DateTime(2026, 5, 2, 16, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 22, 13, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "43B-995.24", "Ô tô nhỏ" },
                    { "TKT0324", new DateTime(2026, 5, 2, 15, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 22, 59, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A25", "Đã ra", "43A-274.91", "Xe máy" },
                    { "TKT0325", new DateTime(2026, 5, 2, 12, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 16, 13, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "43A-639.44", "Xe máy" },
                    { "TKT0326", new DateTime(2026, 5, 2, 19, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 4, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "43A-866.13", "Xe máy" },
                    { "TKT0327", new DateTime(2026, 5, 2, 9, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 16, 5, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "43A-604.29", "Xe máy" },
                    { "TKT0328", new DateTime(2026, 5, 2, 7, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 12, 5, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "43B-669.36", "Ô tô nhỏ" },
                    { "TKT0329", new DateTime(2026, 5, 2, 8, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 10, 44, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "43B-879.25", "Ô tô nhỏ" },
                    { "TKT0330", new DateTime(2026, 5, 2, 16, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 0, 22, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A22", "Đã ra", "43A-136.38", "Xe máy" },
                    { "TKT0331", new DateTime(2026, 5, 3, 17, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 0, 32, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-164.86", "Ô tô lớn" },
                    { "TKT0332", new DateTime(2026, 5, 3, 15, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 19, 22, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "43B-563.89", "Ô tô nhỏ" },
                    { "TKT0333", new DateTime(2026, 5, 3, 12, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 19, 54, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-192.30", "Xe máy" },
                    { "TKT0334", new DateTime(2026, 5, 3, 9, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 17, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "43A-709.91", "Xe máy" },
                    { "TKT0335", new DateTime(2026, 5, 3, 6, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 16, 18, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "43A-769.52", "Xe máy" },
                    { "TKT0336", new DateTime(2026, 5, 3, 12, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 16, 40, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "43A-127.51", "Xe máy" },
                    { "TKT0337", new DateTime(2026, 5, 4, 17, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 22, 1, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C15", "Đã ra", "43B-166.68", "Ô tô lớn" },
                    { "TKT0338", new DateTime(2026, 5, 4, 16, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 17, 30, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "43A-243.72", "Xe máy" },
                    { "TKT0339", new DateTime(2026, 5, 4, 9, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 14, 5, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "43A-334.93", "Xe máy" },
                    { "TKT0340", new DateTime(2026, 5, 4, 12, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 16, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-565.21", "Xe máy" },
                    { "TKT0341", new DateTime(2026, 5, 4, 11, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 19, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "43A-716.75", "Xe máy" },
                    { "TKT0342", new DateTime(2026, 5, 4, 13, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 19, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "43A-736.32", "Xe máy" },
                    { "TKT0343", new DateTime(2026, 5, 4, 18, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 23, 30, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "43B-885.36", "Ô tô nhỏ" },
                    { "TKT0344", new DateTime(2026, 5, 4, 18, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 21, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B31", "Đã ra", "43B-399.21", "Ô tô nhỏ" },
                    { "TKT0345", new DateTime(2026, 5, 4, 19, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 1, 44, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "43A-224.57", "Xe máy" },
                    { "TKT0346", new DateTime(2026, 5, 4, 17, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 2, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A22", "Đã ra", "43A-610.11", "Xe máy" },
                    { "TKT0347", new DateTime(2026, 5, 4, 14, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 18, 55, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C20", "Đã ra", "43B-983.62", "Ô tô lớn" },
                    { "TKT0348", new DateTime(2026, 5, 5, 11, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 17, 11, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-696.83", "Ô tô lớn" },
                    { "TKT0349", new DateTime(2026, 5, 5, 6, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 11, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "43A-305.78", "Xe máy" },
                    { "TKT0350", new DateTime(2026, 5, 5, 11, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 19, 1, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A23", "Đã ra", "43A-172.35", "Xe máy" },
                    { "TKT0351", new DateTime(2026, 5, 5, 7, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 15, 50, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-886.60", "Ô tô lớn" },
                    { "TKT0352", new DateTime(2026, 5, 5, 19, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 1, 55, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "43A-748.45", "Xe máy" },
                    { "TKT0353", new DateTime(2026, 5, 5, 19, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 23, 54, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C11", "Đã ra", "43B-359.60", "Ô tô lớn" },
                    { "TKT0354", new DateTime(2026, 5, 5, 19, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 23, 26, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "43B-566.84", "Ô tô nhỏ" },
                    { "TKT0355", new DateTime(2026, 5, 5, 13, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 22, 13, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "43A-282.37", "Xe máy" },
                    { "TKT0356", new DateTime(2026, 5, 6, 18, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 0, 44, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-224.71", "Ô tô lớn" },
                    { "TKT0357", new DateTime(2026, 5, 6, 11, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 19, 58, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-358.98", "Ô tô nhỏ" },
                    { "TKT0358", new DateTime(2026, 5, 6, 12, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 17, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-312.66", "Xe máy" },
                    { "TKT0359", new DateTime(2026, 5, 6, 7, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 14, 34, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A47", "Đã ra", "43A-183.63", "Xe máy" },
                    { "TKT0360", new DateTime(2026, 5, 6, 11, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 16, 12, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B46", "Đã ra", "43B-643.41", "Ô tô nhỏ" },
                    { "TKT0361", new DateTime(2026, 5, 6, 15, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 19, 59, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "43A-578.13", "Xe máy" },
                    { "TKT0362", new DateTime(2026, 5, 6, 16, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 0, 50, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C02", "Đã ra", "43B-541.33", "Ô tô lớn" },
                    { "TKT0363", new DateTime(2026, 5, 6, 6, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 9, 6, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B42", "Đã ra", "43B-576.25", "Ô tô nhỏ" },
                    { "TKT0364", new DateTime(2026, 5, 6, 8, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 12, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-546.51", "Ô tô nhỏ" },
                    { "TKT0365", new DateTime(2026, 5, 6, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 15, 54, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B43", "Đã ra", "43B-142.78", "Ô tô nhỏ" },
                    { "TKT0366", new DateTime(2026, 5, 6, 11, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 18, 18, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "43B-409.36", "Ô tô nhỏ" },
                    { "TKT0367", new DateTime(2026, 5, 6, 17, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 23, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "43A-217.79", "Xe máy" },
                    { "TKT0368", new DateTime(2026, 5, 6, 11, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 15, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "43A-696.82", "Xe máy" },
                    { "TKT0369", new DateTime(2026, 5, 7, 11, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 15, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A35", "Đã ra", "43A-494.25", "Xe máy" },
                    { "TKT0370", new DateTime(2026, 5, 7, 6, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 12, 58, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A32", "Đã ra", "43A-296.93", "Xe máy" },
                    { "TKT0371", new DateTime(2026, 5, 7, 13, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 23, 6, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A32", "Đã ra", "43A-477.67", "Xe máy" },
                    { "TKT0372", new DateTime(2026, 5, 7, 15, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 21, 48, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "43B-135.95", "Ô tô nhỏ" },
                    { "TKT0373", new DateTime(2026, 5, 7, 19, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 5, 2, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B12", "Đã ra", "43B-593.75", "Ô tô nhỏ" },
                    { "TKT0374", new DateTime(2026, 5, 7, 6, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 10, 17, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C11", "Đã ra", "43B-932.66", "Ô tô lớn" },
                    { "TKT0375", new DateTime(2026, 5, 7, 12, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 16, 2, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A08", "Đã ra", "43A-725.91", "Xe máy" },
                    { "TKT0376", new DateTime(2026, 5, 7, 9, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 17, 59, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A36", "Đã ra", "43A-627.16", "Xe máy" },
                    { "TKT0377", new DateTime(2026, 5, 7, 11, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 20, 53, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B27", "Đã ra", "43B-575.12", "Ô tô nhỏ" },
                    { "TKT0378", new DateTime(2026, 5, 7, 12, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 20, 26, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C07", "Đã ra", "43B-575.27", "Ô tô lớn" },
                    { "TKT0379", new DateTime(2026, 5, 7, 10, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 11, 53, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C12", "Đã ra", "43B-122.79", "Ô tô lớn" },
                    { "TKT0380", new DateTime(2026, 5, 7, 13, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 18, 21, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C06", "Đã ra", "43B-398.69", "Ô tô lớn" },
                    { "TKT0381", new DateTime(2026, 5, 7, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 15, 19, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C13", "Đã ra", "43B-739.77", "Ô tô lớn" },
                    { "TKT0382", new DateTime(2026, 5, 7, 14, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 16, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-803.80", "Xe máy" },
                    { "TKT0383", new DateTime(2026, 5, 8, 14, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 18, 49, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B33", "Đã ra", "43B-386.54", "Ô tô nhỏ" },
                    { "TKT0384", new DateTime(2026, 5, 8, 13, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 23, 6, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C11", "Đã ra", "43B-821.46", "Ô tô lớn" },
                    { "TKT0385", new DateTime(2026, 5, 8, 18, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 21, 40, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C17", "Đã ra", "43B-175.73", "Ô tô lớn" },
                    { "TKT0386", new DateTime(2026, 5, 8, 19, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 2, 27, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C03", "Đã ra", "43B-574.11", "Ô tô lớn" },
                    { "TKT0387", new DateTime(2026, 5, 8, 19, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 4, 55, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C02", "Đã ra", "43B-764.39", "Ô tô lớn" },
                    { "TKT0388", new DateTime(2026, 5, 8, 10, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 13, 45, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "43A-170.28", "Xe máy" },
                    { "TKT0389", new DateTime(2026, 5, 8, 7, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 9, 31, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-635.13", "Ô tô lớn" },
                    { "TKT0390", new DateTime(2026, 5, 9, 9, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 19, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-799.23", "Xe máy" },
                    { "TKT0391", new DateTime(2026, 5, 9, 14, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 20, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "43A-112.78", "Xe máy" },
                    { "TKT0392", new DateTime(2026, 5, 9, 7, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 13, 4, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A03", "Đã ra", "43A-661.18", "Xe máy" },
                    { "TKT0393", new DateTime(2026, 5, 9, 8, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 16, 43, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C05", "Đã ra", "43B-771.59", "Ô tô lớn" },
                    { "TKT0394", new DateTime(2026, 5, 9, 11, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 15, 27, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C01", "Đã ra", "43B-679.46", "Ô tô lớn" },
                    { "TKT0395", new DateTime(2026, 5, 9, 15, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 20, 40, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "43A-602.82", "Xe máy" },
                    { "TKT0396", new DateTime(2026, 5, 9, 15, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 19, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A01", "Đã ra", "43A-695.82", "Xe máy" },
                    { "TKT0397", new DateTime(2026, 5, 9, 10, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 17, 19, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C16", "Đã ra", "43B-861.11", "Ô tô lớn" },
                    { "TKT0398", new DateTime(2026, 5, 10, 11, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 15, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A50", "Đã ra", "43A-461.87", "Xe máy" },
                    { "TKT0399", new DateTime(2026, 5, 10, 16, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 19, 11, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C19", "Đã ra", "43B-152.60", "Ô tô lớn" },
                    { "TKT0400", new DateTime(2026, 5, 10, 12, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 14, 55, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "43A-532.83", "Xe máy" },
                    { "TKT0401", new DateTime(2026, 5, 10, 14, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 20, 28, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "43A-221.34", "Xe máy" },
                    { "TKT0402", new DateTime(2026, 5, 10, 8, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 18, 1, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A01", "Đã ra", "43A-643.91", "Xe máy" },
                    { "TKT0403", new DateTime(2026, 5, 10, 13, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 19, 54, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "43A-360.29", "Xe máy" },
                    { "TKT0404", new DateTime(2026, 5, 10, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 13, 45, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "43A-975.15", "Xe máy" },
                    { "TKT0405", new DateTime(2026, 5, 10, 6, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 11, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A36", "Đã ra", "43A-277.47", "Xe máy" },
                    { "TKT0406", new DateTime(2026, 5, 10, 17, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 1, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "43A-964.58", "Xe máy" },
                    { "TKT0407", new DateTime(2026, 5, 10, 15, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 21, 28, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A24", "Đã ra", "43A-581.70", "Xe máy" },
                    { "TKT0408", new DateTime(2026, 5, 10, 10, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 12, 37, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B41", "Đã ra", "43B-693.70", "Ô tô nhỏ" },
                    { "TKT0409", new DateTime(2026, 5, 10, 6, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 9, 47, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "43A-486.87", "Xe máy" },
                    { "TKT0410", new DateTime(2026, 5, 11, 10, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 15, 0, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "43B-882.18", "Ô tô nhỏ" },
                    { "TKT0411", new DateTime(2026, 5, 11, 11, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 16, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "43A-546.93", "Xe máy" },
                    { "TKT0412", new DateTime(2026, 5, 11, 10, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 17, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "43A-373.60", "Xe máy" },
                    { "TKT0413", new DateTime(2026, 5, 11, 10, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 16, 32, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C10", "Đã ra", "43B-128.36", "Ô tô lớn" },
                    { "TKT0414", new DateTime(2026, 5, 11, 12, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 19, 46, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C06", "Đã ra", "43B-332.89", "Ô tô lớn" },
                    { "TKT0415", new DateTime(2026, 5, 11, 12, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 15, 21, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "43B-139.53", "Ô tô nhỏ" },
                    { "TKT0416", new DateTime(2026, 5, 11, 12, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 16, 56, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C04", "Đã ra", "43B-875.30", "Ô tô lớn" },
                    { "TKT0417", new DateTime(2026, 5, 11, 8, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 10, 36, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C02", "Đã ra", "43B-966.61", "Ô tô lớn" },
                    { "TKT0418", new DateTime(2026, 5, 11, 17, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 22, 39, 0, 0, DateTimeKind.Unspecified), null, 25000m, "C18", "Đã ra", "43B-597.45", "Ô tô lớn" },
                    { "TKT0419", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A01", "Đang trong bãi", "43A-375.98", "Xe máy" },
                    { "TKT0420", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A02", "Đang trong bãi", "43A-967.54", "Xe máy" },
                    { "TKT0421", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A03", "Đang trong bãi", "43A-872.50", "Xe máy" },
                    { "TKT0422", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A04", "Đang trong bãi", "43A-305.71", "Xe máy" },
                    { "TKT0423", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A05", "Đang trong bãi", "43A-775.33", "Xe máy" },
                    { "TKT0424", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A06", "Đang trong bãi", "43A-571.37", "Xe máy" },
                    { "TKT0425", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A07", "Đang trong bãi", "43A-500.40", "Xe máy" },
                    { "TKT0426", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A08", "Đang trong bãi", "43A-198.29", "Xe máy" },
                    { "TKT0427", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A09", "Đang trong bãi", "43A-377.84", "Xe máy" },
                    { "TKT0428", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A10", "Đang trong bãi", "43A-476.77", "Xe máy" },
                    { "TKT0429", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A11", "Đang trong bãi", "43A-331.65", "Xe máy" },
                    { "TKT0430", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A12", "Đang trong bãi", "43A-820.47", "Xe máy" },
                    { "TKT0431", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A13", "Đang trong bãi", "43A-722.61", "Xe máy" },
                    { "TKT0432", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A14", "Đang trong bãi", "43A-729.68", "Xe máy" },
                    { "TKT0433", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A15", "Đang trong bãi", "43A-890.95", "Xe máy" }
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
                values: new object[,]
                {
                    { "PAY0001", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 16, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0001" },
                    { "PAY0002", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 18, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0002" },
                    { "PAY0003", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 17, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0003" },
                    { "PAY0004", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 11, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0004" },
                    { "PAY0005", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 22, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0005" },
                    { "PAY0006", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 11, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0006" },
                    { "PAY0007", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 13, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0007" },
                    { "PAY0008", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 2, 3, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0008" },
                    { "PAY0009", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 15, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0009" },
                    { "PAY0010", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 19, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0010" },
                    { "PAY0011", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 9, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0011" },
                    { "PAY0012", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 14, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0012" },
                    { "PAY0013", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 15, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0013" },
                    { "PAY0014", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 3, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0014" },
                    { "PAY0015", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 2, 11, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0015" },
                    { "PAY0016", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 2, 19, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0016" },
                    { "PAY0017", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 2, 19, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0017" },
                    { "PAY0018", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 2, 19, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0018" },
                    { "PAY0019", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 2, 14, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0019" },
                    { "PAY0020", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 2, 19, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0020" },
                    { "PAY0021", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 1, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0021" },
                    { "PAY0022", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 3, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0022" },
                    { "PAY0023", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 2, 16, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0023" },
                    { "PAY0024", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 18, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0024" },
                    { "PAY0025", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 20, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0025" },
                    { "PAY0026", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 4, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0026" },
                    { "PAY0027", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 17, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0027" },
                    { "PAY0028", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 4, 1, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0028" },
                    { "PAY0029", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 21, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0029" },
                    { "PAY0030", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 19, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0030" },
                    { "PAY0031", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 17, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0031" },
                    { "PAY0032", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 19, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0032" },
                    { "PAY0033", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 16, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0033" },
                    { "PAY0034", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 22, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0034" },
                    { "PAY0035", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 18, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0035" },
                    { "PAY0036", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 11, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0036" },
                    { "PAY0037", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 16, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0037" },
                    { "PAY0038", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 19, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0038" },
                    { "PAY0039", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 5, 3, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0039" },
                    { "PAY0040", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 4, 20, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0040" },
                    { "PAY0041", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 18, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0041" },
                    { "PAY0042", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 19, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0042" },
                    { "PAY0043", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 5, 1, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0043" },
                    { "PAY0044", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 22, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0044" },
                    { "PAY0045", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 15, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0045" },
                    { "PAY0046", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 5, 5, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0046" },
                    { "PAY0047", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 6, 1, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0047" },
                    { "PAY0048", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 6, 0, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0048" },
                    { "PAY0049", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 5, 13, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0049" },
                    { "PAY0050", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 5, 18, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0050" },
                    { "PAY0051", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 4, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0051" },
                    { "PAY0052", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 18, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0052" },
                    { "PAY0053", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 19, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0053" },
                    { "PAY0054", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 18, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0054" },
                    { "PAY0055", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 19, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0055" },
                    { "PAY0056", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 21, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0056" },
                    { "PAY0057", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 16, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0057" },
                    { "PAY0058", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 21, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0058" },
                    { "PAY0059", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 23, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0059" },
                    { "PAY0060", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 14, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0060" },
                    { "PAY0061", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 15, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0061" },
                    { "PAY0062", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 11, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0062" },
                    { "PAY0063", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 9, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0063" },
                    { "PAY0064", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 3, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0064" },
                    { "PAY0065", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 17, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0065" },
                    { "PAY0066", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 8, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0066" },
                    { "PAY0067", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 0, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0067" },
                    { "PAY0068", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 15, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0068" },
                    { "PAY0069", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 16, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0069" },
                    { "PAY0070", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 21, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0070" },
                    { "PAY0071", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 14, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0071" },
                    { "PAY0072", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 8, 23, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0072" },
                    { "PAY0073", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 13, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0073" },
                    { "PAY0074", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 13, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0074" },
                    { "PAY0075", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 8, 13, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0075" },
                    { "PAY0076", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 13, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0076" },
                    { "PAY0077", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 8, 13, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0077" },
                    { "PAY0078", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 2, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0078" },
                    { "PAY0079", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 0, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0079" },
                    { "PAY0080", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 10, 0, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0080" },
                    { "PAY0081", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 15, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0081" },
                    { "PAY0082", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 9, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0082" },
                    { "PAY0083", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 18, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0083" },
                    { "PAY0084", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 19, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0084" },
                    { "PAY0085", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 19, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0085" },
                    { "PAY0086", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 20, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0086" },
                    { "PAY0087", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 13, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0087" },
                    { "PAY0088", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 12, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0088" },
                    { "PAY0089", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 13, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0089" },
                    { "PAY0090", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 22, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0090" },
                    { "PAY0091", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 15, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0091" },
                    { "PAY0092", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 21, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0092" },
                    { "PAY0093", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 16, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0093" },
                    { "PAY0094", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 2, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0094" },
                    { "PAY0095", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 16, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0095" },
                    { "PAY0096", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 12, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0096" },
                    { "PAY0097", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 10, 20, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0097" },
                    { "PAY0098", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 10, 8, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0098" },
                    { "PAY0099", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 18, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0099" },
                    { "PAY0100", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 16, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0100" },
                    { "PAY0101", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 16, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0101" },
                    { "PAY0102", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 2, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0102" },
                    { "PAY0103", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 1, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0103" },
                    { "PAY0104", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 14, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0104" },
                    { "PAY0105", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 20, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0105" },
                    { "PAY0106", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 11, 10, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0106" },
                    { "PAY0107", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 18, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0107" },
                    { "PAY0108", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 11, 14, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0108" },
                    { "PAY0109", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 18, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0109" },
                    { "PAY0110", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 11, 18, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0110" },
                    { "PAY0111", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 11, 18, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0111" },
                    { "PAY0112", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 21, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0112" },
                    { "PAY0113", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 19, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0113" },
                    { "PAY0114", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 23, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0114" },
                    { "PAY0115", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 16, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0115" },
                    { "PAY0116", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 18, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0116" },
                    { "PAY0117", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 22, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0117" },
                    { "PAY0118", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 0, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0118" },
                    { "PAY0119", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 14, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0119" },
                    { "PAY0120", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 16, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0120" },
                    { "PAY0121", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 19, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0121" },
                    { "PAY0122", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 19, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0122" },
                    { "PAY0123", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 23, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0123" },
                    { "PAY0124", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 21, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0124" },
                    { "PAY0125", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 17, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0125" },
                    { "PAY0126", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 18, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0126" },
                    { "PAY0127", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 13, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0127" },
                    { "PAY0128", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 17, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0128" },
                    { "PAY0129", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 18, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0129" },
                    { "PAY0130", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 17, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0130" },
                    { "PAY0131", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 19, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0131" },
                    { "PAY0132", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 10, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0132" },
                    { "PAY0133", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 0, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0133" },
                    { "PAY0134", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 19, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0134" },
                    { "PAY0135", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 1, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0135" },
                    { "PAY0136", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 10, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0136" },
                    { "PAY0137", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 12, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0137" },
                    { "PAY0138", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 17, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0138" },
                    { "PAY0139", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 21, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0139" },
                    { "PAY0140", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 3, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0140" },
                    { "PAY0141", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 17, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0141" },
                    { "PAY0142", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 19, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0142" },
                    { "PAY0143", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 21, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0143" },
                    { "PAY0144", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 9, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0144" },
                    { "PAY0145", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 2, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0145" },
                    { "PAY0146", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 21, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0146" },
                    { "PAY0147", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 21, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0147" },
                    { "PAY0148", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 16, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0148" },
                    { "PAY0149", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 21, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0149" },
                    { "PAY0150", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 10, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0150" },
                    { "PAY0151", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 12, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0151" },
                    { "PAY0152", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 21, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0152" },
                    { "PAY0153", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 21, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0153" },
                    { "PAY0154", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 16, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0154" },
                    { "PAY0155", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 13, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0155" },
                    { "PAY0156", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 16, 14, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0156" },
                    { "PAY0157", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 16, 16, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0157" },
                    { "PAY0158", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 15, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0158" },
                    { "PAY0159", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 13, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0159" },
                    { "PAY0160", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 17, 14, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0160" },
                    { "PAY0161", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 17, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0161" },
                    { "PAY0162", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 16, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0162" },
                    { "PAY0163", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 22, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0163" },
                    { "PAY0164", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 17, 15, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0164" },
                    { "PAY0165", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 21, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0165" },
                    { "PAY0166", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 11, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0166" },
                    { "PAY0167", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 9, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0167" },
                    { "PAY0168", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 21, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0168" },
                    { "PAY0169", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 12, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0169" },
                    { "PAY0170", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 16, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0170" },
                    { "PAY0171", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 16, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0171" },
                    { "PAY0172", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 1, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0172" },
                    { "PAY0173", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 17, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0173" },
                    { "PAY0174", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 20, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0174" },
                    { "PAY0175", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 18, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0175" },
                    { "PAY0176", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 0, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0176" },
                    { "PAY0177", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 15, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0177" },
                    { "PAY0178", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 11, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0178" },
                    { "PAY0179", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 15, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0179" },
                    { "PAY0180", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 16, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0180" },
                    { "PAY0181", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 10, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0181" },
                    { "PAY0182", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 11, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0182" },
                    { "PAY0183", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 12, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0183" },
                    { "PAY0184", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 12, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0184" },
                    { "PAY0185", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 15, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0185" },
                    { "PAY0186", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 20, 2, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0186" },
                    { "PAY0187", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 17, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0187" },
                    { "PAY0188", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 0, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0188" },
                    { "PAY0189", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 20, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0189" },
                    { "PAY0190", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 2, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0190" },
                    { "PAY0191", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 20, 19, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0191" },
                    { "PAY0192", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 16, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0192" },
                    { "PAY0193", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 20, 14, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0193" },
                    { "PAY0194", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 2, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0194" },
                    { "PAY0195", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 21, 1, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0195" },
                    { "PAY0196", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 17, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0196" },
                    { "PAY0197", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 0, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0197" },
                    { "PAY0198", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 15, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0198" },
                    { "PAY0199", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 21, 3, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0199" },
                    { "PAY0200", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 16, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0200" },
                    { "PAY0201", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 17, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0201" },
                    { "PAY0202", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 8, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0202" },
                    { "PAY0203", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 20, 17, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0203" },
                    { "PAY0204", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 16, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0204" },
                    { "PAY0205", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 15, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0205" },
                    { "PAY0206", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 1, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0206" },
                    { "PAY0207", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 13, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0207" },
                    { "PAY0208", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 1, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0208" },
                    { "PAY0209", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 13, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0209" },
                    { "PAY0210", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 14, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0210" },
                    { "PAY0211", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 1, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0211" },
                    { "PAY0212", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 22, 1, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0212" },
                    { "PAY0213", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 21, 12, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0213" },
                    { "PAY0214", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 22, 19, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0214" },
                    { "PAY0215", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 3, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0215" },
                    { "PAY0216", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 12, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0216" },
                    { "PAY0217", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 10, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0217" },
                    { "PAY0218", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 22, 21, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0218" },
                    { "PAY0219", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 18, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0219" },
                    { "PAY0220", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 19, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0220" },
                    { "PAY0221", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 23, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0221" },
                    { "PAY0222", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 10, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0222" },
                    { "PAY0223", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 20, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0223" },
                    { "PAY0224", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 0, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0224" },
                    { "PAY0225", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 19, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0225" },
                    { "PAY0226", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 23, 22, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0226" },
                    { "PAY0227", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 23, 17, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0227" },
                    { "PAY0228", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 17, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0228" },
                    { "PAY0229", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 3, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0229" },
                    { "PAY0230", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 23, 15, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0230" },
                    { "PAY0231", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 2, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0231" },
                    { "PAY0232", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 20, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0232" },
                    { "PAY0233", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 22, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0233" },
                    { "PAY0234", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 10, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0234" },
                    { "PAY0235", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 11, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0235" },
                    { "PAY0236", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 18, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0236" },
                    { "PAY0237", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 18, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0237" },
                    { "PAY0238", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 22, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0238" },
                    { "PAY0239", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 18, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0239" },
                    { "PAY0240", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 25, 14, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0240" },
                    { "PAY0241", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 14, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0241" },
                    { "PAY0242", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 15, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0242" },
                    { "PAY0243", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 19, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0243" },
                    { "PAY0244", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 25, 18, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0244" },
                    { "PAY0245", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 23, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0245" },
                    { "PAY0246", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 17, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0246" },
                    { "PAY0247", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 13, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0247" },
                    { "PAY0248", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 10, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0248" },
                    { "PAY0249", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 21, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0249" },
                    { "PAY0250", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 23, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0250" },
                    { "PAY0251", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 18, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0251" },
                    { "PAY0252", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 15, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0252" },
                    { "PAY0253", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 19, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0253" },
                    { "PAY0254", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 20, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0254" },
                    { "PAY0255", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 13, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0255" },
                    { "PAY0256", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 16, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0256" },
                    { "PAY0257", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 15, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0257" },
                    { "PAY0258", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 14, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0258" },
                    { "PAY0259", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 18, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0259" },
                    { "PAY0260", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 8, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0260" },
                    { "PAY0261", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 17, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0261" },
                    { "PAY0262", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 23, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0262" },
                    { "PAY0263", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 23, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0263" },
                    { "PAY0264", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 17, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0264" },
                    { "PAY0265", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 17, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0265" },
                    { "PAY0266", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 14, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0266" },
                    { "PAY0267", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 10, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0267" },
                    { "PAY0268", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 16, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0268" },
                    { "PAY0269", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 18, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0269" },
                    { "PAY0270", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 23, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0270" },
                    { "PAY0271", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 23, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0271" },
                    { "PAY0272", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 18, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0272" },
                    { "PAY0273", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 19, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0273" },
                    { "PAY0274", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 0, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0274" },
                    { "PAY0275", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 15, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0275" },
                    { "PAY0276", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 14, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0276" },
                    { "PAY0277", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 14, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0277" },
                    { "PAY0278", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 21, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0278" },
                    { "PAY0279", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 13, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0279" },
                    { "PAY0280", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 23, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0280" },
                    { "PAY0281", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 0, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0281" },
                    { "PAY0282", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 0, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0282" },
                    { "PAY0283", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 29, 19, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0283" },
                    { "PAY0284", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 29, 17, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0284" },
                    { "PAY0285", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 30, 2, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0285" },
                    { "PAY0286", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 30, 1, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0286" },
                    { "PAY0287", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 21, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0287" },
                    { "PAY0288", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 17, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0288" },
                    { "PAY0289", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 17, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0289" },
                    { "PAY0290", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 20, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0290" },
                    { "PAY0291", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 21, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0291" },
                    { "PAY0292", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 1, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0292" },
                    { "PAY0293", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 8, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0293" },
                    { "PAY0294", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 30, 20, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0294" },
                    { "PAY0295", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 20, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0295" },
                    { "PAY0296", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 13, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0296" },
                    { "PAY0297", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 15, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0297" },
                    { "PAY0298", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 30, 19, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0298" },
                    { "PAY0299", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 16, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0299" },
                    { "PAY0300", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 19, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0300" },
                    { "PAY0301", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 12, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0301" },
                    { "PAY0302", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 20, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0302" },
                    { "PAY0303", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 12, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0303" },
                    { "PAY0304", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 14, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0304" },
                    { "PAY0305", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 23, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0305" },
                    { "PAY0306", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 2, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0306" },
                    { "PAY0307", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 18, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0307" },
                    { "PAY0308", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 17, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0308" },
                    { "PAY0309", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 14, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0309" },
                    { "PAY0310", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 3, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0310" },
                    { "PAY0311", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 10, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0311" },
                    { "PAY0312", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 21, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0312" },
                    { "PAY0313", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 17, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0313" },
                    { "PAY0314", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 17, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0314" },
                    { "PAY0315", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 19, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0315" },
                    { "PAY0316", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 22, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0316" },
                    { "PAY0317", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 19, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0317" },
                    { "PAY0318", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 14, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0318" },
                    { "PAY0319", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 18, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0319" },
                    { "PAY0320", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 15, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0320" },
                    { "PAY0321", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 4, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0321" },
                    { "PAY0322", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 7, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0322" },
                    { "PAY0323", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 22, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0323" },
                    { "PAY0324", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 22, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0324" },
                    { "PAY0325", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 16, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0325" },
                    { "PAY0326", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 3, 4, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0326" },
                    { "PAY0327", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 16, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0327" },
                    { "PAY0328", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 12, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0328" },
                    { "PAY0329", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 10, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0329" },
                    { "PAY0330", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 3, 0, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0330" },
                    { "PAY0331", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 0, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0331" },
                    { "PAY0332", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 3, 19, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0332" },
                    { "PAY0333", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 19, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0333" },
                    { "PAY0334", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 3, 17, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0334" },
                    { "PAY0335", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 16, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0335" },
                    { "PAY0336", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 16, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0336" },
                    { "PAY0337", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 22, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0337" },
                    { "PAY0338", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 17, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0338" },
                    { "PAY0339", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 14, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0339" },
                    { "PAY0340", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 16, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0340" },
                    { "PAY0341", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 19, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0341" },
                    { "PAY0342", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 19, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0342" },
                    { "PAY0343", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 23, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0343" },
                    { "PAY0344", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 21, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0344" },
                    { "PAY0345", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 1, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0345" },
                    { "PAY0346", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 2, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0346" },
                    { "PAY0347", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 18, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0347" },
                    { "PAY0348", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 17, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0348" },
                    { "PAY0349", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 11, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0349" },
                    { "PAY0350", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 19, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0350" },
                    { "PAY0351", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 15, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0351" },
                    { "PAY0352", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 1, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0352" },
                    { "PAY0353", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 23, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0353" },
                    { "PAY0354", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 23, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0354" },
                    { "PAY0355", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 22, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0355" },
                    { "PAY0356", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 0, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0356" },
                    { "PAY0357", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 19, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0357" },
                    { "PAY0358", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 17, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0358" },
                    { "PAY0359", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 14, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0359" },
                    { "PAY0360", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 16, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0360" },
                    { "PAY0361", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 19, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0361" },
                    { "PAY0362", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 0, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0362" },
                    { "PAY0363", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 9, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0363" },
                    { "PAY0364", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 12, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0364" },
                    { "PAY0365", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 15, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0365" },
                    { "PAY0366", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 18, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0366" },
                    { "PAY0367", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 23, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0367" },
                    { "PAY0368", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 15, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0368" },
                    { "PAY0369", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 15, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0369" },
                    { "PAY0370", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 12, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0370" },
                    { "PAY0371", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 23, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0371" },
                    { "PAY0372", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 21, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0372" },
                    { "PAY0373", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 5, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0373" },
                    { "PAY0374", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 10, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0374" },
                    { "PAY0375", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 16, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0375" },
                    { "PAY0376", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 17, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0376" },
                    { "PAY0377", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 20, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0377" },
                    { "PAY0378", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 20, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0378" },
                    { "PAY0379", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 11, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0379" },
                    { "PAY0380", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 18, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0380" },
                    { "PAY0381", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 15, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0381" },
                    { "PAY0382", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 16, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0382" },
                    { "PAY0383", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 8, 18, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0383" },
                    { "PAY0384", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 23, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0384" },
                    { "PAY0385", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 21, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0385" },
                    { "PAY0386", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 2, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0386" },
                    { "PAY0387", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 4, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0387" },
                    { "PAY0388", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 13, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0388" },
                    { "PAY0389", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 9, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0389" },
                    { "PAY0390", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 19, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0390" },
                    { "PAY0391", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 20, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0391" },
                    { "PAY0392", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 13, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0392" },
                    { "PAY0393", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 16, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0393" },
                    { "PAY0394", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 15, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0394" },
                    { "PAY0395", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 20, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0395" },
                    { "PAY0396", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 19, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0396" },
                    { "PAY0397", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 17, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0397" },
                    { "PAY0398", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 15, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0398" },
                    { "PAY0399", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 19, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0399" },
                    { "PAY0400", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 14, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0400" },
                    { "PAY0401", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 20, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0401" },
                    { "PAY0402", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 18, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0402" },
                    { "PAY0403", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 19, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0403" },
                    { "PAY0404", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 13, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0404" },
                    { "PAY0405", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 11, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0405" },
                    { "PAY0406", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 1, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0406" },
                    { "PAY0407", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 21, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0407" },
                    { "PAY0408", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 12, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0408" },
                    { "PAY0409", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 9, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0409" },
                    { "PAY0410", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 15, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0410" },
                    { "PAY0411", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 16, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0411" },
                    { "PAY0412", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 17, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0412" },
                    { "PAY0413", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 16, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0413" },
                    { "PAY0414", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 19, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0414" },
                    { "PAY0415", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 15, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0415" },
                    { "PAY0416", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 16, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0416" },
                    { "PAY0417", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 10, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0417" },
                    { "PAY0418", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 22, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0418" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehiclePlate", "CustomerId", "VehicleType" },
                values: new object[,]
                {
                    { "43A-102.53", "CUS010", "Xe máy" },
                    { "43A-117.87", "CUS017", "Xe máy" },
                    { "43A-163.78", "CUS001", "Xe máy" },
                    { "43A-176.82", "CUS026", "Xe máy" },
                    { "43A-193.57", "CUS006", "Xe máy" },
                    { "43A-289.56", "CUS009", "Xe máy" },
                    { "43A-294.11", "CUS004", "Xe máy" },
                    { "43A-329.51", "CUS025", "Xe máy" },
                    { "43A-333.16", "CUS020", "Xe máy" },
                    { "43A-349.66", "CUS028", "Xe máy" },
                    { "43A-405.11", "CUS016", "Xe máy" },
                    { "43A-436.67", "CUS019", "Xe máy" },
                    { "43A-456.31", "CUS008", "Xe máy" },
                    { "43A-496.54", "CUS014", "Xe máy" },
                    { "43A-505.34", "CUS011", "Xe máy" },
                    { "43A-582.73", "CUS015", "Xe máy" },
                    { "43A-597.34", "CUS024", "Xe máy" },
                    { "43A-657.75", "CUS007", "Xe máy" },
                    { "43A-671.36", "CUS012", "Xe máy" },
                    { "43A-679.92", "CUS029", "Xe máy" },
                    { "43A-743.15", "CUS023", "Xe máy" },
                    { "43A-761.33", "CUS003", "Xe máy" },
                    { "43A-766.27", "CUS005", "Xe máy" },
                    { "43A-790.42", "CUS018", "Xe máy" },
                    { "43A-791.11", "CUS022", "Xe máy" },
                    { "43A-816.83", "CUS002", "Xe máy" },
                    { "43A-860.80", "CUS030", "Xe máy" },
                    { "43A-866.67", "CUS013", "Xe máy" },
                    { "43A-890.60", "CUS021", "Xe máy" },
                    { "43A-938.25", "CUS027", "Xe máy" },
                    { "43B-160.85", "CUS027", "Ô tô nhỏ" },
                    { "43B-160.92", "CUS015", "Ô tô nhỏ" },
                    { "43B-422.72", "CUS006", "Ô tô nhỏ" },
                    { "43B-432.85", "CUS012", "Ô tô nhỏ" },
                    { "43B-452.36", "CUS024", "Ô tô nhỏ" },
                    { "43B-472.76", "CUS030", "Ô tô nhỏ" },
                    { "43B-536.32", "CUS009", "Ô tô nhỏ" },
                    { "43B-554.30", "CUS003", "Ô tô nhỏ" },
                    { "43B-589.63", "CUS021", "Ô tô nhỏ" },
                    { "43B-763.73", "CUS018", "Ô tô nhỏ" },
                    { "43C-164.35", "CUS020", "Ô tô lớn" },
                    { "43C-259.18", "CUS015", "Ô tô lớn" },
                    { "43C-325.85", "CUS030", "Ô tô lớn" },
                    { "43C-502.53", "CUS010", "Ô tô lớn" },
                    { "43C-653.47", "CUS025", "Ô tô lớn" },
                    { "43C-897.86", "CUS005", "Ô tô lớn" }
                });

            migrationBuilder.InsertData(
                table: "MonthlyTickets",
                columns: new[] { "MonthlyTicketId", "CreatedAt", "CustomerId", "EndDate", "PackageType", "StartDate", "Status", "TotalFee", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "MTK001", new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS002", new DateTime(2026, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-816.83", "Xe máy" },
                    { "MTK002", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-761.33", "Xe máy" },
                    { "MTK003", new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 300000m, "43B-554.30", "Ô tô nhỏ" },
                    { "MTK004", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS004", new DateTime(2026, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-294.11", "Xe máy" },
                    { "MTK005", new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-766.27", "Xe máy" },
                    { "MTK006", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 500000m, "43C-897.86", "Ô tô lớn" },
                    { "MTK007", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS006", new DateTime(2026, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-193.57", "Xe máy" },
                    { "MTK008", new DateTime(2026, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS006", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 300000m, "43B-422.72", "Ô tô nhỏ" },
                    { "MTK009", new DateTime(2026, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS007", new DateTime(2026, 7, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-657.75", "Xe máy" },
                    { "MTK010", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS008", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-456.31", "Xe máy" },
                    { "MTK011", new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS009", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-289.56", "Xe máy" },
                    { "MTK012", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS009", new DateTime(2026, 7, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 300000m, "43B-536.32", "Ô tô nhỏ" },
                    { "MTK013", new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS010", new DateTime(2026, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-102.53", "Xe máy" },
                    { "MTK014", new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS010", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 500000m, "43C-502.53", "Ô tô lớn" },
                    { "MTK015", new DateTime(2026, 4, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS011", new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-505.34", "Xe máy" }
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
