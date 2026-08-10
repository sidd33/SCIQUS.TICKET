export function getRoleLabel(user) {
  if (!user) return "";
  if (typeof user.role === "string") return user.role;
  if (user.role === 3) return "SuperAdmin";
  if (user.role === 2) return "Admin";
  if (user.role === 1) return "Employee";
  return "Employee";
}

export function isAdmin(user) {
  if (!user) return false;
  return (
    user.role === "Admin" ||
    user.role === "admin" ||
    user.role === "SuperAdmin" ||
    user.role === "superadmin" ||
    user.role === 2 ||
    user.role === 3
  );
}

export function isCustomer(user) {
  if (!user) return false;
  return user.role === "Customer" || user.role === "customer";
}

export function isAgent(user) {
  if (!user) return false;
  return user.role === "Employee" || user.role === "employee" || user.role === 1;
}

export function isAdminOrAbove(user) {
  return isAdmin(user);
}

export function getCurrentUser() {
  return JSON.parse(localStorage.getItem("user") || "null");
}

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
  localStorage.removeItem("user");
}