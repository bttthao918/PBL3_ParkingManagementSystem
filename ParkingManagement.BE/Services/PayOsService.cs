using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class PayOsService : IPayOsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly PayOsOptions _options;
        private readonly ILogger<PayOsService> _logger;

        public PayOsService(
            HttpClient httpClient,
            IOptions<PayOsOptions> options,
            ILogger<PayOsService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ServiceResult<PayOsPaymentLinkDto>> CreatePaymentLinkAsync(PayOsCreatePaymentLinkDto dto)
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId) ||
                string.IsNullOrWhiteSpace(_options.ApiKey) ||
                string.IsNullOrWhiteSpace(_options.ChecksumKey))
            {
                return ServiceResult<PayOsPaymentLinkDto>.Fail("PayOS chưa được cấu hình ClientId, ApiKey hoặc ChecksumKey.");
            }

            if (string.IsNullOrWhiteSpace(_options.ReturnUrl) ||
                string.IsNullOrWhiteSpace(_options.CancelUrl))
            {
                return ServiceResult<PayOsPaymentLinkDto>.Fail("PayOS chưa được cấu hình ReturnUrl hoặc CancelUrl.");
            }

            var returnUrl = AppendOrderCode(_options.ReturnUrl, dto.OrderCode);
            var cancelUrl = AppendOrderCode(_options.CancelUrl, dto.OrderCode);
            var signatureData = $"amount={dto.Amount}&cancelUrl={cancelUrl}&description={dto.Description}&orderCode={dto.OrderCode}&returnUrl={returnUrl}";

            // Keep the payload minimal and aligned with the payOS API docs.
            var request = new
            {
                orderCode = dto.OrderCode,
                amount = dto.Amount,
                description = dto.Description,
                cancelUrl,
                returnUrl,
                signature = ComputeHmacSha256(signatureData, _options.ChecksumKey)
            };

            try
            {
                using var httpRequest = CreateAuthorizedRequest(HttpMethod.Post, BuildUri("/v2/payment-requests"));
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Create PayOS payment link failed: {StatusCode} {Body}", response.StatusCode, body);
                    return ServiceResult<PayOsPaymentLinkDto>.Fail($"Không tạo được link thanh toán payOS. Backend payOS trả về {(int)response.StatusCode}.");
                }

                var payOsResponse = JsonSerializer.Deserialize<PayOsCreatePaymentResponse>(body, JsonOptions);
                if (payOsResponse?.Data == null || !string.Equals(payOsResponse.Code, "00", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Create PayOS payment link returned non-success payload: {Body}", body);
                    return ServiceResult<PayOsPaymentLinkDto>.Fail(payOsResponse?.Desc ?? "Không tạo được link thanh toán payOS.");
                }

                return ServiceResult<PayOsPaymentLinkDto>.Ok(new PayOsPaymentLinkDto
                {
                    OrderCode = payOsResponse.Data.OrderCode,
                    PaymentLinkId = payOsResponse.Data.PaymentLinkId ?? string.Empty,
                    CheckoutUrl = payOsResponse.Data.CheckoutUrl ?? string.Empty,
                    QrCode = payOsResponse.Data.QrCode ?? string.Empty,
                    Status = payOsResponse.Data.Status ?? string.Empty,
                    Amount = payOsResponse.Data.Amount
                }, "Đã tạo link thanh toán payOS.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create PayOS payment link exception");
                return ServiceResult<PayOsPaymentLinkDto>.Fail("Không gọi được payOS để tạo QR thanh toán.");
            }
        }

        public async Task<ServiceResult<PayOsPaymentLinkInfoDto>> GetPaymentLinkInformationAsync(long orderCode)
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId) ||
                string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return ServiceResult<PayOsPaymentLinkInfoDto>.Fail("PayOS chưa được cấu hình ClientId hoặc ApiKey.");
            }

            try
            {
                using var httpRequest = CreateAuthorizedRequest(HttpMethod.Get, BuildUri($"/v2/payment-requests/{orderCode}"));
                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Get PayOS payment link failed: {StatusCode} {Body}", response.StatusCode, body);
                    return ServiceResult<PayOsPaymentLinkInfoDto>.Fail($"Không lấy được thông tin thanh toán payOS. Backend payOS trả về {(int)response.StatusCode}.");
                }

                var payOsResponse = JsonSerializer.Deserialize<PayOsGetPaymentResponse>(body, JsonOptions);
                if (payOsResponse?.Data == null || !string.Equals(payOsResponse.Code, "00", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Get PayOS payment link returned non-success payload: {Body}", body);
                    return ServiceResult<PayOsPaymentLinkInfoDto>.Fail(payOsResponse?.Desc ?? "Không lấy được thông tin thanh toán payOS.");
                }

                return ServiceResult<PayOsPaymentLinkInfoDto>.Ok(new PayOsPaymentLinkInfoDto
                {
                    PaymentLinkId = payOsResponse.Data.Id ?? string.Empty,
                    OrderCode = payOsResponse.Data.OrderCode,
                    Amount = payOsResponse.Data.Amount,
                    AmountPaid = payOsResponse.Data.AmountPaid,
                    AmountRemaining = payOsResponse.Data.AmountRemaining,
                    Status = payOsResponse.Data.Status ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get PayOS payment link exception");
                return ServiceResult<PayOsPaymentLinkInfoDto>.Fail("Không gọi được payOS để kiểm tra trạng thái thanh toán.");
            }
        }

        public bool IsValidWebhook(PayOsWebhookDto webhook, JsonElement rawData)
        {
            if (webhook.Data == null || string.IsNullOrWhiteSpace(webhook.Signature))
            {
                return false;
            }

            var sortedData = BuildSortedData(rawData);
            var expected = ComputeHmacSha256(sortedData, _options.ChecksumKey);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(webhook.Signature));
        }

        private Uri BuildUri(string path)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_options.ApiBaseUrl)
                ? "https://api-merchant.payos.vn"
                : _options.ApiBaseUrl.TrimEnd('/');
            return new Uri($"{baseUrl}{path}");
        }

        private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.TryAddWithoutValidation("x-client-id", _options.ClientId.Trim());
            request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey.Trim());
            return request;
        }

        private static string AppendOrderCode(string url, long orderCode)
        {
            var separator = url.Contains('?') ? '&' : '?';
            return $"{url}{separator}orderCode={orderCode}";
        }

        private static string BuildSortedData(JsonElement element)
        {
            var parts = element.EnumerateObject()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => $"{property.Name}={JsonElementToSignatureValue(property.Value)}");
            return string.Join('&', parts);
        }

        private static string JsonElementToSignatureValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => string.Empty,
                JsonValueKind.Undefined => string.Empty,
                _ => element.GetRawText()
            };
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private sealed class PayOsCreatePaymentResponse
        {
            public string Code { get; set; } = string.Empty;
            public string Desc { get; set; } = string.Empty;
            public PayOsCreatePaymentData? Data { get; set; }
        }

        private sealed class PayOsCreatePaymentData
        {
            public string? PaymentLinkId { get; set; }
            public long OrderCode { get; set; }
            public int Amount { get; set; }
            public string? Status { get; set; }
            public string? CheckoutUrl { get; set; }
            public string? QrCode { get; set; }
        }

        private sealed class PayOsGetPaymentResponse
        {
            public string Code { get; set; } = string.Empty;
            public string Desc { get; set; } = string.Empty;
            public PayOsGetPaymentData? Data { get; set; }
        }

        private sealed class PayOsGetPaymentData
        {
            public string? Id { get; set; }
            public long OrderCode { get; set; }
            public int Amount { get; set; }
            public int AmountPaid { get; set; }
            public int AmountRemaining { get; set; }
            public string? Status { get; set; }
        }
    }
}
