import { updateUserProfile } from './services.js';

document.addEventListener("DOMContentLoaded", function () {

    const editToggleBtn = document.getElementById("editToggleBtn");
    const cancelEditBtn = document.getElementById("cancelEditBtn");
    const editForm = document.getElementById("editForm");
    const editFormSection = document.getElementById("editFormSection");
    const messageAlert = document.getElementById("messageAlert");
    const errorAlert = document.getElementById("errorAlert");

    editToggleBtn.addEventListener('click', () => {
        editFormSection.style.display = "block";
    });

    cancelEditBtn.addEventListener('click', () => {
        editFormSection.style.display = "none";
        messageAlert.innerHTML = '';
        errorAlert.innerHTML = '';

    });

    editForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const borrowedCountElement = document.getElementById('borrowedCountDisplay');
        const formData = {
            username: document.getElementById('Username').value,
            email: document.getElementById('Email').value,
            newPassword: document.getElementById('NewPassword').value,
            confirmPassword: document.getElementById('ConfirmPassword').value,
            borrowedCount: borrowedCountElement ? borrowedCountElement.textContent : '0'
        };

        try {
            const data = await updateUserProfile(formData);
            messageAlert.innerHTML = `<div class="alert-success">${data.message || 'Your information has been updated successfully.'}</div>`;
            document.getElementById('usernameDisplay').textContent = formData.username;
            editFormSection.style.display = 'none';
        } catch (error) {
            const errorMessage = error.data?.error || error.data || 'An error occurred.';
            errorAlert.innerHTML = `<div class="alert-danger">${errorMessage}</div>`;
        }
    });

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

    // Password check
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
            ruleMsg.style.color = "#10b981";
            ruleMsg.textContent = "✔ Password meets the requirements.";
        } else {
            ruleMsg.style.color = "#ef4444";
            ruleMsg.textContent =
                "✘ Password must be at least 8 characters long and contain at least one number.";
        }
    }

    newPassInput.addEventListener("input", validatePasswordRules);

});
