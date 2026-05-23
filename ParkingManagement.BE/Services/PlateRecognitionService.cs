using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class PlateRecognitionService : IPlateRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly PlateRecognitionOptions _options;
        private readonly ILogger<PlateRecognitionService> _logger;

        public PlateRecognitionService(
            HttpClient httpClient,
            IOptions<PlateRecognitionOptions> options,
            ILogger<PlateRecognitionService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<PlateRecognitionResponseDto> RecognizeAsync(
            PlateRecognitionRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiToken))
            {
                return new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Chưa cấu hình PlateRecognition:ApiToken."
                };
            }

            if (string.IsNullOrWhiteSpace(request.ImageBase64))
            {
                return new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Thiếu ảnh để nhận diện biển số."
                };
            }

            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(3, _options.TimeoutSeconds)));

                var imageBytes = DecodeBase64Image(request.ImageBase64);
                using var form = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                form.Add(imageContent, "upload", "plate.jpg");

                foreach (var region in SplitCsv(_options.Regions))
                    form.Add(new StringContent(region), "regions");

                if (!string.IsNullOrWhiteSpace(_options.Config))
                    form.Add(new StringContent(_options.Config), "config");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
                {
                    Content = form
                };
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.ApiToken);

                using var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Plate recognition failed: {Status} {Body}", response.StatusCode, responseBody);
                    return new PlateRecognitionResponseDto
                    {
                        Success = false,
                        Provider = _options.Provider,
                        Message = $"Engine đọc biển số trả lỗi {(int)response.StatusCode}."
                    };
                }

                return ParsePlateRecognizerResponse(responseBody);
            }
            catch (FormatException)
            {
                return new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Ảnh gửi lên không đúng định dạng base64."
                };
            }
            catch (OperationCanceledException)
            {
                return new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Engine đọc biển số phản hồi quá lâu."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plate recognition error");
                return new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Không gọi được engine đọc biển số."
                };
            }
        }

        private PlateRecognitionResponseDto ParsePlateRecognizerResponse(string responseBody)
        {
            using var document = JsonDocument.Parse(responseBody);
            if (!document.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
            {
                return new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Engine không trả về biển số."
                };
            }

            var candidates = new List<(string Plate, double Score)>();
            foreach (var result in results.EnumerateArray())
            {
                var plate = GetString(result, "plate");
                var score = GetDouble(result, "score") ?? 0;
                AddCandidate(candidates, plate, score);

                if (result.TryGetProperty("candidates", out var resultCandidates)
                    && resultCandidates.ValueKind == JsonValueKind.Array)
                {
                    foreach (var candidate in resultCandidates.EnumerateArray())
                    {
                        AddCandidate(
                            candidates,
                            GetString(candidate, "plate"),
                            GetDouble(candidate, "score") ?? score);
                    }
                }
            }

            var best = candidates
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Plate)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(best.Plate)
                ? new PlateRecognitionResponseDto
                {
                    Success = false,
                    Provider = _options.Provider,
                    Message = "Engine không đọc được biển số."
                }
                : new PlateRecognitionResponseDto
                {
                    Success = true,
                    Provider = _options.Provider,
                    Plate = best.Plate,
                    Score = best.Score,
                    Candidates = candidates
                        .Select(candidate => candidate.Plate)
                        .Where(plate => !string.IsNullOrWhiteSpace(plate))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(8)
                        .ToList()
                };
        }

        private static void AddCandidate(List<(string Plate, double Score)> candidates, string? plate, double score)
        {
            var normalizedPlate = NormalizeVietnamPlate(plate);
            if (!string.IsNullOrWhiteSpace(normalizedPlate))
                candidates.Add((normalizedPlate, score));
        }

        private static string NormalizeVietnamPlate(string? plate)
        {
            var compact = new string((plate ?? string.Empty)
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());

            if (compact.Length < 7)
                return string.Empty;

            var tail = compact[^5..];
            var prefix = compact[..^5];

            if (prefix.Length < 3 || prefix.Length > 4 || !tail.All(char.IsDigit))
                return string.Empty;

            return $"{prefix}-{tail[..3]}.{tail[3..]}";
        }

        private static byte[] DecodeBase64Image(string imageBase64)
        {
            var commaIndex = imageBase64.IndexOf(',');
            var payload = commaIndex >= 0 ? imageBase64[(commaIndex + 1)..] : imageBase64;
            return Convert.FromBase64String(payload);
        }

        private static IEnumerable<string> SplitCsv(string value)
        {
            return (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(item => !string.IsNullOrWhiteSpace(item));
        }

        private static string? GetString(JsonElement element, string property)
        {
            return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static double? GetDouble(JsonElement element, string property)
        {
            return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : null;
        }
    }
}
