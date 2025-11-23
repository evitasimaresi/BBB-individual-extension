let gamesData = [];
let filterMap = new Map();

// Fetch A list of games from the server, render them and attach event listeners
fetch('/Account/GetGames')
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

    // Add the "no games found" message
    const noGamesLi = document.createElement('li');
    noGamesLi.className = 'no-games-card';
    noGamesLi.style.display = games.length === 0 ? 'flex' : 'none';
    noGamesLi.innerHTML = '<span class="material-symbols-outlined">search_off</span> No games found';
    list.appendChild(noGamesLi);

    // Render All games
    games.forEach(game => {
        const li = document.createElement('li');
        li.className = 'card';
        
        // Determine button text and state based on statusId
        let borrowText = 'Borrow';
        let borrowDisabled = false;

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
                <button class="button button-primary borrow-button" style="background: ${statusColor};">${borrowText}</button>
            </section>
        </article>`;

        list.appendChild(li);
    });

    attachGameButtons();
}