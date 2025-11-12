document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('form');
    const usernameInput = form.querySelector('input[type="text"]');
    const passwordInput = form.querySelector('input[type="password"]');
    const dialog = document.getElementById("dialog-box");
    const closeButton = document.getElementById("close-dialog");

    let readyToSubmit = false;

    form.addEventListener('submit', (e) => {
        e.preventDefault();

        if(readyToSubmit) {
            window.location.replace("/Account/Login");
            return;
        }

        const username = usernameInput.value.trim();
        const password = passwordInput.value;

        const isUsernameValid = username.length >= 4;
        const isPasswordValid = password.length >= 8 && /\d/.test(password);

        if (!isUsernameValid || !isPasswordValid) {
        e.preventDefault(); // is it neccessary for me to repeat it here before the ifs?
        let message = 'Please fix the following:\n';
        if (!isUsernameValid) message += '- Username must be at least 4 characters\n';
        if (!isPasswordValid) message += '- Password must be at least 8 characters and contain at least one number\n';
        alert(message);
        }
        else {
            dialog.showModal();
        }

        closeButton.addEventListener("click", () => {
            dialog.close();
            readyToSubmit = true;
            form.requestSubmit();
            });

    });

    
}); 