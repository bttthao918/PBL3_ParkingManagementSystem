AOS.init({
    once: true,
    duration: 800
});

window.addEventListener('scroll', function () {
    const navbar = document.getElementById('mainNavbar');
    if (window.scrollY > 50) {
        navbar.classList.add('scrolled');
    } else {
        navbar.classList.remove('scrolled');
    }
});

document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

const alertElement = document.querySelector('.alert-modern');
if (alertElement) {
    setTimeout(() => {
        const bsAlert = new bootstrap.Alert(alertElement);
        bsAlert.close();
    }, 5000);
}
script.js
// script.js - Điều khiển chuyển trang và load nội dung động

let currentPage = 'dashboard';
let revenueChart = null;

// Hàm load nội dung từ file HTML bên ngoài
async function loadPageContent(pageName) {
    const contentDiv = document.getElementById('dynamicContent');
    const titleElem = document.getElementById('dynamicTitle');

    // Hiển thị loading
    contentDiv.innerHTML = '<div style="text-align: center; padding: 50px;"><i class="fas fa-spinner fa-pulse"></i> Đang tải...</div>';

    try {
        const response = await fetch(`pages/${pageName}.html`);
        if (!response.ok) throw new Error('Không thể tải trang');
        const html = await response.text();
        contentDiv.innerHTML = html;

        // Cập nhật title theo trang
        const titles = {
            dashboard: '📊 Thống kê & Báo cáo',
            accounts: '👥 Quản lý tài khoản',
            tickets: '🎟️ Quản lý vé',
            employees: '🧑‍💼 Quản lý nhân viên'
        };
        titleElem.innerText = titles[pageName] || 'Quản trị';

        // Sau khi load xong nội dung, khởi tạo các thành phần đặc biệt (nếu cần)
        if (pageName === 'dashboard') {
            initDashboardCharts();
            attachDashboardEvents();
        } else if (pageName === 'accounts') {
            attachAccountsEvents();
        } else if (pageName === 'tickets') {
            attachTicketsEvents();
        } else if (pageName === 'employees') {
            attachEmployeesEvents();
        }

    } catch (error) {
        console.error('Lỗi load trang:', error);
        contentDiv.innerHTML = '<div class="empty-row-msg"><i class="fas fa-exclamation-triangle"></i> Không thể tải nội dung. Vui lòng kiểm tra file pages/' + pageName + '.html</div>';
    }
}

// Khởi tạo biểu đồ dashboard
function initDashboardCharts() {
    const canvas = document.getElementById('revenueChartCanvas');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (revenueChart) revenueChart.destroy();

    revenueChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6'],
            datasets: [{
                label: 'Doanh thu (triệu VND)',
                data: [0, 0, 0, 0, 0, 0],
                borderColor: '#3b82f6',
                tension: 0.3,
                fill: true,
                backgroundColor: 'rgba(59,130,246,0.05)'
            }]
        },
        options: { responsive: true, maintainAspectRatio: true }
    });
}

// Gắn sự kiện cho nút trong dashboard
function attachDashboardEvents() {
    const refreshBtn = document.getElementById('refreshDataBtn');
    if (refreshBtn) {
        refreshBtn.onclick = () => {
            alert('🔁 Đây là nơi bạn gọi API để lấy dữ liệu từ SQL và cập nhật giao diện');
            // Ví dụ: fetch('/api/get-stats').then(...)
        };
    }
}

function attachAccountsEvents() {
    const loadBtn = document.getElementById('sqlLoadAccountsBtn');
    if (loadBtn) {
        loadBtn.onclick = () => {
            alert('📥 Gọi API lấy danh sách tài khoản từ SQL và hiển thị vào bảng');
        };
    }
}

function attachTicketsEvents() {
    const loadBtn = document.getElementById('sqlLoadTicketsBtn');
    if (loadBtn) {
        loadBtn.onclick = () => {
            alert('🎫 Gọi API lấy dữ liệu vé từ SQL');
        };
    }
}

function attachEmployeesEvents() {
    const loadBtn = document.getElementById('sqlLoadEmpBtn');
    if (loadBtn) {
        loadBtn.onclick = () => {
            alert('👥 Gọi API lấy danh sách nhân viên từ SQL');
        };
    }
}

// Xử lý chuyển tab trên sidebar
document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', (e) => {
        // Cập nhật active class
        document.querySelectorAll('.nav-item').forEach(nav => nav.classList.remove('active'));
        item.classList.add('active');

        // Lấy tên trang cần load
        const pageName = item.getAttribute('data-page');
        if (pageName) {
            currentPage = pageName;
            loadPageContent(pageName);
        }
    });
});

// Load trang mặc định khi khởi động
loadPageContent('dashboard');

