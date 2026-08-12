export const getAccessToken = () => localStorage.getItem('accessToken');
export const getRefreshToken = () => localStorage.getItem('refreshToken');

export const setTokens = (accessToken, refreshToken) => {
  if (accessToken) localStorage.setItem('accessToken', accessToken);
  if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
};

export const clearToken = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
};
