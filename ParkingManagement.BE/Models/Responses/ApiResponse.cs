namespace ParkingManagement.Web.Models.Responses;

/// <summary>Wrapper response chuẩn cho tất cả API.</summary>
public record ApiResponse<T>(bool Success, string Message, T? Data)
{
    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new(true, message, data);

    public static ApiResponse<T> Fail(string message)
        => new(false, message, default);
}
