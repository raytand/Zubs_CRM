import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login, register } from "../services/auth";
import { setToken } from "../services/api";
import "./styles/Auth.css";

const emptyLogin = { username: "", password: "" };
const emptyRegister = { username: "", email: "", password: "" };

export default function Auth({ onLogin }) {
  const navigate = useNavigate();
  const [loginForm, setLoginForm] = useState(emptyLogin);
  const [registerForm, setRegisterForm] = useState(emptyRegister);
  const [mode, setMode] = useState("login"); // "login" or "register"
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState("");

  // Auth.jsx - submitLogin
  const submitLogin = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      console.log("1. Sending login request:", loginForm);
      const res = await login(loginForm);
      console.log("2. Login response:", res);
      console.log("3. accessToken:", res?.accessToken);
      console.log("4. refreshToken:", res?.refreshToken);

      if (res?.accessToken) {
        setSuccess("Вхід успішний!");
        if (onLogin) onLogin();
        setTimeout(() => navigate("/"), 500);
      } else {
        console.log("5. No accessToken in response!");
        setError("Не отримано токен");
      }
    } catch (err) {
      console.error("6. Login error:", err);
      console.error("7. Error response:", err?.response?.data);
      setError(
        err?.response?.data?.message ||
          err?.title ||
          "Помилка входу. Перевірте дані.",
      );
    } finally {
      setLoading(false);
    }
  };

  const submitRegister = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);

    try {
      await register(registerForm);
      setSuccess("Реєстрація успішна! Тепер увійдіть.");
      setRegisterForm(emptyRegister);

      setTimeout(() => {
        setMode("login");
        setSuccess("");
      }, 1500);
    } catch (err) {
      console.error("Register error:", err);
      setError(
        err?.response?.data?.message ||
          err?.title ||
          "Помилка реєстрації. Спробуйте ще раз.",
      );
    } finally {
      setLoading(false);
    }
  };

  const switchMode = () => {
    setMode(mode === "login" ? "register" : "login");
    setError("");
    setSuccess("");
  };

  return (
    <div className="auth-page">
      <div className="auth-container">
        {/* Logo/Brand Section */}
        <div className="auth-header">
          <div className="logo-icon">
            <img
              src="../zubslogo.JPG"
              alt="DentalCare Logo"
              className="logo-image"
            />
          </div>
          <h1>{mode === "login" ? "Вітаємо знову" : "Створити акаунт"}</h1>
          <p className="auth-subtitle">
            {mode === "login"
              ? "Увійдіть у систему управління клінікою"
              : "Зареєструйтесь для доступу до системи"}
          </p>
        </div>

        {/* Alert Messages */}
        {error && (
          <div className="alert alert-error">
            <span className="alert-icon">⚠️</span>
            <span>{error}</span>
          </div>
        )}

        {success && (
          <div className="alert alert-success">
            <span className="alert-icon">✓</span>
            <span>{success}</span>
          </div>
        )}

        {/* Forms */}
        {mode === "login" ? (
          <form onSubmit={submitLogin} className="auth-form">
            <div className="form-group">
              <label htmlFor="username">Ім'я користувача</label>
              <input
                id="username"
                required
                placeholder="Введіть ім'я користувача"
                value={loginForm.username}
                onChange={(e) =>
                  setLoginForm({ ...loginForm, username: e.target.value })
                }
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="password">Пароль</label>
              <input
                id="password"
                type="password"
                required
                placeholder="Введіть пароль"
                value={loginForm.password}
                onChange={(e) =>
                  setLoginForm({ ...loginForm, password: e.target.value })
                }
                disabled={loading}
              />
            </div>

            <button type="submit" className="btn-submit" disabled={loading}>
              {loading ? (
                <>
                  <span className="spinner"></span>
                  <span>Вхід...</span>
                </>
              ) : (
                "Увійти"
              )}
            </button>

            <div className="auth-footer">
              <p>
                Немає акаунту?{" "}
                <button
                  type="button"
                  className="link-button"
                  onClick={switchMode}
                  disabled={loading}
                >
                  Зареєструватися
                </button>
              </p>
            </div>
          </form>
        ) : (
          <form onSubmit={submitRegister} className="auth-form">
            <div className="form-group">
              <label htmlFor="reg-username">Ім'я користувача</label>
              <input
                id="reg-username"
                required
                placeholder="Введіть ім'я користувача"
                value={registerForm.username}
                onChange={(e) =>
                  setRegisterForm({ ...registerForm, username: e.target.value })
                }
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="email">Email</label>
              <input
                id="email"
                type="email"
                required
                placeholder="example@email.com"
                value={registerForm.email}
                onChange={(e) =>
                  setRegisterForm({ ...registerForm, email: e.target.value })
                }
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="reg-password">Пароль</label>
              <input
                id="reg-password"
                type="password"
                required
                placeholder="Мінімум 6 символів"
                minLength="6"
                value={registerForm.password}
                onChange={(e) =>
                  setRegisterForm({ ...registerForm, password: e.target.value })
                }
                disabled={loading}
              />
            </div>

            <button type="submit" className="btn-submit" disabled={loading}>
              {loading ? (
                <>
                  <span className="spinner"></span>
                  <span>Реєстрація...</span>
                </>
              ) : (
                "Зареєструватися"
              )}
            </button>

            <div className="auth-footer">
              <p>
                Вже маєте акаунт?{" "}
                <button
                  type="button"
                  className="link-button"
                  onClick={switchMode}
                  disabled={loading}
                >
                  Увійти
                </button>
              </p>
            </div>
          </form>
        )}

        {/* Additional Info */}
        <div className="auth-info">
          <p className="info-text">🔒 Ваші дані захищені та зашифровані</p>
        </div>
      </div>
    </div>
  );
}
