import { logoutUser } from './services.js';

const logoutBtn = document.getElementById('logoutBtn');

if (logoutBtn) {
    logoutBtn.addEventListener('click', async () => {
        try {
            await logoutUser();
            window.location.href = '/';
        } catch (error) {
            console.error('Logout error:', error);
        }
    });
}