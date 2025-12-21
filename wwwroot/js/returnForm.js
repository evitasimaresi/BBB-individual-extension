import { saveReturnForm } from './services.js';

function resetAllCheckboxes() {
    const approveCards = document.querySelectorAll(".return-card");
    approveCards.forEach(card => {
        card.querySelector(".return-checkbox").checked = false;
        card.classList.remove("processed");
    });
}

document.addEventListener("DOMContentLoaded", () => {
    resetAllCheckboxes();
});

window.addEventListener("pageshow", (event) => {
    if (event.persisted) {
        resetAllCheckboxes();
    }
});

const cards = document.querySelectorAll(".return-card");

document.getElementById("return-all").addEventListener("click", () => {
    cards.forEach(card => {
        card.querySelector(".return-checkbox").checked = true;
        card.classList.add("processed");
    });
});

document.getElementById("reset-all").addEventListener("click", () => {
    cards.forEach(card => {
        card.querySelector(".return-checkbox").checked = false;
        card.classList.remove("processed");
    });
});

cards.forEach(card => {
    const checkbox = card.querySelector(".return-checkbox");
    checkbox.addEventListener("change", () => {
        if (checkbox.checked) card.classList.add("processed");
        else card.classList.remove("processed");
    });
});

document.getElementById("save-changes").addEventListener("click", async () => {
    const results = Array.from(cards).map(card => ({
        BoardGameUserId: parseInt(card.dataset.id),
        Returned: card.querySelector(".return-checkbox").checked
    }));

    try {
        await saveReturnForm(results);
        window.location.reload();
    } catch (error) {
        const msg = error.data || error.message || "Unknown error";
        alert("Error: " + msg);
    }
});