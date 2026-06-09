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

    function configureModal(title, message, type) {
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

        okBtn.className = 'confirm-btn-ok ' + (type || 'info');
    }

    function openModal() {
        modal.classList.add('open');

        return new Promise(resolve => {
            resolveCallback = resolve;
        });
    }

    function showConfirm(title, message, type) {
        configureModal(title, message, type);
        modal.classList.remove('notice');
        cancelBtn.hidden = false;
        okBtn.textContent = type === 'danger' ? 'Xác nhận' : 'Đồng ý';

        return openModal();
    }

    function showNotice(title, message, type) {
        configureModal(title || 'Thông báo', message, type);
        modal.classList.add('notice');
        cancelBtn.hidden = true;
        okBtn.textContent = 'Đã hiểu';

        return openModal();
    }

    okBtn.addEventListener('click', () => {
        modal.classList.remove('open');
        runCallback(true);
    });

    cancelBtn.addEventListener('click', () => {
        modal.classList.remove('open');
        runCallback(false);
    });

    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('open');
            runCallback(false);
        }
    });

    function runCallback(ok) {
        const callback = resolveCallback;
        resolveCallback = null;
        if (callback) callback(ok);
    }

    function submitConfirmedForm(form) {
        form.dataset.confirmed = 'true';
        if (typeof form.requestSubmit === 'function') {
            form.requestSubmit();
            window.setTimeout(() => {
                if (form.dataset.confirmed === 'true') form.dataset.confirmed = '';
            }, 0);
            return;
        }

        form.submit();
    }

    function getOptions(element) {
        return {
            title: element.dataset.confirmTitle || 'Xác nhận',
            message: element.dataset.confirmMessage || 'Bạn có chắc muốn thực hiện hành động này?',
            type: element.dataset.confirmType || 'warning'
        };
    }

    // Global function
    window.confirmAction = function (title, message, callback, type) {
        showConfirm(title, message, type || 'info').then(ok => {
            if (ok && callback) callback();
        });
    };

    window.showNotice = function (title, message, type, callback) {
        showNotice(title, message, type || 'info').then(() => {
            if (callback) callback();
        });
    };

    window.confirmNavigation = function (event, link, title, message, type) {
        event.preventDefault();
        showConfirm(title, message, type || 'warning').then(ok => {
            if (ok) window.location.href = link.href;
        });
        return false;
    };

    // For forms — gắn vào onsubmit
    window.confirmSubmit = function (form, title, message, type) {
        if (form.dataset.confirmed === 'true') {
            form.dataset.confirmed = '';
            return true;
        }

        showConfirm(title, message, type || 'warning').then(ok => {
            if (ok) {
                submitConfirmedForm(form);
            }
        });

        return false;
    };

    document.addEventListener('click', (event) => {
        const link = event.target.closest('a[data-confirm-title], a[data-confirm-message]');
        if (!link || link.dataset.confirmManual === 'true') return;

        event.preventDefault();
        const options = getOptions(link);
        showConfirm(options.title, options.message, options.type).then(ok => {
            if (ok) window.location.href = link.href;
        });
    });

    document.addEventListener('submit', (event) => {
        const form = event.target;
        if (!form.matches('form[data-confirm-title], form[data-confirm-message]')) return;

        if (form.dataset.confirmed === 'true') {
            form.dataset.confirmed = '';
            return;
        }

        event.preventDefault();
        const options = getOptions(form);
        showConfirm(options.title, options.message, options.type).then(ok => {
            if (ok) submitConfirmedForm(form);
        });
    });
})();
