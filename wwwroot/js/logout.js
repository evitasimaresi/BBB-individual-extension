document.getElementById('logoutBtn').addEventListener('click', async () => {
    try {
        const response = await fetch('/api/account/logout', {
            method: 'GET'
        });

        if (response.ok) {
            window.location.href = '/';
        } else {
            console.error('Logout failed');
        }
    } catch (error) {
        console.error('Logout error:', error);
    }

});