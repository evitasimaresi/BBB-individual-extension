import { checkUserAvailability, registerUser, loginUser } from './services.js';

document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('form');

    if (!form) return; // Exit if no form on this page

    const usernameInput = form.querySelector('input[name="userName"]');
    const emailInput = form.querySelector('input[name="userEmail"]');
    const passwordInput = form.querySelector('input[name="userPassword"]');
    const dialog = document.getElementById("dialog-box");
    const closeButton = document.getElementById("close-dialog");

    let readyToSubmit = false;

    // Make the submit handler async so we can use await
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        if (readyToSubmit) {
            window.location.replace("/Account/Login");
            return;
        }

        const username = usernameInput.value.trim();
        const email = emailInput.value.trim();
        const password = passwordInput.value;

        let message = '';

        const isUsernameValid = username.length >= 4;
        const isPasswordValid = password.length >= 8 && /\d/.test(password);
        const emailPattern = /^[A-Za-z0-9._%+-]+@student\.sdu\.dk$/i;
        const isEmailValid = emailPattern.test(email);

        // --- Client-side validation ---
        if (!isUsernameValid || !isEmailValid || !isPasswordValid) {
            message = 'Please fix the following:\n';
            if (!isUsernameValid) message += '- Username must be at least 4 characters\n';
            if (!isEmailValid) message += '- Email must be a valid @student.sdu.dk address\n';
            if (!isPasswordValid) message += '- Password must be at least 8 characters and contain at least one number\n';
            alert(message);
            return;
        }

        // --- Server-side validation ---
        try {
            console.log("Checking username/email availability...");

            const result = await checkUserAvailability(username, email);

            if (result.usernameTaken || result.emailTaken) {
                message = 'Please fix the following:\n';
                if (result.usernameTaken) message += '- Username is already taken\n';
                if (result.emailTaken) message += '- Email is already registered\n';
                alert(message);
                return;
            }

            // --- If validation passes, register the user ---
            await registerUser(username, email, password);

            // Auto-login after successful registration
            await loginUser(username, password);

            // Redirect to home page
            window.location.href = "/";

        } catch (error) {
            console.error('Error during registration:', error);
            alert('An error occurred during registration. Please try again.');
        }
    });

    if (closeButton) {
        closeButton.addEventListener("click", () => {
            if (dialog) {
                dialog.close();
            }
            readyToSubmit = true;
            form.requestSubmit();
        });
    }
});
