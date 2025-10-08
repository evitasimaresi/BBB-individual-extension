function fetch_insert(path, id) {
    fetch(path)
        .then(response => response.text())
        .then(data => {
            document.getElementById(id).innerHTML = data;
        })
        .catch(error => console.error(`Error loading "${id}":`, error));
}

document.addEventListener('DOMContentLoaded', function() {
    fetch_insert('components/navbar.html', 'navbar-container');
    fetch_insert('components/sidebar.html', 'sidebar-container');
    fetch_insert('components/footer.html', 'footer-container');
});