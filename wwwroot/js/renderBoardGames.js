let gamesData = [];

// fetch  from server
fetch('/Home/GetGames')
    .then(response => response.json())
    .then(data => {
        gamesData = data;
        renderGames(gamesData);
    })
    .catch(error => console.error('Error fetching games:', error));

function renderGames(games) {
    const list = document.querySelector('.scrolloverflow');
    list.innerHTML = '';

     if (!games.length) {
        list.innerHTML = '<li style="padding: 20px; color:var(--blue-custom-light); display: flex; align-items: center; gap: 8px;"><span class="material-symbols-outlined">search_off</span> No games found matching your filters</li>';
        return;
    }

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
                        ${allowEdit ? `<button class="button button-primary edit-button" data-id="${game.id}">Edit</button>` : ''}
                        <button class="button button-primary borrow-button" data-id="${game.id}">Borrow</button>
                    </div>
                </section>
            </article>`;
        list.appendChild(li);
    });

    // get them grouped by TagGroupId
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

    //re-render
    function applyFilters() {
        const filters = getActiveFilters();
        const filtered = gamesData.filter(g => gameMatchesFilters(g, filters));
        renderGames(filtered);
    }

    // catches checkbox changes
    document.addEventListener('change', e => {
        if (e.target && e.target.matches('.filters-form input[type="checkbox"]')) {
            applyFilters();
        }
    }); 

    document.querySelectorAll('.borrow-button').forEach(button => {
        button.addEventListener('click', function () {
            const gameId = this.getAttribute('data-id');
            borrowGame(gameId, this);
        });
    });

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
}

function borrowGame(gameId, buttonElement) {
    fetch('/Home/BorrowGame', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
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

    document.querySelectorAll('.edit-button').forEach(button => {
        button.addEventListener('click', function () {
            const gameId = this.getAttribute('data-id');
            openModal(gameId);
        });
    });
}

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
/*
document.getElementById('filterInput').addEventListener('input', function() {
    const query = this.value.toLowerCase();
    const filtered = gamesData.filter(game => game.title.toLowerCase().includes(query));

    renderGames(filtered);
});
*/

// populate the EDIT GAME pop-up form

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
