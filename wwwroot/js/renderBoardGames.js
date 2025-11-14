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
                        ${allowEdit ? `<button class="button button-primary edit-button" data-id="${game.id}">Edit</button>` : ''}
                        <button class="button button-primary borrow-button" data-id="${game.id}">Borrow</button>
                    </div>
                </section>
            </article>`
        
        ;
        list.appendChild(li);
    });

    // Attach listeners after rendering
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


// populate the EDIT GAME pop-up form

function openModal(gameId)
{
    fetch(`/Admin/GetOneGame?gameId=${gameId}`)
    /*.then(async response => {
            const errorMessage = await response.json();
            alert("Error: " + errorMessage);
            return;
    })*/
    
    .then(response => response.json())
    .then(data => {
        document.getElementById('gameId').value = data.id;
        document.getElementById('gameTitle').value = data.title;
        document.getElementById('gameDesc').value = data.description;
        //document.getElementById('gameCover').value = data.Image;
        // what is const dialog?
        const dialog = document.getElementById('editGame');
        dialog.showModal();
   })
    // .catch(error => console.error('Error fetching games:', error));
    
}

// Save button
document.getElementById('editGame').addEventListener('submit', function (e) {
  e.preventDefault();
  const formData = new FormData(this);

  fetch('/Admin/EditGame', {
    method: 'POST',
    body: formData
  }).then(() => location.reload());
});

// Delete button
document.getElementById('deleteGameButton').addEventListener('click', function () {
  const gameId = document.getElementById('gameId').value;
  fetch(`/Admin/DeleteGame/${gameId}`, {
    method: 'POST'
  }).then(() => location.reload());
});

// Cancel button
document.getElementById('cancelEditButton').addEventListener('click', function () {
  document.getElementById('editGame').close();
});
