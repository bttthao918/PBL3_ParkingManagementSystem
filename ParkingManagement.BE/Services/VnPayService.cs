using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class VnPayService : IVnPayService
    {
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _payUrl;
        private readonly string _returnUrl;
        private readonly string _version;
        private readonly ILogger<VnPayService> _logger;

        public bool IsConfigured { get; }

        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger)
        {
            _logger = logger;

            var vnpaySection = configuration.GetSection("VNPay");
            _tmnCode = vnpaySection["TmnCode"] ?? "";
            _hashSecret = vnpaySection["HashSecret"] ?? "";
            _payUrl = vnpaySection["PayUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            _returnUrl = vnpaySection["ReturnUrl"] ?? "";
            _version = vnpaySection["Version"] ?? "2.1.0";

            IsConfigured = !string.IsNullOrWhiteSpace(_tmnCode) && !string.IsNullOrWhiteSpace(_hashSecret);

            if (!IsConfigured)
            {
                _logger.LogWarning("VNPay is not configured. TmnCode or HashSecret is missing.");
            }
        }

        public string CreatePaymentUrl(VnPayCreateUrlDto dto, string clientIpAddress)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("VNPay chưa được cấu hình.");

            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", _version },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _tmnCode },
                { "vnp_Amount", ((long)(dto.Amount * 100)).ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", dto.TxnRef },
                { "vnp_OrderInfo", dto.OrderInfo },
                { "vnp_OrderType", dto.OrderType ?? "other" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", _returnUrl },
                { "vnp_IpAddr", clientIpAddress },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            // Build query string (without hash)
            var queryBuilder = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                if (queryBuilder.Length > 0) queryBuilder.Append('&');
                queryBuilder.Append(WebUtility.UrlEncode(kv.Key));
                queryBuilder.Append('=');
                queryBuilder.Append(WebUtility.UrlEncode(kv.Value));
            }

            // Create HMAC-SHA512 hash
            var signData = queryBuilder.ToString();
            var hash = HmacSha512(_hashSecret, signData);

            var paymentUrl = $"{_payUrl}?{signData}&vnp_SecureHash={hash}";

            _logger.LogInformation("VNPay URL created for TxnRef: {TxnRef}, Amount: {Amount}", dto.TxnRef, dto.Amount);

            return paymentUrl;
        }

        public VnPayResponseDto ValidateResponse(IQueryCollection queryParams)
        {
            var response = new VnPayResponseDto();

            if (!queryParams.Any())
            {
                response.Success = false;
                response.Message = "Không có dữ liệu từ VNPay";
                return response;
            }

            // Extract vnp_SecureHash
            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();

            // Build sorted params (exclude vnp_SecureHash and vnp_SecureHashType)
            var vnpParams = new SortedDictionary<string, string>();
            foreach (var key in queryParams.Keys)
            {
                if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    var value = queryParams[key].ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        vnpParams[key] = value;
                    }
                }
            }

            // Build sign data
            var signDataBuilder = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                if (signDataBuilder.Length > 0) signDataBuilder.Append('&');
                signDataBuilder.Append(WebUtility.UrlEncode(kv.Key));
                signDataBuilder.Append('=');
                signDataBuilder.Append(WebUtility.UrlEncode(kv.Value));
            }

            var computedHash = HmacSha512(_hashSecret, signDataBuilder.ToString());

            // Validate hash
            if (!string.Equals(computedHash, vnpSecureHash, StringComparison.OrdinalIgnoreCase))
            {
                response.Success = false;
                response.IsValidHash = false;
                response.Message = "Invalid Checksum";
                _logger.LogWarning("VNPay hash validation failed. TxnRef: {TxnRef}", vnpParams.GetValueOrDefault("vnp_TxnRef"));
                return response;
            }

            response.IsValidHash = true;
            response.TxnRef = vnpParams.GetValueOrDefault("vnp_TxnRef") ?? "";
            response.ResponseCode = vnpParams.GetValueOrDefault("vnp_ResponseCode") ?? "";
            response.TransactionNo = vnpParams.GetValueOrDefault("vnp_TransactionNo") ?? "";
            response.OrderInfo = vnpParams.GetValueOrDefault("vnp_OrderInfo") ?? "";
            response.PayDate = vnpParams.GetValueOrDefault("vnp_PayDate") ?? "";

            if (vnpParams.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var amountLong))
            {
                response.Amount = amountLong / 100m; // VNPay sends amount * 100
            }

            response.Success = response.ResponseCode == "00";
            response.Message = response.Success ? "Thanh toán thành công" : $"Thanh toán thất bại (Mã: {response.ResponseCode})";

            return response;
        }

        private static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
