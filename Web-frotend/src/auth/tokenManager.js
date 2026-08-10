const ACCESS_TOKEN_KEY = "token";
const REFRESH_TOKEN_KEY = "refreshToken";

let accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
let refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

export function setTokens(access, refresh) {
  accessToken = access;
  refreshToken = refresh;

  localStorage.setItem(ACCESS_TOKEN_KEY, access);
  localStorage.setItem(REFRESH_TOKEN_KEY, refresh);
}

export function getToken() {
  return accessToken;
}

export function getRefreshToken() {
  return refreshToken;
}

export function setAccessToken(token) {
  accessToken = token;
  localStorage.setItem(ACCESS_TOKEN_KEY, token);
}

export function setRefreshToken(token) {
  refreshToken = token;
  localStorage.setItem(REFRESH_TOKEN_KEY, token);
}

export function clearToken() {
  accessToken = null;
  refreshToken = null;

  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}