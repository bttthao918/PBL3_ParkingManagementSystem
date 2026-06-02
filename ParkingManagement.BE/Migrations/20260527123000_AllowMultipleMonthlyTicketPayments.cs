using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ParkingManagement.DAL.Data;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260527123000_AllowMultipleMonthlyTicketPayments")]
    public partial class AllowMultipleMonthlyTicketPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Payments_MonthlyTicketId'
                      AND object_id = OBJECT_ID(N'[dbo].[Payments]')
                )
                BEGIN
                    DROP INDEX [IX_Payments_MonthlyTicketId] ON [dbo].[Payments];
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Payments_MonthlyTicketId'
                      AND object_id = OBJECT_ID(N'[dbo].[Payments]')
                )
                BEGIN
                    CREATE INDEX [IX_Payments_MonthlyTicketId]
                    ON [dbo].[Payments] ([MonthlyTicketId])
                    WHERE [MonthlyTicketId] IS NOT NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Payments_MonthlyTicketId'
                      AND object_id = OBJECT_ID(N'[dbo].[Payments]')
                )
                BEGIN
                    DROP INDEX [IX_Payments_MonthlyTicketId] ON [dbo].[Payments];
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Payments_MonthlyTicketId'
                      AND object_id = OBJECT_ID(N'[dbo].[Payments]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Payments_MonthlyTicketId]
                    ON [dbo].[Payments] ([MonthlyTicketId])
                    WHERE [MonthlyTicketId] IS NOT NULL;
                END
                """);
        }
    }
}
