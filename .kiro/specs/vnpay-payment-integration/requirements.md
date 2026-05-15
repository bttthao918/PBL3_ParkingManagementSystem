# Requirements Document

## Introduction

Tích hợp thanh toán QR qua VNPay vào Hệ thống Quản lý Bãi xe. Hiện tại hệ thống hỗ trợ 3 phương thức thanh toán (Tiền mặt, Chuyển khoản, Ví điện tử) nhưng chưa có tích hợp cổng thanh toán thực tế. Feature này sẽ kết nối VNPay Sandbox để tạo mã QR thanh toán, cho phép khách hàng quét mã và thanh toán, hệ thống tự động xác nhận qua IPN callback hoặc Return URL fallback.

Áp dụng cho: đăng ký vé tháng, gia hạn vé tháng, và checkout vé lượt.

Cơ chế xác nhận thanh toán: IPN (ưu tiên, cần public URL) + Return URL fallback (hoạt động trên localhost).

## Glossary

- **VNPay_Gateway**: Cổng thanh toán VNPay, sử dụng môi trường Sandbox cho development/demo (URL: sandbox.vnpayment.vn)
- **Payment_Service**: Service xử lý logic thanh toán trong Backend API, bao gồm tạo giao dịch VNPay, xác nhận thanh toán
- **IPN_Endpoint**: Endpoint công khai (không yêu cầu JWT) nhận callback từ VNPay khi giao dịch hoàn tất. Hoạt động khi server có public URL
- **Return_URL**: URL trên Frontend mà VNPay redirect khách hàng về sau khi thanh toán. Dùng làm fallback xác nhận khi IPN không khả dụng
- **Payment_Transaction**: Bản ghi giao dịch thanh toán trong database (bảng Payments), liên kết với vé lượt hoặc vé tháng, lưu thêm vnp_TxnRef
- **QR_Code**: Mã QR được tạo từ URL thanh toán VNPay, khách hàng quét bằng app ngân hàng hoặc nhập thẻ test trên Sandbox
- **Secure_Hash**: Chữ ký HMAC-SHA512 dùng để xác thực tính toàn vẹn dữ liệu giữa hệ thống và VNPay
- **VNPay_Config**: Cấu hình kết nối VNPay bao gồm TmnCode, HashSecret, PayUrl, ReturnUrl, IpnUrl
- **Parking_System**: Hệ thống Quản lý Bãi xe (ASP.NET Core Web API Backend + Razor Pages Frontend)
- **Sandbox_Mode**: Môi trường test của VNPay, giao dịch không trừ tiền thật, dùng thẻ test để demo

## Requirements

### Requirement 1: Tạo URL thanh toán VNPay

**User Story:** As a khách hàng, I want to receive a VNPay payment URL when selecting online payment, so that I can pay via QR code or VNPay payment page.

#### Acceptance Criteria

1. WHEN a customer selects "Chuyển khoản" or "Ví điện tử" as payment method for a ticket checkout or monthly ticket registration/renewal, THE Payment_Service SHALL create a VNPay payment URL with the correct transaction amount and order reference
2. THE Payment_Service SHALL generate a Secure_Hash using HMAC-SHA512 with the configured HashSecret for each payment URL request
3. THE Payment_Service SHALL include the following parameters in the VNPay URL: vnp_Version, vnp_TmnCode, vnp_Amount (amount x 100 as VNPay requires), vnp_Command ("pay"), vnp_CreateDate, vnp_CurrCode ("VND"), vnp_IpAddr, vnp_Locale ("vn"), vnp_OrderInfo, vnp_OrderType, vnp_ReturnUrl, vnp_TxnRef, vnp_SecureHash
4. THE Payment_Service SHALL generate a unique vnp_TxnRef for each payment transaction to prevent duplicate payments
5. WHEN the VNPay payment URL is created successfully, THE Payment_Service SHALL return the payment URL to the Frontend for QR code generation or redirect

### Requirement 2: Xử lý IPN callback từ VNPay

**User Story:** As a system operator, I want the system to automatically confirm payments via VNPay IPN callback, so that payment status is updated without manual intervention when the server is publicly accessible.

#### Acceptance Criteria

1. THE IPN_Endpoint SHALL be publicly accessible without JWT authentication at path "api/vnpay/ipn"
2. WHEN VNPay sends an IPN callback, THE IPN_Endpoint SHALL validate the Secure_Hash of the incoming request using the configured HashSecret
3. IF the Secure_Hash validation fails, THEN THE IPN_Endpoint SHALL respond with JSON containing RspCode "97" and Message "Invalid Checksum"
4. WHEN the Secure_Hash is valid and the transaction exists with status "Chờ thanh toán", THE Payment_Service SHALL update the Payment_Transaction status to "Hoàn tất" if vnp_ResponseCode is "00"
5. IF the vnp_ResponseCode is not "00", THEN THE Payment_Service SHALL update the Payment_Transaction status to "Thất bại"
6. IF the Payment_Transaction has already been confirmed (status is not "Chờ thanh toán"), THEN THE IPN_Endpoint SHALL respond with RspCode "02" and Message "Order already confirmed"
7. IF the Payment_Transaction does not exist for the given vnp_TxnRef, THEN THE IPN_Endpoint SHALL respond with RspCode "01" and Message "Order not found"
8. WHEN the IPN is processed successfully, THE IPN_Endpoint SHALL respond with RspCode "00" and Message "Confirm Success"

### Requirement 3: Xử lý Return URL (fallback xác nhận thanh toán)

**User Story:** As a khách hàng, I want to be redirected back to the parking system after completing payment on VNPay, so that I can see the payment result and the system confirms my payment even without IPN.

#### Acceptance Criteria

1. WHEN VNPay redirects the customer to the Return_URL, THE Parking_System SHALL validate the Secure_Hash of the return query parameters
2. IF the Secure_Hash validation fails, THEN THE Parking_System SHALL display an error message "Dữ liệu thanh toán không hợp lệ"
3. WHEN the Secure_Hash is valid and vnp_ResponseCode is "00" and the Payment_Transaction status is still "Chờ thanh toán", THE Payment_Service SHALL update the Payment_Transaction status to "Hoàn tất" (Return URL fallback)
4. WHEN the payment is successful, THE Parking_System SHALL display a success page with payment amount, transaction reference, and payment time
5. WHEN the payment fails (vnp_ResponseCode is not "00"), THE Parking_System SHALL display a failure message and offer the option to retry payment
6. THE Parking_System SHALL provide a link to navigate back to the monthly ticket page or ticket management page after displaying the result

### Requirement 4: Quản lý trạng thái giao dịch

**User Story:** As a system operator, I want pending VNPay transactions to be tracked and managed, so that incomplete payments do not block the system.

#### Acceptance Criteria

1. WHEN a VNPay payment URL is created, THE Payment_Service SHALL create a Payment_Transaction record with status "Chờ thanh toán" and store the vnp_TxnRef
2. THE Payment_Transaction SHALL store the vnp_TxnRef in a new column to correlate with VNPay callbacks and Return URL responses
3. WHILE a Payment_Transaction has status "Chờ thanh toán" for more than 15 minutes, THE Parking_System SHALL consider the transaction as expired
4. IF a customer initiates a new payment for the same ticket or monthly ticket while a pending transaction exists, THEN THE Payment_Service SHALL cancel the previous pending transaction (set status to "Hủy") and create a new one

### Requirement 5: Cấu hình VNPay

**User Story:** As a system administrator, I want VNPay configuration to be stored in appsettings, so that the integration can be configured for Sandbox or Production without code changes.

#### Acceptance Criteria

1. THE Parking_System SHALL read VNPay_Config values (TmnCode, HashSecret, PayUrl, ReturnUrl, IpnUrl) from the "VNPay" section in appsettings.json
2. THE Parking_System SHALL default to VNPay Sandbox URL (https://sandbox.vnpayment.vn/paymentv2/vpcpay.html) when no PayUrl is configured
3. THE Parking_System SHALL validate that TmnCode and HashSecret are present at application startup
4. IF TmnCode or HashSecret is missing, THEN THE Parking_System SHALL log a warning and disable VNPay payment options in the payment method list

### Requirement 6: Hiển thị QR Code và trạng thái thanh toán trên giao diện

**User Story:** As a khách hàng, I want to see a QR code or be redirected to VNPay payment page, so that I can complete payment easily.

#### Acceptance Criteria

1. WHEN the customer selects "Chuyển khoản" or "Ví điện tử" for monthly ticket registration/renewal, THE Parking_System SHALL redirect the customer to the VNPay payment page (VNPay handles QR display on their page)
2. WHEN the customer selects "Chuyển khoản" or "Ví điện tử" for ticket checkout, THE Parking_System SHALL redirect the customer to the VNPay payment page
3. THE Parking_System SHALL display a loading/waiting indicator before redirecting to VNPay
4. WHEN the customer returns from VNPay via Return_URL, THE Parking_System SHALL display the payment result clearly (success or failure) with Vietnamese language messages

### Requirement 7: Bảo mật tích hợp VNPay

**User Story:** As a system administrator, I want the VNPay integration to be secure, so that payment data cannot be tampered with.

#### Acceptance Criteria

1. THE Payment_Service SHALL validate the Secure_Hash on all incoming VNPay responses (IPN and Return URL) before processing any payment status update
2. THE Parking_System SHALL communicate with VNPay_Gateway over HTTPS only
3. THE Payment_Service SHALL validate that the vnp_Amount in the callback matches the amount stored in the Payment_Transaction record (amount x 100)
4. IF the vnp_Amount does not match the stored amount, THEN THE Payment_Service SHALL reject the transaction and log a security warning
5. THE Parking_System SHALL not expose HashSecret in client-side code, API responses, or browser-accessible configuration
