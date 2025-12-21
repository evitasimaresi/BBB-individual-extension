// centralized API client for making HTTP requests

const BASE_URL = '/api';

async function request(endpoint, options = {}) {
    // If endpoint is a full URL (http/https) or starts with /Home, use as-is
    // Otherwise prepend /api
    const url = endpoint.startsWith('http') || endpoint.startsWith('/Home')
        ? endpoint
        : `${BASE_URL}${endpoint}`;

    const config = {
        headers: {
            'Content-Type': 'application/json',
            ...options.headers
        },
        ...options
    };

    try {
        const response = await fetch(url, config);

        const contentType = response.headers.get('content-type');
        let data;

        if (contentType && contentType.includes('application/json')) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            throw {
                status: response.status,
                statusText: response.statusText,
                data
            };
        }

        return data;
    } catch (error) {
        console.error(`API Error [${options.method || 'GET'}] ${url}:`, error);
        throw error;
    }
}

export const get = (endpoint, options = {}) =>
    request(endpoint, { ...options, method: 'GET' });

export const post = (endpoint, body, options = {}) =>
    request(endpoint, { ...options, method: 'POST', body: JSON.stringify(body) });

export const put = (endpoint, body, options = {}) =>
    request(endpoint, { ...options, method: 'PUT', body: JSON.stringify(body) });

export const del = (endpoint, options = {}) =>
    request(endpoint, { ...options, method: 'DELETE' });

export const patch = (endpoint, body, options = {}) =>
    request(endpoint, { ...options, method: 'PATCH', body: JSON.stringify(body) });
