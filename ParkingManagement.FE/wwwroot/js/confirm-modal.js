/**
 * Custom Confirm Modal — thay thế browser confirm() xấu
 * Sử dụng: confirmAction('Tiêu đề', 'Nội dung', function() { ... })
 * Hoặc gắn vào form: <form onsubmit="return confirmSubmit(this, 'Xác nhận?', 'Mô tả')">
 */

(function () {
    // Tạo modal HTML 1 lần
    const modalHTML = `
    <div class="confirm-overlay" id="confirmModal">
        <div class="confirm-box">
            <div class="confirm-icon" id="confirmIcon">
                <i class="fa-solid fa-circle-question"></i>
            </div>
            <h3 id="confirmTitle">Xác nhận</h3>
            <p id="confirmMessage">Bạn có chắc muốn thực hiện?</p>
            <div class="confirm-actions">
                <button class="confirm-btn-cancel" id="confirmCancel">Hủy</button>
                <button class="confirm-btn-ok" id="confirmOk">Xác nhận</button>
            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', modalHTML);

    const modal = document.getElementById('confirmModal');
    const titleEl = document.getElementById('confirmTitle');
    const msgEl = document.getElementById('confirmMessage');
    const iconEl = document.getElementById('confirmIcon');
    const okBtn = document.getElementById('confirmOk');
    const cancelBtn = document.getElementById('confirmCancel');

    let resolveCallback = null;

    function showConfirm(title, message, type) {
        titleEl.textContent = title || 'Xác nhận';
        msgEl.textContent = message || 'Bạn có chắc muốn thực hiện hành động này?';

        // Icon theo type
        iconEl.className = 'confirm-icon ' + (type || 'info');
        const iconMap = {
            'danger': 'fa-solid fa-triangle-exclamation',
            'warning': 'fa-solid fa-circle-exclamation',
            'success': 'fa-solid fa-circle-check',
            'info': 'fa-solid fa-circle-question'
        };
        iconEl.innerHTML = `<i class="${iconMap[type] || iconMap.info}"></i>`;

        // Button style theo type
        okBtn.className = 'confirm-btn-ok ' + (type || 'info');
        okBtn.textContent = type === 'danger' ? 'Xác nhận' : 'Đồng ý';

        modal.classList.add('open');

        return new Promise(resolve => {
            resolveCallback = resolve;
        });
    }

    okBtn.addEventListener('click', () => {
        modal.classList.remove('open');
        if (resolveCallback) resolveCallback(true);
    });

    cancelBtn.addEventListener('click', () => {
        modal.classList.remove('open');
        if (resolveCallback) resolveCallback(false);
    });

    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('open');
            if (resolveCallback) resolveCallback(false);
        }
    });

    // Global function
    window.confirmAction = function (title, message, callback, type) {
        showConfirm(title, message, type || 'info').then(ok => {
            if (ok && callback) callback();
        });
    };

    // For forms — gắn vào onsubmit
    window.confirmSubmit = function (form, title, message, type) {
        if (form.dataset.confirmed === 'true') {
            form.dataset.confirmed = '';
            return true;
        }

        showConfirm(title, message, type || 'warning').then(ok => {
            if (ok) {
                form.dataset.confirmed = 'true';
                form.submit();
            }
        });

        return false;
    };
})();
