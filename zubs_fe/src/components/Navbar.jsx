import { Link, useLocation } from "react-router-dom";
import "./Navbar.css";

export default function Navbar({ onLogout }) {
  const location = useLocation();

  const isActive = (path) => {
    if (path === "/" && location.pathname === "/") return true;
    if (path !== "/" && location.pathname.startsWith(path)) return true;
    return false;
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        {/* Logo Section */}
        <div className="navbar-logo">
          <Link to="/" className="logo-link">
            <div className="logo-icon">
              <img
                src="../zubslogo.JPG" // path to your logo
                alt="DentalCare Logo"
                className="logo-image"
              />
            </div>
          </Link>
        </div>

        {/* Navigation Links */}
        <div className="navbar-links">
          <Link
            to="/"
            className={`nav-link ${isActive("/") && location.pathname === "/" ? "active" : ""}`}
          >
            <span className="nav-icon">📅</span>
            <span className="nav-text">Записи</span>
          </Link>

          <Link
            to="/patients"
            className={`nav-link ${isActive("/patients") ? "active" : ""}`}
          >
            <span className="nav-icon">👥</span>
            <span className="nav-text">Пацієнти</span>
          </Link>

          <Link
            to="/services"
            className={`nav-link ${isActive("/services") ? "active" : ""}`}
          >
            <span className="nav-icon">💼</span>
            <span className="nav-text">Послуги</span>
          </Link>

          <Link
            to="/dental-charts"
            className={`nav-link ${isActive("/dental-charts") ? "active" : ""}`}
          >
            <span className="nav-icon">🦷</span>
            <span className="nav-text">Карти</span>
          </Link>

          <Link
            to="/payments"
            className={`nav-link ${isActive("/payments") ? "active" : ""}`}
          >
            <span className="nav-icon">💳</span>
            <span className="nav-text">Платежі</span>
          </Link>

          <Link
            to="/doctors"
            className={`nav-link ${isActive("/doctors") ? "active" : ""}`}
          >
            <span className="nav-icon">👨🏻‍⚕️</span>
            <span className="nav-text"> Лікарі</span>
          </Link>
          <Link
            to="/treatment-records"
            className={`nav-link ${isActive("/treatment-records") ? "active" : ""}`}
          >
            <span className="nav-icon">🩺</span>
            <span className="nav-text">Лікування</span>
          </Link>
          <Link
            to="/medical-records"
            className={`nav-link ${isActive("/medical-records") ? "active" : ""}`}
          >
            <span className="nav-icon">📝</span>
            <span className="nav-text">Медичні карти</span>
          </Link>
        </div>

        {/* User Section */}
        <div className="navbar-user">
          <Link to="/auth" className="user-link">
            <div className="user-avatar">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="8" r="4" fill="currentColor" />
                <path
                  d="M4 20c0-4 3.5-7 8-7s8 3 8 7"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          </Link>
        </div>
        <button className="logout-button" onClick={onLogout} title="Вийти">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v1"
            />
          </svg>
        </button>
      </div>
    </nav>
  );
}
