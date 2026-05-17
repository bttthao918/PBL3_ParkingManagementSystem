using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEmployeeAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollectedByEmployeeId",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            var seedEmployees = new[] { "EMP001", "EMP002", "EMP003", "EMP004" };
            for (var i = 1; i <= 41; i++)
            {
                var employeeId = seedEmployees[(i - 1) % seedEmployees.Length];
                migrationBuilder.Sql($@"
UPDATE Payments
SET CollectedByEmployeeId = '{employeeId}'
WHERE PaymentId = 'PAY{i:0000}'
  AND EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = '{employeeId}')");
            }

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CollectedByEmployeeId",
                table: "Payments",
                column: "CollectedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Employees_CollectedByEmployeeId",
                table: "Payments",
                column: "CollectedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Employees_CollectedByEmployeeId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CollectedByEmployeeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CollectedByEmployeeId",
                table: "Payments");
        }
    }
}
