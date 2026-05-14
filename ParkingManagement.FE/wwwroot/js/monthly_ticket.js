const apiBase = "/api/monthly-ticket";
let cancelTicketId = null;

document.addEventListener("DOMContentLoaded", async () => {
    await loadPlans();
    await loadCurrentTicket();
    await loadHistory();
});

function formatMoney(value) {
    return new Intl.NumberFormat("vi-VN").format(value || 0) + " đ";
}

async function getJson(url) {
    const res = await fetch(url);
    if (!res.ok) return null;
    return await res.json();
}

async function loadPlans() {
    const plans = await getJson(`${apiBase}/plans`);
    const planList = document.getElementById("planList");

    if (!plans || plans.length === 0) {
        planList.innerHTML = `<div class="empty">Chưa có gói vé tháng.</div>`;
        return;
    }

    planList.innerHTML = plans.map((plan, index) => {
        const isMotorbike = plan.vehicleType === "MOTORBIKE";
        const icon = isMotorbike ? "🛵" : "🚗";

        return `
            <article class="plan-card ${index === 0 ? "selected" : ""}">
                <div class="plan-head">
                    <span class="radio ${index === 0 ? "checked" : ""}"></span>

                    <div class="plan-icon">${icon}</div>

                    <div>
                        <h4>${plan.name}</h4>
                        <div class="price">
                            ${formatMoney(plan.price)}
                            <span>/ tháng</span>
                        </div>
                    </div>
                </div>

                <ul class="features">
                    <li>Gửi xe không giới hạn trong 1 tháng</li>
                    <li>Áp dụng tại mọi bãi giữ xe</li>
                    <li>Ra vào tự do</li>
                </ul>

                <button class="btn" onclick="registerTicket(${plan.id})">
                    Đăng ký ngay
                </button>
            </article>
        `;
    }).join("");
}

async function loadCurrentTicket() {
    const ticket = await getJson(`${apiBase}/current`);
    const container = document.getElementById("currentTicket");

    if (!ticket) {
        container.innerHTML = `
            <div class="empty current-empty">
                Bạn chưa có vé tháng đang hoạt động.
            </div>
        `;
        return;
    }

    container.innerHTML = `
        <article class="ticket-box">
            <div class="ticket-status-bar">
                ● Đang hoạt động
            </div>

            <div class="ticket-body">
                <div class="ticket-title">
                    <div class="plan-icon">🛵</div>

                    <div>
                        <h4>${ticket.planName}</h4>
                        <small>Mã vé: ${ticket.code}</small>
                    </div>
                </div>

                <div class="ticket-row">
                    <span>Ngày bắt đầu</span>
                    <strong>${ticket.startDate}</strong>
                </div>

                <div class="ticket-row">
                    <span>Ngày hết hạn</span>
                    <strong>${ticket.endDate}</strong>
                </div>

                <div class="ticket-row">
                    <span>Còn lại</span>
                    <strong class="days-left">${ticket.remainingDays} ngày</strong>
                </div>

                <div class="ticket-row">
                    <span>Giá trị</span>
                    <strong>${formatMoney(ticket.price)}</strong>
                </div>

                <div class="ticket-actions">
                    <button class="btn primary" onclick="renewTicket(${ticket.id})">
                        🔄 Gia hạn vé
                    </button>

                    <button class="btn danger" onclick="openCancelModal(${ticket.id})">
                        ✕ Hủy vé
                    </button>
                </div>
            </div>
        </article>
    `;
}

async function loadHistory() {
    const histories = await getJson(`${apiBase}/history`);
    const tbody = document.getElementById("historyBody");

    if (!histories || histories.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="empty-table">
                    Chưa có lịch sử vé tháng.
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = histories.map(item => `
        <tr>
            <td>${item.code}</td>
            <td>${item.planName}</td>
            <td>${item.startDate} - ${item.endDate}</td>

            <td class="${item.remainingDays > 0 ? "green" : "red"}">
                ${item.remainingDays > 0 ? item.remainingDays + " ngày" : "0 ngày"}
            </td>

            <td>${formatMoney(item.price)}</td>

            <td>
                <span class="status ${item.status === "ACTIVE" ? "active" : "expired"}">
                    ${item.status === "ACTIVE" ? "Đang hoạt động" : "Đã hết hạn"}
                </span>
            </td>

            <td>
                <div class="table-actions">
                    <button 
                        type="button"
                        class="action-btn"
                        aria-label="Gia hạn vé"
                        onclick="renewTicket(${item.id})"
                    >
                        🔄
                    </button>

                    ${item.status === "ACTIVE" ? `
                        <button 
                            type="button"
                            class="action-btn danger"
                            aria-label="Hủy vé"
                            onclick="openCancelModal(${item.id})"
                        >
                            ✕
                        </button>
                    ` : ""}
                </div>
            </td>
        </tr>
    `).join("");
}

async function registerTicket(planId) {
    await fetch(`${apiBase}/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ planId })
    });

    await loadCurrentTicket();
    await loadHistory();
}

async function renewTicket(ticketId) {
    await fetch(`${apiBase}/renew/${ticketId}`, {
        method: "POST"
    });

    await loadCurrentTicket();
    await loadHistory();
}

function openCancelModal(ticketId) {
    cancelTicketId = ticketId;
    document.getElementById("confirmModal").classList.remove("hidden");
}

function closeCancelModal() {
    cancelTicketId = null;
    document.getElementById("confirmModal").classList.add("hidden");
}

async function confirmCancelTicket() {
    if (!cancelTicketId) return;

    await fetch(`${apiBase}/cancel/${cancelTicketId}`, {
        method: "DELETE"
    });

    closeCancelModal();
    await loadCurrentTicket();
    await loadHistory();
}