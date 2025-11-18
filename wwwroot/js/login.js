document.querySelector('form').addEventListener('submit', async (e) => {
    e.preventDefault();
    check();
});

function check() {
    const form = document.querySelector('form');
    const formData = new FormData(form);

    fetch('/Account/Login', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (response.status === 418) {
            alert("Invalid username or password");
        }
        else {
            window.location.href = "/Home/Index";
        }
    })
}