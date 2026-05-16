using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    [Migration("20260516120000_LinkWorkLogsToShiftSchedules")]
    public partial class LinkWorkLogsToShiftSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduleId",
                table: "WorkLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_ScheduleId",
                table: "WorkLogs",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkLogs_ShiftSchedules_ScheduleId",
                table: "WorkLogs",
                column: "ScheduleId",
                principalTable: "ShiftSchedules",
                principalColumn: "ScheduleId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkLogs_ShiftSchedules_ScheduleId",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkLogs_ScheduleId",
                table: "WorkLogs");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "WorkLogs");
        }
    }
}
