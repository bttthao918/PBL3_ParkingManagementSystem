document.addEventListener("DOMContentLoaded", function () {
    const ticketContent = document.getElementById("ticketContent");
    const detailPanel = document.getElementById("ticketDetailPanel");
    const rows = Array.from(document.querySelectorAll(".ticket-row"));

    const closeDetailBtn = document.getElementById("closeDetailBtn");
    const bottomCloseDetailBtn = document.getElementById("bottomCloseDetailBtn");
    const applyFilterBtn = document.getElementById("applyTicketFilterBtn");
    const dateFilter = document.getElementById("ticketDateFilter");
    const vehicleFilter = document.getElementById("ticketVehicleFilter");
    const statusFilter = document.getElementById("ticketStatusFilter");
    const searchFilter = document.getElementById("ticketSearchFilter");
    const noResultsRow = document.getElementById("ticketNoResults");

    let selectedTicketId = null;

    rows.forEach(row => {
        row.addEventListener("click", function () {
            const currentTicketId = row.dataset.ticketId;

            if (selectedTicketId === currentTicketId) {
                hideDetail();
                return;
            }

            selectedTicketId = currentTicketId;

            rows.forEach(item => item.classList.remove("active"));
            row.classList.add("active");

            renderDetail(row);
            showDetail();
        });
    });

    closeDetailBtn.addEventListener("click", hideDetail);
    bottomCloseDetailBtn.addEventListener("click", hideDetail);

    applyFilterBtn.addEventListener("click", applyFilters);

    searchFilter.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            applyFilters();
        }
    });

    function applyFilters() {
        const selectedDate = dateFilter.value;
        const selectedVehicle = vehicleFilter.value;
        const selectedStatus = statusFilter.value;
        const keyword = normalize(searchFilter.value);
        let visibleCount = 0;

        rows.forEach(row => {
            const matchesDate = !selectedDate || row.dataset.filterDate === selectedDate;
            const matchesVehicle = !selectedVehicle || row.dataset.type === selectedVehicle;
            const matchesStatus = !selectedStatus || row.dataset.status === selectedStatus;
            const matchesKeyword = !keyword
                || normalize(row.dataset.code).includes(keyword)
                || normalize(row.dataset.plate).includes(keyword);
            const isVisible = matchesDate && matchesVehicle && matchesStatus && matchesKeyword;

            row.classList.toggle("hidden", !isVisible);
            if (isVisible) visibleCount += 1;
        });

        noResultsRow.classList.toggle("hidden", visibleCount > 0);

        if (selectedTicketId) {
            const selectedRow = rows.find(row => row.dataset.ticketId === selectedTicketId);
            if (!selectedRow || selectedRow.classList.contains("hidden")) {
                hideDetail();
            }
        }
    }

    function showDetail() {
        detailPanel.classList.remove("hidden");
        ticketContent.classList.remove("no-detail");
        ticketContent.classList.add("has-detail");
    }

    function hideDetail() {
        selectedTicketId = null;

        rows.forEach(row => row.classList.remove("active"));

        detailPanel.classList.add("hidden");
        ticketContent.classList.remove("has-detail");
        ticketContent.classList.add("no-detail");
    }

    function renderDetail(row) {
        setText("detailCode", row.dataset.code);
        setText("detailStatus", row.dataset.status);
        setText("detailVehicleType", row.dataset.type);
        setText("detailPlate", row.dataset.plate);
        setText("detailCheckIn", row.dataset.checkin);
        setText("detailCheckOut", row.dataset.checkout || "-");
        setText("detailDuration", row.dataset.duration);
        setText("detailParkingFee", row.dataset.parkingFee);
        setText("detailDiscount", row.dataset.discount);
        setText("detailTotal", row.dataset.total);
        setText("detailPaymentMethod", row.dataset.paymentMethod);
        setText("detailCreatedBy", row.dataset.createdBy);
        setText("detailNote", row.dataset.note || "-");

        const iconBox = document.getElementById("detailVehicleIcon");
        iconBox.className = `detail-vehicle-icon ${row.dataset.iconClass}`;
        iconBox.innerHTML = `<i class="${row.dataset.icon}"></i>`;
    }

    function setText(id, value) {
        const element = document.getElementById(id);

        if (element) {
            element.textContent = value || "-";
        }
    }

    function normalize(value) {
        return (value || "").trim().toLowerCase();
    }
});
