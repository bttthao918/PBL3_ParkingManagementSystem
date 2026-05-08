using ParkingManagement.FE.Models.ViewModels.Customer;

namespace ParkingManagement.FE.Helpers;

public static class CustomerBookingFakeData
{
    public static List<VehicleProfileViewModel> Profiles = new()
    {
        new()
        {
            Id = 1,
            Label = "Xe cá nhân",
            CustomerName = "Nguyễn Văn Nam",
            Phone = "0901234567",
            PlateNumber = "51H-12345",
            VehicleType = "Ô tô"
        },
        new()
        {
            Id = 2,
            Label = "Xe máy đi làm",
            CustomerName = "Nguyễn Văn Nam",
            Phone = "0901234567",
            PlateNumber = "59X1-88888",
            VehicleType = "Xe máy"
        }
    };

    public static List<ParkingSlotViewModel> Slots = new()
    {
        new() { Code = "A01", Status = "Available" },
        new() { Code = "A02", Status = "Available" },
        new() { Code = "A03", Status = "Occupied" },
        new() { Code = "A04", Status = "Available" },
        new() { Code = "A05", Status = "Available" },
        new() { Code = "A06", Status = "Available" },

        new() { Code = "B01", Status = "Available" },
        new() { Code = "B02", Status = "Holding" },
        new() { Code = "B03", Status = "Available" },
        new() { Code = "B04", Status = "Available" },
        new() { Code = "B05", Status = "Available" },
        new() { Code = "B06", Status = "Occupied" },

        new() { Code = "C01", Status = "Available" },
        new() { Code = "C02", Status = "Available" },
        new() { Code = "C03", Status = "Available" },
        new() { Code = "C04", Status = "Occupied" },
        new() { Code = "C05", Status = "Available" },
        new() { Code = "C06", Status = "Available" },
    };

    public static List<BookingViewModel> Bookings = new()
    {
        new()
        {
            Id = 1,
            Code = "DC-2024-000123",
            CustomerName = "Nguyễn Văn Nam",
            Phone = "0901234567",
            PlateNumber = "51H-12345",
            VehicleType = "Ô tô",
            ParkingSlot = "A05",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(10),
            TotalPrice = 200000,
            Status = "Sắp tới"
        },
        new()
        {
            Id = 2,
            Code = "DC-2024-000122",
            CustomerName = "Nguyễn Văn Nam",
            Phone = "0901234567",
            PlateNumber = "59X1-88888",
            VehicleType = "Xe máy",
            ParkingSlot = "B02",
            StartTime = DateTime.Now.AddDays(-1),
            EndTime = DateTime.Now.AddDays(-1).AddHours(8),
            TotalPrice = 80000,
            Status = "Đã hoàn thành"
        }
    };
}