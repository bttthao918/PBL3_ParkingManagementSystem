document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".eye-icon").forEach(icon => {
        icon.addEventListener("click", function () {
            const targetId = icon.dataset.target;
            const input = document.getElementById(targetId);

            if (!input) {
                return;
            }

            if (input.type === "password") {
                input.type = "text";
                icon.classList.remove("fa-eye");
                icon.classList.add("fa-eye-slash");
            } else {
                input.type = "password";
                icon.classList.remove("fa-eye-slash");
                icon.classList.add("fa-eye");
            }
        });
    });
});
