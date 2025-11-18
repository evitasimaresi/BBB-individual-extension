document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('form');
    const usernameInput = form.querySelector('input[name="userName"]');
    const emailInput = form.querySelector('input[name="userEmail"]');
    const passwordInput = form.querySelector('input[name="userPassword"]');

    // Make the submit handler async so we can use await
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

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

        const data = {
            userName: username,
            userEmail: email
        };

        // --- Server-side validation ---
        try {
            console.log("Checking username/email availability...");

            const response = await fetch('/Account/CheckUserAvailability', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                alert('Server error while checking availability.');
                return;
            }

            const result = await response.json();

            if (result.usernameTaken || result.emailTaken) {
                message = 'Please fix the following:\n';
                if (result.usernameTaken) message += '- Username is already taken\n';
                if (result.emailTaken) message += '- Email is already registered\n';
                alert(message);
                return;
            }

            console.log("1");
            // --- If everything passes, submit form normally ---
            form.submit();
            console.log("2");

        } catch (error) {
            console.error('Error checking availability:', error);
            alert('An error occurred while checking username/email availability.');
        }
    });
});
