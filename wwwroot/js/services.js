//  API Services - Individual exported functions for each API call

import { get, post, put, del } from './apiClient.js';

export async function loginUser(username, password) {
    return post('/auth/login', { username, password });
}

export async function logoutUser() {
    return post('/auth/logout');
}

export async function registerUser(userName, userEmail, userPassword) {
    return post('/auth/register', { userName, userEmail, userPassword });
}

export async function getUserProfile() {
    return get('/account/index');
}

export async function updateUserProfile(profileData) {
    return put('/account/account', profileData);
}

export async function getUserGames() {
    return get('/account/games');
}

export async function checkUserAvailability(userName, userEmail) {
    return post('/account/check-availability', { userName, userEmail });
}

export async function saveApproveForm(decisions) {
    return post('/admin/save-approve-form', decisions);
}

export async function addGame(formData) {
    return post('/admin/add-game', formData);
}

export async function saveReturnForm(results) {
    return post('/admin/save-return-form', results);
}

export async function getOneGame(gameId) {
    return get(`/admin/get-one-game?gameId=${gameId}`);
}

export async function editGame(formData) {
    return post('/admin/edit-game', formData);
}

export async function deleteGame(id) {
    return post('/admin/delete-game', { Id: id });
}

export async function getAllGames() {
    return get('/Home/GetGames');
}


