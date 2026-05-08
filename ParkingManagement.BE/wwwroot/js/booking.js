document.addEventListener('DOMContentLoaded', function () {
    // 1. Khai báo các phần tử dựa trên ID trong HTML của bạn
    const bookingForm = document.getElementById('bookingForm');
    const vehiclePlateInput = document.getElementById('vehiclePlate');
    const expectedTimeInput = document.getElementById('expectedTime');
    const slotSelect = document.getElementById('preferredSlotIdSelect');

    // Các phần tử hiển thị trong bảng tóm tắt
    const summaryDiv = document.getElementById('bookingSummary');
    const summarySpot = document.getElementById('summarySpot'); // Biển số
    const summaryTime = document.getElementById('summaryTime'); // Thời gian
    // Lưu ý: Nếu bạn muốn hiển thị giá hoặc vị trí cụ thể, hãy thêm các ID tương ứng vào HTML

    // 2. Set thời gian mặc định là hiện tại
    if (expectedTimeInput) {
        const now = new Date();
        // Định dạng yyyy-MM-ddThh:mm để khớp với input datetime-local
        now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
        expectedTimeInput.value = now.toISOString().slice(0, 16);
    }

    // 3. Hàm cập nhật bảng tóm tắt (Summary)
    function updateSummary() {
        const plate = vehiclePlateInput?.value;
        const time = expectedTimeInput?.value;

        // Kiểm tra nếu đã nhập đủ thông tin cơ bản
        if (plate && time) {
            summaryDiv.style.display = 'block';

            // Đổ dữ liệu vào bảng tóm tắt
            if (summarySpot) summarySpot.innerText = plate;
            if (summaryTime) {
                // Định dạng lại thời gian hiển thị cho đẹp (VD: 23/04/2026 14:30)
                const dateObj = new Date(time);
                summaryTime.innerText = dateObj.toLocaleString('vi-VN');
            }
        } else {
            summaryDiv.style.display = 'none';
        }
    }

    // 4. Gắn sự kiện lắng nghe (Event Listeners)
    vehiclePlateInput?.addEventListener('input', updateSummary);
    expectedTimeInput?.addEventListener('change', updateSummary);

    // 5. Xử lý kiểm tra trước khi gửi form (Submit Validation)
    bookingForm?.addEventListener('submit', function (e) {
        const plate = vehiclePlateInput.value.trim().toUpperCase(); // Chuyển chữ hoa cho chuẩn
        const type = document.getElementById('vehicleType').value;

        // Regex kiểm tra biển số xe Việt Nam (Cả mẫu cũ và mẫu mới)
        // Định dạng: 43A-1234, 43A-123.45, 43-A1-123.45...
        const plateRegex = /^([0-9]{2}[A-Z]-[0-9]{4,5}|[0-9]{2}[A-Z][0-9]-[0-9]{4,5}|[0-9]{2}-[A-Z][0-9]-[0-9]{4,5})$/;

        if (!plate || !type) {
            e.preventDefault();
            alert('Vui lòng nhập đầy đủ thông tin!');
            return;
        }

        if (!plateRegex.test(plate)) {
            e.preventDefault(); // Chặn gửi form

            // Thêm class lỗi để kích hoạt CSS rung và viền đỏ
            vehiclePlateInput.classList.add('input-error');
            vehiclePlateInput.focus();

            // Tự động xóa class lỗi sau 1 giây để người dùng nhập lại
            setTimeout(() => {
                vehiclePlateInput.classList.remove('input-error');
            }, 1000);
        }
    });
});