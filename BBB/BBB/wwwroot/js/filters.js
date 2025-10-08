document.addEventListener('DOMContentLoaded', function() {
    const filterToggles = document.querySelectorAll('.filter-toggle');
    
    filterToggles.forEach(toggle => {
        toggle.addEventListener('click', function() {
            const filterId = this.getAttribute('data-filter');
            const filterContent = document.getElementById(filterId);
            const isActive = this.classList.contains('active');
            
            if (isActive) {
                this.classList.remove('active');
                filterContent.classList.remove('active');
            } else {
                this.classList.add('active');
                filterContent.classList.add('active');
            }
        });
    });
});