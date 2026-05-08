// Khởi tạo AOS nếu có
if (typeof AOS !== 'undefined') {
    AOS.init({
        once: true,
        duration: 800
    });
}

// Lấy modal elements
var modal = document.getElementById('spotModal');
var closeBtn = document.getElementsByClassName('close')[0];
var bookSpotBtn = document.getElementById('bookSpotBtn');

// Biến lưu thông tin chỗ đỗ xe hiện tại
var currentSpot = null;

// Thêm sự kiện click cho tất cả các ô đỗ xe
document.querySelectorAll('.parking-spot').forEach(function (spot) {
    spot.addEventListener('click', function (e) {
        // Ngăn chặn nếu đang bảo trì
        if (this.classList.contains('spot-maintenance')) {
            showNotification('Chỗ đỗ xe đang bảo trì!', 'error');
            return;
        }

        // Lấy thông tin từ data attributes hoặc từ nội dung
        var spotNumber = this.querySelector('.spot-number')?.innerText || 'Không xác định';
        var statusElement = this.querySelector('.spot-status span');
        var status = statusElement ? statusElement.innerText : 'Không xác định';
        var typeElement = this.querySelector('.spot-type span');
        var vehicleType = typeElement ? typeElement.innerText : 'Không xác định';
        var lastUpdated = new Date().toLocaleString('vi-VN');

        // Cập nhật modal
        document.getElementById('modalSpotNumber').innerText = spotNumber;

        var statusSpan = document.getElementById('modalStatus');
        statusSpan.innerHTML = '';
        var statusBadge = document.createElement('span');

        if (status === 'Trống') {
            statusBadge.className = 'badge';
            statusBadge.style.background = 'linear-gradient(135deg, #00f2b0, #00d4ff)';
            statusBadge.style.padding = '0.3rem 0.8rem';
            statusBadge.style.borderRadius = '20px';
            statusBadge.innerHTML = '<i class="fas fa-check-circle"></i> ' + status;
        } else {
            statusBadge.className = 'badge';
            statusBadge.style.background = 'linear-gradient(135deg, #ff2d75, #ff9a2e)';
            statusBadge.style.padding = '0.3rem 0.8rem';
            statusBadge.style.borderRadius = '20px';
            statusBadge.innerHTML = '<i class="fas fa-car"></i> ' + status;
        }
        statusSpan.appendChild(statusBadge);

        document.getElementById('modalVehicleType').innerHTML = '<i class="fas fa-motorcycle"></i> ' + vehicleType;
        document.getElementById('modalLastUpdated').innerHTML = '<i class="fas fa-clock"></i> ' + lastUpdated;

        // Lưu thông tin chỗ đỗ xe hiện tại
        currentSpot = {
            id: this.getAttribute('data-spot-id'),
            number: spotNumber,
            status: status
        };

        // Hiển thị modal
        modal.style.display = 'block';

        // Thêm hiệu ứng
        document.body.style.overflow = 'hidden';
    });
});

// Đóng modal
if (closeBtn) {
    closeBtn.onclick = function () {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    };
}

// Click ra ngoài modal để đóng
window.onclick = function (event) {
    if (event.target == modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
};

// Xử lý đặt chỗ
if (bookSpotBtn) {
    bookSpotBtn.onclick = function () {
        if (currentSpot && currentSpot.status === 'Trống') {
            showNotification('Đang chuyển đến trang đặt chỗ cho ' + currentSpot.number, 'success');
            // Chuyển hướng đến trang đặt chỗ
            setTimeout(function () {
                window.location.href = '/Booking?spotId=' + currentSpot.id;
            }, 1500);
        } else if (currentSpot && currentSpot.status !== 'Trống') {
            showNotification('Chỗ đỗ xe này đã được sử dụng!', 'error');
        }
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    };
}

// Hàm hiển thị thông báo
function showNotification(message, type) {
    // Tạo notification element
    var notification = document.createElement('div');
    notification.className = 'notification';
    notification.innerHTML = `
        <div class="notification-content ${type}">
            <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'}"></i>
            <span>${message}</span>
        </div>
    `;

    // Style cho notification
    notification.style.position = 'fixed';
    notification.style.bottom = '20px';
    notification.style.right = '20px';
    notification.style.zIndex = '10000';
    notification.style.animation = 'slideInRight 0.3s ease';

    var style = document.createElement('style');
    style.textContent = `
        @keyframes slideInRight {
            from {
                opacity: 0;
                transform: translateX(100px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        @keyframes slideOutRight {
            from {
                opacity: 1;
                transform: translateX(0);
            }
            to {
                opacity: 0;
                transform: translateX(100px);
            }
        }
        .notification-content {
            background: linear-gradient(135deg, #1a1a1a, #111);
            backdrop-filter: blur(10px);
            border-radius: 16px;
            padding: 1rem 1.5rem;
            display: flex;
            align-items: center;
            gap: 0.8rem;
            border: 1px solid rgba(255,255,255,0.1);
            box-shadow: 0 10px 30px rgba(0,0,0,0.5);
        }
        .notification-content.success {
            border-left: 4px solid #00f2b0;
        }
        .notification-content.success i {
            color: #00f2b0;
        }
        .notification-content.error {
            border-left: 4px solid #ff2d75;
        }
        .notification-content.error i {
            color: #ff2d75;
        }
    `;
    document.head.appendChild(style);

    document.body.appendChild(notification);

    // Tự động ẩn sau 3 giây
    setTimeout(function () {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(function () {
            notification.remove();
        }, 300);
    }, 3000);
}

// Thêm hiệu ứng scroll cho navbar (nếu có)
window.addEventListener('scroll', function () {
    var navbar = document.getElementById('mainNavbar');
    if (navbar && window.scrollY > 50) {
        navbar.classList.add('scrolled');
    } else if (navbar) {
        navbar.classList.remove('scrolled');
    }
});

// Lọc và tìm kiếm (nếu cần)
var searchInput = document.getElementById('searchSpot');
if (searchInput) {
    searchInput.addEventListener('input', function (e) {
        var searchTerm = e.target.value.toLowerCase();
        document.querySelectorAll('.parking-spot').forEach(function (spot) {
            var spotNumber = spot.querySelector('.spot-number')?.innerText.toLowerCase() || '';
            if (spotNumber.includes(searchTerm)) {
                spot.style.display = 'flex';
                spot.style.animation = 'fadeInUp 0.3s ease';
            } else {
                spot.style.display = 'none';
            }
        });
    });
}

// Auto refresh dữ liệu mỗi 30 giây (có thể bật/tắt)
var autoRefresh = false; // Set true để bật auto refresh
if (autoRefresh) {
    setInterval(function () {
        location.reload();
    }, 30000);
}

console.log('Sơ đồ bãi đỗ xe đã sẵn sàng!');
