using System.Text.Json;
using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IPayOsService
    {
        Task<ServiceResult<PayOsPaymentLinkDto>> CreatePaymentLinkAsync(PayOsCreatePaymentLinkDto dto);
        Task<ServiceResult<PayOsPaymentLinkInfoDto>> GetPaymentLinkInformationAsync(long orderCode);
        bool IsValidWebhook(PayOsWebhookDto webhook, JsonElement rawData);
    }
}
