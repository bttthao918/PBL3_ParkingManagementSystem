namespace ParkingManagement.Web.Models.Requests;

/// <summary>Request body cho API POST /api/parking/check-in</summary>
public record CheckInRequest(
    /// <summary>Biển số xe, ví dụ: 43A-12345</summary>
    string LicensePlate,

    /// <summary>Loại xe: Motorcycle | Car | Truck | Bicycle</summary>
    string VehicleType,

    /// <summary>ID khu bãi đỗ xe</summary>
    int AreaId
);

/// <summary>Request body cho API POST /api/parking/check-out</summary>
public record CheckOutRequest(
    /// <summary>ID vé cần checkout</summary>
    int TicketId
);
