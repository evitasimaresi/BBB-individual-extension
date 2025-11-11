let gamesData = [];

// Fetch data from server
fetch('/Home/GetGames')
    .then(response => response.json())
    .then(data => {
        gamesData = data;
        renderGames(gamesData);
    })
    .catch(error => console.error('Error fetching games:', error));

// Render function
function renderGames(games) {
    const list = document.querySelector('.scrolloverflow');
    list.innerHTML = '';

    games.forEach(game => {
        const li = document.createElement('li');
        li.className = 'card';
        li.innerHTML = `
            <img src="${game.image}" class="images"/>
            <article class="content">
                <h1>${game.title}</h1>
                <p>${game.description}</p>
                <section class="button-container">
                    <Span>Available</Span>
                    <div style="display:flex; gap:12px">
                        ${allowEdit ? '<button class="button button-primary edit-button">Edit</button>' : ''}
                        <button class="button button-primary" data-id="${game.id}">Borrow</button>
                    </div>
                </section>
            </article>`
        
        ;
        list.appendChild(li);
    });

    // Attach click listeners
    document.querySelectorAll('.button-primary').forEach(button => {
        button.addEventListener('click', function () {
            const gameId = this.getAttribute('data-id');
            borrowGame(gameId, this);
        });
    });
}

// Borrow game
function borrowGame(gameId, buttonElement) {
    fetch('/Home/BorrowGame', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(gameId)
    })
    .then(async response => {
        if (response.ok) {
            buttonElement.textContent = 'Borrowed';
            buttonElement.disabled = true;
        } else {
            const errorText = await response.text();
            if (response.status === 401) {
                alert('Please log in to borrow games.');
            } else if (response.status === 409) {
                alert('This game is already borrowed or unavailable.');
            } else {
                alert('Failed to borrow game: ' + errorText);
            }
        }
    })
    .catch(error => console.error('Error borrowing game:', error));
}

// Filter logic
document.getElementById('filterInput').addEventListener('input', function() {
    const query = this.value.toLowerCase();
    const filtered = gamesData.filter(game => game.title.toLowerCase().includes(query));

    renderGames(filtered);
});