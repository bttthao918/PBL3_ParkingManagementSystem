using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IVnPayService
    {
        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        string CreatePaymentUrl(VnPayCreateUrlDto dto, string clientIpAddress);

        /// <summary>
        /// Validate response từ VNPay (IPN hoặc Return URL)
        /// </summary>
        VnPayResponseDto ValidateResponse(IQueryCollection queryParams);

        /// <summary>
        /// Kiểm tra VNPay đã được cấu hình chưa
        /// </summary>
        bool IsConfigured { get; }
    }
}
