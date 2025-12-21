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
        errorAlert.innerHTML= '';
        
    });

    editForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = {
            username: document.getElementById('Username').value,
            email: document.getElementById('Email').value,
            password: document.getElementById('Password').value,
            confirmpassword: document.getElementById('ConfirmPassword').value,
            borrowedCount: document.getElementById('borrowedCountDisplay').textContent
        };

        try {
            const response = await fetch('/api/account', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (response.ok) {
                messageAlert.innerHTML = `<div class="alert-success">${data.message || 'Your information has been updated successfully.'}</div>`;
                document.getElementById('usernameDisplay').textContent = formData.username;
                editFormSection.style.display = 'none';
            } else {
                errorAlert.innerHTML =  `<div class="alert-danger">${data.error || 'An error occurred.'}</div>`;
            }
        } catch (error) {
            errorAlert.innerHTML = `<div class="alert-danger">Error updating account: ${error.message}</div>`;
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
