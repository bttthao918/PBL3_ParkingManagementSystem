namespace ParkingManagement.FE.Models
{
    /// <summary>
    /// Generic service result DTO for API responses
    /// </summary>
    public class ServiceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class ServiceResultDto<T> : ServiceResultDto
    {
        public T? Data { get; set; }
    }
}
