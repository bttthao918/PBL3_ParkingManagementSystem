using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    [ApiController]
    [Route("api/payments/payos")]
    [Produces("application/json")]
    public class PayOsController : ControllerBase
    {
        private readonly IPayOsService _payOsService;
        private readonly IMonthlyTicketService _monthlyTicketService;
        private readonly ILogger<PayOsController> _logger;

        public PayOsController(
            IPayOsService payOsService,
            IMonthlyTicketService monthlyTicketService,
            ILogger<PayOsController> logger)
        {
            _payOsService = payOsService;
            _monthlyTicketService = monthlyTicketService;
            _logger = logger;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] JsonElement payload)
        {
            PayOsWebhookDto? webhook;
            try
            {
                webhook = payload.Deserialize<PayOsWebhookDto>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid PayOS webhook JSON");
                return BadRequest(new { success = false, message = "Invalid webhook payload" });
            }

            if (webhook?.Data == null || !payload.TryGetProperty("data", out var rawData))
            {
                return BadRequest(new { success = false, message = "Missing webhook data" });
            }

            if (!_payOsService.IsValidWebhook(webhook, rawData))
            {
                _logger.LogWarning("PayOS webhook signature invalid for order {OrderCode}", webhook.Data.OrderCode);
                return BadRequest(new { success = false, message = "Invalid signature" });
            }

            if (!webhook.Success || !string.Equals(webhook.Data.Code, "00", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("PayOS webhook ignored: Order={OrderCode}, Code={Code}, Desc={Desc}",
                    webhook.Data.OrderCode,
                    webhook.Data.Code,
                    webhook.Data.Desc);
                return Ok(new { success = true });
            }

            var result = await _monthlyTicketService.ConfirmPayOsPaymentAsync(
                webhook.Data.OrderCode,
                webhook.Data.Amount,
                webhook.Data.PaymentLinkId,
                webhook.Data.Reference);

            if (!result.Success)
            {
                _logger.LogWarning("PayOS webhook was valid but could not activate monthly ticket. Order={OrderCode}, Message={Message}",
                    webhook.Data.OrderCode,
                    result.Message);
            }

            return Ok(new { success = true });
        }

        [HttpPost("confirm-return/{orderCode:long}")]
        [Authorize]
        public async Task<IActionResult> ConfirmReturn(long orderCode)
        {
            var result = await _monthlyTicketService.ConfirmPayOsReturnAsync(orderCode);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                monthlyTicketId = result.Data
            });
        }

        [HttpPost("confirm-monthly-ticket/{monthlyTicketId}")]
        [Authorize]
        public async Task<IActionResult> ConfirmMonthlyTicket(string monthlyTicketId)
        {
            var customerId = User.FindFirst("customerId")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            var result = await _monthlyTicketService.ConfirmPayOsMonthlyTicketAsync(monthlyTicketId, customerId);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                monthlyTicketId = result.Data
            });
        }
    }
}
