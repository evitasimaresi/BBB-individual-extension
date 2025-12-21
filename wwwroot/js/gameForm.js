import { addGame } from './services.js';

document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('.game-upload-form');

    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = new FormData(form);

        try {
            await addGame(formData);
            window.location.href = '/';
        } catch (error) {
            console.error('Error submitting form:', error);
            const errorMessage = error.data?.message || error.message || 'Failed to add game';
            alert('Error: ' + errorMessage);
        }
    });
});
