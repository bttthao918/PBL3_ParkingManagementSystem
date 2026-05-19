using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingMonthlyTicketPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MonthlyTicket_Status",
                table: "MonthlyTickets");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MonthlyTicket_Status",
                table: "MonthlyTickets",
                sql: "Status IN (N'Hoạt động', N'Hết hạn', N'Đã hủy', N'Chờ thanh toán')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MonthlyTicket_Status",
                table: "MonthlyTickets");

            migrationBuilder.Sql("UPDATE MonthlyTickets SET Status = N'Đã hủy' WHERE Status = N'Chờ thanh toán'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MonthlyTicket_Status",
                table: "MonthlyTickets",
                sql: "Status IN (N'Hoạt động', N'Hết hạn', N'Đã hủy')");
        }
    }
}
