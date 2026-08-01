import axios from 'axios';
import { Platform } from 'react-native';

// Use 10.0.2.2 for Android emulator to access local host, or your backend's local IP (e.g. 192.168.1.100)
const BASE_URL = Platform.OS === 'android' ? 'http://10.0.2.2:5241/api' : 'http://localhost:5241/api';

const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
});

import EncryptedStorage from 'react-native-encrypted-storage';
import { authEmitter } from './authService';

api.interceptors.request.use(
  async (config) => {
    const token = await EncryptedStorage.getItem('@auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

let isRefreshing = false;
let failedQueue: Array<{resolve: (value?: unknown) => void, reject: (reason?: any) => void}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (originalRequest.url?.includes('/auth/login') || originalRequest.url?.includes('/auth/refresh')) {
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise(function(resolve, reject) {
          failedQueue.push({ resolve, reject });
        }).then(token => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        }).catch(err => {
          return Promise.reject(err);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = await EncryptedStorage.getItem('@refresh_token');
        if (!refreshToken) {
          throw new Error('No refresh token');
        }

        const res = await axios.post(`${BASE_URL}/auth/refresh`, { refreshToken });
        const { accessToken, refreshToken: newRefreshToken } = res.data;
        
        await EncryptedStorage.setItem('@auth_token', accessToken);
        await EncryptedStorage.setItem('@refresh_token', newRefreshToken);
        
        processQueue(null, accessToken);
        
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch (err) {
        processQueue(err, null);
        await EncryptedStorage.removeItem('@auth_token');
        await EncryptedStorage.removeItem('@refresh_token');
        authEmitter.emit('session_expired');
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }

    if (error.response?.status === 403) {
      console.warn('API 403 Forbidden:', error.response?.data || error.message);
    } else if (error.response?.status === 401) {
       // if we hit 401 and _retry was true (refresh failed), trigger logout
       authEmitter.emit('session_expired');
    }

    return Promise.reject(error);
  }
);

export default api;
