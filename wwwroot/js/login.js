document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    try {
      const response = await fetch(`/api/account/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });
      
      if (response.ok) {
        const data = await response.json();
        window.location.href = data.redirectUrl || '/';
      } else {
        alert('Invalid credentials');
      }
    } catch (error) {
      console.error('Login error:', error);
    }
  });