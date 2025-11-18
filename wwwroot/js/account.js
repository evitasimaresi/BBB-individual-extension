// account.js

document.addEventListener("DOMContentLoaded", function () {

    const editToggleBtn = document.getElementById("editToggleBtn");
    const cancelEditBtn = document.getElementById("cancelEditBtn");
    const editFormSection = document.getElementById("editFormSection");

    function toggleForm(show) {
        if (!editFormSection) return;
        editFormSection.style.display = show ? "block" : "none";
    }

    if (editToggleBtn) {
        editToggleBtn.addEventListener("click", () => toggleForm(true));
    }

    if (cancelEditBtn) {
        cancelEditBtn.addEventListener("click", () => toggleForm(false));
    }

    document.querySelectorAll(".eye-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            const target = document.querySelector(btn.dataset.target);
            if (!target) return;
            target.type = target.type === "password" ? "text" : "password";
        });
    });

    const hasErrorAttr = editFormSection?.dataset?.hasError; // "True" / "False" / undefined
    const hasError = hasErrorAttr === "True" || hasErrorAttr === "true";

    if (hasError) {
        toggleForm(true);
    }

    // --- LIVE PASSWORD RULE CHECK ---
    const newPassInput = document.getElementById("NewPassword");
    const ruleMsg = document.getElementById("passwordRuleMsg");

    function validatePasswordRules() {
        const pass = newPassInput.value;

        const isLong = pass.length >= 8;
        const hasNumber = /\d/.test(pass);

        if (pass === "") {
            ruleMsg.textContent = "";
            return;
        }

        if (isLong && hasNumber) {
            ruleMsg.style.color = "#10b981"; // grean fn
            ruleMsg.textContent = "✔ Password meets the requirements.";
        } else {
            ruleMsg.style.color = "#ef4444"; // red bruh
            ruleMsg.textContent =
                "✘ Password must be at least 8 characters long and contain at least one number.";
        }
    }

    newPassInput.addEventListener("input", validatePasswordRules);

});
