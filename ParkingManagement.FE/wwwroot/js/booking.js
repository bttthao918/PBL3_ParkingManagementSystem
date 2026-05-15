document.addEventListener("DOMContentLoaded", function () {
    const bookingContent = document.getElementById("bookingContent");
    const detailPanel = document.getElementById("bookingDetailPanel");
    const bookingRows = document.querySelectorAll(".booking-row");

    const closeDetailBtn = document.getElementById("closeDetailBtn");
    const bottomCloseDetailBtn = document.getElementById("bottomCloseDetailBtn");
    const cancelBookingBtn = document.getElementById("cancelBookingBtn");

    let selectedBookingId = null;

    bookingRows.forEach(row => {
        row.addEventListener("click", function (event) {
            if (event.target.closest("button")) return;

            const currentId = row.dataset.id;

            if (selectedBookingId === currentId) {
                hideDetail();
                return;
            }

            selectedBookingId = currentId;

            bookingRows.forEach(x => x.classList.remove("active"));
            row.classList.add("active");

            renderDetail(row);
            showDetail();
        });
    });

    closeDetailBtn.addEventListener("click", hideDetail);
    bottomCloseDetailBtn.addEventListener("click", hideDetail);

    cancelBookingBtn.addEventListener("click", function () {
        if (confirm("Bạn có chắc muốn hủy đơn đặt chỗ này không?")) {
            alert("Đã hủy đơn đặt chỗ.");
            hideDetail();
        }
    });

    function showDetail() {
        detailPanel.classList.remove("hidden");
        bookingContent.classList.remove("no-detail");
        bookingContent.classList.add("has-detail");
    }

    function hideDetail() {
        selectedBookingId = null;
        bookingRows.forEach(x => x.classList.remove("active"));
        detailPanel.classList.add("hidden");
        bookingContent.classList.remove("has-detail");
        bookingContent.classList.add("no-detail");
    }

    function renderDetail(row) {
        setText("detailCode", row.dataset.code);
        setText("detailParking", row.dataset.parking);
        setText("detailPosition", row.dataset.position);
        setText("detailPlate", row.dataset.plate);
        setText("detailVehicle", row.dataset.vehicle);
        setText("detailBookingTime", row.dataset.bookingTime);
        setText("detailTimeRange", row.dataset.timeRange);
        setText("detailPrice", row.dataset.price);
        setText("detailCustomer", row.dataset.customer);
        setText("detailPhone", row.dataset.phone);

        const status = document.getElementById("detailStatus");
        status.textContent = row.dataset.status;
        status.className = `status-badge ${row.dataset.statusClass}`;

        const canCancel = row.dataset.canCancel === "true";
        cancelBookingBtn.style.display = canCancel ? "block" : "none";
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value || "-";
    }

    const wizard = document.getElementById("bookingWizardModal");
    const wizardBackdrop = document.querySelector(".booking-modal-backdrop");
    const openWizardBtn = document.getElementById("openBookingWizardBtn");
    const closeWizardBtn = document.getElementById("closeWizardBtn");

    if (!wizard || !openWizardBtn || !closeWizardBtn) return;

    openWizardBtn.addEventListener("click", function () {
        openWizard();
    });

    closeWizardBtn.addEventListener("click", closeWizard);

    wizardBackdrop.addEventListener("click", closeWizard);

    function closeWizard() {
        wizard.classList.add("hidden");
        document.body.classList.remove("modal-open");
    }

    function openWizard() {
        wizard.classList.remove("hidden");
        document.body.classList.add("modal-open");
        setStep("1");
    }

    if (new URLSearchParams(window.location.search).get("openCreate") === "1") {
        openWizard();
    }

    document.querySelectorAll(".next-step").forEach(btn => {
        btn.addEventListener("click", function () {
            setStep(btn.dataset.next);
        });
    });

    document.querySelectorAll(".prev-step").forEach(btn => {
        btn.addEventListener("click", function () {
            setStep(btn.dataset.prev);
        });
    });

    function setStep(step) {
        const stepNumber = Number(step);

        document.querySelectorAll(".wizard-panel").forEach(panel => {
            panel.classList.toggle("active", panel.dataset.step === step);
        });

        document.querySelectorAll(".wizard-step").forEach(item => {
            const indicatorNumber = Number(item.dataset.stepIndicator);

            item.classList.remove("active", "completed");

            if (indicatorNumber < stepNumber) {
                item.classList.add("completed");
            }

            if (indicatorNumber === stepNumber) {
                item.classList.add("active");
            }
        });
    }

    const savedVehicleSelect = document.getElementById("savedVehicleSelect");
    const showNewVehicleFormBtn = document.getElementById("showNewVehicleFormBtn");
    const newVehicleForm = document.getElementById("newVehicleForm");
    const saveNewVehicleBtn = document.getElementById("saveNewVehicleBtn");
    const cancelNewVehicleBtn = document.getElementById("cancelNewVehicleBtn");
    const newVehicleError = document.getElementById("newVehicleError");
    const newVehiclePlate = document.getElementById("newVehiclePlate");
    const newVehicleType = document.getElementById("newVehicleType");
    const newCustomerName = document.getElementById("newCustomerName");
    const newCustomerPhone = document.getElementById("newCustomerPhone");

    savedVehicleSelect.addEventListener("change", function () {
        applySelectedVehicle();
    });

    showNewVehicleFormBtn.addEventListener("click", function () {
        newVehicleForm.classList.remove("hidden");
        newVehiclePlate.focus();
    });

    cancelNewVehicleBtn.addEventListener("click", function () {
        clearNewVehicleForm();
        newVehicleForm.classList.add("hidden");
    });

    saveNewVehicleBtn.addEventListener("click", function () {
        const plate = newVehiclePlate.value.trim();
        const vehicle = newVehicleType.value;
        const customer = newCustomerName.value.trim();
        const phone = newCustomerPhone.value.trim();

        if (!plate || !customer || !phone) {
            showNewVehicleError("Vui lòng nhập đầy đủ biển số, khách hàng và số điện thoại.");
            return;
        }

        if (!/^\d{2}[A-Za-z0-9]{1,3}-\d{3}\.\d{2}$/.test(plate)) {
            showNewVehicleError("Biển số cần đúng định dạng, ví dụ 59C1-123.45.");
            return;
        }

        const value = `${plate}|${vehicle}|${customer}|${phone}`;
        const existingOption = Array.from(savedVehicleSelect.options)
            .find(option => option.value.toLowerCase() === value.toLowerCase());

        if (existingOption) {
            savedVehicleSelect.value = existingOption.value;
        } else {
            const option = new Option(`${plate} - ${vehicle}`, value, true, true);
            savedVehicleSelect.add(option);
        }

        applySelectedVehicle();
        clearNewVehicleForm();
        newVehicleForm.classList.add("hidden");
    });

    function applySelectedVehicle() {
        if (!savedVehicleSelect.value) return;
        const [plate, vehicle, customer, phone] = savedVehicleSelect.value.split("|");

        setText("previewCustomer", customer);
        setText("previewPhone", phone);
        setText("confirmPlate", plate);
        setText("confirmVehicle", vehicle);
    }

    function showNewVehicleError(message) {
        newVehicleError.textContent = message;
        newVehicleError.classList.remove("hidden");
    }

    function clearNewVehicleForm() {
        newVehiclePlate.value = "";
        newVehicleType.value = "Xe máy";
        newCustomerName.value = "";
        newCustomerPhone.value = "";
        newVehicleError.textContent = "";
        newVehicleError.classList.add("hidden");
    }

    const slotButtons = document.querySelectorAll(".slot");
    const selectedSlotText = document.getElementById("selectedSlotText");
    const confirmSlot = document.getElementById("confirmSlot");
    const randomSlotBtn = document.getElementById("randomSlotBtn");

    slotButtons.forEach(slot => {
        slot.addEventListener("click", function () {
            if (slot.dataset.selectable !== "true") return;
            selectSlot(slot);
        });
    });

    randomSlotBtn.addEventListener("click", function () {
        const availableSlots = Array.from(slotButtons)
            .filter(slot => slot.dataset.selectable === "true");

        if (availableSlots.length === 0) {
            alert("Hiện không còn chỗ trống.");
            return;
        }

        const randomIndex = Math.floor(Math.random() * availableSlots.length);
        selectSlot(availableSlots[randomIndex]);
    });

    function selectSlot(slot) {
        slotButtons.forEach(x => x.classList.remove("selected"));
        slot.classList.add("selected");

        const text = `${slot.dataset.slotCode} - ${slot.dataset.slotPosition}`;

        selectedSlotText.textContent = text;
        confirmSlot.textContent = text;
        
        // Also update hidden form field
        const formSlotId = document.getElementById("formSlotId");
        if (formSlotId) formSlotId.value = slot.dataset.slotCode;
    }

    const confirmPaymentBtn = document.getElementById("confirmPaymentBtn");

    // Add listener to next step to update time
    document.querySelectorAll(".next-step").forEach(btn => {
        btn.addEventListener("click", function () {
            const nextStep = btn.dataset.next;
            if (nextStep === "3") {
                const now = new Date();
                const expected = new Date(now.getTime() + 60 * 60 * 1000); // +1 hour
                const timeStr = `${expected.getDate().toString().padStart(2, '0')}/${(expected.getMonth()+1).toString().padStart(2, '0')}/${expected.getFullYear()} ${expected.getHours().toString().padStart(2, '0')}:${expected.getMinutes().toString().padStart(2, '0')}`;
                
                const confirmTimeEl = document.getElementById("confirmTime");
                if (confirmTimeEl) confirmTimeEl.textContent = `Dự kiến: ${timeStr}`;
                
                const formExpectedTime = document.getElementById("formExpectedTime");
                if (formExpectedTime) formExpectedTime.value = expected.toISOString();
            }
        });
    });

    confirmPaymentBtn.addEventListener("click", function () {
        // Populating hidden form fields before submit
        const formVehiclePlate = document.getElementById("formVehiclePlate");
        const formVehicleType = document.getElementById("formVehicleType");
        
        const plateEl = document.getElementById("confirmPlate");
        const vehicleEl = document.getElementById("confirmVehicle");
        
        if (formVehiclePlate && plateEl) formVehiclePlate.value = plateEl.textContent;
        if (formVehicleType && vehicleEl) formVehicleType.value = vehicleEl.textContent;
        
        // Let the form submit normally
    });
});
