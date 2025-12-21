import { saveApproveForm } from './services.js';

function resetAllCheckboxes() {
    const approveCards = document.querySelectorAll(".approve-card");
    approveCards.forEach(card => {
        card.querySelector(".approve-checkbox").checked = false;
        card.querySelector(".deny-checkbox").checked = false;
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

const approveCards = document.querySelectorAll(".approve-card");

function updateLinked(card, resetAll = false) {
    const boardGameId = card.dataset.boardgameid;
    const linkedCards = Array.from(approveCards).filter(c => c.dataset.boardgameid === boardGameId);
    const approve = card.querySelector(".approve-checkbox");
    const deny = card.querySelector(".deny-checkbox");

    if (resetAll || (!approve.checked && !deny.checked)) {
        linkedCards.forEach(c => {
            c.querySelector(".approve-checkbox").checked = false;
            c.querySelector(".deny-checkbox").checked = false;
            c.classList.remove("processed");
        });
        return;
    }

    if (approve.checked) {
        linkedCards.forEach(c => {
            if (c !== card) {
                c.querySelector(".approve-checkbox").checked = false;
                c.querySelector(".deny-checkbox").checked = true;
                c.classList.add("processed");
            }
        });
    }
}

approveCards.forEach(card => {
    const approve = card.querySelector(".approve-checkbox");
    const deny = card.querySelector(".deny-checkbox");

    approve.addEventListener("change", () => {
        if (approve.checked) {
            card.classList.add("processed");
            deny.checked = false;
            updateLinked(card);
        } else {
            card.classList.remove("processed");
            updateLinked(card, true);
        }
    });

    deny.addEventListener("change", () => {
        if (deny.checked) {
            card.classList.add("processed");
            approve.checked = false;
            updateLinked(card);
        } else {
            card.classList.remove("processed");
            updateLinked(card, true);
        }
    });
});

document.getElementById("approve-all").addEventListener("click", () => {
    const grouped = {};
    approveCards.forEach(card => {
        const bgId = card.dataset.boardgameid;
        if (!grouped[bgId]) grouped[bgId] = [];
        grouped[bgId].push(card);
    });

    Object.values(grouped).forEach(group => {
        group.forEach((card, index) => {
            if (index === 0) {
                card.querySelector(".approve-checkbox").checked = true;
                card.querySelector(".deny-checkbox").checked = false;
            } else {
                card.querySelector(".approve-checkbox").checked = false;
                card.querySelector(".deny-checkbox").checked = true;
            }
            card.classList.add("processed");
        });
    });
});

document.getElementById("deny-all").addEventListener("click", () => {
    approveCards.forEach(card => {
        card.querySelector(".approve-checkbox").checked = false;
        card.querySelector(".deny-checkbox").checked = true;
        card.classList.add("processed");
    });
});

document.getElementById("reset-all").addEventListener("click", () => {
    approveCards.forEach(card => {
        card.querySelector(".approve-checkbox").checked = false;
        card.querySelector(".deny-checkbox").checked = false;
        card.classList.remove("processed");
    });
});

document.getElementById("save-changes").addEventListener("click", async () => {
    const decisions = Array.from(approveCards).map(card => {
        const approve = card.querySelector(".approve-checkbox").checked;
        const deny = card.querySelector(".deny-checkbox").checked;
        let result = 0;
        if (approve) result = 1;
        else if (deny) result = 2;

        return {
            BoardGameUserId: parseInt(card.dataset.id),
            Result: result
        };
    });

    try {
        await saveApproveForm(decisions);
        window.location.reload();
    } catch (error) {
        const errorMessage = error.data || error.message || "Unknown error";
        alert("Error: " + errorMessage);
    }
});