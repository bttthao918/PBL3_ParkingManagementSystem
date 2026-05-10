using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ParkingManagement.FE.Services
{
    public abstract class BaseHttpService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseHttpService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected void AttachToken()
        {
            var token = _httpContextAccessor?.HttpContext?.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            // Thêm log để debug
            Console.WriteLine($"Token attached: {(string.IsNullOrEmpty(token) ? "NO" : "YES")}");
        }

        protected async Task<T?> GetAsync<T>(string url) where T : class
        {
            AttachToken();
            Console.WriteLine($"Calling API: {_httpClient.BaseAddress}{url}");
            var response = await _httpClient.GetAsync(url);
            Console.WriteLine($"Response: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            return null;
        }

        protected async Task<List<T>> GetListAsync<T>(string url) where T : class
        {
            AttachToken();
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
            }

            return new List<T>();
        }

        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
            where TResponse : class
            where TRequest : class
        {
            AttachToken();
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            return null;
        }

        protected async Task<bool> PutAsync<TRequest>(string url, TRequest request) where TRequest : class
        {
            AttachToken();
            var response = await _httpClient.PutAsJsonAsync(url, request);
            return response.IsSuccessStatusCode;
        }

        protected async Task<bool> DeleteAsync(string url)
        {
            AttachToken();
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        protected async Task<T?> GetFromJsonAsync<T>(string url) where T : class
        {
            AttachToken();
            return await GetAsync<T>(url);
        }

        protected class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}