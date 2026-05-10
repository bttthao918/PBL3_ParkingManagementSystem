// Services/CustomerService.cs
using System.Net.Http.Json;
using ParkingManagement.FE.Models.Customer;

namespace ParkingManagement.FE.Services
{
    public interface ICustomerService
    {
        Task<CustomerProfileDto?> GetProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateCustomerProfileRequest request);
        Task<List<VehicleDto>> GetVehiclesAsync();
    }

    public class CustomerService : BaseHttpService, ICustomerService
    {
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<CustomerService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<CustomerProfileDto?> GetProfileAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/customers/me");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CustomerProfileDto>();
                }

                _logger.LogWarning("Failed to get profile: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer profile");
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UpdateCustomerProfileRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PutAsJsonAsync("api/customers/me", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer profile");
                return false;
            }
        }

        public async Task<List<VehicleDto>> GetVehiclesAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/vehicles");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<VehicleDto>>() ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles");
                return new();
            }
        }
    }
}