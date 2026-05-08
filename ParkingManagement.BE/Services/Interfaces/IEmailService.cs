using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        // Hàm gửi mail chung
        Task SendEmailAsync(string email, string subject, string htmlMessage);

        // Hàm chuyên biệt để gửi OTP (UI nằm trong này)
        Task SendOtpEmailAsync(string email, string fullName, string otp);
    }
}