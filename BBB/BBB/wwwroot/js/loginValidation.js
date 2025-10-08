document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('form');
    const usernameInput = form.querySelector('input[type="text"]');
    const passwordInput = form.querySelector('input[type="password"]');

    form.addEventListener('submit', (e) => {
        const username = usernameInput.value.trim();
        const password = passwordInput.value;

        const isUsernameValid = username.length >= 4;
        const isPasswordValid = password.length >= 8 && /\d/.test(password);

        if (!isUsernameValid || !isPasswordValid) {
        e.preventDefault(); // Stop form from submitting
        let message = 'Please fix the following:\n';
        if (!isUsernameValid) message += '- Username must be at least 4 characters\n';
        if (!isPasswordValid) message += '- Password must be at least 8 characters and contain at least one number\n';
        alert(message);
        }
    });
});