using System;
using System.Threading.Tasks;
using System.Net;
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

        public async Task SendPasswordResetOtpEmailAsync(string email, string fullName, string otp)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            {
                return;
            }

            var safeName = string.IsNullOrWhiteSpace(fullName) ? "bạn" : WebUtility.HtmlEncode(fullName.Trim());
            var subject = "Mã OTP đặt lại mật khẩu - ParkSmart";
            var body = $@"
        <div style='font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif; max-width: 560px; margin: auto; border: 1px solid #dbe7f7; border-radius: 16px; overflow: hidden; background: #ffffff;'>
            <div style='background: linear-gradient(135deg, #0db7ff, #005bff); padding: 24px; text-align: center;'>
                <div style='width: 58px; height: 58px; margin: 0 auto 12px; border-radius: 18px; background: rgba(255,255,255,.18); color: #fff; display: inline-grid; place-items: center; font-size: 28px; font-weight: 800;'>P</div>
                <h2 style='color: white; margin: 0; font-size: 24px;'>Đặt lại mật khẩu</h2>
                <p style='color: rgba(255,255,255,.86); margin: 8px 0 0;'>ParkSmart Management</p>
            </div>
            <div style='padding: 28px; line-height: 1.6; color: #1f2937;'>
                <p>Xin chào <strong>{safeName}</strong>,</p>
                <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản trong hệ thống quản lý bãi xe.</p>
                <div style='background: #f1f7ff; border: 1px solid #cfe4ff; padding: 18px; text-align: center; border-radius: 14px; margin: 22px 0;'>
                    <p style='margin: 0 0 8px; font-size: 14px; color: #64748b;'>Mã OTP của bạn</p>
                    <div style='font-size: 34px; color: #0869ff; font-weight: 900; letter-spacing: 8px;'>{otp}</div>
                </div>
                <p style='font-size: 13px; color: #dc2626; margin: 0;'>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                <p style='font-size: 13px; color: #64748b;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            </div>
            <div style='background: #f8fafc; padding: 16px; text-align: center; font-size: 12px; color: #64748b;'>
                Email tự động từ ParkSmart, vui lòng không phản hồi.
            </div>
        </div>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmployeeInviteConfirmationEmailAsync(string email, string fullName, string employeeCode, string confirmationUrl, DateTime expiryTime)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(confirmationUrl))
            {
                return;
            }

            var safeName = string.IsNullOrWhiteSpace(fullName) ? "bạn" : fullName.Trim();
            var subject = "Xác nhận tài khoản nhân viên - ParkSmart";
            var body = $@"
        <div style='font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif; max-width: 560px; margin: auto; border: 1px solid #e0e0e0; border-radius: 10px; overflow: hidden;'>
            <div style='background-color: #1e88e5; padding: 20px; text-align: center;'>
                <h2 style='color: white; margin: 0;'>ParkSmart Employee Account</h2>
            </div>
            <div style='padding: 24px; line-height: 1.6; color: #333;'>
                <p>Xin chào <strong>{safeName}</strong>,</p>
                <p>Quản lý đã tạo tài khoản nhân viên cho bạn trên hệ thống ParkSmart.</p>
                <p>Mã nhân viên của bạn: <strong>{employeeCode}</strong></p>
                <p>Vui lòng xác nhận thông tin để kích hoạt tài khoản:</p>
                <p style='text-align:center; margin: 24px 0;'>
                    <a href='{confirmationUrl}' style='background:#1e88e5; color:#fff; text-decoration:none; padding:12px 18px; border-radius:8px; display:inline-block; font-weight:600;'>
                        Xác nhận kích hoạt tài khoản
                    </a>
                </p>
                <p style='font-size:13px; color:#666;'>Link có hiệu lực đến: <strong>{expiryTime:dd/MM/yyyy HH:mm}</strong>.</p>
                <p style='font-size:13px; color:#666;'>Nếu bạn không mong đợi email này, vui lòng liên hệ quản lý.</p>
            </div>
        </div>";

            await SendEmailAsync(email, subject, body);
        }
    }
}
