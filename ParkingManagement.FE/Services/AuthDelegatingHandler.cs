using System.Net.Http.Headers;

namespace ParkingManagement.FE.Services;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Thử lấy token từ Session
        var token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");

        // 2. Nếu Session bị mất (reload, expired) thử lấy từ Claims Cookie
        if (string.IsNullOrEmpty(token))
        {
            token = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "jwt_token")?.Value;
        }

        // 3. Đính kèm vào request Http Client hướng về BE
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}