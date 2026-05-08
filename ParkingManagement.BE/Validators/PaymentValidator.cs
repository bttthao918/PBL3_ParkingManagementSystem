using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Validators
{
    /// <summary>
    /// Validator cho Payment logic
    /// Validate dữ liệu thanh toán theo SOLID principles
    /// </summary>
    public interface IPaymentValidator
    {
        /// <summary>
        /// Kiểm tra phương thức thanh toán có hợp lệ
        /// </summary>
        ValidationResult ValidatePaymentMethod(string paymentMethod);

        /// <summary>
        /// Kiểm tra số tiền có hợp lệ
        /// </summary>
        ValidationResult ValidateAmount(decimal amount);

        /// <summary>
        /// Kiểm tra ConfirmPaymentDto
        /// </summary>
        ValidationResult ValidateConfirmPayment(ConfirmPaymentDto dto);

        /// <summary>
        /// Kiểm tra CashPaymentDto
        /// </summary>
        ValidationResult ValidateCashPayment(CashPaymentDto dto);

        /// <summary>
        /// Kiểm tra BankTransferPaymentDto
        /// </summary>
        ValidationResult ValidateBankTransfer(BankTransferPaymentDto dto);

        /// <summary>
        /// Kiểm tra EWalletPaymentDto
        /// </summary>
        ValidationResult ValidateEWallet(EWalletPaymentDto dto);
    }

    /// <summary>
    /// Implementation của PaymentValidator
    /// </summary>
    public class PaymentValidator : IPaymentValidator
    {
        public ValidationResult ValidatePaymentMethod(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                return ValidationResult.Fail("Phương thức thanh toán không được để trống");

            var allowedMethods = PaymentMethods.GetAll();
            if (!allowedMethods.Contains(paymentMethod))
                return ValidationResult.Fail($"Phương thức thanh toán '{paymentMethod}' không hợp lệ");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                return ValidationResult.Fail("Số tiền phải lớn hơn 0");

            if (amount > 999999999)
                return ValidationResult.Fail("Số tiền quá lớn");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateConfirmPayment(ConfirmPaymentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TicketId))
                return ValidationResult.Fail("Mã vé không được để trống");

            var methodValidation = ValidatePaymentMethod(dto.PaymentMethod);
            if (!methodValidation.IsValid)
                return methodValidation;

            var amountValidation = ValidateAmount(dto.Amount);
            if (!amountValidation.IsValid)
                return amountValidation;

            return ValidationResult.Success();
        }

        public ValidationResult ValidateCashPayment(CashPaymentDto dto)
        {
            if (dto.Amount <= 0)
                return ValidationResult.Fail("Số tiền thanh toán phải lớn hơn 0");

            if (dto.ReceivedAmount < dto.Amount)
                return ValidationResult.Fail("Số tiền nhận phải >= số tiền thanh toán");

            if (dto.Change < 0)
                return ValidationResult.Fail("Tiền thừa không hợp lệ");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateBankTransfer(BankTransferPaymentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BankName))
                return ValidationResult.Fail("Tên ngân hàng không được để trống");

            if (string.IsNullOrWhiteSpace(dto.BankAccount))
                return ValidationResult.Fail("Số tài khoản không được để trống");

            if (string.IsNullOrWhiteSpace(dto.AccountHolder))
                return ValidationResult.Fail("Chủ tài khoản không được để trống");

            if (dto.Amount <= 0)
                return ValidationResult.Fail("Số tiền phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(dto.TransferContent))
                return ValidationResult.Fail("Nội dung chuyển khoản không được để trống");

            if (string.IsNullOrWhiteSpace(dto.QRCodeBase64))
                return ValidationResult.Fail("QR Code không được để trống");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateEWallet(EWalletPaymentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.WalletType))
                return ValidationResult.Fail("Loại ví điện tử không được để trống");

            if (dto.WalletType != "Momo" && dto.WalletType != "ZaloPay")
                return ValidationResult.Fail("Loại ví điện tử không hợp lệ (chỉ Momo hoặc ZaloPay)");

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || dto.PhoneNumber.Length < 10)
                return ValidationResult.Fail("Số điện thoại không hợp lệ");

            if (dto.Amount <= 0)
                return ValidationResult.Fail("Số tiền phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(dto.QRCodeBase64))
                return ValidationResult.Fail("QR Code không được để trống");

            if (string.IsNullOrWhiteSpace(dto.PaymentLink))
                return ValidationResult.Fail("Link thanh toán không được để trống");

            return ValidationResult.Success();
        }
    }
}
