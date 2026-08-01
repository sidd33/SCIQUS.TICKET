import EncryptedStorage from 'react-native-encrypted-storage';
import { jwtDecode } from 'jwt-decode';
import EventEmitter from 'events';
import api from './api';

const TOKEN_KEY = '@auth_token';
const REFRESH_TOKEN_KEY = '@refresh_token';

export const authEmitter = new EventEmitter();

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: number;
}

export const authService = {
  async login(email: string, password: string):Promise<{token: string, user: User}> {
    const response = await api.post('/auth/login', { email, password });
    const { accessToken, refreshToken, user } = response.data;
    await EncryptedStorage.setItem(TOKEN_KEY, accessToken);
    if (refreshToken) await EncryptedStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    return { token: accessToken, user };
  },

  async register(firstName: string, lastName: string, email: string, password: string): Promise<void> {
    await api.post('/auth/register', { name: `${firstName} ${lastName}`, email, password });
  },

  async logout(): Promise<void> {
    await EncryptedStorage.removeItem(TOKEN_KEY);
    await EncryptedStorage.removeItem(REFRESH_TOKEN_KEY);
  },

  async getToken(): Promise<string | null> {
    return await EncryptedStorage.getItem(TOKEN_KEY);
  },

  async getRefreshToken(): Promise<string | null> {
    return await EncryptedStorage.getItem(REFRESH_TOKEN_KEY);
  },

  async setTokens(token: string, refreshToken: string): Promise<void> {
    await EncryptedStorage.setItem(TOKEN_KEY, token);
    await EncryptedStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },

  async getUser(): Promise<User | null> {
    const token = await this.getToken();
    if (!token) return null;
    
    try {
      const decoded: any = jwtDecode(token);
      // For simplicity, we are returning a mapped object, but in a real app
      // you would use the user object returned from login and store it.
      const rawRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      let roleNum = 0; // default to customer
      if (rawRole === 'SuperAdmin' || rawRole === '3') roleNum = 3;
      else if (rawRole === 'Admin' || rawRole === '2') roleNum = 2;
      else if (rawRole === 'Employee' || rawRole === '1') roleNum = 1;
      else if (rawRole === 'Customer' || rawRole === '0') roleNum = 0;
      else roleNum = Number(rawRole) || 0;

      return {
        id: decoded.nameid || decoded.sub || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        firstName: '', // Token doesn't contain first name, only ID/Email/Role
        lastName: '',
        email: decoded.email || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        role: roleNum
      };
    } catch (e) {
      return null;
    }
  }
};
