const profileSelect = document.getElementById("profileSelect");

const customerName = document.getElementById("customerName");
const phone = document.getElementById("phone");
const plateNumber = document.getElementById("plateNumber");
const vehicleType = document.getElementById("vehicleType");

const startTime = document.getElementById("startTime");
const endTime = document.getElementById("endTime");

const estimatedPrice = document.getElementById("estimatedPrice");
const sumPrice = document.getElementById("sumPrice");

if (profileSelect) {
    profileSelect.addEventListener("change", function () {
        const selected = this.options[this.selectedIndex];

        customerName.value = selected.dataset.name || "";
        phone.value = selected.dataset.phone || "";
        plateNumber.value = selected.dataset.plate || "";
        vehicleType.value = selected.dataset.type || "";

        updateSummary();
    });
}

document.querySelectorAll(".slot").forEach(slot => {
    slot.addEventListener("click", function () {
        document.querySelectorAll(".slot").forEach(x => x.classList.remove("selected"));

        this.classList.add("selected");

        document.getElementById("selectedSlot").value = this.dataset.slot;
        document.getElementById("sumSlot").innerText = this.dataset.slot;
    });
});

[customerName, phone, plateNumber, vehicleType].forEach(input => {
    if (input) {
        input.addEventListener("input", updateSummary);
    }
});

[startTime, endTime].forEach(input => {
    if (input) {
        input.addEventListener("change", updatePrice);
    }
});

function updateSummary() {
    document.getElementById("sumName").innerText = customerName.value || "---";
    document.getElementById("sumPhone").innerText = phone.value || "---";
    document.getElementById("sumPlate").innerText = plateNumber.value || "---";
    document.getElementById("sumType").innerText = vehicleType.value || "---";
}

function updatePrice() {
    if (!startTime || !endTime || !startTime.value || !endTime.value) {
        return;
    }

    const start = new Date(startTime.value);
    const end = new Date(endTime.value);

    let hours = Math.ceil((end - start) / 1000 / 60 / 60);

    if (hours <= 0) {
        hours = 1;
    }

    const price = hours * 20000;
    const formatted = price.toLocaleString("vi-VN") + " đ";

    estimatedPrice.innerText = formatted;
    sumPrice.innerText = formatted;
}

updateSummary();
updatePrice();