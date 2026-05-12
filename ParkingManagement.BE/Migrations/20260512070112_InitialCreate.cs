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
                    { "A04", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 04", "Trống", "Xe máy" },
                    { "A05", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 05", "Đang sử dụng", "Xe máy" },
                    { "A06", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 06", "Đang sử dụng", "Xe máy" },
                    { "A07", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 07", "Trống", "Xe máy" },
                    { "A08", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 08", "Đang sử dụng", "Xe máy" },
                    { "A09", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 09", "Trống", "Xe máy" },
                    { "A10", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 10", "Đang sử dụng", "Xe máy" },
                    { "A11", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 11", "Đang sử dụng", "Xe máy" },
                    { "A12", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 12", "Đang sử dụng", "Xe máy" },
                    { "A13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 13", "Trống", "Xe máy" },
                    { "A14", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 14", "Đang sử dụng", "Xe máy" },
                    { "A15", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 15", "Trống", "Xe máy" },
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
                    { "B04", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 04", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B05", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 05", "Trống", "Ô tô nhỏ" },
                    { "B06", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 06", "Trống", "Ô tô nhỏ" },
                    { "B07", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 07", "Trống", "Ô tô nhỏ" },
                    { "B08", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 08", "Trống", "Ô tô nhỏ" },
                    { "B09", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 09", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 10", "Trống", "Ô tô nhỏ" },
                    { "B11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 11", "Trống", "Ô tô nhỏ" },
                    { "B12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 12", "Trống", "Ô tô nhỏ" },
                    { "B13", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 13", "Đang sử dụng", "Ô tô nhỏ" },
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
                    { "C07", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 07", "Đang sử dụng", "Ô tô lớn" },
                    { "C08", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 08", "Trống", "Ô tô lớn" },
                    { "C09", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 09", "Trống", "Ô tô lớn" },
                    { "C10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 10", "Trống", "Ô tô lớn" },
                    { "C11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 11", "Trống", "Ô tô lớn" },
                    { "C12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 12", "Trống", "Ô tô lớn" },
                    { "C13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 13", "Trống", "Ô tô lớn" },
                    { "C14", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 14", "Trống", "Ô tô lớn" },
                    { "C15", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 15", "Đang sử dụng", "Ô tô lớn" },
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
                table: "Vehicles",
                columns: new[] { "VehiclePlate", "CustomerId", "VehicleType" },
                values: new object[,]
                {
                    { "74A-175.54", null, "Ô tô nhỏ" },
                    { "74A-177.66", null, "Ô tô nhỏ" },
                    { "74A-241.24", null, "Ô tô nhỏ" },
                    { "74A-247.89", null, "Ô tô nhỏ" },
                    { "74A-266.34", null, "Ô tô nhỏ" },
                    { "74A-316.24", null, "Ô tô nhỏ" },
                    { "74A-328.20", null, "Ô tô nhỏ" },
                    { "74A-347.16", null, "Ô tô nhỏ" },
                    { "74A-409.65", null, "Ô tô nhỏ" },
                    { "74A-426.12", null, "Ô tô nhỏ" },
                    { "74A-459.45", null, "Ô tô nhỏ" },
                    { "74A-486.69", null, "Ô tô nhỏ" },
                    { "74A-493.47", null, "Ô tô nhỏ" },
                    { "74A-501.54", null, "Ô tô nhỏ" },
                    { "74A-502.38", null, "Ô tô nhỏ" },
                    { "74A-532.50", null, "Ô tô nhỏ" },
                    { "74A-650.50", null, "Ô tô nhỏ" },
                    { "74A-780.42", null, "Ô tô nhỏ" },
                    { "74A-805.37", null, "Ô tô nhỏ" },
                    { "74A-867.96", null, "Ô tô nhỏ" },
                    { "74A-868.58", null, "Ô tô nhỏ" },
                    { "74A-875.32", null, "Ô tô nhỏ" },
                    { "74A-896.89", null, "Ô tô nhỏ" },
                    { "74A-908.24", null, "Ô tô nhỏ" },
                    { "74A-968.18", null, "Ô tô nhỏ" },
                    { "92C-120.57", null, "Xe máy" },
                    { "92C-135.34", null, "Xe máy" },
                    { "92C-144.58", null, "Xe máy" },
                    { "92C-150.34", null, "Xe máy" },
                    { "92C-158.68", null, "Xe máy" },
                    { "92C-182.35", null, "Xe máy" },
                    { "92C-198.21", null, "Xe máy" },
                    { "92C-211.12", null, "Xe máy" },
                    { "92C-213.80", null, "Xe máy" },
                    { "92C-230.97", null, "Xe máy" },
                    { "92C-254.16", null, "Xe máy" },
                    { "92C-288.96", null, "Xe máy" },
                    { "92C-316.89", null, "Xe máy" },
                    { "92C-323.77", null, "Xe máy" },
                    { "92C-347.14", null, "Xe máy" },
                    { "92C-373.81", null, "Xe máy" },
                    { "92C-410.25", null, "Xe máy" },
                    { "92C-427.61", null, "Xe máy" },
                    { "92C-428.76", null, "Xe máy" },
                    { "92C-446.17", null, "Xe máy" },
                    { "92C-459.45", null, "Xe máy" },
                    { "92C-475.65", null, "Xe máy" },
                    { "92C-477.17", null, "Xe máy" },
                    { "92C-503.56", null, "Xe máy" },
                    { "92C-555.37", null, "Xe máy" },
                    { "92C-577.72", null, "Xe máy" },
                    { "92C-586.10", null, "Xe máy" },
                    { "92C-629.64", null, "Xe máy" },
                    { "92C-652.88", null, "Xe máy" },
                    { "92C-655.70", null, "Xe máy" },
                    { "92C-702.42", null, "Xe máy" },
                    { "92C-708.27", null, "Xe máy" },
                    { "92C-728.40", null, "Xe máy" },
                    { "92C-796.10", null, "Xe máy" },
                    { "92C-814.77", null, "Xe máy" },
                    { "92C-839.81", null, "Xe máy" },
                    { "92C-847.31", null, "Xe máy" },
                    { "92C-852.34", null, "Xe máy" },
                    { "92C-856.73", null, "Xe máy" },
                    { "92C-872.82", null, "Xe máy" },
                    { "92C-873.64", null, "Xe máy" },
                    { "92C-902.88", null, "Xe máy" },
                    { "92C-903.72", null, "Xe máy" },
                    { "92C-914.30", null, "Xe máy" },
                    { "92C-936.10", null, "Xe máy" },
                    { "92C-945.90", null, "Xe máy" },
                    { "92C-953.18", null, "Xe máy" },
                    { "92C-959.49", null, "Xe máy" },
                    { "92C-959.65", null, "Xe máy" },
                    { "92C-983.41", null, "Xe máy" }
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
                    { "TKT0001", new DateTime(2026, 4, 1, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 14, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0002", new DateTime(2026, 4, 1, 17, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 21, 38, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B44", "Đã ra", "74A-805.37", "Ô tô nhỏ" },
                    { "TKT0003", new DateTime(2026, 4, 1, 15, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 19, 4, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A03", "Đã ra", "92C-945.90", "Xe máy" },
                    { "TKT0006", new DateTime(2026, 4, 1, 12, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 19, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "92C-959.65", "Xe máy" },
                    { "TKT0008", new DateTime(2026, 4, 1, 13, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 19, 49, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "74A-409.65", "Ô tô nhỏ" },
                    { "TKT0009", new DateTime(2026, 4, 2, 19, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 22, 31, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B23", "Đã ra", "74A-780.42", "Ô tô nhỏ" },
                    { "TKT0010", new DateTime(2026, 4, 2, 7, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 17, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "92C-555.37", "Xe máy" },
                    { "TKT0013", new DateTime(2026, 4, 2, 17, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 1, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B47", "Đã ra", "74A-247.89", "Ô tô nhỏ" },
                    { "TKT0014", new DateTime(2026, 4, 3, 7, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 10, 28, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "74A-780.42", "Ô tô nhỏ" },
                    { "TKT0016", new DateTime(2026, 4, 3, 18, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 1, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A36", "Đã ra", "92C-914.30", "Xe máy" },
                    { "TKT0017", new DateTime(2026, 4, 3, 10, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 12, 54, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B45", "Đã ra", "74A-896.89", "Ô tô nhỏ" },
                    { "TKT0018", new DateTime(2026, 4, 3, 7, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 14, 2, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B09", "Đã ra", "74A-241.24", "Ô tô nhỏ" },
                    { "TKT0019", new DateTime(2026, 4, 3, 15, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 23, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "92C-211.12", "Xe máy" },
                    { "TKT0020", new DateTime(2026, 4, 3, 16, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 22, 12, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A22", "Đã ra", "92C-475.65", "Xe máy" },
                    { "TKT0022", new DateTime(2026, 4, 3, 9, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 12, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-158.68", "Xe máy" },
                    { "TKT0023", new DateTime(2026, 4, 3, 13, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 16, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "92C-288.96", "Xe máy" },
                    { "TKT0025", new DateTime(2026, 4, 3, 15, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 17, 7, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B05", "Đã ra", "74A-868.58", "Ô tô nhỏ" },
                    { "TKT0026", new DateTime(2026, 4, 3, 19, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 23, 49, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-198.21", "Xe máy" },
                    { "TKT0027", new DateTime(2026, 4, 3, 7, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 11, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0028", new DateTime(2026, 4, 4, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 11, 45, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "74A-347.16", "Ô tô nhỏ" },
                    { "TKT0029", new DateTime(2026, 4, 4, 9, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 14, 10, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-780.42", "Ô tô nhỏ" },
                    { "TKT0030", new DateTime(2026, 4, 4, 9, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 12, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "92C-873.64", "Xe máy" },
                    { "TKT0032", new DateTime(2026, 4, 4, 14, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 17, 55, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-555.37", "Xe máy" },
                    { "TKT0033", new DateTime(2026, 4, 4, 13, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 21, 21, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A37", "Đã ra", "92C-347.14", "Xe máy" },
                    { "TKT0034", new DateTime(2026, 4, 5, 19, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 3, 29, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0036", new DateTime(2026, 4, 5, 9, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 13, 7, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "92C-428.76", "Xe máy" },
                    { "TKT0038", new DateTime(2026, 4, 5, 12, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 17, 44, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "92C-847.31", "Xe máy" },
                    { "TKT0039", new DateTime(2026, 4, 5, 9, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 10, 48, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-316.89", "Xe máy" },
                    { "TKT0040", new DateTime(2026, 4, 5, 18, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 2, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-702.42", "Xe máy" },
                    { "TKT0041", new DateTime(2026, 4, 6, 18, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 20, 21, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B30", "Đã ra", "74A-868.58", "Ô tô nhỏ" },
                    { "TKT0042", new DateTime(2026, 4, 6, 18, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 1, 23, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B23", "Đã ra", "74A-875.32", "Ô tô nhỏ" },
                    { "TKT0044", new DateTime(2026, 4, 6, 8, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 16, 44, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-959.65", "Xe máy" },
                    { "TKT0046", new DateTime(2026, 4, 6, 14, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 18, 46, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B07", "Đã ra", "74A-175.54", "Ô tô nhỏ" },
                    { "TKT0047", new DateTime(2026, 4, 6, 7, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 17, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "92C-347.14", "Xe máy" },
                    { "TKT0048", new DateTime(2026, 4, 6, 12, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 19, 28, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A50", "Đã ra", "92C-475.65", "Xe máy" },
                    { "TKT0049", new DateTime(2026, 4, 6, 12, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 14, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "92C-459.45", "Xe máy" },
                    { "TKT0050", new DateTime(2026, 4, 6, 7, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 15, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-150.34", "Xe máy" },
                    { "TKT0051", new DateTime(2026, 4, 6, 7, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 15, 1, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-254.16", "Xe máy" },
                    { "TKT0053", new DateTime(2026, 4, 6, 15, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 20, 12, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-502.38", "Ô tô nhỏ" },
                    { "TKT0055", new DateTime(2026, 4, 6, 10, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 13, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A20", "Đã ra", "92C-959.65", "Xe máy" },
                    { "TKT0056", new DateTime(2026, 4, 7, 19, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 1, 55, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "92C-708.27", "Xe máy" },
                    { "TKT0057", new DateTime(2026, 4, 7, 15, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 18, 48, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B22", "Đã ra", "74A-493.47", "Ô tô nhỏ" },
                    { "TKT0060", new DateTime(2026, 4, 7, 9, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 11, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A05", "Đã ra", "92C-428.76", "Xe máy" },
                    { "TKT0061", new DateTime(2026, 4, 7, 13, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 14, 29, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-873.64", "Xe máy" },
                    { "TKT0062", new DateTime(2026, 4, 7, 10, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 13, 22, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B24", "Đã ra", "74A-493.47", "Ô tô nhỏ" },
                    { "TKT0063", new DateTime(2026, 4, 7, 11, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 18, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "92C-446.17", "Xe máy" },
                    { "TKT0064", new DateTime(2026, 4, 7, 18, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 19, 47, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0065", new DateTime(2026, 4, 8, 8, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 8, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B37", "Đã ra", "74A-459.45", "Ô tô nhỏ" },
                    { "TKT0066", new DateTime(2026, 4, 8, 19, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 21, 0, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "92C-852.34", "Xe máy" },
                    { "TKT0067", new DateTime(2026, 4, 8, 18, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 1, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A35", "Đã ra", "92C-708.27", "Xe máy" },
                    { "TKT0069", new DateTime(2026, 4, 8, 7, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 13, 26, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B03", "Đã ra", "74A-532.50", "Ô tô nhỏ" },
                    { "TKT0071", new DateTime(2026, 4, 8, 11, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 17, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "92C-477.17", "Xe máy" },
                    { "TKT0072", new DateTime(2026, 4, 8, 19, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 2, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A29", "Đã ra", "92C-410.25", "Xe máy" },
                    { "TKT0073", new DateTime(2026, 4, 8, 14, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 21, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-728.40", "Xe máy" },
                    { "TKT0074", new DateTime(2026, 4, 9, 13, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 21, 57, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-410.25", "Xe máy" },
                    { "TKT0079", new DateTime(2026, 4, 9, 13, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 17, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "92C-410.25", "Xe máy" },
                    { "TKT0082", new DateTime(2026, 4, 9, 15, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 17, 30, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-475.65", "Xe máy" },
                    { "TKT0084", new DateTime(2026, 4, 9, 19, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 1, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "92C-323.77", "Xe máy" },
                    { "TKT0085", new DateTime(2026, 4, 9, 17, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 3, 4, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A27", "Đã ra", "92C-428.76", "Xe máy" },
                    { "TKT0087", new DateTime(2026, 4, 9, 7, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 14, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-213.80", "Xe máy" },
                    { "TKT0088", new DateTime(2026, 4, 10, 16, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 19, 29, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B33", "Đã ra", "74A-459.45", "Ô tô nhỏ" },
                    { "TKT0089", new DateTime(2026, 4, 10, 7, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 10, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A09", "Đã ra", "92C-814.77", "Xe máy" },
                    { "TKT0090", new DateTime(2026, 4, 10, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 18, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "74A-493.47", "Ô tô nhỏ" },
                    { "TKT0095", new DateTime(2026, 4, 10, 16, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 17, 54, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "92C-144.58", "Xe máy" },
                    { "TKT0096", new DateTime(2026, 4, 11, 17, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 2, 7, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-708.27", "Xe máy" },
                    { "TKT0097", new DateTime(2026, 4, 11, 8, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 16, 56, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "92C-652.88", "Xe máy" },
                    { "TKT0098", new DateTime(2026, 4, 11, 7, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 13, 54, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "92C-702.42", "Xe máy" },
                    { "TKT0099", new DateTime(2026, 4, 11, 18, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 2, 31, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B24", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0100", new DateTime(2026, 4, 11, 6, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 7, 18, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B24", "Đã ra", "74A-247.89", "Ô tô nhỏ" },
                    { "TKT0102", new DateTime(2026, 4, 11, 13, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 23, 2, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "74A-175.54", "Ô tô nhỏ" },
                    { "TKT0103", new DateTime(2026, 4, 11, 17, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 23, 7, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B16", "Đã ra", "74A-266.34", "Ô tô nhỏ" },
                    { "TKT0104", new DateTime(2026, 4, 11, 12, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 14, 39, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A08", "Đã ra", "92C-959.65", "Xe máy" },
                    { "TKT0107", new DateTime(2026, 4, 11, 14, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 23, 34, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0108", new DateTime(2026, 4, 12, 13, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 15, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "92C-211.12", "Xe máy" },
                    { "TKT0110", new DateTime(2026, 4, 12, 6, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 8, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "92C-728.40", "Xe máy" },
                    { "TKT0111", new DateTime(2026, 4, 12, 6, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 15, 49, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "92C-144.58", "Xe máy" },
                    { "TKT0114", new DateTime(2026, 4, 12, 18, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 21, 43, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B29", "Đã ra", "74A-532.50", "Ô tô nhỏ" },
                    { "TKT0116", new DateTime(2026, 4, 13, 8, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 15, 42, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B08", "Đã ra", "74A-968.18", "Ô tô nhỏ" },
                    { "TKT0117", new DateTime(2026, 4, 13, 18, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 1, 48, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "92C-902.88", "Xe máy" },
                    { "TKT0118", new DateTime(2026, 4, 13, 19, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 5, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A01", "Đã ra", "92C-475.65", "Xe máy" },
                    { "TKT0121", new DateTime(2026, 4, 13, 7, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 9, 6, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "92C-852.34", "Xe máy" },
                    { "TKT0122", new DateTime(2026, 4, 13, 9, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 16, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0123", new DateTime(2026, 4, 13, 11, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 19, 4, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "92C-428.76", "Xe máy" },
                    { "TKT0124", new DateTime(2026, 4, 13, 8, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 14, 33, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B29", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0126", new DateTime(2026, 4, 13, 19, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 21, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-936.10", "Xe máy" },
                    { "TKT0127", new DateTime(2026, 4, 13, 6, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 7, 17, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "92C-254.16", "Xe máy" },
                    { "TKT0129", new DateTime(2026, 4, 14, 12, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 21, 26, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B13", "Đã ra", "74A-459.45", "Ô tô nhỏ" },
                    { "TKT0130", new DateTime(2026, 4, 14, 19, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 22, 28, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "74A-409.65", "Ô tô nhỏ" },
                    { "TKT0132", new DateTime(2026, 4, 14, 12, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 16, 20, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "74A-493.47", "Ô tô nhỏ" },
                    { "TKT0134", new DateTime(2026, 4, 14, 12, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 14, 25, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B16", "Đã ra", "74A-502.38", "Ô tô nhỏ" },
                    { "TKT0138", new DateTime(2026, 4, 14, 12, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 17, 50, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A25", "Đã ra", "92C-796.10", "Xe máy" },
                    { "TKT0140", new DateTime(2026, 4, 14, 19, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 1, 13, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "92C-945.90", "Xe máy" },
                    { "TKT0142", new DateTime(2026, 4, 14, 10, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 12, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "92C-211.12", "Xe máy" },
                    { "TKT0143", new DateTime(2026, 4, 14, 17, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 19, 57, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A31", "Đã ra", "92C-555.37", "Xe máy" },
                    { "TKT0144", new DateTime(2026, 4, 15, 12, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 20, 49, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A27", "Đã ra", "92C-708.27", "Xe máy" },
                    { "TKT0146", new DateTime(2026, 4, 15, 12, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 14, 3, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B25", "Đã ra", "74A-175.54", "Ô tô nhỏ" },
                    { "TKT0147", new DateTime(2026, 4, 15, 9, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 17, 27, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "92C-120.57", "Xe máy" },
                    { "TKT0148", new DateTime(2026, 4, 15, 11, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 17, 15, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B29", "Đã ra", "74A-266.34", "Ô tô nhỏ" },
                    { "TKT0149", new DateTime(2026, 4, 15, 11, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 14, 10, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B46", "Đã ra", "74A-968.18", "Ô tô nhỏ" },
                    { "TKT0150", new DateTime(2026, 4, 15, 13, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 15, 39, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B39", "Đã ra", "74A-896.89", "Ô tô nhỏ" },
                    { "TKT0153", new DateTime(2026, 4, 15, 7, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 10, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A34", "Đã ra", "92C-347.14", "Xe máy" },
                    { "TKT0155", new DateTime(2026, 4, 15, 18, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 3, 37, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A45", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0159", new DateTime(2026, 4, 16, 6, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 16, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A24", "Đã ra", "92C-410.25", "Xe máy" },
                    { "TKT0160", new DateTime(2026, 4, 16, 18, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 2, 12, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "92C-902.88", "Xe máy" },
                    { "TKT0161", new DateTime(2026, 4, 16, 6, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 13, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "92C-158.68", "Xe máy" },
                    { "TKT0162", new DateTime(2026, 4, 16, 19, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 21, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "92C-852.34", "Xe máy" },
                    { "TKT0163", new DateTime(2026, 4, 16, 19, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 21, 38, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "92C-852.34", "Xe máy" },
                    { "TKT0165", new DateTime(2026, 4, 16, 19, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 3, 47, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B25", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0166", new DateTime(2026, 4, 16, 6, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 15, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "92C-135.34", "Xe máy" },
                    { "TKT0168", new DateTime(2026, 4, 16, 11, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 13, 33, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "74A-409.65", "Ô tô nhỏ" },
                    { "TKT0169", new DateTime(2026, 4, 16, 6, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 13, 0, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-475.65", "Xe máy" },
                    { "TKT0170", new DateTime(2026, 4, 16, 16, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 0, 7, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A23", "Đã ra", "92C-288.96", "Xe máy" },
                    { "TKT0171", new DateTime(2026, 4, 17, 11, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 13, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "92C-428.76", "Xe máy" },
                    { "TKT0174", new DateTime(2026, 4, 17, 8, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 11, 42, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B07", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0175", new DateTime(2026, 4, 17, 15, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 20, 48, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0177", new DateTime(2026, 4, 17, 12, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 21, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A20", "Đã ra", "92C-135.34", "Xe máy" },
                    { "TKT0178", new DateTime(2026, 4, 17, 13, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 22, 12, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "92C-914.30", "Xe máy" },
                    { "TKT0180", new DateTime(2026, 4, 17, 16, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 1, 25, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "92C-410.25", "Xe máy" },
                    { "TKT0182", new DateTime(2026, 4, 17, 9, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 12, 2, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "92C-655.70", "Xe máy" },
                    { "TKT0184", new DateTime(2026, 4, 17, 8, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 15, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A08", "Đã ra", "92C-459.45", "Xe máy" },
                    { "TKT0185", new DateTime(2026, 4, 17, 16, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 17, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "74A-908.24", "Ô tô nhỏ" },
                    { "TKT0187", new DateTime(2026, 4, 18, 15, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 20, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "92C-796.10", "Xe máy" },
                    { "TKT0189", new DateTime(2026, 4, 18, 12, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 13, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "92C-211.12", "Xe máy" },
                    { "TKT0191", new DateTime(2026, 4, 18, 10, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 19, 40, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A10", "Đã ra", "92C-316.89", "Xe máy" },
                    { "TKT0198", new DateTime(2026, 4, 19, 15, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 20, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A36", "Đã ra", "92C-144.58", "Xe máy" },
                    { "TKT0199", new DateTime(2026, 4, 19, 12, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 15, 2, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B28", "Đã ra", "74A-328.20", "Ô tô nhỏ" },
                    { "TKT0201", new DateTime(2026, 4, 20, 15, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 0, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A38", "Đã ra", "92C-230.97", "Xe máy" },
                    { "TKT0205", new DateTime(2026, 4, 20, 8, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 13, 42, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "92C-652.88", "Xe máy" },
                    { "TKT0206", new DateTime(2026, 4, 21, 8, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 14, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "92C-120.57", "Xe máy" },
                    { "TKT0208", new DateTime(2026, 4, 21, 16, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 17, 27, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "92C-427.61", "Xe máy" },
                    { "TKT0209", new DateTime(2026, 4, 21, 16, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 22, 10, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "74A-347.16", "Ô tô nhỏ" },
                    { "TKT0211", new DateTime(2026, 4, 21, 7, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 14, 34, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B34", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0212", new DateTime(2026, 4, 21, 13, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 18, 33, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B09", "Đã ra", "74A-409.65", "Ô tô nhỏ" },
                    { "TKT0213", new DateTime(2026, 4, 21, 10, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 17, 40, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A05", "Đã ra", "92C-959.65", "Xe máy" },
                    { "TKT0215", new DateTime(2026, 4, 21, 14, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 18, 18, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-945.90", "Xe máy" },
                    { "TKT0216", new DateTime(2026, 4, 21, 9, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 15, 47, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B30", "Đã ra", "74A-875.32", "Ô tô nhỏ" },
                    { "TKT0219", new DateTime(2026, 4, 22, 8, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 13, 26, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "74A-896.89", "Ô tô nhỏ" },
                    { "TKT0221", new DateTime(2026, 4, 22, 19, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 1, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A05", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0222", new DateTime(2026, 4, 22, 8, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 16, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "92C-959.65", "Xe máy" },
                    { "TKT0225", new DateTime(2026, 4, 22, 9, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 18, 26, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A17", "Đã ra", "92C-198.21", "Xe máy" },
                    { "TKT0227", new DateTime(2026, 4, 23, 16, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 20, 4, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-254.16", "Xe máy" },
                    { "TKT0229", new DateTime(2026, 4, 23, 7, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 14, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B08", "Đã ra", "74A-316.24", "Ô tô nhỏ" },
                    { "TKT0230", new DateTime(2026, 4, 23, 17, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 2, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A20", "Đã ra", "92C-814.77", "Xe máy" },
                    { "TKT0231", new DateTime(2026, 4, 23, 7, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 8, 20, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "92C-796.10", "Xe máy" },
                    { "TKT0235", new DateTime(2026, 4, 23, 13, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 17, 12, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "74A-896.89", "Ô tô nhỏ" },
                    { "TKT0236", new DateTime(2026, 4, 23, 12, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 17, 45, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B39", "Đã ra", "74A-177.66", "Ô tô nhỏ" },
                    { "TKT0237", new DateTime(2026, 4, 23, 18, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 19, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A01", "Đã ra", "92C-254.16", "Xe máy" },
                    { "TKT0238", new DateTime(2026, 4, 23, 10, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 14, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A24", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0239", new DateTime(2026, 4, 24, 19, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 0, 12, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "74A-867.96", "Ô tô nhỏ" },
                    { "TKT0240", new DateTime(2026, 4, 24, 17, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 19, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "74A-266.34", "Ô tô nhỏ" },
                    { "TKT0241", new DateTime(2026, 4, 24, 8, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 13, 48, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B32", "Đã ra", "74A-177.66", "Ô tô nhỏ" },
                    { "TKT0242", new DateTime(2026, 4, 24, 6, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 16, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A29", "Đã ra", "92C-323.77", "Xe máy" },
                    { "TKT0245", new DateTime(2026, 4, 25, 19, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 3, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "92C-953.18", "Xe máy" },
                    { "TKT0246", new DateTime(2026, 4, 25, 17, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 1, 37, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B08", "Đã ra", "74A-175.54", "Ô tô nhỏ" },
                    { "TKT0247", new DateTime(2026, 4, 25, 11, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 17, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A24", "Đã ra", "92C-655.70", "Xe máy" },
                    { "TKT0248", new DateTime(2026, 4, 25, 19, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 2, 22, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B42", "Đã ra", "74A-867.96", "Ô tô nhỏ" },
                    { "TKT0249", new DateTime(2026, 4, 25, 12, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 16, 18, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "92C-945.90", "Xe máy" },
                    { "TKT0250", new DateTime(2026, 4, 25, 9, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 12, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-953.18", "Xe máy" },
                    { "TKT0251", new DateTime(2026, 4, 25, 16, 23, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 22, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-902.88", "Xe máy" },
                    { "TKT0257", new DateTime(2026, 4, 25, 10, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 14, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "92C-983.41", "Xe máy" },
                    { "TKT0259", new DateTime(2026, 4, 26, 12, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 16, 34, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A46", "Đã ra", "92C-158.68", "Xe máy" },
                    { "TKT0260", new DateTime(2026, 4, 26, 12, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 17, 9, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "92C-211.12", "Xe máy" },
                    { "TKT0261", new DateTime(2026, 4, 26, 9, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 13, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-577.72", "Xe máy" },
                    { "TKT0262", new DateTime(2026, 4, 26, 18, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 21, 35, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "92C-158.68", "Xe máy" },
                    { "TKT0264", new DateTime(2026, 4, 26, 15, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 18, 51, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "92C-839.81", "Xe máy" },
                    { "TKT0266", new DateTime(2026, 4, 26, 13, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 20, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A42", "Đã ra", "92C-211.12", "Xe máy" },
                    { "TKT0267", new DateTime(2026, 4, 26, 17, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 19, 30, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "92C-120.57", "Xe máy" },
                    { "TKT0268", new DateTime(2026, 4, 26, 12, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 21, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A07", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0269", new DateTime(2026, 4, 27, 18, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 3, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A25", "Đã ra", "92C-856.73", "Xe máy" },
                    { "TKT0270", new DateTime(2026, 4, 27, 15, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 0, 41, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "92C-629.64", "Xe máy" },
                    { "TKT0271", new DateTime(2026, 4, 27, 13, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 21, 21, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "92C-959.49", "Xe máy" },
                    { "TKT0273", new DateTime(2026, 4, 27, 15, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 21, 21, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0275", new DateTime(2026, 4, 27, 16, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 23, 11, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B27", "Đã ra", "74A-241.24", "Ô tô nhỏ" },
                    { "TKT0276", new DateTime(2026, 4, 27, 11, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 14, 57, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "92C-914.30", "Xe máy" },
                    { "TKT0277", new DateTime(2026, 4, 27, 18, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 20, 33, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "92C-475.65", "Xe máy" },
                    { "TKT0278", new DateTime(2026, 4, 27, 19, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 23, 58, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-459.45", "Ô tô nhỏ" },
                    { "TKT0279", new DateTime(2026, 4, 27, 6, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 10, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "92C-213.80", "Xe máy" },
                    { "TKT0280", new DateTime(2026, 4, 28, 15, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 17, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B38", "Đã ra", "74A-247.89", "Ô tô nhỏ" },
                    { "TKT0281", new DateTime(2026, 4, 28, 8, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 12, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A21", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0282", new DateTime(2026, 4, 28, 13, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 15, 16, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B21", "Đã ra", "74A-501.54", "Ô tô nhỏ" },
                    { "TKT0284", new DateTime(2026, 4, 28, 19, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 21, 19, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B16", "Đã ra", "74A-868.58", "Ô tô nhỏ" },
                    { "TKT0287", new DateTime(2026, 4, 28, 16, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 17, 45, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A37", "Đã ra", "92C-555.37", "Xe máy" },
                    { "TKT0288", new DateTime(2026, 4, 28, 11, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 20, 42, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B47", "Đã ra", "74A-241.24", "Ô tô nhỏ" },
                    { "TKT0289", new DateTime(2026, 4, 29, 10, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 11, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A40", "Đã ra", "92C-213.80", "Xe máy" },
                    { "TKT0290", new DateTime(2026, 4, 29, 9, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 16, 30, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B41", "Đã ra", "74A-266.34", "Ô tô nhỏ" },
                    { "TKT0291", new DateTime(2026, 4, 29, 16, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 23, 47, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "92C-728.40", "Xe máy" },
                    { "TKT0292", new DateTime(2026, 4, 29, 19, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 1, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "92C-586.10", "Xe máy" },
                    { "TKT0294", new DateTime(2026, 4, 29, 11, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 16, 38, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A22", "Đã ra", "92C-150.34", "Xe máy" },
                    { "TKT0299", new DateTime(2026, 4, 30, 9, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 15, 52, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B16", "Đã ra", "74A-502.38", "Ô tô nhỏ" },
                    { "TKT0300", new DateTime(2026, 4, 30, 10, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 18, 24, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-903.72", "Xe máy" },
                    { "TKT0301", new DateTime(2026, 4, 30, 19, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 4, 25, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B29", "Đã ra", "74A-486.69", "Ô tô nhỏ" },
                    { "TKT0302", new DateTime(2026, 4, 30, 13, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 17, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A41", "Đã ra", "92C-902.88", "Xe máy" },
                    { "TKT0303", new DateTime(2026, 4, 30, 18, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 29, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A29", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0304", new DateTime(2026, 4, 30, 8, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 16, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A23", "Đã ra", "92C-288.96", "Xe máy" },
                    { "TKT0306", new DateTime(2026, 4, 30, 10, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 15, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A03", "Đã ra", "92C-230.97", "Xe máy" },
                    { "TKT0307", new DateTime(2026, 4, 30, 11, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A12", "Đã ra", "92C-655.70", "Xe máy" },
                    { "TKT0308", new DateTime(2026, 4, 30, 18, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 20, 12, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B16", "Đã ra", "74A-650.50", "Ô tô nhỏ" },
                    { "TKT0309", new DateTime(2026, 4, 30, 14, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 17, 14, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B39", "Đã ra", "74A-266.34", "Ô tô nhỏ" },
                    { "TKT0310", new DateTime(2026, 4, 30, 17, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 1, 19, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A39", "Đã ra", "92C-945.90", "Xe máy" },
                    { "TKT0313", new DateTime(2026, 5, 1, 12, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 13, 27, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-908.24", "Ô tô nhỏ" },
                    { "TKT0314", new DateTime(2026, 5, 1, 7, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 10, 35, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-908.24", "Ô tô nhỏ" },
                    { "TKT0316", new DateTime(2026, 5, 1, 18, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 23, 10, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-316.89", "Xe máy" },
                    { "TKT0318", new DateTime(2026, 5, 1, 15, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 18, 53, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A43", "Đã ra", "92C-873.64", "Xe máy" },
                    { "TKT0319", new DateTime(2026, 5, 1, 17, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 22, 19, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B13", "Đã ra", "74A-875.32", "Ô tô nhỏ" },
                    { "TKT0320", new DateTime(2026, 5, 1, 13, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 23, 40, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B20", "Đã ra", "74A-247.89", "Ô tô nhỏ" },
                    { "TKT0321", new DateTime(2026, 5, 1, 6, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 9, 47, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "92C-852.34", "Xe máy" },
                    { "TKT0323", new DateTime(2026, 5, 2, 7, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 16, 50, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "74A-316.24", "Ô tô nhỏ" },
                    { "TKT0324", new DateTime(2026, 5, 2, 6, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 13, 35, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A30", "Đã ra", "92C-503.56", "Xe máy" },
                    { "TKT0326", new DateTime(2026, 5, 2, 7, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 13, 8, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B09", "Đã ra", "74A-501.54", "Ô tô nhỏ" },
                    { "TKT0327", new DateTime(2026, 5, 2, 18, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 3, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A06", "Đã ra", "92C-135.34", "Xe máy" },
                    { "TKT0328", new DateTime(2026, 5, 2, 12, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 22, 21, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A26", "Đã ra", "92C-652.88", "Xe máy" },
                    { "TKT0329", new DateTime(2026, 5, 2, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 17, 23, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B18", "Đã ra", "74A-502.38", "Ô tô nhỏ" },
                    { "TKT0332", new DateTime(2026, 5, 3, 19, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 0, 46, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A28", "Đã ra", "92C-702.42", "Xe máy" },
                    { "TKT0333", new DateTime(2026, 5, 3, 19, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 0, 16, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B15", "Đã ra", "74A-409.65", "Ô tô nhỏ" },
                    { "TKT0334", new DateTime(2026, 5, 3, 14, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 23, 15, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A27", "Đã ra", "92C-629.64", "Xe máy" },
                    { "TKT0336", new DateTime(2026, 5, 4, 9, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 14, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "92C-983.41", "Xe máy" },
                    { "TKT0339", new DateTime(2026, 5, 4, 10, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 14, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B36", "Đã ra", "74A-868.58", "Ô tô nhỏ" },
                    { "TKT0340", new DateTime(2026, 5, 4, 9, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 18, 29, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B24", "Đã ra", "74A-868.58", "Ô tô nhỏ" },
                    { "TKT0341", new DateTime(2026, 5, 4, 11, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 20, 41, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-805.37", "Ô tô nhỏ" },
                    { "TKT0342", new DateTime(2026, 5, 4, 15, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 22, 54, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B06", "Đã ra", "74A-175.54", "Ô tô nhỏ" },
                    { "TKT0344", new DateTime(2026, 5, 4, 16, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 19, 32, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-150.34", "Xe máy" },
                    { "TKT0345", new DateTime(2026, 5, 5, 15, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 20, 5, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A37", "Đã ra", "92C-959.49", "Xe máy" },
                    { "TKT0347", new DateTime(2026, 5, 5, 14, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 20, 48, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "92C-120.57", "Xe máy" },
                    { "TKT0348", new DateTime(2026, 5, 5, 14, 14, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 23, 56, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B33", "Đã ra", "74A-177.66", "Ô tô nhỏ" },
                    { "TKT0351", new DateTime(2026, 5, 5, 18, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 20, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B15", "Đã ra", "74A-780.42", "Ô tô nhỏ" },
                    { "TKT0352", new DateTime(2026, 5, 6, 19, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B46", "Đã ra", "74A-486.69", "Ô tô nhỏ" },
                    { "TKT0354", new DateTime(2026, 5, 6, 8, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 16, 40, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B28", "Đã ra", "74A-241.24", "Ô tô nhỏ" },
                    { "TKT0355", new DateTime(2026, 5, 6, 6, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 10, 23, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "92C-316.89", "Xe máy" },
                    { "TKT0356", new DateTime(2026, 5, 6, 18, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 23, 30, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A32", "Đã ra", "92C-347.14", "Xe máy" },
                    { "TKT0359", new DateTime(2026, 5, 6, 7, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 12, 40, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B02", "Đã ra", "74A-316.24", "Ô tô nhỏ" },
                    { "TKT0360", new DateTime(2026, 5, 6, 16, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 0, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A18", "Đã ra", "92C-702.42", "Xe máy" },
                    { "TKT0361", new DateTime(2026, 5, 7, 16, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 0, 36, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B12", "Đã ra", "74A-347.16", "Ô tô nhỏ" },
                    { "TKT0362", new DateTime(2026, 5, 7, 7, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 14, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B30", "Đã ra", "74A-328.20", "Ô tô nhỏ" },
                    { "TKT0363", new DateTime(2026, 5, 7, 15, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 20, 2, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A35", "Đã ra", "92C-316.89", "Xe máy" },
                    { "TKT0364", new DateTime(2026, 5, 7, 17, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 0, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A13", "Đã ra", "92C-135.34", "Xe máy" },
                    { "TKT0366", new DateTime(2026, 5, 7, 14, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 16, 40, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A15", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0367", new DateTime(2026, 5, 7, 16, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 21, 55, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B31", "Đã ra", "74A-896.89", "Ô tô nhỏ" },
                    { "TKT0368", new DateTime(2026, 5, 7, 19, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 21, 31, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B40", "Đã ra", "74A-426.12", "Ô tô nhỏ" },
                    { "TKT0369", new DateTime(2026, 5, 7, 19, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 5, 3, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B23", "Đã ra", "74A-177.66", "Ô tô nhỏ" },
                    { "TKT0372", new DateTime(2026, 5, 8, 6, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 12, 59, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A27", "Đã ra", "92C-150.34", "Xe máy" },
                    { "TKT0373", new DateTime(2026, 5, 8, 12, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 14, 35, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A41", "Đã ra", "92C-577.72", "Xe máy" },
                    { "TKT0378", new DateTime(2026, 5, 9, 16, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 20, 55, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B04", "Đã ra", "74A-501.54", "Ô tô nhỏ" },
                    { "TKT0379", new DateTime(2026, 5, 9, 13, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 17, 26, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A44", "Đã ra", "92C-182.35", "Xe máy" },
                    { "TKT0380", new DateTime(2026, 5, 9, 12, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 13, 43, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A48", "Đã ra", "92C-872.82", "Xe máy" },
                    { "TKT0381", new DateTime(2026, 5, 9, 13, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 15, 21, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "92C-213.80", "Xe máy" },
                    { "TKT0382", new DateTime(2026, 5, 9, 13, 44, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 15, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B28", "Đã ra", "74A-968.18", "Ô tô nhỏ" },
                    { "TKT0385", new DateTime(2026, 5, 9, 16, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 20, 42, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A14", "Đã ra", "92C-135.34", "Xe máy" },
                    { "TKT0389", new DateTime(2026, 5, 10, 7, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 11, 16, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A04", "Đã ra", "92C-288.96", "Xe máy" },
                    { "TKT0390", new DateTime(2026, 5, 10, 15, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 19, 11, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A08", "Đã ra", "92C-655.70", "Xe máy" },
                    { "TKT0391", new DateTime(2026, 5, 10, 10, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 12, 36, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A16", "Đã ra", "92C-629.64", "Xe máy" },
                    { "TKT0392", new DateTime(2026, 5, 10, 12, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 16, 45, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "92C-796.10", "Xe máy" },
                    { "TKT0395", new DateTime(2026, 5, 10, 12, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 17, 16, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B01", "Đã ra", "74A-868.58", "Ô tô nhỏ" },
                    { "TKT0396", new DateTime(2026, 5, 10, 19, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 3, 13, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A23", "Đã ra", "92C-120.57", "Xe máy" },
                    { "TKT0397", new DateTime(2026, 5, 10, 13, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 22, 6, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "92C-914.30", "Xe máy" },
                    { "TKT0398", new DateTime(2026, 5, 10, 9, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 18, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A02", "Đã ra", "92C-577.72", "Xe máy" },
                    { "TKT0399", new DateTime(2026, 5, 10, 10, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 13, 3, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A05", "Đã ra", "92C-316.89", "Xe máy" },
                    { "TKT0401", new DateTime(2026, 5, 10, 14, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 16, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A19", "Đã ra", "92C-503.56", "Xe máy" },
                    { "TKT0402", new DateTime(2026, 5, 11, 7, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 16, 8, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A33", "Đã ra", "92C-373.81", "Xe máy" },
                    { "TKT0406", new DateTime(2026, 5, 11, 15, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 22, 17, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B07", "Đã ra", "74A-177.66", "Ô tô nhỏ" },
                    { "TKT0407", new DateTime(2026, 5, 11, 14, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 19, 32, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B42", "Đã ra", "74A-409.65", "Ô tô nhỏ" },
                    { "TKT0410", new DateTime(2026, 5, 11, 12, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 16, 14, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A11", "Đã ra", "92C-796.10", "Xe máy" },
                    { "TKT0411", new DateTime(2026, 5, 11, 7, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 15, 50, 0, 0, DateTimeKind.Unspecified), null, 15000m, "B11", "Đã ra", "74A-780.42", "Ô tô nhỏ" },
                    { "TKT0413", new DateTime(2026, 5, 11, 6, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 7, 52, 0, 0, DateTimeKind.Unspecified), null, 5000m, "A49", "Đã ra", "92C-213.80", "Xe máy" }
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
                    { "PAY0001", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 14, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0001" },
                    { "PAY0002", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 21, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0002" },
                    { "PAY0003", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 19, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0003" },
                    { "PAY0006", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 19, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0006" },
                    { "PAY0008", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 19, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0008" },
                    { "PAY0009", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 2, 22, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0009" },
                    { "PAY0010", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 2, 17, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0010" },
                    { "PAY0013", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 1, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0013" },
                    { "PAY0014", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 10, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0014" },
                    { "PAY0016", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 4, 1, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0016" },
                    { "PAY0017", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 12, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0017" },
                    { "PAY0018", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 14, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0018" },
                    { "PAY0019", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 23, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0019" },
                    { "PAY0020", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 22, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0020" },
                    { "PAY0022", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 12, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0022" },
                    { "PAY0023", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 16, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0023" },
                    { "PAY0025", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 17, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0025" },
                    { "PAY0026", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 23, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0026" },
                    { "PAY0027", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 11, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0027" },
                    { "PAY0028", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 4, 11, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0028" },
                    { "PAY0029", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 14, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0029" },
                    { "PAY0030", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 12, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0030" },
                    { "PAY0032", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 17, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0032" },
                    { "PAY0033", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 4, 21, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0033" },
                    { "PAY0034", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 6, 3, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0034" },
                    { "PAY0036", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 5, 13, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0036" },
                    { "PAY0038", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 5, 17, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0038" },
                    { "PAY0039", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 5, 10, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0039" },
                    { "PAY0040", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 2, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0040" },
                    { "PAY0041", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 20, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0041" },
                    { "PAY0042", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 1, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0042" },
                    { "PAY0044", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 6, 16, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0044" },
                    { "PAY0046", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 18, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0046" },
                    { "PAY0047", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 17, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0047" },
                    { "PAY0048", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 19, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0048" },
                    { "PAY0049", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 14, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0049" },
                    { "PAY0050", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 15, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0050" },
                    { "PAY0051", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 15, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0051" },
                    { "PAY0053", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 20, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0053" },
                    { "PAY0055", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 6, 13, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0055" },
                    { "PAY0056", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 1, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0056" },
                    { "PAY0057", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 18, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0057" },
                    { "PAY0060", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 11, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0060" },
                    { "PAY0061", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 14, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0061" },
                    { "PAY0062", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 13, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0062" },
                    { "PAY0063", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 18, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0063" },
                    { "PAY0064", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 7, 19, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0064" },
                    { "PAY0065", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 13, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0065" },
                    { "PAY0066", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 8, 21, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0066" },
                    { "PAY0067", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 1, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0067" },
                    { "PAY0069", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 8, 13, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0069" },
                    { "PAY0071", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 8, 17, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0071" },
                    { "PAY0072", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 2, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0072" },
                    { "PAY0073", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 21, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0073" },
                    { "PAY0074", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 21, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0074" },
                    { "PAY0079", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 17, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0079" },
                    { "PAY0082", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 17, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0082" },
                    { "PAY0084", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 10, 1, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0084" },
                    { "PAY0085", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 3, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0085" },
                    { "PAY0087", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 14, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0087" },
                    { "PAY0088", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 10, 19, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0088" },
                    { "PAY0089", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 10, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0089" },
                    { "PAY0090", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 10, 18, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0090" },
                    { "PAY0095", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 17, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0095" },
                    { "PAY0096", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 2, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0096" },
                    { "PAY0097", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 11, 16, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0097" },
                    { "PAY0098", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 13, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0098" },
                    { "PAY0099", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 2, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0099" },
                    { "PAY0100", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 7, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0100" },
                    { "PAY0102", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 23, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0102" },
                    { "PAY0103", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 11, 23, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0103" },
                    { "PAY0104", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 14, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0104" },
                    { "PAY0107", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 23, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0107" },
                    { "PAY0108", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 15, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0108" },
                    { "PAY0110", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 8, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0110" },
                    { "PAY0111", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 15, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0111" },
                    { "PAY0114", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 21, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0114" },
                    { "PAY0116", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 15, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0116" },
                    { "PAY0117", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 1, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0117" },
                    { "PAY0118", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 5, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0118" },
                    { "PAY0121", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 9, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0121" },
                    { "PAY0122", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 16, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0122" },
                    { "PAY0123", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 19, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0123" },
                    { "PAY0124", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 14, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0124" },
                    { "PAY0126", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 21, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0126" },
                    { "PAY0127", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 7, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0127" },
                    { "PAY0129", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 21, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0129" },
                    { "PAY0130", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 22, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0130" },
                    { "PAY0132", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 16, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0132" },
                    { "PAY0134", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 14, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0134" },
                    { "PAY0138", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 17, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0138" },
                    { "PAY0140", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 1, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0140" },
                    { "PAY0142", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 12, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0142" },
                    { "PAY0143", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 19, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0143" },
                    { "PAY0144", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 20, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0144" },
                    { "PAY0146", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 14, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0146" },
                    { "PAY0147", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 17, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0147" },
                    { "PAY0148", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 17, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0148" },
                    { "PAY0149", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 14, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0149" },
                    { "PAY0150", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 15, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0150" },
                    { "PAY0153", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 10, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0153" },
                    { "PAY0155", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 3, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0155" },
                    { "PAY0159", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 16, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0159" },
                    { "PAY0160", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 2, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0160" },
                    { "PAY0161", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 13, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0161" },
                    { "PAY0162", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 21, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0162" },
                    { "PAY0163", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 21, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0163" },
                    { "PAY0165", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 17, 3, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0165" },
                    { "PAY0166", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 15, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0166" },
                    { "PAY0168", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 13, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0168" },
                    { "PAY0169", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 16, 13, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0169" },
                    { "PAY0170", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 0, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0170" },
                    { "PAY0171", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 13, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0171" },
                    { "PAY0174", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 11, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0174" },
                    { "PAY0175", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 20, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0175" },
                    { "PAY0177", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 21, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0177" },
                    { "PAY0178", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 17, 22, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0178" },
                    { "PAY0180", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 1, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0180" },
                    { "PAY0182", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 12, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0182" },
                    { "PAY0184", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 15, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0184" },
                    { "PAY0185", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 17, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0185" },
                    { "PAY0187", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 20, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0187" },
                    { "PAY0189", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 13, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0189" },
                    { "PAY0191", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 19, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0191" },
                    { "PAY0198", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 19, 20, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0198" },
                    { "PAY0199", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 15, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0199" },
                    { "PAY0201", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 0, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0201" },
                    { "PAY0205", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 13, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0205" },
                    { "PAY0206", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 14, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0206" },
                    { "PAY0208", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 17, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0208" },
                    { "PAY0209", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 21, 22, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0209" },
                    { "PAY0211", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 14, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0211" },
                    { "PAY0212", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 18, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0212" },
                    { "PAY0213", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 17, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0213" },
                    { "PAY0215", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 21, 18, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0215" },
                    { "PAY0216", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 15, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0216" },
                    { "PAY0219", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 13, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0219" },
                    { "PAY0221", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 23, 1, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0221" },
                    { "PAY0222", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 16, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0222" },
                    { "PAY0225", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 18, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0225" },
                    { "PAY0227", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 20, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0227" },
                    { "PAY0229", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 14, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0229" },
                    { "PAY0230", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 2, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0230" },
                    { "PAY0231", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 8, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0231" },
                    { "PAY0235", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 17, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0235" },
                    { "PAY0236", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 17, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0236" },
                    { "PAY0237", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 23, 19, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0237" },
                    { "PAY0238", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 14, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0238" },
                    { "PAY0239", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 25, 0, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0239" },
                    { "PAY0240", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 19, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0240" },
                    { "PAY0241", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 13, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0241" },
                    { "PAY0242", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 16, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0242" },
                    { "PAY0245", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 3, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0245" },
                    { "PAY0246", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 1, 37, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0246" },
                    { "PAY0247", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 17, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0247" },
                    { "PAY0248", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 2, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0248" },
                    { "PAY0249", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 25, 16, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0249" },
                    { "PAY0250", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 12, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0250" },
                    { "PAY0251", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 22, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0251" },
                    { "PAY0257", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 14, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0257" },
                    { "PAY0259", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 16, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0259" },
                    { "PAY0260", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 17, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0260" },
                    { "PAY0261", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 13, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0261" },
                    { "PAY0262", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 21, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0262" },
                    { "PAY0264", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 18, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0264" },
                    { "PAY0266", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 20, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0266" },
                    { "PAY0267", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 19, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0267" },
                    { "PAY0268", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 26, 21, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0268" },
                    { "PAY0269", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 3, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0269" },
                    { "PAY0270", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 0, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0270" },
                    { "PAY0271", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 21, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0271" },
                    { "PAY0273", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 21, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0273" },
                    { "PAY0275", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 23, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0275" },
                    { "PAY0276", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 14, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0276" },
                    { "PAY0277", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 20, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0277" },
                    { "PAY0278", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 23, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0278" },
                    { "PAY0279", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 10, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0279" },
                    { "PAY0280", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 17, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0280" },
                    { "PAY0281", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 12, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0281" },
                    { "PAY0282", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 15, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0282" },
                    { "PAY0284", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 21, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0284" },
                    { "PAY0287", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 17, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0287" },
                    { "PAY0288", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 20, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0288" },
                    { "PAY0289", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 29, 11, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0289" },
                    { "PAY0290", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 16, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0290" },
                    { "PAY0291", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 29, 23, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0291" },
                    { "PAY0292", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 30, 1, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0292" },
                    { "PAY0294", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 16, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0294" },
                    { "PAY0299", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 15, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0299" },
                    { "PAY0300", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 18, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0300" },
                    { "PAY0301", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 4, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0301" },
                    { "PAY0302", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 17, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0302" },
                    { "PAY0303", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 20, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0303" },
                    { "PAY0304", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 16, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0304" },
                    { "PAY0306", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 15, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0306" },
                    { "PAY0307", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 20, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0307" },
                    { "PAY0308", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 20, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0308" },
                    { "PAY0309", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 17, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0309" },
                    { "PAY0310", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 1, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0310" },
                    { "PAY0313", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 13, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0313" },
                    { "PAY0314", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 10, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0314" },
                    { "PAY0316", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 23, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0316" },
                    { "PAY0318", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 18, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0318" },
                    { "PAY0319", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 22, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0319" },
                    { "PAY0320", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 23, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0320" },
                    { "PAY0321", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 9, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0321" },
                    { "PAY0323", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 16, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0323" },
                    { "PAY0324", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 13, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0324" },
                    { "PAY0326", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 13, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0326" },
                    { "PAY0327", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 3, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0327" },
                    { "PAY0328", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 22, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0328" },
                    { "PAY0329", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 17, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0329" },
                    { "PAY0332", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 0, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0332" },
                    { "PAY0333", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 0, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0333" },
                    { "PAY0334", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 3, 23, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0334" },
                    { "PAY0336", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 14, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0336" },
                    { "PAY0339", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 14, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0339" },
                    { "PAY0340", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 18, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0340" },
                    { "PAY0341", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 20, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0341" },
                    { "PAY0342", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 22, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0342" },
                    { "PAY0344", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 19, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0344" },
                    { "PAY0345", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 20, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0345" },
                    { "PAY0347", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 20, 48, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0347" },
                    { "PAY0348", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 23, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0348" },
                    { "PAY0351", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 20, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0351" },
                    { "PAY0352", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0352" },
                    { "PAY0354", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 16, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0354" },
                    { "PAY0355", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 10, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0355" },
                    { "PAY0356", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 6, 23, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0356" },
                    { "PAY0359", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 12, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0359" },
                    { "PAY0360", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 0, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0360" },
                    { "PAY0361", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 0, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0361" },
                    { "PAY0362", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 14, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0362" },
                    { "PAY0363", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 20, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0363" },
                    { "PAY0364", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 0, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0364" },
                    { "PAY0366", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 16, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0366" },
                    { "PAY0367", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 21, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0367" },
                    { "PAY0368", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 21, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0368" },
                    { "PAY0369", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 5, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0369" },
                    { "PAY0372", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 12, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0372" },
                    { "PAY0373", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 14, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0373" },
                    { "PAY0378", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 20, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0378" },
                    { "PAY0379", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 17, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0379" },
                    { "PAY0380", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 13, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0380" },
                    { "PAY0381", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 15, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0381" },
                    { "PAY0382", 15000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 15, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0382" },
                    { "PAY0385", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 20, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0385" },
                    { "PAY0389", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 11, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0389" },
                    { "PAY0390", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 19, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0390" },
                    { "PAY0391", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 12, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0391" },
                    { "PAY0392", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 16, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0392" },
                    { "PAY0395", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 17, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0395" },
                    { "PAY0396", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 3, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0396" },
                    { "PAY0397", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 22, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0397" },
                    { "PAY0398", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 18, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0398" },
                    { "PAY0399", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 13, 3, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0399" },
                    { "PAY0401", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 16, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0401" },
                    { "PAY0402", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 16, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0402" },
                    { "PAY0406", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 22, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0406" },
                    { "PAY0407", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 19, 32, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0407" },
                    { "PAY0410", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 16, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0410" },
                    { "PAY0411", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 15, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0411" },
                    { "PAY0413", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 7, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0413" }
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
                    { "MTK001", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS002", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-816.83", "Xe máy" },
                    { "MTK002", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-761.33", "Xe máy" },
                    { "MTK003", new DateTime(2026, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 7, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 300000m, "43B-554.30", "Ô tô nhỏ" },
                    { "MTK004", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS004", new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-294.11", "Xe máy" },
                    { "MTK005", new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-766.27", "Xe máy" },
                    { "MTK006", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 500000m, "43C-897.86", "Ô tô lớn" },
                    { "MTK007", new DateTime(2026, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS006", new DateTime(2026, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-193.57", "Xe máy" },
                    { "MTK008", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS006", new DateTime(2026, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 300000m, "43B-422.72", "Ô tô nhỏ" },
                    { "MTK009", new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS007", new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-657.75", "Xe máy" },
                    { "MTK010", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS008", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-456.31", "Xe máy" },
                    { "MTK011", new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS009", new DateTime(2026, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-289.56", "Xe máy" },
                    { "MTK012", new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS009", new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 300000m, "43B-536.32", "Ô tô nhỏ" },
                    { "MTK013", new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS010", new DateTime(2026, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 150000m, "43A-102.53", "Xe máy" },
                    { "MTK014", new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS010", new DateTime(2026, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 500000m, "43C-502.53", "Ô tô lớn" },
                    { "MTK015", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "CUS011", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 150000m, "43A-505.34", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "CheckInTime", "CheckOutTime", "CustomerId", "Fee", "SlotId", "Status", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "TKT0004", new DateTime(2026, 4, 1, 8, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 14, 45, 0, 0, DateTimeKind.Unspecified), "CUS002", 5000m, "A40", "Đã ra", "43A-816.83", "Xe máy" },
                    { "TKT0005", new DateTime(2026, 4, 1, 16, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 1, 20, 35, 0, 0, DateTimeKind.Unspecified), "CUS011", 5000m, "A45", "Đã ra", "43A-505.34", "Xe máy" },
                    { "TKT0007", new DateTime(2026, 4, 1, 19, 46, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 1, 52, 0, 0, DateTimeKind.Unspecified), "CUS018", 5000m, "A47", "Đã ra", "43A-790.42", "Xe máy" },
                    { "TKT0011", new DateTime(2026, 4, 2, 16, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 18, 21, 0, 0, DateTimeKind.Unspecified), "CUS006", 15000m, "B41", "Đã ra", "43B-422.72", "Ô tô nhỏ" },
                    { "TKT0012", new DateTime(2026, 4, 2, 18, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 3, 28, 0, 0, DateTimeKind.Unspecified), "CUS001", 5000m, "A12", "Đã ra", "43A-163.78", "Xe máy" },
                    { "TKT0015", new DateTime(2026, 4, 3, 7, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 9, 49, 0, 0, DateTimeKind.Unspecified), "CUS010", 25000m, "C10", "Đã ra", "43C-502.53", "Ô tô lớn" },
                    { "TKT0021", new DateTime(2026, 4, 3, 10, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 12, 39, 0, 0, DateTimeKind.Unspecified), "CUS020", 25000m, "C16", "Đã ra", "43C-164.35", "Ô tô lớn" },
                    { "TKT0024", new DateTime(2026, 4, 3, 9, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 19, 17, 0, 0, DateTimeKind.Unspecified), "CUS017", 5000m, "A18", "Đã ra", "43A-117.87", "Xe máy" },
                    { "TKT0031", new DateTime(2026, 4, 4, 18, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 3, 54, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B41", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0035", new DateTime(2026, 4, 5, 14, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 21, 4, 0, 0, DateTimeKind.Unspecified), "CUS022", 5000m, "A39", "Đã ra", "43A-791.11", "Xe máy" },
                    { "TKT0037", new DateTime(2026, 4, 5, 7, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 15, 54, 0, 0, DateTimeKind.Unspecified), "CUS013", 5000m, "A03", "Đã ra", "43A-866.67", "Xe máy" },
                    { "TKT0043", new DateTime(2026, 4, 6, 7, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 15, 35, 0, 0, DateTimeKind.Unspecified), "CUS002", 5000m, "A32", "Đã ra", "43A-816.83", "Xe máy" },
                    { "TKT0045", new DateTime(2026, 4, 6, 19, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 21, 31, 0, 0, DateTimeKind.Unspecified), "CUS019", 5000m, "A41", "Đã ra", "43A-436.67", "Xe máy" },
                    { "TKT0052", new DateTime(2026, 4, 6, 13, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 19, 15, 0, 0, DateTimeKind.Unspecified), "CUS010", 25000m, "C09", "Đã ra", "43C-502.53", "Ô tô lớn" },
                    { "TKT0054", new DateTime(2026, 4, 6, 19, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 21, 41, 0, 0, DateTimeKind.Unspecified), "CUS016", 5000m, "A11", "Đã ra", "43A-405.11", "Xe máy" },
                    { "TKT0058", new DateTime(2026, 4, 7, 10, 48, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 12, 24, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A14", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0059", new DateTime(2026, 4, 7, 13, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 7, 16, 2, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B10", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0068", new DateTime(2026, 4, 8, 9, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 17, 58, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C01", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0070", new DateTime(2026, 4, 8, 13, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 15, 7, 0, 0, DateTimeKind.Unspecified), "CUS015", 25000m, "C03", "Đã ra", "43C-259.18", "Ô tô lớn" },
                    { "TKT0075", new DateTime(2026, 4, 9, 15, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 22, 33, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B40", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0076", new DateTime(2026, 4, 9, 8, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 16, 53, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A18", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0077", new DateTime(2026, 4, 9, 16, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 22, 6, 0, 0, DateTimeKind.Unspecified), "CUS015", 5000m, "A14", "Đã ra", "43A-582.73", "Xe máy" },
                    { "TKT0078", new DateTime(2026, 4, 9, 12, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 13, 50, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A36", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0080", new DateTime(2026, 4, 9, 8, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 15, 24, 0, 0, DateTimeKind.Unspecified), "CUS004", 5000m, "A04", "Đã ra", "43A-294.11", "Xe máy" },
                    { "TKT0081", new DateTime(2026, 4, 9, 7, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 9, 15, 16, 0, 0, DateTimeKind.Unspecified), "CUS027", 5000m, "A38", "Đã ra", "43A-938.25", "Xe máy" },
                    { "TKT0083", new DateTime(2026, 4, 9, 18, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 2, 39, 0, 0, DateTimeKind.Unspecified), "CUS015", 5000m, "A46", "Đã ra", "43A-582.73", "Xe máy" },
                    { "TKT0086", new DateTime(2026, 4, 9, 17, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 2, 16, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C12", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0091", new DateTime(2026, 4, 10, 6, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 10, 2, 0, 0, DateTimeKind.Unspecified), "CUS023", 5000m, "A41", "Đã ra", "43A-743.15", "Xe máy" },
                    { "TKT0092", new DateTime(2026, 4, 10, 18, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 19, 46, 0, 0, DateTimeKind.Unspecified), "CUS027", 15000m, "B15", "Đã ra", "43B-160.85", "Ô tô nhỏ" },
                    { "TKT0093", new DateTime(2026, 4, 10, 8, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 16, 12, 0, 0, DateTimeKind.Unspecified), "CUS013", 5000m, "A12", "Đã ra", "43A-866.67", "Xe máy" },
                    { "TKT0094", new DateTime(2026, 4, 10, 15, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 20, 15, 0, 0, DateTimeKind.Unspecified), "CUS017", 5000m, "A50", "Đã ra", "43A-117.87", "Xe máy" },
                    { "TKT0101", new DateTime(2026, 4, 11, 19, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 4, 34, 0, 0, DateTimeKind.Unspecified), "CUS017", 5000m, "A48", "Đã ra", "43A-117.87", "Xe máy" },
                    { "TKT0105", new DateTime(2026, 4, 11, 15, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 1, 38, 0, 0, DateTimeKind.Unspecified), "CUS008", 5000m, "A29", "Đã ra", "43A-456.31", "Xe máy" },
                    { "TKT0106", new DateTime(2026, 4, 11, 17, 38, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 11, 22, 45, 0, 0, DateTimeKind.Unspecified), "CUS011", 5000m, "A17", "Đã ra", "43A-505.34", "Xe máy" },
                    { "TKT0109", new DateTime(2026, 4, 12, 17, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 1, 39, 0, 0, DateTimeKind.Unspecified), "CUS025", 25000m, "C10", "Đã ra", "43C-653.47", "Ô tô lớn" },
                    { "TKT0112", new DateTime(2026, 4, 12, 10, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 20, 0, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A44", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0113", new DateTime(2026, 4, 12, 12, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 15, 9, 0, 0, DateTimeKind.Unspecified), "CUS020", 5000m, "A33", "Đã ra", "43A-333.16", "Xe máy" },
                    { "TKT0115", new DateTime(2026, 4, 13, 13, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 16, 18, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B43", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0119", new DateTime(2026, 4, 13, 16, 53, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 1, 59, 0, 0, DateTimeKind.Unspecified), "CUS017", 5000m, "A17", "Đã ra", "43A-117.87", "Xe máy" },
                    { "TKT0120", new DateTime(2026, 4, 13, 16, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 22, 58, 0, 0, DateTimeKind.Unspecified), "CUS030", 5000m, "A36", "Đã ra", "43A-860.80", "Xe máy" },
                    { "TKT0125", new DateTime(2026, 4, 13, 7, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 15, 53, 0, 0, DateTimeKind.Unspecified), "CUS012", 15000m, "B03", "Đã ra", "43B-432.85", "Ô tô nhỏ" },
                    { "TKT0128", new DateTime(2026, 4, 13, 14, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 18, 14, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A39", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0131", new DateTime(2026, 4, 14, 15, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 20, 5, 0, 0, DateTimeKind.Unspecified), "CUS021", 15000m, "B49", "Đã ra", "43B-589.63", "Ô tô nhỏ" },
                    { "TKT0133", new DateTime(2026, 4, 14, 12, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 15, 41, 0, 0, DateTimeKind.Unspecified), "CUS003", 5000m, "A45", "Đã ra", "43A-761.33", "Xe máy" },
                    { "TKT0135", new DateTime(2026, 4, 14, 19, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 4, 47, 0, 0, DateTimeKind.Unspecified), "CUS022", 5000m, "A45", "Đã ra", "43A-791.11", "Xe máy" },
                    { "TKT0136", new DateTime(2026, 4, 14, 7, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 16, 5, 0, 0, DateTimeKind.Unspecified), "CUS018", 15000m, "B41", "Đã ra", "43B-763.73", "Ô tô nhỏ" },
                    { "TKT0137", new DateTime(2026, 4, 14, 16, 32, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 19, 59, 0, 0, DateTimeKind.Unspecified), "CUS002", 5000m, "A34", "Đã ra", "43A-816.83", "Xe máy" },
                    { "TKT0139", new DateTime(2026, 4, 14, 10, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 14, 14, 57, 0, 0, DateTimeKind.Unspecified), "CUS016", 5000m, "A49", "Đã ra", "43A-405.11", "Xe máy" },
                    { "TKT0141", new DateTime(2026, 4, 14, 15, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 0, 29, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A23", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0145", new DateTime(2026, 4, 15, 17, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 22, 38, 0, 0, DateTimeKind.Unspecified), "CUS018", 5000m, "A10", "Đã ra", "43A-790.42", "Xe máy" },
                    { "TKT0151", new DateTime(2026, 4, 15, 10, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 14, 53, 0, 0, DateTimeKind.Unspecified), "CUS010", 25000m, "C17", "Đã ra", "43C-502.53", "Ô tô lớn" },
                    { "TKT0152", new DateTime(2026, 4, 15, 13, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 21, 44, 0, 0, DateTimeKind.Unspecified), "CUS018", 5000m, "A32", "Đã ra", "43A-790.42", "Xe máy" },
                    { "TKT0154", new DateTime(2026, 4, 15, 13, 52, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 17, 26, 0, 0, DateTimeKind.Unspecified), "CUS005", 25000m, "C18", "Đã ra", "43C-897.86", "Ô tô lớn" },
                    { "TKT0156", new DateTime(2026, 4, 15, 7, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 15, 10, 56, 0, 0, DateTimeKind.Unspecified), "CUS006", 5000m, "A02", "Đã ra", "43A-193.57", "Xe máy" },
                    { "TKT0157", new DateTime(2026, 4, 16, 10, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 14, 2, 0, 0, DateTimeKind.Unspecified), "CUS006", 15000m, "B30", "Đã ra", "43B-422.72", "Ô tô nhỏ" },
                    { "TKT0158", new DateTime(2026, 4, 16, 18, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 20, 34, 0, 0, DateTimeKind.Unspecified), "CUS010", 25000m, "C02", "Đã ra", "43C-502.53", "Ô tô lớn" },
                    { "TKT0164", new DateTime(2026, 4, 16, 9, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 16, 6, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A11", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0167", new DateTime(2026, 4, 16, 15, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 19, 52, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B45", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0172", new DateTime(2026, 4, 17, 8, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 11, 21, 0, 0, DateTimeKind.Unspecified), "CUS011", 5000m, "A27", "Đã ra", "43A-505.34", "Xe máy" },
                    { "TKT0173", new DateTime(2026, 4, 17, 15, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 17, 52, 0, 0, DateTimeKind.Unspecified), "CUS003", 5000m, "A07", "Đã ra", "43A-761.33", "Xe máy" },
                    { "TKT0176", new DateTime(2026, 4, 17, 12, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 18, 36, 0, 0, DateTimeKind.Unspecified), "CUS009", 5000m, "A08", "Đã ra", "43A-289.56", "Xe máy" },
                    { "TKT0179", new DateTime(2026, 4, 17, 17, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 2, 46, 0, 0, DateTimeKind.Unspecified), "CUS020", 5000m, "A33", "Đã ra", "43A-333.16", "Xe máy" },
                    { "TKT0181", new DateTime(2026, 4, 17, 8, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 17, 10, 27, 0, 0, DateTimeKind.Unspecified), "CUS015", 25000m, "C04", "Đã ra", "43C-259.18", "Ô tô lớn" },
                    { "TKT0183", new DateTime(2026, 4, 17, 15, 39, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 0, 44, 0, 0, DateTimeKind.Unspecified), "CUS012", 15000m, "B30", "Đã ra", "43B-432.85", "Ô tô nhỏ" },
                    { "TKT0186", new DateTime(2026, 4, 18, 15, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 23, 11, 0, 0, DateTimeKind.Unspecified), "CUS016", 5000m, "A28", "Đã ra", "43A-405.11", "Xe máy" },
                    { "TKT0188", new DateTime(2026, 4, 18, 18, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 21, 30, 0, 0, DateTimeKind.Unspecified), "CUS018", 15000m, "B24", "Đã ra", "43B-763.73", "Ô tô nhỏ" },
                    { "TKT0190", new DateTime(2026, 4, 18, 8, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 12, 28, 0, 0, DateTimeKind.Unspecified), "CUS021", 5000m, "A41", "Đã ra", "43A-890.60", "Xe máy" },
                    { "TKT0192", new DateTime(2026, 4, 18, 12, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 14, 7, 0, 0, DateTimeKind.Unspecified), "CUS008", 5000m, "A34", "Đã ra", "43A-456.31", "Xe máy" },
                    { "TKT0193", new DateTime(2026, 4, 18, 14, 19, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 22, 17, 0, 0, DateTimeKind.Unspecified), "CUS013", 5000m, "A16", "Đã ra", "43A-866.67", "Xe máy" },
                    { "TKT0194", new DateTime(2026, 4, 19, 15, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 21, 28, 0, 0, DateTimeKind.Unspecified), "CUS010", 25000m, "C06", "Đã ra", "43C-502.53", "Ô tô lớn" },
                    { "TKT0195", new DateTime(2026, 4, 19, 19, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 1, 16, 0, 0, DateTimeKind.Unspecified), "CUS002", 5000m, "A13", "Đã ra", "43A-816.83", "Xe máy" },
                    { "TKT0196", new DateTime(2026, 4, 19, 9, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 13, 8, 0, 0, DateTimeKind.Unspecified), "CUS007", 5000m, "A32", "Đã ra", "43A-657.75", "Xe máy" },
                    { "TKT0197", new DateTime(2026, 4, 19, 17, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 21, 4, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B05", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0200", new DateTime(2026, 4, 19, 12, 29, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 19, 13, 46, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B30", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0202", new DateTime(2026, 4, 20, 10, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 13, 1, 0, 0, DateTimeKind.Unspecified), "CUS030", 5000m, "A08", "Đã ra", "43A-860.80", "Xe máy" },
                    { "TKT0203", new DateTime(2026, 4, 20, 6, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 11, 56, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C09", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0204", new DateTime(2026, 4, 20, 7, 2, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 8, 35, 0, 0, DateTimeKind.Unspecified), "CUS005", 5000m, "A14", "Đã ra", "43A-766.27", "Xe máy" },
                    { "TKT0207", new DateTime(2026, 4, 21, 8, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 12, 21, 0, 0, DateTimeKind.Unspecified), "CUS012", 15000m, "B05", "Đã ra", "43B-432.85", "Ô tô nhỏ" },
                    { "TKT0210", new DateTime(2026, 4, 21, 15, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 19, 31, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A37", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0214", new DateTime(2026, 4, 21, 15, 57, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 20, 19, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B11", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0217", new DateTime(2026, 4, 21, 12, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 21, 14, 35, 0, 0, DateTimeKind.Unspecified), "CUS023", 5000m, "A25", "Đã ra", "43A-743.15", "Xe máy" },
                    { "TKT0218", new DateTime(2026, 4, 22, 16, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 23, 36, 0, 0, DateTimeKind.Unspecified), "CUS003", 5000m, "A03", "Đã ra", "43A-761.33", "Xe máy" },
                    { "TKT0220", new DateTime(2026, 4, 22, 9, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 12, 30, 0, 0, DateTimeKind.Unspecified), "CUS025", 5000m, "A16", "Đã ra", "43A-329.51", "Xe máy" },
                    { "TKT0223", new DateTime(2026, 4, 22, 11, 42, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 13, 44, 0, 0, DateTimeKind.Unspecified), "CUS016", 5000m, "A11", "Đã ra", "43A-405.11", "Xe máy" },
                    { "TKT0224", new DateTime(2026, 4, 22, 10, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 15, 20, 0, 0, DateTimeKind.Unspecified), "CUS027", 15000m, "B20", "Đã ra", "43B-160.85", "Ô tô nhỏ" },
                    { "TKT0226", new DateTime(2026, 4, 23, 8, 4, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 17, 13, 0, 0, DateTimeKind.Unspecified), "CUS026", 5000m, "A02", "Đã ra", "43A-176.82", "Xe máy" },
                    { "TKT0228", new DateTime(2026, 4, 23, 6, 31, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 8, 14, 0, 0, DateTimeKind.Unspecified), "CUS020", 5000m, "A03", "Đã ra", "43A-333.16", "Xe máy" },
                    { "TKT0232", new DateTime(2026, 4, 23, 14, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 15, 18, 0, 0, DateTimeKind.Unspecified), "CUS005", 5000m, "A49", "Đã ra", "43A-766.27", "Xe máy" },
                    { "TKT0233", new DateTime(2026, 4, 23, 11, 59, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 18, 33, 0, 0, DateTimeKind.Unspecified), "CUS027", 15000m, "B09", "Đã ra", "43B-160.85", "Ô tô nhỏ" },
                    { "TKT0234", new DateTime(2026, 4, 23, 18, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 23, 20, 8, 0, 0, DateTimeKind.Unspecified), "CUS008", 5000m, "A50", "Đã ra", "43A-456.31", "Xe máy" },
                    { "TKT0243", new DateTime(2026, 4, 24, 18, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 21, 43, 0, 0, DateTimeKind.Unspecified), "CUS025", 5000m, "A38", "Đã ra", "43A-329.51", "Xe máy" },
                    { "TKT0244", new DateTime(2026, 4, 24, 6, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 15, 56, 0, 0, DateTimeKind.Unspecified), "CUS008", 5000m, "A19", "Đã ra", "43A-456.31", "Xe máy" },
                    { "TKT0252", new DateTime(2026, 4, 25, 6, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 11, 42, 0, 0, DateTimeKind.Unspecified), "CUS002", 5000m, "A46", "Đã ra", "43A-816.83", "Xe máy" },
                    { "TKT0253", new DateTime(2026, 4, 25, 15, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 17, 33, 0, 0, DateTimeKind.Unspecified), "CUS004", 5000m, "A48", "Đã ra", "43A-294.11", "Xe máy" },
                    { "TKT0254", new DateTime(2026, 4, 25, 7, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 9, 51, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C14", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0255", new DateTime(2026, 4, 25, 15, 22, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 16, 35, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B27", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0256", new DateTime(2026, 4, 25, 15, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 23, 46, 0, 0, DateTimeKind.Unspecified), "CUS013", 5000m, "A37", "Đã ra", "43A-866.67", "Xe máy" },
                    { "TKT0258", new DateTime(2026, 4, 26, 19, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 22, 35, 0, 0, DateTimeKind.Unspecified), "CUS017", 5000m, "A16", "Đã ra", "43A-117.87", "Xe máy" },
                    { "TKT0263", new DateTime(2026, 4, 26, 16, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 26, 20, 44, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C11", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0265", new DateTime(2026, 4, 26, 18, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 1, 22, 0, 0, DateTimeKind.Unspecified), "CUS026", 5000m, "A31", "Đã ra", "43A-176.82", "Xe máy" },
                    { "TKT0272", new DateTime(2026, 4, 27, 8, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 11, 23, 0, 0, DateTimeKind.Unspecified), "CUS016", 5000m, "A30", "Đã ra", "43A-405.11", "Xe máy" },
                    { "TKT0274", new DateTime(2026, 4, 27, 15, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 21, 57, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A31", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0283", new DateTime(2026, 4, 28, 9, 8, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 13, 6, 0, 0, DateTimeKind.Unspecified), "CUS006", 5000m, "A40", "Đã ra", "43A-193.57", "Xe máy" },
                    { "TKT0285", new DateTime(2026, 4, 28, 12, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 21, 28, 0, 0, DateTimeKind.Unspecified), "CUS001", 5000m, "A36", "Đã ra", "43A-163.78", "Xe máy" },
                    { "TKT0286", new DateTime(2026, 4, 28, 15, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 20, 29, 0, 0, DateTimeKind.Unspecified), "CUS021", 5000m, "A03", "Đã ra", "43A-890.60", "Xe máy" },
                    { "TKT0293", new DateTime(2026, 4, 29, 6, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 13, 46, 0, 0, DateTimeKind.Unspecified), "CUS009", 5000m, "A50", "Đã ra", "43A-289.56", "Xe máy" },
                    { "TKT0295", new DateTime(2026, 4, 29, 6, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 10, 26, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B42", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0296", new DateTime(2026, 4, 30, 9, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 10, 52, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B22", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0297", new DateTime(2026, 4, 30, 13, 55, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 15, 57, 0, 0, DateTimeKind.Unspecified), "CUS003", 15000m, "B34", "Đã ra", "43B-554.30", "Ô tô nhỏ" },
                    { "TKT0298", new DateTime(2026, 4, 30, 19, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 4, 5, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B49", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0305", new DateTime(2026, 4, 30, 7, 25, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 12, 58, 0, 0, DateTimeKind.Unspecified), "CUS015", 25000m, "C08", "Đã ra", "43C-259.18", "Ô tô lớn" },
                    { "TKT0311", new DateTime(2026, 5, 1, 16, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 21, 53, 0, 0, DateTimeKind.Unspecified), "CUS009", 15000m, "B15", "Đã ra", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0312", new DateTime(2026, 5, 1, 18, 36, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 21, 27, 0, 0, DateTimeKind.Unspecified), "CUS018", 5000m, "A38", "Đã ra", "43A-790.42", "Xe máy" },
                    { "TKT0315", new DateTime(2026, 5, 1, 9, 3, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 15, 0, 0, 0, DateTimeKind.Unspecified), "CUS028", 5000m, "A43", "Đã ra", "43A-349.66", "Xe máy" },
                    { "TKT0317", new DateTime(2026, 5, 1, 15, 54, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 1, 38, 0, 0, DateTimeKind.Unspecified), "CUS021", 5000m, "A15", "Đã ra", "43A-890.60", "Xe máy" },
                    { "TKT0322", new DateTime(2026, 5, 2, 11, 34, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 21, 1, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A46", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0325", new DateTime(2026, 5, 2, 6, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 16, 6, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A45", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0330", new DateTime(2026, 5, 2, 16, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 18, 1, 0, 0, DateTimeKind.Unspecified), "CUS027", 15000m, "B43", "Đã ra", "43B-160.85", "Ô tô nhỏ" },
                    { "TKT0331", new DateTime(2026, 5, 3, 13, 33, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 16, 5, 0, 0, DateTimeKind.Unspecified), "CUS011", 5000m, "A45", "Đã ra", "43A-505.34", "Xe máy" },
                    { "TKT0335", new DateTime(2026, 5, 3, 14, 56, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 18, 53, 0, 0, DateTimeKind.Unspecified), "CUS015", 25000m, "C12", "Đã ra", "43C-259.18", "Ô tô lớn" },
                    { "TKT0337", new DateTime(2026, 5, 4, 11, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 17, 52, 0, 0, DateTimeKind.Unspecified), "CUS025", 25000m, "C18", "Đã ra", "43C-653.47", "Ô tô lớn" },
                    { "TKT0338", new DateTime(2026, 5, 4, 15, 26, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 21, 35, 0, 0, DateTimeKind.Unspecified), "CUS025", 25000m, "C07", "Đã ra", "43C-653.47", "Ô tô lớn" },
                    { "TKT0343", new DateTime(2026, 5, 4, 17, 21, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 22, 20, 0, 0, DateTimeKind.Unspecified), "CUS024", 15000m, "B28", "Đã ra", "43B-452.36", "Ô tô nhỏ" },
                    { "TKT0346", new DateTime(2026, 5, 5, 14, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 17, 1, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C01", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0349", new DateTime(2026, 5, 5, 15, 6, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 0, 43, 0, 0, DateTimeKind.Unspecified), "CUS005", 25000m, "C08", "Đã ra", "43C-897.86", "Ô tô lớn" },
                    { "TKT0350", new DateTime(2026, 5, 5, 8, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 15, 25, 0, 0, DateTimeKind.Unspecified), "CUS003", 5000m, "A05", "Đã ra", "43A-761.33", "Xe máy" },
                    { "TKT0353", new DateTime(2026, 5, 6, 12, 49, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 14, 19, 0, 0, DateTimeKind.Unspecified), "CUS025", 25000m, "C07", "Đã ra", "43C-653.47", "Ô tô lớn" },
                    { "TKT0357", new DateTime(2026, 5, 6, 16, 13, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 23, 10, 0, 0, DateTimeKind.Unspecified), "CUS014", 5000m, "A49", "Đã ra", "43A-496.54", "Xe máy" },
                    { "TKT0358", new DateTime(2026, 5, 6, 16, 28, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 22, 35, 0, 0, DateTimeKind.Unspecified), "CUS030", 25000m, "C16", "Đã ra", "43C-325.85", "Ô tô lớn" },
                    { "TKT0365", new DateTime(2026, 5, 7, 16, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 23, 13, 0, 0, DateTimeKind.Unspecified), "CUS005", 25000m, "C17", "Đã ra", "43C-897.86", "Ô tô lớn" },
                    { "TKT0370", new DateTime(2026, 5, 7, 13, 58, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 19, 44, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A22", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0371", new DateTime(2026, 5, 7, 11, 17, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 20, 40, 0, 0, DateTimeKind.Unspecified), "CUS003", 5000m, "A17", "Đã ra", "43A-761.33", "Xe máy" },
                    { "TKT0374", new DateTime(2026, 5, 8, 8, 16, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 17, 5, 0, 0, DateTimeKind.Unspecified), "CUS028", 5000m, "A31", "Đã ra", "43A-349.66", "Xe máy" },
                    { "TKT0375", new DateTime(2026, 5, 8, 6, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 8, 42, 0, 0, DateTimeKind.Unspecified), "CUS006", 5000m, "A26", "Đã ra", "43A-193.57", "Xe máy" },
                    { "TKT0376", new DateTime(2026, 5, 8, 9, 51, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 14, 56, 0, 0, DateTimeKind.Unspecified), "CUS001", 5000m, "A41", "Đã ra", "43A-163.78", "Xe máy" },
                    { "TKT0377", new DateTime(2026, 5, 8, 6, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 9, 25, 0, 0, DateTimeKind.Unspecified), "CUS004", 5000m, "A30", "Đã ra", "43A-294.11", "Xe máy" },
                    { "TKT0383", new DateTime(2026, 5, 9, 14, 11, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 16, 41, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A47", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0384", new DateTime(2026, 5, 9, 19, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 3, 57, 0, 0, DateTimeKind.Unspecified), "CUS026", 5000m, "A01", "Đã ra", "43A-176.82", "Xe máy" },
                    { "TKT0386", new DateTime(2026, 5, 9, 17, 24, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 2, 9, 0, 0, DateTimeKind.Unspecified), "CUS018", 5000m, "A46", "Đã ra", "43A-790.42", "Xe máy" },
                    { "TKT0387", new DateTime(2026, 5, 9, 16, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 20, 34, 0, 0, DateTimeKind.Unspecified), "CUS001", 5000m, "A40", "Đã ra", "43A-163.78", "Xe máy" },
                    { "TKT0388", new DateTime(2026, 5, 10, 18, 41, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 20, 21, 0, 0, DateTimeKind.Unspecified), "CUS030", 5000m, "A27", "Đã ra", "43A-860.80", "Xe máy" },
                    { "TKT0393", new DateTime(2026, 5, 10, 15, 37, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 22, 1, 0, 0, DateTimeKind.Unspecified), "CUS012", 15000m, "B30", "Đã ra", "43B-432.85", "Ô tô nhỏ" },
                    { "TKT0394", new DateTime(2026, 5, 10, 11, 27, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 13, 5, 0, 0, DateTimeKind.Unspecified), "CUS023", 5000m, "A12", "Đã ra", "43A-743.15", "Xe máy" },
                    { "TKT0400", new DateTime(2026, 5, 10, 17, 43, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 0, 44, 0, 0, DateTimeKind.Unspecified), "CUS003", 5000m, "A42", "Đã ra", "43A-761.33", "Xe máy" },
                    { "TKT0403", new DateTime(2026, 5, 11, 14, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 23, 44, 0, 0, DateTimeKind.Unspecified), "CUS030", 15000m, "B31", "Đã ra", "43B-472.76", "Ô tô nhỏ" },
                    { "TKT0404", new DateTime(2026, 5, 11, 14, 7, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 18, 36, 0, 0, DateTimeKind.Unspecified), "CUS004", 5000m, "A31", "Đã ra", "43A-294.11", "Xe máy" },
                    { "TKT0405", new DateTime(2026, 5, 11, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 16, 57, 0, 0, DateTimeKind.Unspecified), "CUS029", 5000m, "A10", "Đã ra", "43A-679.92", "Xe máy" },
                    { "TKT0408", new DateTime(2026, 5, 11, 14, 9, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 15, 54, 0, 0, DateTimeKind.Unspecified), "CUS011", 5000m, "A44", "Đã ra", "43A-505.34", "Xe máy" },
                    { "TKT0409", new DateTime(2026, 5, 11, 6, 47, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 13, 4, 0, 0, DateTimeKind.Unspecified), "CUS007", 5000m, "A30", "Đã ra", "43A-657.75", "Xe máy" },
                    { "TKT0412", new DateTime(2026, 5, 11, 9, 18, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 19, 6, 0, 0, DateTimeKind.Unspecified), "CUS025", 25000m, "C20", "Đã ra", "43C-653.47", "Ô tô lớn" },
                    { "TKT0414", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS001", 0m, "A01", "Đang trong bãi", "43A-163.78", "Xe máy" },
                    { "TKT0415", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS002", 0m, "A02", "Đang trong bãi", "43A-816.83", "Xe máy" },
                    { "TKT0416", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS003", 0m, "A03", "Đang trong bãi", "43A-761.33", "Xe máy" },
                    { "TKT0417", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS003", 0m, "B04", "Đang trong bãi", "43B-554.30", "Ô tô nhỏ" },
                    { "TKT0418", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS004", 0m, "A05", "Đang trong bãi", "43A-294.11", "Xe máy" },
                    { "TKT0419", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS005", 0m, "A06", "Đang trong bãi", "43A-766.27", "Xe máy" },
                    { "TKT0420", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS005", 0m, "C07", "Đang trong bãi", "43C-897.86", "Ô tô lớn" },
                    { "TKT0421", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS006", 0m, "A08", "Đang trong bãi", "43A-193.57", "Xe máy" },
                    { "TKT0422", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS006", 0m, "B09", "Đang trong bãi", "43B-422.72", "Ô tô nhỏ" },
                    { "TKT0423", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS007", 0m, "A10", "Đang trong bãi", "43A-657.75", "Xe máy" },
                    { "TKT0424", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS008", 0m, "A11", "Đang trong bãi", "43A-456.31", "Xe máy" },
                    { "TKT0425", new DateTime(2026, 5, 12, 7, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS009", 0m, "A12", "Đang trong bãi", "43A-289.56", "Xe máy" },
                    { "TKT0426", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS009", 0m, "B13", "Đang trong bãi", "43B-536.32", "Ô tô nhỏ" },
                    { "TKT0427", new DateTime(2026, 5, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS010", 0m, "A14", "Đang trong bãi", "43A-102.53", "Xe máy" },
                    { "TKT0428", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), null, "CUS010", 0m, "C15", "Đang trong bãi", "43C-502.53", "Ô tô lớn" }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Method", "MonthlyTicketId", "PaymentTime", "Status", "TicketId" },
                values: new object[,]
                {
                    { "PAY0004", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 1, 14, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0004" },
                    { "PAY0005", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 1, 20, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0005" },
                    { "PAY0007", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 2, 1, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0007" },
                    { "PAY0011", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 2, 18, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0011" },
                    { "PAY0012", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 3, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0012" },
                    { "PAY0015", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 9, 49, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0015" },
                    { "PAY0021", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 3, 12, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0021" },
                    { "PAY0024", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 3, 19, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0024" },
                    { "PAY0031", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 5, 3, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0031" },
                    { "PAY0035", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 5, 21, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0035" },
                    { "PAY0037", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 5, 15, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0037" },
                    { "PAY0043", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 6, 15, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0043" },
                    { "PAY0045", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 21, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0045" },
                    { "PAY0052", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 19, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0052" },
                    { "PAY0054", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 6, 21, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0054" },
                    { "PAY0058", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 12, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0058" },
                    { "PAY0059", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 7, 16, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0059" },
                    { "PAY0068", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 17, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0068" },
                    { "PAY0070", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 8, 15, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0070" },
                    { "PAY0075", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 22, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0075" },
                    { "PAY0076", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 16, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0076" },
                    { "PAY0077", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 22, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0077" },
                    { "PAY0078", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 13, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0078" },
                    { "PAY0080", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 9, 15, 24, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0080" },
                    { "PAY0081", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 9, 15, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0081" },
                    { "PAY0083", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 2, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0083" },
                    { "PAY0086", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 2, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0086" },
                    { "PAY0091", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 10, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0091" },
                    { "PAY0092", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 19, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0092" },
                    { "PAY0093", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 16, 12, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0093" },
                    { "PAY0094", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 10, 20, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0094" },
                    { "PAY0101", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 12, 4, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0101" },
                    { "PAY0105", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 1, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0105" },
                    { "PAY0106", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 11, 22, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0106" },
                    { "PAY0109", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 1, 39, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0109" },
                    { "PAY0112", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 20, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0112" },
                    { "PAY0113", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 15, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0113" },
                    { "PAY0115", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 16, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0115" },
                    { "PAY0119", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 1, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0119" },
                    { "PAY0120", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 22, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0120" },
                    { "PAY0125", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 15, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0125" },
                    { "PAY0128", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 13, 18, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0128" },
                    { "PAY0131", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 20, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0131" },
                    { "PAY0133", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 15, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0133" },
                    { "PAY0135", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 4, 47, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0135" },
                    { "PAY0136", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 16, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0136" },
                    { "PAY0137", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 14, 19, 59, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0137" },
                    { "PAY0139", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 14, 14, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0139" },
                    { "PAY0141", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 0, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0141" },
                    { "PAY0145", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 22, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0145" },
                    { "PAY0151", 25000m, "Chuyển khoản", null, new DateTime(2026, 4, 15, 14, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0151" },
                    { "PAY0152", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 21, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0152" },
                    { "PAY0154", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 17, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0154" },
                    { "PAY0156", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 15, 10, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0156" },
                    { "PAY0157", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 14, 2, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0157" },
                    { "PAY0158", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 20, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0158" },
                    { "PAY0164", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 16, 16, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0164" },
                    { "PAY0167", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 16, 19, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0167" },
                    { "PAY0172", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 11, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0172" },
                    { "PAY0173", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 17, 17, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0173" },
                    { "PAY0176", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 18, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0176" },
                    { "PAY0179", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 2, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0179" },
                    { "PAY0181", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 17, 10, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0181" },
                    { "PAY0183", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 0, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0183" },
                    { "PAY0186", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 23, 11, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0186" },
                    { "PAY0188", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 18, 21, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0188" },
                    { "PAY0190", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 12, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0190" },
                    { "PAY0192", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 14, 7, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0192" },
                    { "PAY0193", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 18, 22, 17, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0193" },
                    { "PAY0194", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 21, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0194" },
                    { "PAY0195", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 1, 16, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0195" },
                    { "PAY0196", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 13, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0196" },
                    { "PAY0197", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 21, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0197" },
                    { "PAY0200", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 19, 13, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0200" },
                    { "PAY0202", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 20, 13, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0202" },
                    { "PAY0203", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 11, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0203" },
                    { "PAY0204", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 20, 8, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0204" },
                    { "PAY0207", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 12, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0207" },
                    { "PAY0210", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 19, 31, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0210" },
                    { "PAY0214", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 20, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0214" },
                    { "PAY0217", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 21, 14, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0217" },
                    { "PAY0218", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 22, 23, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0218" },
                    { "PAY0220", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 22, 12, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0220" },
                    { "PAY0223", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 22, 13, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0223" },
                    { "PAY0224", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 22, 15, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0224" },
                    { "PAY0226", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 17, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0226" },
                    { "PAY0228", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 8, 14, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0228" },
                    { "PAY0232", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 15, 18, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0232" },
                    { "PAY0233", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 23, 18, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0233" },
                    { "PAY0234", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 23, 20, 8, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0234" },
                    { "PAY0243", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 24, 21, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0243" },
                    { "PAY0244", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 24, 15, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0244" },
                    { "PAY0252", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 11, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0252" },
                    { "PAY0253", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 25, 17, 33, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0253" },
                    { "PAY0254", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 9, 51, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0254" },
                    { "PAY0255", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 25, 16, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0255" },
                    { "PAY0256", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 23, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0256" },
                    { "PAY0258", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 22, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0258" },
                    { "PAY0263", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 26, 20, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0263" },
                    { "PAY0265", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 1, 22, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0265" },
                    { "PAY0272", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 11, 23, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0272" },
                    { "PAY0274", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 27, 21, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0274" },
                    { "PAY0283", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 13, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0283" },
                    { "PAY0285", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 21, 28, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0285" },
                    { "PAY0286", 5000m, "Chuyển khoản", null, new DateTime(2026, 4, 28, 20, 29, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0286" },
                    { "PAY0293", 5000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 13, 46, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0293" },
                    { "PAY0295", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 29, 10, 26, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0295" },
                    { "PAY0296", 15000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 10, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0296" },
                    { "PAY0297", 15000m, "Chuyển khoản", null, new DateTime(2026, 4, 30, 15, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0297" },
                    { "PAY0298", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 4, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0298" },
                    { "PAY0305", 25000m, "Tiền mặt", null, new DateTime(2026, 4, 30, 12, 58, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0305" },
                    { "PAY0311", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 21, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0311" },
                    { "PAY0312", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 1, 21, 27, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0312" },
                    { "PAY0315", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 15, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0315" },
                    { "PAY0317", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 1, 38, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0317" },
                    { "PAY0322", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 21, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0322" },
                    { "PAY0325", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 16, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0325" },
                    { "PAY0330", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 18, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0330" },
                    { "PAY0331", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 3, 16, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0331" },
                    { "PAY0335", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 18, 53, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0335" },
                    { "PAY0337", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 17, 52, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0337" },
                    { "PAY0338", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 21, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0338" },
                    { "PAY0343", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 4, 22, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0343" },
                    { "PAY0346", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 17, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0346" },
                    { "PAY0349", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 0, 43, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0349" },
                    { "PAY0350", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 5, 15, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0350" },
                    { "PAY0353", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 14, 19, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0353" },
                    { "PAY0357", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 23, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0357" },
                    { "PAY0358", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 22, 35, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0358" },
                    { "PAY0365", 25000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 23, 13, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0365" },
                    { "PAY0370", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 19, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0370" },
                    { "PAY0371", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 20, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0371" },
                    { "PAY0374", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 8, 17, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0374" },
                    { "PAY0375", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 8, 42, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0375" },
                    { "PAY0376", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 8, 14, 56, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0376" },
                    { "PAY0377", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 8, 9, 25, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0377" },
                    { "PAY0383", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 16, 41, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0383" },
                    { "PAY0384", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 3, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0384" },
                    { "PAY0386", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 2, 9, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0386" },
                    { "PAY0387", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 9, 20, 34, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0387" },
                    { "PAY0388", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 20, 21, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0388" },
                    { "PAY0393", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 10, 22, 1, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0393" },
                    { "PAY0394", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 10, 13, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0394" },
                    { "PAY0400", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 0, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0400" },
                    { "PAY0403", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 23, 44, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0403" },
                    { "PAY0404", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 18, 36, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0404" },
                    { "PAY0405", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 16, 57, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0405" },
                    { "PAY0408", 5000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 15, 54, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0408" },
                    { "PAY0409", 5000m, "Tiền mặt", null, new DateTime(2026, 5, 11, 13, 4, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0409" },
                    { "PAY0412", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 19, 6, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0412" }
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
