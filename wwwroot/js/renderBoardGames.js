// Variables
let gamesData = [];
let filterMap = { tagFilters: [], statusFilters: [], queryFilters: [], queryActive: false };

// Fetch A list of games from the server, render them and attach event listeners
fetch('/Home/GetGames')
    .then(response => response.json())
    .then(data => {
        gamesData = data; // Store fetched games
        renderGames(gamesData); // Initial render
        attachGameButtons(); // Attach button listeners
        attachModalListeners(); // Attach modal listeners
    })
    .catch(error => console.error('Error fetching games:', error));

// Render Games Function
function renderGames(games) {
    // Clear existing list
    const list = document.querySelector('.scrolloverflow');
    list.innerHTML = '';

    // Add the "no games found" message so it can be shown/hidden via card-hidden
    const noGamesLi = document.createElement('li');
    noGamesLi.className = 'no-games-card card-hidden';
    noGamesLi.innerHTML = '<span class="material-symbols-outlined">search_off</span> No games found matching your filters';
    list.appendChild(noGamesLi);

    // Render All games
    games.forEach(game => {
        const li = document.createElement('li');
        li.className = 'card';
        li.dataset.gameId = game.id;
        
        // Determine button text and state based on statusId
        let borrowText = 'Borrow';
        let borrowDisabled = false;

        // Populate tags
        const tagsHTML = game.tags && game.tags.length > 0
            ? game.tags.map(tag => `<span class="boxes">${tag.name}</span>`).join('')
            : '<span class="boxes">No tags</span>';

        let statusColor = '';

        switch (game.statusId) {
            case 1:
                borrowText = 'Borrow';
                borrowDisabled = false;
                break;
            case 2:
                borrowText = game.statusName;
                borrowDisabled = true;
                statusColor = 'var(--blue-300)';
                break;
            case 3:
                borrowText = 'Requested';
                borrowDisabled = false;
                statusColor = 'var(--warning)';
                break;
            case 4:
                borrowText = game.statusName;
                borrowDisabled = true;
                statusColor = 'var(--blue-300)';
                break;
        }

        li.innerHTML = `
        <img src="${game.image}" class="images"/>
        <article class="content">
            <h1>${game.title}</h1>
            <p>${game.description}</p>
            <section class="button-container">
                <div class="tags-container">${tagsHTML}</div>
                <div class="button-group">
                    ${allowEdit ? `<button class="button button-primary edit-button" data-id="${game.id}">Edit</button>` : ''}                        
                    <button class="button button-primary borrow-button" data-id="${game.id}" ${borrowDisabled ? 'disabled' : ''} style="background: ${statusColor};">${borrowText}</button>
                </div>
            </section>
        </article>`;

        list.appendChild(li);
    });
}

// Get Active Filters
function getActiveFilters() {
    const tagFilters = new Map();
    const statusFilters = new Set();

    // Get tag filters (those with data-group attribute)
    document.querySelectorAll('.filters-form input[type="checkbox"]:checked[data-group]').forEach(cb => {
        const tagId = Number(cb.value);
        const groupId = Number(cb.dataset.group);

        if (!tagFilters.has(groupId)) {
            tagFilters.set(groupId, new Set());
        }
        tagFilters.get(groupId).add(tagId);
    });

    // Get status filters (those with data-status attribute)
    document.querySelectorAll('.filters-form input[type="checkbox"]:checked[data-status]').forEach(cb => {
        statusFilters.add(Number(cb.value));
    });

    // Persist values in filterMap
    const queryFilters = filterMap.queryFilters;
    const queryActive = filterMap.queryActive;

    return { tagFilters, statusFilters, queryFilters, queryActive };
}

// Game Matches Filters
function gameMatchesFilters(game, filters) {
    // Check query filter
    if (filters.queryActive && !filters.queryFilters.includes(game.id)) {
        return false;
    }

    // Check status filter
    if (filters.statusFilters.size > 0 && !filters.statusFilters.has(game.statusId)) {
        return false;
    }

    // Check tag filters (AND between groups, OR within group)
    if (filters.tagFilters.size === 0) return true;

    for (const [groupId, selectedTagIds] of filters.tagFilters.entries()) {
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
    let listEmpty = true;

    cards.forEach(card => {
        const gameId = Number(card.dataset.gameId);
        const game = gamesData.find(g => g.id === gameId);
        if (!game) return;

        if (gameMatchesFilters(game, filters)) {
            card.classList.remove('card-hidden');
            listEmpty = false;
        } else {
            card.classList.add('card-hidden');
        }
    });

    // Show or hide the "no games found" message
    const noGamesLi = document.querySelector('.scrolloverflow .no-games-card');
    if (noGamesLi) {
        if (listEmpty) {
            noGamesLi.classList.remove('card-hidden');
        } else {
            noGamesLi.classList.add('card-hidden');
        }
    }
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
function attachModalListeners() {
    document.getElementById('button-save').addEventListener('submit', function (e) {
        e.preventDefault();
        const formData = new FormData(this);

        fetch('/Admin/EditGame', {
            method: 'POST',
            body: formData
        }).then(() => location.reload());
    });

    document.getElementById('button-delete').addEventListener('click', function () {
        const gameId = document.getElementById('gameId').value;
        fetch(`/Admin/DeleteGame/${gameId}`, {
            method: 'POST'
        }).then(() => location.reload());
    });

    document.getElementById('button-cancel').addEventListener('click', function () {
        document.getElementById('editGame').close();
    });
}

// Attach Search Bar listener
const searchButton = document.getElementById('game-search-form');
if (searchButton) {
    searchButton.addEventListener('submit', function (e) {
        e.preventDefault();
        const query = (this.querySelector('input[name="query"]')?.value || '').trim();

        if (!query) {
            filterMap.queryActive = false;
            filterMap.queryFilters = [];
            filterMap = getActiveFilters();
            applyFilters(filterMap);
            return;
        }

        fetch(`/Home/SearchGames?query=${encodeURIComponent(query)}`, { method: 'GET' })
            .then(response => response.json())
            .then(data => {
                filterMap.queryActive = true;
                filterMap.queryFilters = data;
                filterMap = getActiveFilters();
                applyFilters(filterMap);
            })
            .catch(error => console.error('Error searching games:', error));
    });
}

// Attach Search Clear Button listener
const searchInput = document.querySelector('#game-search-form input[name="query"]');
const searchClearButton = document.getElementById('search-clear');
if (searchClearButton && searchInput) {
    // show/hide clear button as user types
    searchInput.addEventListener('input', function () {
        searchClearButton.style.display = (this.value || '') ? 'inline' : 'none';
    });

    // clear input, disable query filter and reapply filters
    searchClearButton.addEventListener('click', function () {
        searchInput.value = '';
        searchClearButton.style.display = 'none';
        filterMap.queryActive = false;
        filterMap.queryFilters = [];
        filterMap = getActiveFilters();
        applyFilters(filterMap);
        searchInput.focus();
    });
}