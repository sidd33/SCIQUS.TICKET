import axios from "axios";
import {
    getToken,
    getRefreshToken,
    setAccessToken,
    setRefreshToken,
    clearToken
} from "../auth/tokenManager";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5239/api";

export const SERVER_ORIGIN = API_BASE_URL.replace(/\/api\/?$/, "");

export function resolveImageUrl(relativeUrl) {
  if (!relativeUrl) return null;
  if (/^https?:\/\//i.test(relativeUrl)) return relativeUrl;
  return `${SERVER_ORIGIN}${relativeUrl}`;
}

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let isRefreshing = false;
let failedQueue = [];

function processQueue(error, token = null) {
  failedQueue.forEach((p) => {
    if (error) p.reject(error);
    else p.resolve(token);
  });
  failedQueue = [];
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config || {};
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = getRefreshToken();
        const res = await axios.post(`${API_BASE_URL}/auth/refresh`, { refreshToken });

        // AuthResponseDto: { accessToken, refreshToken } — NOT { token, refreshToken }
        const newAccess = res.data.accessToken;
        const newRefresh = res.data.refreshToken;

        setAccessToken(newAccess);
        setRefreshToken(newRefresh);
        processQueue(null, newAccess);

        originalRequest.headers.Authorization = `Bearer ${newAccess}`;
        return api(originalRequest);
      } catch (err) {
        processQueue(err);
        clearToken();
        localStorage.removeItem("user");
        sessionStorage.setItem("authMessage", "Your session has expired. Please sign in again.");
        window.location.href = "/login";
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;