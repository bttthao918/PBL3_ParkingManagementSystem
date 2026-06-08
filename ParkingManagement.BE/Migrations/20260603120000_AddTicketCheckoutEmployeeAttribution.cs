using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ParkingManagement.DAL.Data;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260603120000_AddTicketCheckoutEmployeeAttribution")]
    public partial class AddTicketCheckoutEmployeeAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckedOutByEmployeeId",
                table: "Tickets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE t
                SET CheckedOutByEmployeeId = p.CollectedByEmployeeId
                FROM Tickets t
                OUTER APPLY (
                    SELECT TOP 1 CollectedByEmployeeId
                    FROM Payments
                    WHERE TicketId = t.TicketId
                      AND CollectedByEmployeeId IS NOT NULL
                    ORDER BY PaymentTime DESC
                ) p
                WHERE t.CheckedOutByEmployeeId IS NULL
                  AND p.CollectedByEmployeeId IS NOT NULL
                  AND EXISTS (
                      SELECT 1
                      FROM Employees e
                      WHERE e.EmployeeId = p.CollectedByEmployeeId
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CheckedOutByEmployeeId",
                table: "Tickets",
                column: "CheckedOutByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Employees_CheckedOutByEmployeeId",
                table: "Tickets",
                column: "CheckedOutByEmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Employees_CheckedOutByEmployeeId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CheckedOutByEmployeeId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CheckedOutByEmployeeId",
                table: "Tickets");
        }
    }
}
