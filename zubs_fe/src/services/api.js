import axios from "axios";

const API_URL = "/api";

// Token management
let accessToken = localStorage.getItem("accessToken") || null;
let refreshToken = localStorage.getItem("refreshToken") || null;

export const setTokens = (access, refresh) => {
  accessToken = access;
  refreshToken = refresh;

  if (access) {
    localStorage.setItem("accessToken", access);
  } else {
    localStorage.removeItem("accessToken");
  }

  if (refresh) {
    localStorage.setItem("refreshToken", refresh);
  } else {
    localStorage.removeItem("refreshToken");
  }
};

export const getAccessToken = () => accessToken;
export const getRefreshToken = () => refreshToken;

export const clearTokens = () => {
  accessToken = null;
  refreshToken = null;
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("user");
};

// Legacy setToken for compatibility
export const setToken = (token) => {
  setTokens(token, refreshToken);
};

// Create axios instance
export const api = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Flag to prevent multiple refresh attempts
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });

  failedQueue = [];
};

// Request interceptor - add access token
api.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

// Response interceptor - handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If error is not 401 or request already retried, reject
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    // If already refreshing, queue this request
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const currentRefreshToken = getRefreshToken();

    if (!currentRefreshToken) {
      isRefreshing = false;
      clearTokens();
      window.location.href = "/auth";
      return Promise.reject(error);
    }

    try {
      // Call refresh endpoint
      const response = await axios.post(`${API_URL}/auth/refresh`, {
        refreshToken: currentRefreshToken,
      });

      const { accessToken: newAccessToken, refreshToken: newRefreshToken } =
        response.data;

      // Update tokens
      setTokens(newAccessToken, newRefreshToken);

      // Update failed requests with new token
      processQueue(null, newAccessToken);

      // Retry original request with new token
      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      // Refresh failed - clear tokens and redirect to login
      processQueue(refreshError, null);
      clearTokens();
      window.location.href = "/auth";
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

// Helper functions for fetch API (legacy support)
const authHeaders = () => ({
  "Content-Type": "application/json",
  ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
});

const handleResponse = async (res) => {
  let data = null;

  try {
    data = await res.json();
  } catch {
    console.log("No JSON response");
  }

  if (!res.ok) {
    // If 401, try to refresh token
    if (res.status === 401) {
      const refreshed = await tryRefreshToken();
      if (refreshed) {
        // Retry the original request
        throw { retry: true };
      }
    }
    throw data ?? { title: res.statusText };
  }

  return data;
};

const tryRefreshToken = async () => {
  const currentRefreshToken = getRefreshToken();

  if (!currentRefreshToken) {
    clearTokens();
    window.location.href = "/auth";
    return false;
  }

  try {
    const response = await fetch(`${API_URL}/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: currentRefreshToken }),
    });

    if (response.ok) {
      const data = await response.json();
      setTokens(data.accessToken, data.refreshToken);
      return true;
    }
  } catch (error) {
    console.error("Token refresh failed:", error);
  }

  clearTokens();
  window.location.href = "/auth";
  return false;
};

// Fetch wrapper with retry logic
const fetchWithRetry = async (fetchFn) => {
  try {
    return await fetchFn();
  } catch (error) {
    if (error?.retry) {
      // Retry after token refresh
      return await fetchFn();
    }
    throw error;
  }
};

export const fetchGet = async (url) => {
  return fetchWithRetry(async () => {
    const res = await fetch(`${API_URL}${url}`, {
      headers: authHeaders(),
    });
    return handleResponse(res);
  });
};

export const fetchPost = async (url, body) => {
  return fetchWithRetry(async () => {
    const res = await fetch(`${API_URL}${url}`, {
      method: "POST",
      headers: authHeaders(),
      body: JSON.stringify(body),
    });
    return handleResponse(res);
  });
};

export const fetchPut = async (url, body) => {
  return fetchWithRetry(async () => {
    const res = await fetch(`${API_URL}${url}`, {
      method: "PUT",
      headers: authHeaders(),
      body: JSON.stringify(body),
    });
    return handleResponse(res);
  });
};

export const fetchDelete = async (url) => {
  return fetchWithRetry(async () => {
    const res = await fetch(`${API_URL}${url}`, {
      method: "DELETE",
      headers: authHeaders(),
    });
    return handleResponse(res);
  });
};

export default api;
