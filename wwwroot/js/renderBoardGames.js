// Variables
let gamesData = [];
let filterMap = new Map();

// Fetch A list of games from the server, render them and attach event listeners
fetch('/Home/GetGames')
    .then(response => response.json())
    .then(data => {
        gamesData = data; // Store fetched games
        renderGames(gamesData); // Initial render
        attachGameButtons(); // Attach event listeners
    })
    .catch(error => console.error('Error fetching games:', error));

// Render Games Function
function renderGames(games) {
    // Clear existing list
    const list = document.querySelector('.scrolloverflow');
    list.innerHTML = '';

    // Display message if no games are found with current filters
    if (!games.length) {
        list.innerHTML = '<li style="padding: 20px; color:var(--blue-custom-light); display: flex; align-items: center; gap: 8px;"><span class="material-symbols-outlined">search_off</span> No games found matching your filters</li>';
        return;
    }

    // Render All games
    games.forEach(game => {
        const li = document.createElement('li');
        li.className = 'card';
        li.dataset.gameId = game.id;

        // Determine button text and state based on statusId
        let borrowText = 'Borrow';
        let borrowDisabled = false;

        switch (game.statusId) {
            case 1:
                borrowText = 'Borrow';
                borrowDisabled = false;
                break;
            case 2:
                borrowText = 'Already Borrowed';
                borrowDisabled = true;
                break;
            case 3:
                borrowText = 'Try Borrow';
                borrowDisabled = false;
                break;
            case 4:
                borrowText = 'Unavailable';
                borrowDisabled = true;
                break;
        }

        li.innerHTML = `
        <img src="${game.image}" class="images"/>
        <article class="content">
            <h1>${game.title}</h1>
            <p>${game.description}</p>
            <section class="button-container">
                <div class="button-group">
                    ${allowEdit ? `<button class="button button-primary edit-button" data-id="${game.id}">Edit</button>` : ''}                        
                    <button class="button button-primary borrow-button" data-id="${game.id}" ${borrowDisabled ? 'disabled' : ''}>${borrowText}</button>
                </div>
            </section>
        </article>`;

        list.appendChild(li);
    });
}

// Get Active Filters
function getActiveFilters() {
    const filterMap = new Map();

    document.querySelectorAll('.filters-form input[type="checkbox"]:checked').forEach(cb => {
        const tagId = Number(cb.value);
        const groupId = Number(cb.dataset.group);

        if (!filterMap.has(groupId)) {
            filterMap.set(groupId, new Set());
        }
        filterMap.get(groupId).add(tagId);
    });

    return filterMap;
}

// Game Matches Filters
function gameMatchesFilters(game, filters) {
    if (filters.size === 0) return true;

    for (const [groupId, selectedTagIds] of filters.entries()) {
        const hasMatchInGroup = (game.tags || []).some(t =>
            t.tagGroupId === groupId && selectedTagIds.has(t.id)
        );

        if (!hasMatchInGroup) return false;
    }

    return true;
}

// Filter Rendered Games
function applyFilters(filters) {
    const cards = document.querySelectorAll('.scrolloverflow .card');

    cards.forEach(card => {
        const gameId = Number(card.dataset.gameId);
        const game = gamesData.find(g => g.id === gameId);
        if (!game) return;

        if (gameMatchesFilters(game, filters)) {
            card.classList.remove('card-hidden');
        } else {
            card.classList.add('card-hidden');
        }
    });
}

// Attach Event Listeners to Card Buttons and Filters
function attachGameButtons() {
    document.querySelectorAll('.borrow-button').forEach(button => {
        button.addEventListener('click', function () {
            const gameId = this.getAttribute('data-id');
            borrowGame(gameId, this);
        });
    });

    document.querySelectorAll('.edit-button').forEach(button => {
        button.addEventListener('click', function () {
            const gameId = this.getAttribute('data-id');
            openModal(gameId);
        });
    });

    document.addEventListener('change', e => {
        if (e.target && e.target.matches('.filters-form input[type="checkbox"]')) {
            filterMap = getActiveFilters();
            applyFilters(filterMap);
        }
    });
}

// Borrow Game
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
            } else {
                const errorText = await response.text();
                if (response.status === 401) {
                    alert('Please log in to borrow games.');
                } else if (response.status === 409) {
                    alert('This game is already borrowed or unavailable.');
                } else if (response.status === 419) {
                    alert('You have requested/borrowed too many games');
                } else if (response.status === 420) {
                    alert('Already requested/Borrowed');
                } else {
                    alert('Failed to borrow game: ' + errorText);
                }
            }
        })
        .catch(error => console.error('Error borrowing game:', error));
}

// Edit Game Modal
function openModal(gameId) {
    fetch(`/Admin/GetOneGame?gameId=${gameId}`)
        .then(response => response.json())
        .then(data => {
            document.getElementById('gameId').value = data.id;
            document.getElementById('gameTitle').value = data.title;
            document.getElementById('gameDesc').value = data.description;

            const dialog = document.getElementById('editGame');
            dialog.showModal();
        })
        .catch(error => console.error('Error fetching game:', error));
}

// Attach Modal listeners
document.getElementById('editGame').addEventListener('submit', function (e) {
    e.preventDefault();
    const formData = new FormData(this);

    fetch('/Admin/EditGame', {
        method: 'POST',
        body: formData
    }).then(() => location.reload());
});

document.getElementById('deleteGameButton').addEventListener('click', function () {
    const gameId = document.getElementById('gameId').value;
    fetch(`/Admin/DeleteGame/${gameId}`, {
        method: 'POST'
    }).then(() => location.reload());
});

document.getElementById('cancelEditButton').addEventListener('click', function () {
    document.getElementById('editGame').close();
});