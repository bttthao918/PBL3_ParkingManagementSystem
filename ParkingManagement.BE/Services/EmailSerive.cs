using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.DTOs; 

namespace ParkingManagement.BLL.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        // 1. Hàm gửi mail gốc
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(emailMessage);
            await smtp.DisconnectAsync(true);
        }

        // 2. Hàm soạn UI cho OTP
        // 2. Hàm soạn UI cho OTP đã được tối ưu
        public async Task SendOtpEmailAsync(string email, string fullName, string otp)
        {
            // Kiểm tra đầu vào để tránh crash tại file này
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp)) return;

            string subject = "Mã xác thực OTP - ParkSmart Management";
            string body = $@"
        <div style='font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif; max-width: 500px; margin: auto; border: 1px solid #e0e0e0; border-radius: 10px; overflow: hidden;'>
            <div style='background-color: #4e73df; padding: 20px; text-align: center;'>
                <h1 style='color: white; margin: 0;'>ParkSmart</h1>
            </div>
            <div style='padding: 30px; line-height: 1.6; color: #333;'>
                <p>Xin chào <strong>{fullName}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký thành viên tại hệ thống quản lý bãi xe <strong>ParkSmart</strong>.</p>
                <div style='background-color: #f8f9fc; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin-bottom: 5px; font-size: 14px;'>Mã xác thực (OTP) của bạn là:</p>
                    <span style='font-size: 32px; color: #4e73df; font-weight: bold; letter-spacing: 5px;'>{otp}</span>
                </div>
                <p style='font-size: 13px; color: #e74a3b;'>* Mã này sẽ hết hạn trong vòng <strong>5 phút</strong>.</p>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
            </div>
            <div style='background-color: #f1f3f7; padding: 15px; text-align: center; font-size: 12px; color: #858796;'>
                Đây là email tự động, vui lòng không phản hồi.<br/>
                &copy; 2026 ParkSmart Team.
            </div>
        </div>";

            await SendEmailAsync(email, subject, body);
        }
    }
}
