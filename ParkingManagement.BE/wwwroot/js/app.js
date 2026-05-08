const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0
  }).format(Number(value || 0));

const setText = (id, value) => {
  const element = document.getElementById(id);
  if (element) element.textContent = value;
};

async function requestJson(url, options) {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...options
  });

  const contentType = response.headers.get("content-type") || "";
  const payload = contentType.includes("application/json")
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    const message = typeof payload === "string" ? payload : payload.message;
    throw new Error(message || `HTTP ${response.status}`);
  }

  return payload;
}

async function loadStats() {
  const apiState = document.getElementById("apiState");

  try {
    const stats = await requestJson("/admin/ticket/get-stats");
    setText("totalLaneTickets", stats.totalLaneTickets ?? 0);
    setText("totalMonthlyTickets", stats.totalMonthlyTickets ?? 0);
    setText("expiringSoon", stats.expiringSoon ?? 0);
    setText("todayRevenue", formatCurrency(stats.todayRevenue));
    apiState.textContent = "Sẵn sàng";
    apiState.className = "status ready";
  } catch {
    apiState.textContent = "Mất kết nối";
    apiState.className = "status error";
  }
}

async function loadSessions() {
  const body = document.getElementById("sessionsBody");
  body.innerHTML = `<tr><td colspan="4">Đang tải dữ liệu...</td></tr>`;

  try {
    const sessions = await requestJson("/admin/ticket/active-sessions");
    if (!Array.isArray(sessions) || sessions.length === 0) {
      body.innerHTML = `<tr><td colspan="4">Không có phiên đang gửi.</td></tr>`;
      return;
    }

    body.innerHTML = sessions.map((session) => `
      <tr>
        <td>${session.licensePlate ?? ""}</td>
        <td>${session.spotNumber ?? ""}</td>
        <td>${session.checkinTime ?? ""}</td>
        <td>${session.duration ?? ""}</td>
      </tr>
    `).join("");
  } catch (error) {
    body.innerHTML = `<tr><td colspan="4">${error.message}</td></tr>`;
  }
}

document.getElementById("refreshSessions")?.addEventListener("click", loadSessions);

document.getElementById("checkInForm")?.addEventListener("submit", async (event) => {
  event.preventDefault();

  const form = event.currentTarget;
  const resultBox = document.getElementById("checkInResult");
  const formData = new FormData(form);
  const payload = {
    licensePlate: String(formData.get("licensePlate") || "").trim().toUpperCase(),
    vehicleType: formData.get("vehicleType"),
    areaId: Number(formData.get("areaId"))
  };

  resultBox.textContent = "Đang xử lý...";

  try {
    const result = await requestJson("/api/old/parking/check-in", {
      method: "POST",
      body: JSON.stringify(payload)
    });
    resultBox.textContent = JSON.stringify(result, null, 2);
    await Promise.all([loadStats(), loadSessions()]);
    form.reset();
    document.getElementById("areaId").value = 1;
  } catch (error) {
    resultBox.textContent = error.message;
  }
});

loadStats();
loadSessions();
