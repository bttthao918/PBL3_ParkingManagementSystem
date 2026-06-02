using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ParkingManagement.DAL.Data;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260527120000_AddMonthlyTicketAutoRenew")]
    public partial class AddMonthlyTicketAutoRenew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[MonthlyTickets]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[MonthlyTickets]', N'AutoRenew') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[MonthlyTickets]
                    ADD [AutoRenew] bit NOT NULL
                        CONSTRAINT [DF_MonthlyTickets_AutoRenew] DEFAULT(1) WITH VALUES;
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[MonthlyTickets]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[MonthlyTickets]', N'AutoRenew') IS NOT NULL
                BEGIN
                    EXEC(N'UPDATE [dbo].[MonthlyTickets]
                           SET [AutoRenew] = 0
                           WHERE [Status] IN (N''Đã hủy'', N''Hết hạn'');');
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[MonthlyTickets]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[MonthlyTickets]', N'AutoRenew') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;

                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[MonthlyTickets]')
                      AND c.name = N'AutoRenew';

                    IF @constraintName IS NOT NULL
                        EXEC(N'ALTER TABLE [dbo].[MonthlyTickets] DROP CONSTRAINT [' + @constraintName + N']');

                    ALTER TABLE [dbo].[MonthlyTickets] DROP COLUMN [AutoRenew];
                END
                """);
        }
    }
}
