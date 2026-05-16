using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    public partial class AddParkingSlotsSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert missing slots A01..A50 (Xe máy), B01..B50 (Ô tô nhỏ), C01..C20 (Ô tô lớn)
            migrationBuilder.Sql(@"BEGIN TRY
    BEGIN TRAN;

    DECLARE @i INT = 1;
    DECLARE @id NVARCHAR(20);
    DECLARE @loc NVARCHAR(50);

    -- Area A: Xe máy (A01..A50)
    SET @i = 1;
    WHILE @i <= 50
    BEGIN
        SET @id = 'A' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        SET @loc = 'Khu A - Ô ' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        IF NOT EXISTS (SELECT 1 FROM dbo.ParkingSlots WHERE SlotId = @id)
        BEGIN
            INSERT INTO dbo.ParkingSlots (SlotId, Location, VehicleType, Status, LastUpdated)
            VALUES (@id, @loc, N'Xe máy', N'Trống', GETDATE());
        END
        SET @i = @i + 1;
    END

    -- Area B: Ô tô nhỏ (B01..B50)
    SET @i = 1;
    WHILE @i <= 50
    BEGIN
        SET @id = 'B' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        SET @loc = 'Khu B - Ô ' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        IF NOT EXISTS (SELECT 1 FROM dbo.ParkingSlots WHERE SlotId = @id)
        BEGIN
            INSERT INTO dbo.ParkingSlots (SlotId, Location, VehicleType, Status, LastUpdated)
            VALUES (@id, @loc, N'Ô tô nhỏ', N'Trống', GETDATE());
        END
        SET @i = @i + 1;
    END

    -- Area C: Ô tô lớn (C01..C20)
    SET @i = 1;
    WHILE @i <= 20
    BEGIN
        SET @id = 'C' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        SET @loc = 'Khu C - Ô ' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        IF NOT EXISTS (SELECT 1 FROM dbo.ParkingSlots WHERE SlotId = @id)
        BEGIN
            INSERT INTO dbo.ParkingSlots (SlotId, Location, VehicleType, Status, LastUpdated)
            VALUES (@id, @loc, N'Ô tô lớn', N'Trống', GETDATE());
        END
        SET @i = @i + 1;
    END

    COMMIT;
END TRY
BEGIN CATCH
    ROLLBACK;
    THROW;
END CATCH");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the seeded slots if present
            migrationBuilder.Sql(@"DELETE FROM dbo.ParkingSlots
WHERE (SlotId LIKE 'A__' OR SlotId LIKE 'B__' OR SlotId LIKE 'C__')
  AND VehicleType IN (N'Xe máy', N'Ô tô nhỏ', N'Ô tô lớn');");
        }
    }
}
