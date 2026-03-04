import { useState, useEffect, useRef } from "react";
import { Link, useLocation } from "react-router-dom";
import "./Navbar.css";

const navLinks = [
  { to: "/", icon: "📅", text: "Записи", exact: true },
  { to: "/patients", icon: "👥", text: "Пацієнти" },
  { to: "/services", icon: "💼", text: "Послуги" },
  { to: "/dental-charts", icon: "🦷", text: "Карти" },
  { to: "/payments", icon: "💳", text: "Платежі" },
  { to: "/doctors", icon: "👨🏻‍⚕️", text: "Лікарі" },
  { to: "/treatment-records", icon: "🩺", text: "Лікування" },
  { to: "/medical-records", icon: "📝", text: "Медичні карти" },
];

export default function Navbar({ onLogout }) {
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);

  const isActive = (to, exact) => {
    if (exact) return location.pathname === "/";
    return location.pathname.startsWith(to);
  };

  // Close menu on outside click
  useEffect(() => {
    const handleClick = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) {
        setMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  // Close menu on route change
  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  return (
    <nav className="navbar">
      <div className="navbar-container">
        {/* Logo */}
        <div className="navbar-logo">
          <Link to="/" className="logo-link">
            <div className="logo-icon">
              <img
                src="../zubslogo.JPG"
                alt="DentalCare Logo"
                className="logo-image"
              />
            </div>
          </Link>
        </div>

        {/* Desktop Navigation Links */}
        <div className="navbar-links">
          {navLinks.map(({ to, icon, text, exact }) => (
            <Link
              key={to}
              to={to}
              className={`nav-link ${isActive(to, exact) ? "active" : ""}`}
            >
              <span className="nav-icon">{icon}</span>
              <span className="nav-text">{text}</span>
            </Link>
          ))}
        </div>

        {/* User + Logout */}
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

        {/* Hamburger button — mobile only, left side near logo */}
        <div className="hamburger-wrapper" ref={menuRef}>
          <button
            className={`hamburger-button ${menuOpen ? "open" : ""}`}
            onClick={() => setMenuOpen((v) => !v)}
            aria-label="Меню"
          >
            <span />
            <span />
            <span />
          </button>

          {/* Dropdown */}
          {menuOpen && (
            <div className="mobile-dropdown">
              {navLinks.map(({ to, icon, text, exact }) => (
                <Link
                  key={to}
                  to={to}
                  className={`mobile-nav-link ${isActive(to, exact) ? "active" : ""}`}
                >
                  <span className="mobile-nav-icon">{icon}</span>
                  <span className="mobile-nav-text">{text}</span>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </nav>
  );
}
