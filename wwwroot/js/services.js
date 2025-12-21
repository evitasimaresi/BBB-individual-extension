//  API Services - Individual exported functions for each API call

import { get, post, put, del } from './apiClient.js';

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

// non-API routes
export async function getAllGames() {
    return get('/Home/GetGames');
}


