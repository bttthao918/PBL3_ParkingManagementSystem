document.addEventListener("DOMContentLoaded", function () {
    const loginEmail = document.getElementById("loginEmail");
    const loginPassword = document.getElementById("loginPassword");

    if (loginEmail && loginPassword) {
        let previousEmail = loginEmail.value;

        // Xóa logic tự động xóa mật khẩu khi email thay đổi.
        // Điều này gây lỗi khi trình duyệt autofill điền email sau đó.
        /*
        loginEmail.addEventListener("input", function () {
            if (loginEmail.value !== previousEmail) {
                loginPassword.value = "";
                previousEmail = loginEmail.value;
            }
        });
        */
    }

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
