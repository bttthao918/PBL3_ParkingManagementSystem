document.addEventListener("DOMContentLoaded", function () {
    // ═══════════════════════════════════════════════════════
    // DETAIL PANEL (xem chi tiết đơn đặt chỗ)
    // ═══════════════════════════════════════════════════════
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

    if (closeDetailBtn) closeDetailBtn.addEventListener("click", hideDetail);
    if (bottomCloseDetailBtn) bottomCloseDetailBtn.addEventListener("click", hideDetail);

    // Cancel booking with confirm dialog
    if (cancelBookingBtn) {
        cancelBookingBtn.addEventListener("click", function () {
            const reservationId = cancelBookingBtn.dataset.reservationId;
            if (!reservationId) return;

            showConfirmDialog({
                title: "Hủy đơn đặt chỗ",
                message: "Bạn có chắc chắn muốn hủy đơn đặt chỗ này? Thao tác không thể hoàn tác.",
                icon: "warning",
                confirmText: "Hủy đơn",
                onConfirm: function () {
                    const form = document.getElementById("cancelBookingForm");
                    const input = document.getElementById("cancelReservationId");
                    if (form && input) {
                        input.value = reservationId;
                        form.submit();
                    }
                }
            });
        });
    }

    function showDetail() {
        if (!detailPanel || !bookingContent) return;
        detailPanel.classList.remove("hidden");
        bookingContent.classList.remove("no-detail");
        bookingContent.classList.add("has-detail");
    }

    function hideDetail() {
        selectedBookingId = null;
        bookingRows.forEach(x => x.classList.remove("active"));
        if (detailPanel) detailPanel.classList.add("hidden");
        if (bookingContent) {
            bookingContent.classList.remove("has-detail");
            bookingContent.classList.add("no-detail");
        }
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
        if (status) {
            status.textContent = row.dataset.status;
            status.className = "status-badge " + row.dataset.statusClass;
        }

        const canCancel = row.dataset.canCancel === "true";
        if (cancelBookingBtn) {
            cancelBookingBtn.style.display = canCancel ? "inline-flex" : "none";
            cancelBookingBtn.dataset.reservationId = row.dataset.reservationId || "";
        }
    }

    // ═══════════════════════════════════════════════════════
    // WIZARD (đặt chỗ mới - 3 bước)
    // ═══════════════════════════════════════════════════════
    const wizard = document.getElementById("bookingWizardModal");
    const wizardBackdrop = wizard ? wizard.querySelector(".booking-modal-backdrop") : null;
    const openWizardBtn = document.getElementById("openBookingWizardBtn");
    const closeWizardBtn = document.getElementById("closeWizardBtn");

    if (!wizard || !openWizardBtn) return;

    openWizardBtn.addEventListener("click", openWizard);
    if (closeWizardBtn) closeWizardBtn.addEventListener("click", closeWizard);
    if (wizardBackdrop) wizardBackdrop.addEventListener("click", closeWizard);

    function openWizard() {
        wizard.classList.remove("hidden");
        document.body.classList.add("modal-open");
        setStep("1");
        resetWizardState();
    }

    function closeWizard() {
        wizard.classList.add("hidden");
        document.body.classList.remove("modal-open");
    }

    // Set default expected time to now + 15 min
    const expectedTimeInput = document.getElementById("expectedTimeInput");
    if (expectedTimeInput) {
        const now = new Date();
        now.setMinutes(now.getMinutes() + 15);
        const localISO = now.toISOString().slice(0, 16);
        expectedTimeInput.value = localISO;
        expectedTimeInput.min = new Date().toISOString().slice(0, 16);
    }

    // Step navigation
    document.querySelectorAll(".prev-step").forEach(btn => {
        btn.addEventListener("click", () => setStep(btn.dataset.prev));
    });

    // Step 1 → Step 2 validation
    const step1NextBtn = document.getElementById("step1NextBtn");
    if (step1NextBtn) {
        step1NextBtn.addEventListener("click", async function () {
            const error = document.getElementById("step1Error");
            error.classList.add("hidden");

            // Validate vehicle info
            const vehicleInfo = getSelectedVehicle();
            if (!vehicleInfo.plate) {
                showStepError("step1Error", "Vui lòng chọn hoặc nhập thông tin xe.");
                return;
            }

            // Validate expected time
            const timeVal = expectedTimeInput ? expectedTimeInput.value : "";
            if (!timeVal) {
                showStepError("step1Error", "Vui lòng chọn thời gian dự kiến đến.");
                return;
            }

            const expectedDate = new Date(timeVal);
            if (expectedDate <= new Date()) {
                showStepError("step1Error", "Thời gian dự kiến phải ở tương lai.");
                return;
            }

            // Move to step 2 and load slots for the selected vehicle type
            setStep("2");
            await loadSlotsForVehicleType(vehicleInfo.type);
        });
    }

    // Step 2 → Step 3 validation
    const step2NextBtn = document.getElementById("step2NextBtn");
    if (step2NextBtn) {
        step2NextBtn.addEventListener("click", function () {
            const error = document.getElementById("step2Error");
            error.classList.add("hidden");

            // If there are available slots displayed, user must pick one
            const availableSlots = Array.from(currentSlotButtons).filter(s => s.dataset.selectable === "true");
            if (availableSlots.length > 0 && !selectedSlotCode) {
                showStepError("step2Error", "Vui lòng chọn một chỗ đỗ hoặc nhấn 'Chọn ngẫu nhiên'.");
                return;
            }

            // If no slots available at all, allow proceeding (system will assign)
            populateConfirmation();
            setStep("3");
        });
    }

    function setStep(step) {
        const stepNumber = Number(step);

        document.querySelectorAll(".wizard-panel").forEach(panel => {
            panel.classList.toggle("active", panel.dataset.step === step);
        });

        document.querySelectorAll(".wizard-step").forEach(item => {
            const n = Number(item.dataset.stepIndicator);
            item.classList.remove("active", "completed");
            if (n < stepNumber) item.classList.add("completed");
            if (n === stepNumber) item.classList.add("active");
        });
    }

    // ── Vehicle selection ──
    const savedVehicleSelect = document.getElementById("savedVehicleSelect");
    const showNewVehicleFormBtn = document.getElementById("showNewVehicleFormBtn");
    const newVehicleForm = document.getElementById("newVehicleForm");
    const saveNewVehicleBtn = document.getElementById("saveNewVehicleBtn");
    const cancelNewVehicleBtn = document.getElementById("cancelNewVehicleBtn");
    const newVehiclePlate = document.getElementById("newVehiclePlate");
    const newVehicleType = document.getElementById("newVehicleType");

    if (showNewVehicleFormBtn) {
        showNewVehicleFormBtn.addEventListener("click", () => {
            newVehicleForm.classList.remove("hidden");
            if (newVehiclePlate) newVehiclePlate.focus();
        });
    }

    if (cancelNewVehicleBtn) {
        cancelNewVehicleBtn.addEventListener("click", () => {
            newVehicleForm.classList.add("hidden");
            clearNewVehicleForm();
        });
    }

    if (saveNewVehicleBtn) {
        saveNewVehicleBtn.addEventListener("click", function () {
            const plate = newVehiclePlate.value.trim();
            const vehicle = newVehicleType.value;
            const errorEl = document.getElementById("newVehicleError");

            if (!plate) {
                errorEl.textContent = "Vui lòng nhập biển số xe.";
                errorEl.classList.remove("hidden");
                return;
            }

            // Add to select
            const value = plate + "|" + vehicle;
            const option = new Option(plate + " - " + vehicle, value, true, true);
            savedVehicleSelect.add(option);
            savedVehicleSelect.value = value;

            newVehicleForm.classList.add("hidden");
            clearNewVehicleForm();
        });
    }

    function getSelectedVehicle() {
        const val = savedVehicleSelect ? savedVehicleSelect.value : "";
        if (!val) return { plate: "", type: "" };
        const parts = val.split("|");
        return { plate: parts[0] || "", type: parts[1] || "Xe máy" };
    }

    function clearNewVehicleForm() {
        if (newVehiclePlate) newVehiclePlate.value = "";
        if (newVehicleType) newVehicleType.value = "Xe máy";
        const errorEl = document.getElementById("newVehicleError");
        if (errorEl) errorEl.classList.add("hidden");
    }

    // ── Slot selection ──
    let selectedSlotCode = "";
    let selectedSlotPosition = "";
    let currentSlotButtons = [];
    const selectedSlotText = document.getElementById("selectedSlotText");
    const randomSlotBtn = document.getElementById("randomSlotBtn");
    const parkingMap = document.getElementById("parkingMap");

    async function loadSlotsForVehicleType(vehicleType) {
        selectedSlotCode = "";
        selectedSlotPosition = "";
        if (selectedSlotText) selectedSlotText.textContent = "Chưa chọn";

        // Show loading
        if (parkingMap) {
            parkingMap.innerHTML = '<p class="no-slots-msg"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải chỗ đỗ cho ' + vehicleType + '...</p>';
        }

        const zoneCapacity = document.getElementById("zoneCapacity");
        const zoneTitle = document.getElementById("zoneTitle");
        if (zoneTitle) zoneTitle.textContent = "Chỗ đỗ cho " + vehicleType;
        if (zoneCapacity) zoneCapacity.textContent = "Đang tải...";

        try {
            const url = "?handler=Slots&vehicleType=" + encodeURIComponent(vehicleType);
            const response = await fetch(url, {
                headers: { "RequestVerificationToken": getAntiForgeryToken() }
            });

            if (!response.ok) {
                throw new Error("API returned " + response.status);
            }

            const slots = await response.json();

            if (zoneCapacity) zoneCapacity.textContent = slots.length + " chỗ trống";

            if (slots.length === 0) {
                parkingMap.innerHTML = '<p class="no-slots-msg"><i class="fa-solid fa-circle-info"></i> Hiện không có chỗ đỗ trống cho ' + vehicleType + '. Bạn vẫn có thể đặt chỗ và hệ thống sẽ tự phân bổ.</p>';
                currentSlotButtons = [];
                return;
            }

            // Render slot buttons
            parkingMap.innerHTML = "";
            slots.forEach(function (slot) {
                const btn = document.createElement("button");
                btn.type = "button";
                btn.className = "slot empty";
                btn.dataset.selectable = "true";
                btn.dataset.slotCode = slot.slotId;
                btn.dataset.slotPosition = slot.location;
                btn.dataset.slotVehicleType = slot.vehicleType;
                btn.textContent = slot.slotId;

                btn.addEventListener("click", function () {
                    selectSlot(btn);
                });

                parkingMap.appendChild(btn);
            });

            currentSlotButtons = parkingMap.querySelectorAll(".slot");

        } catch (err) {
            console.error("Failed to load slots:", err);
            if (parkingMap) {
                parkingMap.innerHTML = '<p class="no-slots-msg"><i class="fa-solid fa-circle-exclamation"></i> Không tải được chỗ đỗ. Bạn vẫn có thể đặt chỗ và hệ thống sẽ tự phân bổ.</p>';
            }
            if (zoneCapacity) zoneCapacity.textContent = "Lỗi kết nối";
            currentSlotButtons = [];
        }
    }

    if (randomSlotBtn) {
        randomSlotBtn.addEventListener("click", function () {
            const available = Array.from(currentSlotButtons).filter(s => s.dataset.selectable === "true");
            if (available.length === 0) {
                showStepError("step2Error", "Hiện không có chỗ đỗ trống nào.");
                return;
            }
            const randomIndex = Math.floor(Math.random() * available.length);
            selectSlot(available[randomIndex]);
        });
    }

    function selectSlot(slot) {
        // Remove selected from all
        if (currentSlotButtons.length > 0) {
            currentSlotButtons.forEach(x => x.classList.remove("selected"));
        }
        slot.classList.add("selected");

        selectedSlotCode = slot.dataset.slotCode;
        selectedSlotPosition = slot.dataset.slotPosition || slot.dataset.slotCode;

        const text = selectedSlotCode + (selectedSlotPosition ? " - " + selectedSlotPosition : "");
        if (selectedSlotText) selectedSlotText.textContent = text;

        // Clear error
        const error = document.getElementById("step2Error");
        if (error) error.classList.add("hidden");
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : "";
    }

    // ── Confirmation ──
    function populateConfirmation() {
        const vehicle = getSelectedVehicle();
        const timeVal = expectedTimeInput ? expectedTimeInput.value : "";
        const timeDisplay = timeVal ? formatDateTime(new Date(timeVal)) : "-";

        setText("confirmPlate", vehicle.plate);
        setText("confirmVehicle", vehicle.type);
        setText("confirmSlot", selectedSlotCode
            ? selectedSlotCode + (selectedSlotPosition ? " - " + selectedSlotPosition : "")
            : "Hệ thống tự phân bổ");
        setText("confirmTime", timeDisplay);

        // Fill hidden form fields
        const formPlate = document.getElementById("formVehiclePlate");
        const formType = document.getElementById("formVehicleType");
        const formSlot = document.getElementById("formSlotId");
        const formTime = document.getElementById("formExpectedTime");

        if (formPlate) formPlate.value = vehicle.plate;
        if (formType) formType.value = vehicle.type;
        if (formSlot) formSlot.value = selectedSlotCode || "";
        if (formTime) formTime.value = timeVal ? new Date(timeVal).toISOString() : "";
    }

    function resetWizardState() {
        selectedSlotCode = "";
        selectedSlotPosition = "";
        currentSlotButtons = [];
        if (selectedSlotText) selectedSlotText.textContent = "Chưa chọn";
        if (parkingMap) {
            parkingMap.innerHTML = '<p class="no-slots-msg"><i class="fa-solid fa-circle-info"></i> Chọn thông tin xe ở bước 1 để xem chỗ đỗ phù hợp.</p>';
        }

        // Reset time to +15 min
        if (expectedTimeInput) {
            const now = new Date();
            now.setMinutes(now.getMinutes() + 15);
            expectedTimeInput.value = now.toISOString().slice(0, 16);
        }
    }

    // ═══════════════════════════════════════════════════════
    // CONFIRM DIALOG (modal xác nhận đẹp)
    // ═══════════════════════════════════════════════════════
    function showConfirmDialog(options) {
        const overlay = document.getElementById("confirmDialog");
        const title = document.getElementById("confirmDialogTitle");
        const message = document.getElementById("confirmDialogMessage");
        const icon = document.getElementById("confirmDialogIcon");
        const okBtn = document.getElementById("confirmDialogOk");
        const cancelBtn = document.getElementById("confirmDialogCancel");

        if (!overlay) return;

        title.textContent = options.title || "Xác nhận";
        message.textContent = options.message || "";
        okBtn.textContent = options.confirmText || "Xác nhận";

        // Icon style
        icon.className = "confirm-dialog-icon";
        if (options.icon === "warning") {
            icon.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i>';
            icon.classList.add("warning");
            okBtn.className = "danger-btn";
        } else if (options.icon === "success") {
            icon.innerHTML = '<i class="fa-solid fa-circle-check"></i>';
            icon.classList.add("success");
            okBtn.className = "primary-btn";
        } else {
            icon.innerHTML = '<i class="fa-solid fa-circle-question"></i>';
            icon.classList.add("info");
            okBtn.className = "primary-btn";
        }

        overlay.classList.remove("hidden");

        // Cleanup old listeners
        const newOk = okBtn.cloneNode(true);
        okBtn.parentNode.replaceChild(newOk, okBtn);
        const newCancel = cancelBtn.cloneNode(true);
        cancelBtn.parentNode.replaceChild(newCancel, cancelBtn);

        newOk.addEventListener("click", function () {
            overlay.classList.add("hidden");
            if (options.onConfirm) options.onConfirm();
        });

        newCancel.addEventListener("click", function () {
            overlay.classList.add("hidden");
            if (options.onCancel) options.onCancel();
        });

        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) overlay.classList.add("hidden");
        });
    }

    // ═══════════════════════════════════════════════════════
    // UTILITIES
    // ═══════════════════════════════════════════════════════
    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value || "-";
    }

    function showStepError(id, message) {
        const el = document.getElementById(id);
        if (el) {
            el.textContent = message;
            el.classList.remove("hidden");
        }
    }

    function formatDateTime(date) {
        const d = date.getDate().toString().padStart(2, "0");
        const m = (date.getMonth() + 1).toString().padStart(2, "0");
        const y = date.getFullYear();
        const h = date.getHours().toString().padStart(2, "0");
        const min = date.getMinutes().toString().padStart(2, "0");
        return d + "/" + m + "/" + y + " " + h + ":" + min;
    }

    // Auto-open wizard if URL has ?openCreate=1
    if (new URLSearchParams(window.location.search).get("openCreate") === "1") {
        openWizard();
    }
});
