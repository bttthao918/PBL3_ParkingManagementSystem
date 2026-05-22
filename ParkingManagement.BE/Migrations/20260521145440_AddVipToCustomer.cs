using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVipToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MemberSince",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpent",
                table: "Customers",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalTickets",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VipLevel",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS001",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 413, DateTimeKind.Local).AddTicks(9344), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS002",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3378), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS003",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3402), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS004",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3403), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS005",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3404), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS006",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3405), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS007",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3406), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS008",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3406), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS009",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3407), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS010",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3408), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS011",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3409), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS012",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3409), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS013",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3410), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS014",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3411), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS015",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3412), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS016",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3412), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS017",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3413), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS018",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3501), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS019",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3502), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS020",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3503), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS021",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3504), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS022",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3504), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS023",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3505), 0m, 0, "Thành viên" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: "CUS024",
                columns: new[] { "MemberSince", "TotalSpent", "TotalTickets", "VipLevel" },
                values: new object[] { new DateTime(2026, 5, 21, 21, 54, 39, 416, DateTimeKind.Local).AddTicks(3506), 0m, 0, "Thành viên" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberSince",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalSpent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalTickets",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "VipLevel",
                table: "Customers");
        }
    }
}
