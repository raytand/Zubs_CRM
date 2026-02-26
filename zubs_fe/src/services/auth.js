import { fetchPost, setTokens, clearTokens, getRefreshToken } from "./api";

/**
 * Login user and store tokens
 * @param {Object} dto - { username, password }
 * @returns {Promise<Object>} - { accessToken, refreshToken, expiresIn }
 */
export const login = async (dto) => {
  try {
    const res = await fetchPost("/auth/login", dto);
    console.log("Login response:", res);

    if (res?.accessToken && res?.refreshToken) {
      // Store both tokens
      setTokens(res.accessToken, res.refreshToken);

      // Optionally store user info if returned
      if (res.user) {
        localStorage.setItem("user", JSON.stringify(res.user));
      }
    }

    return res;
  } catch (error) {
    console.error("Login error:", error);
    throw error;
  }
};

/**
 * Register new user
 * @param {Object} dto - { username, email, password }
 * @returns {Promise<Object>} - Registration response
 */
export const register = async (dto) => {
  try {
    const res = await fetchPost("/auth/register", dto);
    console.log("Register response:", res);
    return res;
  } catch (error) {
    console.error("Register error:", error);
    throw error;
  }
};

/**
 * Logout user and revoke refresh token
 */ export const logout = async () => {
  try {
    const currentRefreshToken = getRefreshToken();
    if (currentRefreshToken) {
      await fetchPost("/auth/logout", { refreshToken: currentRefreshToken });
    }
  } catch (error) {
    console.error("Logout error:", error);
  } finally {
    clearTokens(); // тільки очищаємо токени
  }
};
/**
 * Check if user is authenticated
 * @returns {boolean}
 */
export const isAuthenticated = () => {
  const accessToken = localStorage.getItem("accessToken");
  const refreshToken = localStorage.getItem("refreshToken");
  return !!(accessToken || refreshToken);
};

/**
 * Get current user info from localStorage
 * @returns {Object|null}
 */
export const getCurrentUser = () => {
  const userStr = localStorage.getItem("user");
  if (!userStr) return null;

  try {
    return JSON.parse(userStr);
  } catch (error) {
    console.error("Error parsing user data:", error);
    return null;
  }
};
