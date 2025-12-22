//  API Services - exportable functions for API call

import { get, post, put, del, patch } from './apiClient.js';

// Auth
export async function loginUser(username, password) {
    return post('/auth/login', { username, password });
}

export async function logoutUser() {
    return post('/auth/logout');
}

export async function registerUser(userName, userEmail, userPassword) {
    return post('/auth/register', { userName, userEmail, userPassword });
}

// Account
export async function getUserProfile() {
    return get('/account');
}

export async function updateUserProfile(profileData) {
    return put('/account', profileData);
}

export async function getUserGames() {
    return get('/account/games');
}

export async function checkUserAvailability(userName, userEmail) {
    return post('/account/check-availability', { userName, userEmail });
}


// Admin
export async function saveApproveForm(decisions) {
    return patch('/admin/borrow-requests', decisions);
}

export async function saveReturnForm(results) {
    return patch('/admin/returns', results);
}

export async function addGame(formData) {
    return post('/admin/games', formData);
}

export async function getOneGame(gameId) {
    return get(`/admin/games/${gameId}`);
}

export async function editGame(gameId, formData) {
    return put(`/admin/games/${gameId}`, formData);
}

export async function deleteGame(id) {
    return del(`/admin/games/${id}`);
}

// Games
export async function getAllGames(searchQuery = null) {
    const url = searchQuery ? `/games?search=${encodeURIComponent(searchQuery)}` : '/games';
    return get(url);
}

export async function borrowGame(gameId) {
    return post('/games/borrow-requests', { gameId });
}


