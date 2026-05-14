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
                name: "ShiftSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftSchedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_ShiftSchedules_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkLogs",
                columns: table => new
                {
                    WorkLogId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalMinutes = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLogs", x => x.WorkLogId);
                    table.ForeignKey(
                        name: "FK_WorkLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
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
                    { "ACC003", new DateTime(2026, 1, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "hung.levan@parking.local", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC004", new DateTime(2026, 2, 2, 8, 0, 0, 0, DateTimeKind.Unspecified), "linh.tranmai@parking.local", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC005", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "bao.doquoc@parking.local", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC006", new DateTime(2026, 2, 20, 8, 0, 0, 0, DateTimeKind.Unspecified), "nam.phanquoc@parking.local", false, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Employee" },
                    { "ACC101", new DateTime(2026, 1, 12, 8, 15, 0, 0, DateTimeKind.Unspecified), "minhanh.nguyen@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC102", new DateTime(2026, 1, 14, 9, 20, 0, 0, DateTimeKind.Unspecified), "quocbao.tran@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC103", new DateTime(2026, 1, 20, 10, 10, 0, 0, DateTimeKind.Unspecified), "hoangnam.le@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC104", new DateTime(2026, 1, 25, 15, 0, 0, 0, DateTimeKind.Unspecified), "thuha.pham@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC105", new DateTime(2026, 2, 1, 8, 45, 0, 0, DateTimeKind.Unspecified), "thanhtung.vo@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC106", new DateTime(2026, 2, 4, 11, 25, 0, 0, DateTimeKind.Unspecified), "ngocmai.dang@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC107", new DateTime(2026, 2, 7, 14, 30, 0, 0, DateTimeKind.Unspecified), "giahuy.hoang@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC108", new DateTime(2026, 2, 10, 9, 5, 0, 0, DateTimeKind.Unspecified), "khanhlinh.bui@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC109", new DateTime(2026, 2, 15, 16, 15, 0, 0, DateTimeKind.Unspecified), "duclong.nguyen@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC110", new DateTime(2026, 2, 18, 8, 35, 0, 0, DateTimeKind.Unspecified), "myduyen.truong@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC111", new DateTime(2026, 2, 21, 13, 10, 0, 0, DateTimeKind.Unspecified), "anhkhoa.phan@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC112", new DateTime(2026, 2, 24, 10, 40, 0, 0, DateTimeKind.Unspecified), "quynhchi.lam@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC113", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "nhatminh.do@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC114", new DateTime(2026, 3, 4, 14, 20, 0, 0, DateTimeKind.Unspecified), "thaovy.huynh@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC115", new DateTime(2026, 3, 8, 9, 15, 0, 0, DateTimeKind.Unspecified), "congthanh.vu@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC116", new DateTime(2026, 3, 12, 17, 10, 0, 0, DateTimeKind.Unspecified), "hongphuc.mai@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC117", new DateTime(2026, 3, 16, 8, 30, 0, 0, DateTimeKind.Unspecified), "minhduc.cao@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC118", new DateTime(2026, 3, 20, 12, 50, 0, 0, DateTimeKind.Unspecified), "phuongnhi.nguyen@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC119", new DateTime(2026, 3, 24, 9, 45, 0, 0, DateTimeKind.Unspecified), "giabao.dinh@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC120", new DateTime(2026, 3, 28, 15, 30, 0, 0, DateTimeKind.Unspecified), "tuankiet.ha@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC121", new DateTime(2026, 4, 2, 8, 25, 0, 0, DateTimeKind.Unspecified), "baongoc.ly@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC122", new DateTime(2026, 4, 5, 11, 5, 0, 0, DateTimeKind.Unspecified), "minhchau.ta@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC123", new DateTime(2026, 4, 9, 13, 40, 0, 0, DateTimeKind.Unspecified), "vietanh.ho@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" },
                    { "ACC124", new DateTime(2026, 4, 13, 16, 0, 0, 0, DateTimeKind.Unspecified), "quanghung.nguyen@example.com", true, "$2a$12$jmcPkhIubiP8SaSOemPnSO8gzj6CH3KJRGXKyGdymfPdcHx.lRL1.", false, "Customer" }
                });

            migrationBuilder.InsertData(
                table: "EmployeeInvites",
                columns: new[] { "InviteToken", "CreatedAt", "Email", "EmployeeCode", "ExpiryTime", "FullName", "IsUsed", "PhoneNumber", "Shift" },
                values: new object[,]
                {
                    { "INVITE-EMP006-2026", new DateTime(2026, 5, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "an.ngominh@parking.local", "EMP006", new DateTime(2030, 12, 31, 23, 59, 59, 0, DateTimeKind.Unspecified), "Ngô Minh An", false, "0977000222", "Chiều" },
                    { "INVITE-USED-EMP007", new DateTime(2026, 4, 20, 8, 0, 0, 0, DateTimeKind.Unspecified), "binh.dothanh@parking.local", "EMP007", new DateTime(2026, 4, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), "Đỗ Thanh Bình", true, "0977000333", "Tối" }
                });

            migrationBuilder.InsertData(
                table: "Otps",
                columns: new[] { "OtpId", "Code", "CreatedAt", "Email", "ExpiresAt", "IsVerified", "VerifiedAt" },
                values: new object[] { "OTP001", "123456", new DateTime(2026, 5, 13, 8, 0, 0, 0, DateTimeKind.Unspecified), "dangky.moi@example.com", new DateTime(2030, 12, 31, 23, 59, 59, 0, DateTimeKind.Unspecified), false, null });

            migrationBuilder.InsertData(
                table: "ParkingSlots",
                columns: new[] { "SlotId", "LastUpdated", "Location", "Status", "VehicleType" },
                values: new object[,]
                {
                    { "A01", new DateTime(2026, 5, 13, 7, 35, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 01", "Đang sử dụng", "Xe máy" },
                    { "A02", new DateTime(2026, 5, 13, 8, 25, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 02", "Đang sử dụng", "Xe máy" },
                    { "A03", new DateTime(2026, 5, 13, 9, 5, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 03", "Đang sử dụng", "Xe máy" },
                    { "A04", new DateTime(2026, 5, 13, 8, 55, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 04", "Đang sử dụng", "Xe máy" },
                    { "A05", new DateTime(2026, 5, 13, 9, 15, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 05", "Đang sử dụng", "Xe máy" },
                    { "A06", new DateTime(2026, 5, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 06", "Đã đặt", "Xe máy" },
                    { "A07", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 07", "Trống", "Xe máy" },
                    { "A08", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 08", "Trống", "Xe máy" },
                    { "A09", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 09", "Trống", "Xe máy" },
                    { "A10", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 10", "Trống", "Xe máy" },
                    { "A11", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 11", "Trống", "Xe máy" },
                    { "A12", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 12", "Trống", "Xe máy" },
                    { "A13", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 13", "Trống", "Xe máy" },
                    { "A14", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 14", "Trống", "Xe máy" },
                    { "A15", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 15", "Trống", "Xe máy" },
                    { "A16", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 16", "Trống", "Xe máy" },
                    { "A17", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 17", "Trống", "Xe máy" },
                    { "A18", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 18", "Trống", "Xe máy" },
                    { "A19", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 19", "Trống", "Xe máy" },
                    { "A20", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 20", "Trống", "Xe máy" },
                    { "A21", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 21", "Trống", "Xe máy" },
                    { "A22", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 22", "Trống", "Xe máy" },
                    { "A23", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 23", "Trống", "Xe máy" },
                    { "A24", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 24", "Trống", "Xe máy" },
                    { "A25", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 25", "Trống", "Xe máy" },
                    { "A26", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 26", "Trống", "Xe máy" },
                    { "A27", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 27", "Trống", "Xe máy" },
                    { "A28", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 28", "Trống", "Xe máy" },
                    { "A29", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 29", "Trống", "Xe máy" },
                    { "A30", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 30", "Trống", "Xe máy" },
                    { "A31", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 31", "Trống", "Xe máy" },
                    { "A32", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 32", "Trống", "Xe máy" },
                    { "A33", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 33", "Trống", "Xe máy" },
                    { "A34", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 34", "Trống", "Xe máy" },
                    { "A35", new DateTime(2026, 5, 11, 10, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 35", "Bảo trì", "Xe máy" },
                    { "A36", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 36", "Trống", "Xe máy" },
                    { "A37", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 37", "Trống", "Xe máy" },
                    { "A38", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 38", "Trống", "Xe máy" },
                    { "A39", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 39", "Trống", "Xe máy" },
                    { "A40", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 40", "Trống", "Xe máy" },
                    { "A41", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 41", "Trống", "Xe máy" },
                    { "A42", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 42", "Trống", "Xe máy" },
                    { "A43", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 43", "Trống", "Xe máy" },
                    { "A44", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 44", "Trống", "Xe máy" },
                    { "A45", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 45", "Trống", "Xe máy" },
                    { "A46", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 46", "Trống", "Xe máy" },
                    { "A47", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 47", "Trống", "Xe máy" },
                    { "A48", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 48", "Trống", "Xe máy" },
                    { "A49", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 49", "Trống", "Xe máy" },
                    { "A50", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu A - Ô 50", "Trống", "Xe máy" },
                    { "B01", new DateTime(2026, 5, 13, 8, 10, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 01", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B02", new DateTime(2026, 5, 13, 9, 20, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 02", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B03", new DateTime(2026, 5, 13, 9, 45, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 03", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B04", new DateTime(2026, 5, 13, 8, 40, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 04", "Đang sử dụng", "Ô tô nhỏ" },
                    { "B05", new DateTime(2026, 5, 14, 9, 10, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 05", "Đã đặt", "Ô tô nhỏ" },
                    { "B06", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 06", "Trống", "Ô tô nhỏ" },
                    { "B07", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 07", "Trống", "Ô tô nhỏ" },
                    { "B08", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 08", "Trống", "Ô tô nhỏ" },
                    { "B09", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 09", "Trống", "Ô tô nhỏ" },
                    { "B10", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 10", "Trống", "Ô tô nhỏ" },
                    { "B11", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 11", "Trống", "Ô tô nhỏ" },
                    { "B12", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 12", "Trống", "Ô tô nhỏ" },
                    { "B13", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 13", "Trống", "Ô tô nhỏ" },
                    { "B14", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 14", "Trống", "Ô tô nhỏ" },
                    { "B15", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 15", "Trống", "Ô tô nhỏ" },
                    { "B16", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 16", "Trống", "Ô tô nhỏ" },
                    { "B17", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 17", "Trống", "Ô tô nhỏ" },
                    { "B18", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 18", "Trống", "Ô tô nhỏ" },
                    { "B19", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 19", "Trống", "Ô tô nhỏ" },
                    { "B20", new DateTime(2026, 5, 12, 10, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 20", "Bảo trì", "Ô tô nhỏ" },
                    { "B21", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 21", "Trống", "Ô tô nhỏ" },
                    { "B22", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 22", "Trống", "Ô tô nhỏ" },
                    { "B23", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 23", "Trống", "Ô tô nhỏ" },
                    { "B24", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 24", "Trống", "Ô tô nhỏ" },
                    { "B25", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 25", "Trống", "Ô tô nhỏ" },
                    { "B26", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 26", "Trống", "Ô tô nhỏ" },
                    { "B27", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 27", "Trống", "Ô tô nhỏ" },
                    { "B28", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 28", "Trống", "Ô tô nhỏ" },
                    { "B29", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 29", "Trống", "Ô tô nhỏ" },
                    { "B30", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 30", "Trống", "Ô tô nhỏ" },
                    { "B31", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 31", "Trống", "Ô tô nhỏ" },
                    { "B32", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 32", "Trống", "Ô tô nhỏ" },
                    { "B33", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 33", "Trống", "Ô tô nhỏ" },
                    { "B34", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 34", "Trống", "Ô tô nhỏ" },
                    { "B35", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 35", "Trống", "Ô tô nhỏ" },
                    { "B36", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 36", "Trống", "Ô tô nhỏ" },
                    { "B37", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 37", "Trống", "Ô tô nhỏ" },
                    { "B38", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 38", "Trống", "Ô tô nhỏ" },
                    { "B39", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 39", "Trống", "Ô tô nhỏ" },
                    { "B40", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 40", "Trống", "Ô tô nhỏ" },
                    { "B41", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 41", "Trống", "Ô tô nhỏ" },
                    { "B42", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 42", "Trống", "Ô tô nhỏ" },
                    { "B43", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 43", "Trống", "Ô tô nhỏ" },
                    { "B44", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 44", "Trống", "Ô tô nhỏ" },
                    { "B45", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 45", "Trống", "Ô tô nhỏ" },
                    { "B46", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 46", "Trống", "Ô tô nhỏ" },
                    { "B47", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 47", "Trống", "Ô tô nhỏ" },
                    { "B48", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 48", "Trống", "Ô tô nhỏ" },
                    { "B49", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 49", "Trống", "Ô tô nhỏ" },
                    { "B50", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu B - Ô 50", "Trống", "Ô tô nhỏ" },
                    { "C01", new DateTime(2026, 5, 13, 7, 50, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 01", "Đang sử dụng", "Ô tô lớn" },
                    { "C02", new DateTime(2026, 5, 13, 9, 30, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 02", "Đang sử dụng", "Ô tô lớn" },
                    { "C03", new DateTime(2026, 5, 14, 9, 20, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 03", "Đã đặt", "Ô tô lớn" },
                    { "C04", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 04", "Trống", "Ô tô lớn" },
                    { "C05", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 05", "Trống", "Ô tô lớn" },
                    { "C06", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 06", "Trống", "Ô tô lớn" },
                    { "C07", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 07", "Trống", "Ô tô lớn" },
                    { "C08", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 08", "Trống", "Ô tô lớn" },
                    { "C09", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 09", "Trống", "Ô tô lớn" },
                    { "C10", new DateTime(2026, 5, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 10", "Bảo trì", "Ô tô lớn" },
                    { "C11", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 11", "Trống", "Ô tô lớn" },
                    { "C12", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 12", "Trống", "Ô tô lớn" },
                    { "C13", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 13", "Trống", "Ô tô lớn" },
                    { "C14", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 14", "Trống", "Ô tô lớn" },
                    { "C15", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 15", "Trống", "Ô tô lớn" },
                    { "C16", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 16", "Trống", "Ô tô lớn" },
                    { "C17", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 17", "Trống", "Ô tô lớn" },
                    { "C18", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 18", "Trống", "Ô tô lớn" },
                    { "C19", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 19", "Trống", "Ô tô lớn" },
                    { "C20", new DateTime(2026, 5, 1, 6, 0, 0, 0, DateTimeKind.Unspecified), "Khu C - Ô 20", "Trống", "Ô tô lớn" }
                });

            migrationBuilder.InsertData(
                table: "PricingConfigurations",
                columns: new[] { "PricingId", "Amount", "RateType", "UpdatedAt", "UpdatedBy", "VehicleType" },
                values: new object[,]
                {
                    { "PRICE-OTOL-FIRST", 25000m, "FirstHour", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-M1", 2000000m, "Monthly1M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-M3", 5500000m, "Monthly3M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-NEXT", 8000m, "PerHourAfter", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTOL-NIGHT", 60000m, "Overnight", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô lớn" },
                    { "PRICE-OTON-FIRST", 15000m, "FirstHour", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-M1", 1200000m, "Monthly1M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-M3", 3200000m, "Monthly3M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-NEXT", 5000m, "PerHourAfter", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-OTON-NIGHT", 40000m, "Overnight", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Ô tô nhỏ" },
                    { "PRICE-XM-FIRST", 5000m, "FirstHour", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-M1", 400000m, "Monthly1M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-M3", 1100000m, "Monthly3M", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-NEXT", 2000m, "PerHourAfter", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" },
                    { "PRICE-XM-NIGHT", 10000m, "Overnight", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "MGR001", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehiclePlate", "CustomerId", "VehicleType" },
                values: new object[,]
                {
                    { "74D1-704.33", null, "Xe máy" },
                    { "75A-663.40", null, "Ô tô nhỏ" },
                    { "76C-219.05", null, "Ô tô lớn" },
                    { "77A-904.52", null, "Ô tô nhỏ" },
                    { "92A-518.26", null, "Ô tô nhỏ" },
                    { "92D1-222.11", null, "Xe máy" },
                    { "92D1-445.18", null, "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "AccountId", "FullName", "Gender", "IsDeleted", "PhoneNumber" },
                values: new object[,]
                {
                    { "CUS001", "ACC101", "Nguyễn Minh Anh", "Female", false, "0905123456" },
                    { "CUS002", "ACC102", "Trần Quốc Bảo", "Male", false, "0916234578" },
                    { "CUS003", "ACC103", "Lê Hoàng Nam", "Male", false, "0935129087" },
                    { "CUS004", "ACC104", "Phạm Thu Hà", "Female", false, "0974306125" },
                    { "CUS005", "ACC105", "Võ Thanh Tùng", "Male", false, "0982187345" },
                    { "CUS006", "ACC106", "Đặng Ngọc Mai", "Female", false, "0946025178" },
                    { "CUS007", "ACC107", "Hoàng Gia Huy", "Male", false, "0967852143" },
                    { "CUS008", "ACC108", "Bùi Khánh Linh", "Female", false, "0926017845" },
                    { "CUS009", "ACC109", "Nguyễn Đức Long", "Male", false, "0907485126" },
                    { "CUS010", "ACC110", "Trương Mỹ Duyên", "Female", false, "0938741206" },
                    { "CUS011", "ACC111", "Phan Anh Khoa", "Male", false, "0919082746" },
                    { "CUS012", "ACC112", "Lâm Quỳnh Chi", "Female", false, "0976012498" },
                    { "CUS013", "ACC113", "Đỗ Nhật Minh", "Male", false, "0948127603" },
                    { "CUS014", "ACC114", "Huỳnh Thảo Vy", "Female", false, "0965425006" },
                    { "CUS015", "ACC115", "Vũ Công Thành", "Male", false, "0906712485" },
                    { "CUS016", "ACC116", "Mai Hồng Phúc", "Female", false, "0928174506" },
                    { "CUS017", "ACC117", "Cao Minh Đức", "Male", false, "0986401725" },
                    { "CUS018", "ACC118", "Nguyễn Phương Nhi", "Female", false, "0937084512" },
                    { "CUS019", "ACC119", "Đinh Gia Bảo", "Male", false, "0975306184" },
                    { "CUS020", "ACC120", "Hà Tuấn Kiệt", "Male", false, "0962748031" },
                    { "CUS021", "ACC121", "Lý Bảo Ngọc", "Female", false, "0914057826" },
                    { "CUS022", "ACC122", "Tạ Minh Châu", "Female", false, "0947081625" },
                    { "CUS023", "ACC123", "Hồ Việt Anh", "Male", false, "0902476813" },
                    { "CUS024", "ACC124", "Nguyễn Quang Hưng", "Male", false, "0981276405" }
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
                    { "TKT0036", new DateTime(2026, 5, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 11, 20, 0, 0, DateTimeKind.Unspecified), null, 7000m, "A27", "Đã ra", "92D1-222.11", "Xe máy" },
                    { "TKT0037", new DateTime(2026, 5, 2, 13, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 16, 10, 0, 0, DateTimeKind.Unspecified), null, 25000m, "B16", "Đã ra", "92A-518.26", "Ô tô nhỏ" },
                    { "TKT0038", new DateTime(2026, 5, 3, 18, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 20, 30, 0, 0, DateTimeKind.Unspecified), null, 9000m, "A28", "Đã ra", "74D1-704.33", "Xe máy" },
                    { "TKT0039", new DateTime(2026, 5, 4, 7, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 12, 0, 0, 0, DateTimeKind.Unspecified), null, 35000m, "B17", "Đã ra", "75A-663.40", "Ô tô nhỏ" },
                    { "TKT0040", new DateTime(2026, 5, 5, 9, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 13, 45, 0, 0, DateTimeKind.Unspecified), null, 57000m, "C08", "Đã ra", "76C-219.05", "Ô tô lớn" },
                    { "TKT0041", new DateTime(2026, 5, 6, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), null, 7000m, "A29", "Đã ra", "92D1-445.18", "Xe máy" },
                    { "TKT0042", new DateTime(2026, 5, 7, 16, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 21, 0, 0, 0, DateTimeKind.Unspecified), null, 35000m, "B18", "Đã ra", "77A-904.52", "Ô tô nhỏ" },
                    { "TKT0051", new DateTime(2026, 5, 13, 9, 15, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "A05", "Đang trong bãi", "92D1-222.11", "Xe máy" },
                    { "TKT0052", new DateTime(2026, 5, 13, 8, 40, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "B04", "Đang trong bãi", "92A-518.26", "Ô tô nhỏ" },
                    { "TKT0053", new DateTime(2026, 5, 13, 9, 30, 0, 0, DateTimeKind.Unspecified), null, null, 0m, "C02", "Đang trong bãi", "76C-219.05", "Ô tô lớn" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "AccountId", "EmployeeCode", "FullName", "Gender", "IsDeleted", "ManagerId", "PhoneNumber", "Shift" },
                values: new object[,]
                {
                    { "EMP001", "ACC002", "EMP001", "Nguyễn Thanh", "Male", false, "MGR001", "0912345678", "Sáng" },
                    { "EMP002", "ACC003", "EMP002", "Lê Văn Hùng", "Male", false, "MGR001", "0923456789", "Chiều" },
                    { "EMP003", "ACC004", "EMP003", "Trần Mai Linh", "Female", false, "MGR001", "0934567890", "Tối" },
                    { "EMP004", "ACC005", "EMP004", "Đỗ Quốc Bảo", "Male", false, "MGR001", "0977000111", "Sáng" },
                    { "EMP005", "ACC006", "EMP005", "Phan Quốc Nam", "Male", true, "MGR001", "0987654321", null }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Method", "MonthlyTicketId", "PaymentTime", "Status", "TicketId" },
                values: new object[,]
                {
                    { "PAY0035", 7000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 11, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0036" },
                    { "PAY0036", 25000m, "Chuyển khoản", null, new DateTime(2026, 5, 2, 16, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0037" },
                    { "PAY0037", 9000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 20, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0038" },
                    { "PAY0038", 35000m, "Ví điện tử", null, new DateTime(2026, 5, 4, 12, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0039" },
                    { "PAY0039", 57000m, "Chuyển khoản", null, new DateTime(2026, 5, 5, 13, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0040" },
                    { "PAY0040", 7000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0041" },
                    { "PAY0041", 35000m, "Chuyển khoản", null, new DateTime(2026, 5, 7, 21, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0042" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehiclePlate", "CustomerId", "VehicleType" },
                values: new object[,]
                {
                    { "43A-246.80", "CUS010", "Ô tô nhỏ" },
                    { "43A-332.16", "CUS011", "Ô tô nhỏ" },
                    { "43A-509.34", "CUS015", "Ô tô nhỏ" },
                    { "43A-657.20", "CUS003", "Ô tô nhỏ" },
                    { "43A-694.15", "CUS018", "Ô tô nhỏ" },
                    { "43A-735.18", "CUS007", "Ô tô nhỏ" },
                    { "43A-807.51", "CUS023", "Ô tô nhỏ" },
                    { "43A-918.42", "CUS001", "Ô tô nhỏ" },
                    { "43C-112.67", "CUS005", "Ô tô lớn" },
                    { "43C-245.19", "CUS013", "Ô tô lớn" },
                    { "43C-318.72", "CUS020", "Ô tô lớn" },
                    { "43D1-256.31", "CUS001", "Xe máy" },
                    { "43D1-344.88", "CUS002", "Xe máy" },
                    { "43D1-490.12", "CUS004", "Xe máy" },
                    { "43D1-628.09", "CUS006", "Xe máy" },
                    { "43D1-812.43", "CUS008", "Xe máy" },
                    { "43D1-921.54", "CUS009", "Xe máy" },
                    { "43D2-105.77", "CUS011", "Xe máy" },
                    { "43D2-218.90", "CUS012", "Xe máy" },
                    { "43D2-387.66", "CUS014", "Xe máy" },
                    { "43D2-474.21", "CUS016", "Xe máy" },
                    { "43D2-588.64", "CUS017", "Xe máy" },
                    { "43D2-730.08", "CUS019", "Xe máy" },
                    { "43D3-044.39", "CUS021", "Xe máy" },
                    { "43D3-115.84", "CUS022", "Xe máy" },
                    { "43D3-236.97", "CUS024", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "MonthlyTickets",
                columns: new[] { "MonthlyTicketId", "CreatedAt", "CustomerId", "EndDate", "PackageType", "StartDate", "Status", "TotalFee", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "MTK001", new DateTime(2026, 4, 20, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS001", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 400000m, "43D1-256.31", "Xe máy" },
                    { "MTK002", new DateTime(2026, 3, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS002", new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 1100000m, "43D1-344.88", "Xe máy" },
                    { "MTK003", new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 3200000m, "43A-657.20", "Ô tô nhỏ" },
                    { "MTK004", new DateTime(2026, 1, 20, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS005", new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 5500000m, "43C-112.67", "Ô tô lớn" },
                    { "MTK005", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS006", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 400000m, "43D1-628.09", "Xe máy" },
                    { "MTK006", new DateTime(2026, 2, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS007", new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 3200000m, "43A-735.18", "Ô tô nhỏ" },
                    { "MTK007", new DateTime(2026, 4, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS010", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã hủy", 1200000m, "43A-246.80", "Ô tô nhỏ" },
                    { "MTK008", new DateTime(2026, 4, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS011", new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 1200000m, "43A-332.16", "Ô tô nhỏ" },
                    { "MTK009", new DateTime(2026, 1, 5, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS013", new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hết hạn", 5500000m, "43C-245.19", "Ô tô lớn" },
                    { "MTK010", new DateTime(2026, 4, 25, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS018", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "3 tháng", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 3200000m, "43A-694.15", "Ô tô nhỏ" },
                    { "MTK011", new DateTime(2026, 5, 5, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS020", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 2000000m, "43C-318.72", "Ô tô lớn" },
                    { "MTK012", new DateTime(2026, 5, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS021", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "1 tháng", new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoạt động", 400000m, "43D3-044.39", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "ParkingSlotAuditLogs",
                columns: new[] { "LogId", "ChangedAt", "EmployeeId", "NewStatus", "Note", "OldStatus", "Reason", "SlotId" },
                values: new object[,]
                {
                    { "LOG001", new DateTime(2026, 5, 13, 7, 35, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0043", "Trống", null, "A01" },
                    { "LOG002", new DateTime(2026, 5, 13, 8, 10, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0044", "Trống", null, "B01" },
                    { "LOG003", new DateTime(2026, 5, 13, 8, 25, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0045", "Trống", null, "A02" },
                    { "LOG004", new DateTime(2026, 5, 13, 7, 50, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0046", "Trống", null, "C01" },
                    { "LOG005", new DateTime(2026, 5, 13, 9, 5, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0047", "Trống", null, "A03" },
                    { "LOG006", new DateTime(2026, 5, 13, 9, 20, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0048", "Trống", null, "B02" },
                    { "LOG007", new DateTime(2026, 5, 13, 9, 45, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0049", "Trống", null, "B03" },
                    { "LOG008", new DateTime(2026, 5, 13, 8, 55, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0050", "Trống", null, "A04" },
                    { "LOG009", new DateTime(2026, 5, 13, 9, 15, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0051", "Trống", null, "A05" },
                    { "LOG010", new DateTime(2026, 5, 13, 8, 40, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0052", "Trống", null, "B04" },
                    { "LOG011", new DateTime(2026, 5, 13, 9, 30, 0, 0, DateTimeKind.Unspecified), "EMP001", "Đang sử dụng", "Check-in TKT0053", "Trống", null, "C02" },
                    { "LOG012", new DateTime(2026, 5, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), "EMP002", "Đã đặt", "Giữ chỗ cho RES001", "Trống", null, "A06" },
                    { "LOG013", new DateTime(2026, 5, 14, 9, 10, 0, 0, DateTimeKind.Unspecified), "EMP002", "Đã đặt", "Giữ chỗ cho RES002", "Trống", null, "B05" },
                    { "LOG014", new DateTime(2026, 5, 14, 9, 20, 0, 0, DateTimeKind.Unspecified), "EMP002", "Đã đặt", "Giữ chỗ cho RES003", "Trống", null, "C03" },
                    { "LOG015", new DateTime(2026, 5, 11, 10, 0, 0, 0, DateTimeKind.Unspecified), "EMP004", "Bảo trì", "Bảo trì cảm biến khu A", "Trống", "Thiết bị báo chiếm chỗ chập chờn", "A35" },
                    { "LOG016", new DateTime(2026, 5, 12, 10, 0, 0, 0, DateTimeKind.Unspecified), "EMP004", "Bảo trì", "Sơn lại vạch khu B", "Trống", "Vạch đỗ bị mờ", "B20" },
                    { "LOG017", new DateTime(2026, 5, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), "EMP004", "Bảo trì", "Kiểm tra camera khu C", "Trống", "Camera góc khuất mất tín hiệu", "C10" }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "ReservationId", "CreatedAt", "CustomerId", "ExpectedTime", "SlotId", "Status", "VehiclePlate" },
                values: new object[,]
                {
                    { "RES001", new DateTime(2026, 5, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), "CUS006", new DateTime(2026, 5, 20, 18, 15, 0, 0, DateTimeKind.Unspecified), "A06", "Chờ", "43D1-628.09" },
                    { "RES002", new DateTime(2026, 5, 14, 9, 10, 0, 0, DateTimeKind.Unspecified), "CUS010", new DateTime(2026, 5, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), "B05", "Chờ", "43A-246.80" },
                    { "RES003", new DateTime(2026, 5, 14, 9, 20, 0, 0, DateTimeKind.Unspecified), "CUS020", new DateTime(2026, 5, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), "C03", "Chờ", "43C-318.72" },
                    { "RES004", new DateTime(2026, 5, 10, 17, 30, 0, 0, DateTimeKind.Unspecified), "CUS012", new DateTime(2026, 5, 11, 9, 0, 0, 0, DateTimeKind.Unspecified), "A07", "Hết hạn", "43D2-218.90" },
                    { "RES005", new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), "CUS014", new DateTime(2026, 5, 12, 16, 0, 0, 0, DateTimeKind.Unspecified), "A08", "Hủy", "43D2-387.66" },
                    { "RES006", new DateTime(2026, 4, 24, 12, 0, 0, 0, DateTimeKind.Unspecified), "CUS001", new DateTime(2026, 4, 24, 18, 0, 0, 0, DateTimeKind.Unspecified), "B07", "Đã nhận", "43A-918.42" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "CheckInTime", "CheckOutTime", "CustomerId", "Fee", "SlotId", "Status", "VehiclePlate", "VehicleType" },
                values: new object[,]
                {
                    { "TKT0001", new DateTime(2026, 4, 22, 7, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 22, 17, 20, 0, 0, DateTimeKind.Unspecified), "CUS001", 0m, "A09", "Đã ra", "43D1-256.31", "Xe máy" },
                    { "TKT0002", new DateTime(2026, 4, 24, 18, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 24, 20, 10, 0, 0, DateTimeKind.Unspecified), "CUS001", 25000m, "B07", "Đã ra", "43A-918.42", "Ô tô nhỏ" },
                    { "TKT0003", new DateTime(2026, 5, 6, 7, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 17, 15, 0, 0, DateTimeKind.Unspecified), "CUS001", 0m, "A10", "Đã ra", "43D1-256.31", "Xe máy" },
                    { "TKT0004", new DateTime(2026, 4, 2, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 2, 17, 0, 0, 0, DateTimeKind.Unspecified), "CUS002", 0m, "A11", "Đã ra", "43D1-344.88", "Xe máy" },
                    { "TKT0005", new DateTime(2026, 5, 10, 8, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 16, 45, 0, 0, DateTimeKind.Unspecified), "CUS002", 0m, "A12", "Đã ra", "43D1-344.88", "Xe máy" },
                    { "TKT0006", new DateTime(2026, 5, 2, 8, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 18, 0, 0, 0, DateTimeKind.Unspecified), "CUS003", 0m, "B08", "Đã ra", "43A-657.20", "Ô tô nhỏ" },
                    { "TKT0007", new DateTime(2026, 4, 25, 9, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 25, 15, 40, 0, 0, DateTimeKind.Unspecified), "CUS003", 45000m, "B09", "Đã ra", "43A-657.20", "Ô tô nhỏ" },
                    { "TKT0008", new DateTime(2026, 4, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 11, 0, 0, 0, DateTimeKind.Unspecified), "CUS004", 7000m, "A13", "Đã ra", "43D1-490.12", "Xe máy" },
                    { "TKT0009", new DateTime(2026, 5, 8, 13, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 17, 20, 0, 0, DateTimeKind.Unspecified), "CUS004", 13000m, "A14", "Đã ra", "43D1-490.12", "Xe máy" },
                    { "TKT0010", new DateTime(2026, 4, 3, 7, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 3, 18, 30, 0, 0, DateTimeKind.Unspecified), "CUS005", 0m, "C04", "Đã ra", "43C-112.67", "Ô tô lớn" },
                    { "TKT0011", new DateTime(2026, 5, 9, 8, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 9, 19, 10, 0, 0, DateTimeKind.Unspecified), "CUS005", 105000m, "C05", "Đã ra", "43C-112.67", "Ô tô lớn" },
                    { "TKT0012", new DateTime(2026, 3, 20, 8, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 3, 20, 17, 5, 0, 0, DateTimeKind.Unspecified), "CUS006", 0m, "A15", "Đã ra", "43D1-628.09", "Xe máy" },
                    { "TKT0013", new DateTime(2026, 4, 28, 8, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 16, 30, 0, 0, DateTimeKind.Unspecified), "CUS006", 21000m, "A15", "Đã ra", "43D1-628.09", "Xe máy" },
                    { "TKT0014", new DateTime(2026, 4, 16, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 16, 18, 30, 0, 0, DateTimeKind.Unspecified), "CUS007", 0m, "B10", "Đã ra", "43A-735.18", "Ô tô nhỏ" },
                    { "TKT0015", new DateTime(2026, 5, 11, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 17, 20, 0, 0, DateTimeKind.Unspecified), "CUS007", 55000m, "B10", "Đã ra", "43A-735.18", "Ô tô nhỏ" },
                    { "TKT0016", new DateTime(2026, 4, 18, 9, 35, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 18, 13, 30, 0, 0, DateTimeKind.Unspecified), "CUS008", 11000m, "A16", "Đã ra", "43D1-812.43", "Xe máy" },
                    { "TKT0017", new DateTime(2026, 4, 27, 7, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 27, 12, 15, 0, 0, DateTimeKind.Unspecified), "CUS009", 13000m, "A17", "Đã ra", "43D1-921.54", "Xe máy" },
                    { "TKT0018", new DateTime(2026, 4, 4, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 14, 30, 0, 0, DateTimeKind.Unspecified), "CUS010", 35000m, "B11", "Đã ra", "43A-246.80", "Ô tô nhỏ" },
                    { "TKT0019", new DateTime(2026, 5, 4, 8, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 4, 13, 50, 0, 0, DateTimeKind.Unspecified), "CUS010", 40000m, "B11", "Đã ra", "43A-246.80", "Ô tô nhỏ" },
                    { "TKT0020", new DateTime(2026, 4, 13, 12, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 13, 14, 45, 0, 0, DateTimeKind.Unspecified), "CUS011", 9000m, "A18", "Đã ra", "43D2-105.77", "Xe máy" },
                    { "TKT0021", new DateTime(2026, 4, 20, 7, 50, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 17, 25, 0, 0, DateTimeKind.Unspecified), "CUS011", 0m, "B12", "Đã ra", "43A-332.16", "Ô tô nhỏ" },
                    { "TKT0022", new DateTime(2026, 5, 1, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 14, 5, 0, 0, DateTimeKind.Unspecified), "CUS012", 15000m, "A19", "Đã ra", "43D2-218.90", "Xe máy" },
                    { "TKT0023", new DateTime(2026, 3, 12, 9, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 3, 12, 17, 30, 0, 0, DateTimeKind.Unspecified), "CUS013", 0m, "C06", "Đã ra", "43C-245.19", "Ô tô lớn" },
                    { "TKT0024", new DateTime(2026, 4, 20, 8, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 20, 15, 5, 0, 0, DateTimeKind.Unspecified), "CUS013", 73000m, "C06", "Đã ra", "43C-245.19", "Ô tô lớn" },
                    { "TKT0025", new DateTime(2026, 4, 29, 14, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 29, 16, 10, 0, 0, DateTimeKind.Unspecified), "CUS014", 9000m, "A20", "Đã ra", "43D2-387.66", "Xe máy" },
                    { "TKT0026", new DateTime(2026, 5, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 14, 20, 0, 0, DateTimeKind.Unspecified), "CUS015", 40000m, "B13", "Đã ra", "43A-509.34", "Ô tô nhỏ" },
                    { "TKT0027", new DateTime(2026, 5, 7, 10, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 14, 0, 0, 0, DateTimeKind.Unspecified), "CUS016", 11000m, "A21", "Đã ra", "43D2-474.21", "Xe máy" },
                    { "TKT0028", new DateTime(2026, 5, 10, 15, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 16, 0, 0, 0, DateTimeKind.Unspecified), "CUS017", 5000m, "A22", "Đã ra", "43D2-588.64", "Xe máy" },
                    { "TKT0029", new DateTime(2026, 4, 28, 8, 45, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 28, 17, 45, 0, 0, DateTimeKind.Unspecified), "CUS018", 0m, "B14", "Đã ra", "43A-694.15", "Ô tô nhỏ" },
                    { "TKT0030", new DateTime(2026, 5, 2, 8, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 2, 14, 40, 0, 0, DateTimeKind.Unspecified), "CUS019", 17000m, "A23", "Đã ra", "43D2-730.08", "Xe máy" },
                    { "TKT0031", new DateTime(2026, 5, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 18, 10, 0, 0, DateTimeKind.Unspecified), "CUS020", 0m, "C07", "Đã ra", "43C-318.72", "Ô tô lớn" },
                    { "TKT0032", new DateTime(2026, 5, 11, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 17, 0, 0, 0, DateTimeKind.Unspecified), "CUS021", 0m, "A24", "Đã ra", "43D3-044.39", "Xe máy" },
                    { "TKT0033", new DateTime(2026, 5, 5, 9, 40, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 12, 5, 0, 0, DateTimeKind.Unspecified), "CUS022", 9000m, "A25", "Đã ra", "43D3-115.84", "Xe máy" },
                    { "TKT0034", new DateTime(2026, 5, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 14, 55, 0, 0, DateTimeKind.Unspecified), "CUS023", 35000m, "B15", "Đã ra", "43A-807.51", "Ô tô nhỏ" },
                    { "TKT0035", new DateTime(2026, 5, 12, 8, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 12, 12, 45, 0, 0, DateTimeKind.Unspecified), "CUS024", 13000m, "A26", "Đã ra", "43D3-236.97", "Xe máy" },
                    { "TKT0043", new DateTime(2026, 5, 13, 7, 35, 0, 0, DateTimeKind.Unspecified), null, "CUS001", 0m, "A01", "Đang trong bãi", "43D1-256.31", "Xe máy" },
                    { "TKT0044", new DateTime(2026, 5, 13, 8, 10, 0, 0, DateTimeKind.Unspecified), null, "CUS003", 0m, "B01", "Đang trong bãi", "43A-657.20", "Ô tô nhỏ" },
                    { "TKT0045", new DateTime(2026, 5, 13, 8, 25, 0, 0, DateTimeKind.Unspecified), null, "CUS004", 0m, "A02", "Đang trong bãi", "43D1-490.12", "Xe máy" },
                    { "TKT0046", new DateTime(2026, 5, 13, 7, 50, 0, 0, DateTimeKind.Unspecified), null, "CUS005", 0m, "C01", "Đang trong bãi", "43C-112.67", "Ô tô lớn" },
                    { "TKT0047", new DateTime(2026, 5, 13, 9, 5, 0, 0, DateTimeKind.Unspecified), null, "CUS008", 0m, "A03", "Đang trong bãi", "43D1-812.43", "Xe máy" },
                    { "TKT0048", new DateTime(2026, 5, 13, 9, 20, 0, 0, DateTimeKind.Unspecified), null, "CUS011", 0m, "B02", "Đang trong bãi", "43A-332.16", "Ô tô nhỏ" },
                    { "TKT0049", new DateTime(2026, 5, 13, 9, 45, 0, 0, DateTimeKind.Unspecified), null, "CUS015", 0m, "B03", "Đang trong bãi", "43A-509.34", "Ô tô nhỏ" },
                    { "TKT0050", new DateTime(2026, 5, 13, 8, 55, 0, 0, DateTimeKind.Unspecified), null, "CUS019", 0m, "A04", "Đang trong bãi", "43D2-730.08", "Xe máy" }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Method", "MonthlyTicketId", "PaymentTime", "Status", "TicketId" },
                values: new object[,]
                {
                    { "PAY0001", 400000m, "Chuyển khoản", "MTK001", new DateTime(2026, 4, 20, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0002", 1100000m, "Ví điện tử", "MTK002", new DateTime(2026, 3, 15, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0003", 3200000m, "Chuyển khoản", "MTK003", new DateTime(2026, 5, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0004", 5500000m, "Chuyển khoản", "MTK004", new DateTime(2026, 1, 20, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0005", 400000m, "Tiền mặt", "MTK005", new DateTime(2026, 3, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0006", 3200000m, "Chuyển khoản", "MTK006", new DateTime(2026, 2, 10, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0007", 1200000m, "Tiền mặt", "MTK007", new DateTime(2026, 4, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0008", 1200000m, "Ví điện tử", "MTK008", new DateTime(2026, 4, 15, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0009", 5500000m, "Chuyển khoản", "MTK009", new DateTime(2026, 1, 5, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0010", 3200000m, "Chuyển khoản", "MTK010", new DateTime(2026, 4, 25, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0011", 2000000m, "Ví điện tử", "MTK011", new DateTime(2026, 5, 5, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0012", 400000m, "Tiền mặt", "MTK012", new DateTime(2026, 5, 10, 8, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", null },
                    { "PAY0013", 25000m, "Ví điện tử", null, new DateTime(2026, 4, 24, 20, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0002" },
                    { "PAY0014", 45000m, "Tiền mặt", null, new DateTime(2026, 4, 25, 15, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0007" },
                    { "PAY0015", 7000m, "Tiền mặt", null, new DateTime(2026, 4, 12, 11, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0008" },
                    { "PAY0016", 13000m, "Ví điện tử", null, new DateTime(2026, 5, 8, 17, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0009" },
                    { "PAY0017", 105000m, "Chuyển khoản", null, new DateTime(2026, 5, 9, 19, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0011" },
                    { "PAY0018", 21000m, "Tiền mặt", null, new DateTime(2026, 4, 28, 16, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0013" },
                    { "PAY0019", 55000m, "Chuyển khoản", null, new DateTime(2026, 5, 11, 17, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0015" },
                    { "PAY0020", 11000m, "Ví điện tử", null, new DateTime(2026, 4, 18, 13, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0016" },
                    { "PAY0021", 13000m, "Tiền mặt", null, new DateTime(2026, 4, 27, 12, 15, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0017" },
                    { "PAY0022", 35000m, "Tiền mặt", null, new DateTime(2026, 4, 4, 14, 30, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0018" },
                    { "PAY0023", 40000m, "Chuyển khoản", null, new DateTime(2026, 5, 4, 13, 50, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0019" },
                    { "PAY0024", 9000m, "Tiền mặt", null, new DateTime(2026, 4, 13, 14, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0020" },
                    { "PAY0025", 15000m, "Tiền mặt", null, new DateTime(2026, 5, 1, 14, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0022" },
                    { "PAY0026", 73000m, "Chuyển khoản", null, new DateTime(2026, 4, 20, 15, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0024" },
                    { "PAY0027", 9000m, "Ví điện tử", null, new DateTime(2026, 4, 29, 16, 10, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0025" },
                    { "PAY0028", 40000m, "Tiền mặt", null, new DateTime(2026, 5, 3, 14, 20, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0026" },
                    { "PAY0029", 11000m, "Tiền mặt", null, new DateTime(2026, 5, 7, 14, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0027" },
                    { "PAY0030", 5000m, "Ví điện tử", null, new DateTime(2026, 5, 10, 16, 0, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0028" },
                    { "PAY0031", 17000m, "Tiền mặt", null, new DateTime(2026, 5, 2, 14, 40, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0030" },
                    { "PAY0032", 9000m, "Ví điện tử", null, new DateTime(2026, 5, 5, 12, 5, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0033" },
                    { "PAY0033", 35000m, "Tiền mặt", null, new DateTime(2026, 5, 6, 14, 55, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0034" },
                    { "PAY0034", 13000m, "Tiền mặt", null, new DateTime(2026, 5, 12, 12, 45, 0, 0, DateTimeKind.Unspecified), "Thành công", "TKT0035" }
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
                name: "IX_ShiftSchedules_EmployeeId",
                table: "ShiftSchedules",
                column: "EmployeeId");

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

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_EmployeeId",
                table: "WorkLogs",
                column: "EmployeeId");
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
                name: "ShiftSchedules");

            migrationBuilder.DropTable(
                name: "WorkLogs");

            migrationBuilder.DropTable(
                name: "MonthlyTickets");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ParkingSlots");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
