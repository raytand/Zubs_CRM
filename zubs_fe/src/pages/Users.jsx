import { useEffect, useState } from "react";
import {
  getAllUsers,
  createUser,
  updateUser,
  deleteUser,
} from "../services/users";
import "./styles/Users.css";

const emptyForm = { username: "", email: "", role: "Doctor" };

// Role translations and icons
const ROLES = [
  { value: "Admin", label: "Адміністратор", icon: "👑", color: "#dc3545" },
  { value: "Doctor", label: "Лікар", icon: "👨‍⚕️", color: "#3e68a3" },
  { value: "Secretary", label: "Секретар", icon: "📋", color: "#28a745" },
];

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= USER CARD =================
const UserCard = ({ user, onEdit, onDelete }) => {
  const roleData = ROLES.find((r) => r.value === user.role);

  return (
    <div className="user-card">
      <div className="user-header">
        <div
          className="user-avatar"
          style={{ background: `${roleData?.color}15` }}
        >
          <span className="user-icon">{roleData?.icon || "👤"}</span>
        </div>
        <div className="user-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(user)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(user.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      <h3 className="user-name">{user.username}</h3>

      <div
        className="user-role"
        style={{
          backgroundColor: `${roleData?.color}15`,
          color: roleData?.color,
        }}
      >
        {roleData?.icon} {roleData?.label || user.role}
      </div>

      <div className="user-details">
        <div className="detail-row">
          <span className="detail-icon">📧</span>
          <span className="detail-value">{user.email}</span>
        </div>
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function Users() {
  const [users, setUsers] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [error, setError] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [filterRole, setFilterRole] = useState("all");
  const [viewMode, setViewMode] = useState("cards");

  const loadUsers = async () => {
    try {
      const data = await getAllUsers();
      setUsers(data || []);
    } catch (e) {
      console.error("Помилка завантаження користувачів:", e);
      setError("Помилка завантаження користувачів");
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const submitForm = async (e) => {
    e.preventDefault();
    setError("");

    try {
      if (editingId) {
        await updateUser(editingId, { ...form, id: editingId });
      } else {
        await createUser(form);
      }

      setForm(emptyForm);
      setEditingId(null);
      setShowModal(false);
      loadUsers();
    } catch (err) {
      console.error("Помилка збереження:", err);
      setError(
        err?.response?.data?.message || err?.title || "Помилка збереження",
      );
    }
  };

  const startEdit = (user) => {
    setForm({
      username: user.username,
      email: user.email,
      role: user.role,
    });
    setEditingId(user.id);
    setShowModal(true);
  };

  const removeUser = async (id) => {
    if (!confirm("Видалити користувача?")) return;

    try {
      await deleteUser(id);
      loadUsers();
    } catch (err) {
      console.error("Помилка видалення:", err);
      setError("Помилка видалення користувача");
    }
  };

  // Filter users
  const filteredUsers = users.filter((u) => {
    const matchesSearch =
      u.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.email.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesRole = filterRole === "all" || u.role === filterRole;

    return matchesSearch && matchesRole;
  });

  // Statistics
  const stats = {
    total: users.length,
    admins: users.filter((u) => u.role === "Admin").length,
    doctors: users.filter((u) => u.role === "Doctor").length,
    secretaries: users.filter((u) => u.role === "Secretary").length,
  };

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Користувачі</h1>
            <p className="page-subtitle">
              Всього: {stats.total} • Адмінів: {stats.admins} • Лікарів:{" "}
              {stats.doctors} • Секретарів: {stats.secretaries}
            </p>
          </div>
          <button
            className="btn-primary"
            onClick={() => {
              setForm(emptyForm);
              setEditingId(null);
              setShowModal(true);
            }}
          >
            + Додати користувача
          </button>
        </div>

        {/* Error Message */}
        {error && (
          <div className="alert alert-error">
            <span className="alert-icon">⚠️</span>
            <span>{error}</span>
            <button className="alert-close" onClick={() => setError("")}>
              ✕
            </button>
          </div>
        )}

        {/* Controls */}
        <div className="controls">
          <div className="search-box">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Пошук за ім'ям або email..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
            {searchTerm && (
              <button
                className="clear-search"
                onClick={() => setSearchTerm("")}
              >
                ✕
              </button>
            )}
          </div>

          <div className="filter-dropdown">
            <label>Роль:</label>
            <select
              value={filterRole}
              onChange={(e) => setFilterRole(e.target.value)}
            >
              <option value="all">Всі</option>
              {ROLES.map((role) => (
                <option key={role.value} value={role.value}>
                  {role.icon} {role.label}
                </option>
              ))}
            </select>
          </div>

          <div className="view-toggle">
            <button
              className={`toggle-btn ${viewMode === "cards" ? "active" : ""}`}
              onClick={() => setViewMode("cards")}
              title="Картки"
            >
              ▦
            </button>
            <button
              className={`toggle-btn ${viewMode === "table" ? "active" : ""}`}
              onClick={() => setViewMode("table")}
              title="Таблиця"
            >
              ☰
            </button>
          </div>
        </div>

        {/* Results Count */}
        {searchTerm && (
          <p className="results-info">
            Знайдено: {filteredUsers.length} з {users.length}
          </p>
        )}

        {/* Data Display */}
        {filteredUsers.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">👥</div>
            <h3>Користувачів не знайдено</h3>
            <p>
              {searchTerm
                ? "Спробуйте змінити критерії пошуку"
                : "Додайте першого користувача для початку роботи"}
            </p>
          </div>
        ) : viewMode === "cards" ? (
          <div className="users-grid">
            {filteredUsers.map((u) => (
              <UserCard
                key={u.id}
                user={u}
                onEdit={startEdit}
                onDelete={removeUser}
              />
            ))}
          </div>
        ) : (
          <div className="table-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th>Користувач</th>
                  <th>Email</th>
                  <th>Роль</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((u) => {
                  const roleData = ROLES.find((r) => r.value === u.role);
                  return (
                    <tr key={u.id}>
                      <td>
                        <strong>{u.username}</strong>
                      </td>
                      <td>{u.email}</td>
                      <td>
                        <span
                          className="role-badge"
                          style={{
                            backgroundColor: `${roleData?.color}15`,
                            color: roleData?.color,
                          }}
                        >
                          {roleData?.icon} {roleData?.label || u.role}
                        </span>
                      </td>
                      <td>
                        <div className="table-actions">
                          <button
                            className="btn-edit-sm"
                            onClick={() => startEdit(u)}
                          >
                            ✏️
                          </button>
                          <button
                            className="btn-delete-sm"
                            onClick={() => removeUser(u.id)}
                          >
                            🗑️
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="user-form" onSubmit={submitForm}>
              <h2>
                {editingId ? "Редагувати користувача" : "Новий користувач"}
              </h2>

              <div className="form-group">
                <label>Ім'я користувача *</label>
                <input
                  required
                  placeholder="Введіть ім'я користувача"
                  value={form.username}
                  onChange={(e) =>
                    setForm({ ...form, username: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Email *</label>
                <input
                  required
                  type="email"
                  placeholder="example@email.com"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>Роль *</label>
                <select
                  required
                  value={form.role}
                  onChange={(e) => setForm({ ...form, role: e.target.value })}
                >
                  {ROLES.map((role) => (
                    <option key={role.value} value={role.value}>
                      {role.icon} {role.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {editingId ? "Зберегти зміни" : "Створити користувача"}
                </button>
                <button
                  type="button"
                  className="btn-cancel"
                  onClick={() => setShowModal(false)}
                >
                  Скасувати
                </button>
              </div>
            </form>
          </Modal>
        )}
      </div>
    </div>
  );
}
