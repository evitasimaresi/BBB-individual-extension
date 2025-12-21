import { loginUser } from './services.js';

document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const username = document.getElementById('username').value;
  const password = document.getElementById('password').value;

  try {
    const data = await loginUser(username, password);
    window.location.href = data.redirectUrl || '/';
  } catch (error) {
    console.error('Login error:', error);
    alert('Invalid credentials');
  }
});