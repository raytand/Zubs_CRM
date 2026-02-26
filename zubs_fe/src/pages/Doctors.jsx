import { useEffect, useState } from "react";
import {
  getAllDoctors,
  createDoctor,
  updateDoctor,
  deleteDoctor,
} from "../services/doctors";
import { getAllUsers } from "../services/users";
import "./styles/Doctors.css";

const empty = {
  id: null,
  userId: "",
  firstName: "",
  lastName: "",
  specialization: "",
  phone: "",
  email: "",
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= DOCTOR CARD =================
const DoctorCard = ({ doctor, onEdit, onDelete }) => {
  return (
    <div className="doctor-card">
      <div className="doctor-header">
        <div className="doctor-avatar">👨‍⚕️</div>
        <div className="doctor-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(doctor)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(doctor.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      <h3 className="doctor-name">
        {doctor.firstName} {doctor.lastName}
      </h3>

      <div className="doctor-specialization">
        {doctor.specialization || "Спеціалізація не вказана"}
      </div>

      <div className="doctor-details">
        {doctor.phone && (
          <div className="detail-row">
            <span className="detail-icon">📞</span>
            <span className="detail-value">{doctor.phone}</span>
          </div>
        )}
        {doctor.email && (
          <div className="detail-row">
            <span className="detail-icon">📧</span>
            <span className="detail-value">{doctor.email}</span>
          </div>
        )}
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function Doctors() {
  const [list, setList] = useState([]);
  const [users, setUsers] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterSpec, setFilterSpec] = useState("all");
  const [viewMode, setViewMode] = useState("cards");

  const load = async () => {
    try {
      const doctors = await getAllDoctors();
      setList(doctors || []);

      // Завантажуємо користувачів
      try {
        const usersData = await getAllUsers();
        setUsers(usersData || []);
        console.log("Завантажено користувачів:", usersData?.length || 0);
      } catch (userError) {
        console.error("Помилка завантаження користувачів:", userError);
        alert(
          "Увага: Не вдалося завантажити список користувачів. Перевірте підключення до API.",
        );
        setUsers([]);
      }
    } catch (error) {
      console.error("Помилка завантаження лікарів:", error);
      alert("Помилка завантаження даних");
    }
  };

  useEffect(() => {
    load();
  }, []);

  const submit = async (e) => {
    e.preventDefault();

    // Валідація userId при створенні
    if (!edit && !form.userId) {
      alert("Оберіть користувача!");
      return;
    }

    const dto = {
      userId: form.userId || null,
      firstName: form.firstName,
      lastName: form.lastName,
      specialization: form.specialization,
      phone: form.phone,
      email: form.email,
    };

    try {
      if (edit) {
        await updateDoctor(form.id, { ...dto, id: form.id });
      } else {
        await createDoctor(dto);
      }

      setForm(empty);
      setEdit(false);
      setShowModal(false);
      load();
    } catch (error) {
      console.error("Помилка збереження:", error);
      const errorMessage = error.response?.data?.errors
        ? Object.values(error.response.data.errors).flat().join(", ")
        : error.response?.data?.message || error.message || "Невідома помилка";
      alert("Помилка збереження: " + errorMessage);
    }
  };

  const editRow = (d) => {
    setForm(d);
    setEdit(true);
    setShowModal(true);
  };

  const remove = async (id) => {
    if (!confirm("Видалити лікаря?")) return;
    await deleteDoctor(id);
    setList((x) => x.filter((d) => d.id !== id));
  };

  // Filter doctors
  const filteredList = list.filter((d) => {
    const matchesSearch =
      d.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      d.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      d.specialization?.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesSpec = filterSpec === "all" || d.specialization === filterSpec;

    return matchesSearch && matchesSpec;
  });

  // Get unique specializations
  const specializations = [
    ...new Set(list.map((d) => d.specialization).filter(Boolean)),
  ];

  // Statistics
  const stats = {
    total: list.length,
    specializations: specializations.length,
  };

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Лікарі</h1>
            <p className="page-subtitle">
              Всього: {stats.total} • Спеціалізацій: {stats.specializations}
            </p>
          </div>
          <button
            className="btn-primary"
            onClick={() => {
              setForm(empty);
              setEdit(false);
              setShowModal(true);
            }}
          >
            + Додати лікаря
          </button>
        </div>

        {/* Controls */}
        <div className="controls">
          <div className="search-box">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Пошук за ім'ям або спеціалізацією..."
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

          {specializations.length > 0 && (
            <div className="filter-dropdown">
              <label>Спеціалізація:</label>
              <select
                value={filterSpec}
                onChange={(e) => setFilterSpec(e.target.value)}
              >
                <option value="all">Всі</option>
                {specializations.map((spec) => (
                  <option key={spec} value={spec}>
                    {spec}
                  </option>
                ))}
              </select>
            </div>
          )}

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
            Знайдено: {filteredList.length} з {list.length}
          </p>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="doctor-form" onSubmit={submit}>
              <h2>{edit ? "Редагувати лікаря" : "Новий лікар"}</h2>

              {!edit && (
                <div className="form-group">
                  <label>Користувач *</label>
                  <select
                    required
                    value={form.userId}
                    onChange={(e) =>
                      setForm({ ...form, userId: e.target.value })
                    }
                  >
                    <option value="">Оберіть користувача</option>
                    {users.length === 0 ? (
                      <option disabled>Завантаження...</option>
                    ) : (
                      users.map((u) => (
                        <option key={u.id} value={u.id}>
                          {u.username} {u.email ? `(${u.email})` : ""}
                        </option>
                      ))
                    )}
                  </select>
                  {users.length === 0 && (
                    <span className="form-hint error">
                      ⚠️ Не вдалося завантажити користувачів. Перевірте
                      з'єднання з API.
                    </span>
                  )}
                </div>
              )}

              <div className="form-row">
                <div className="form-group">
                  <label>Ім'я *</label>
                  <input
                    required
                    placeholder="Ім'я"
                    value={form.firstName}
                    onChange={(e) =>
                      setForm({ ...form, firstName: e.target.value })
                    }
                  />
                </div>

                <div className="form-group">
                  <label>Прізвище *</label>
                  <input
                    required
                    placeholder="Прізвище"
                    value={form.lastName}
                    onChange={(e) =>
                      setForm({ ...form, lastName: e.target.value })
                    }
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Спеціалізація *</label>
                <input
                  required
                  placeholder="Наприклад: Стоматолог-терапевт"
                  value={form.specialization}
                  onChange={(e) =>
                    setForm({ ...form, specialization: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Номер телефону</label>
                <input
                  type="tel"
                  placeholder="+380 XX XXX XX XX"
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>Email</label>
                <input
                  type="email"
                  placeholder="example@email.com"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {edit ? "Зберегти зміни" : "Створити лікаря"}
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

        {/* Data Display */}
        {filteredList.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">👨‍⚕️</div>
            <h3>Лікарів не знайдено</h3>
            <p>
              {searchTerm
                ? "Спробуйте змінити критерії пошуку"
                : "Додайте першого лікаря для початку роботи"}
            </p>
          </div>
        ) : viewMode === "cards" ? (
          <div className="doctors-grid">
            {filteredList.map((d) => (
              <DoctorCard
                key={d.id}
                doctor={d}
                onEdit={editRow}
                onDelete={remove}
              />
            ))}
          </div>
        ) : (
          <div className="table-container">
            <table className="doctors-table">
              <thead>
                <tr>
                  <th>Лікар</th>
                  <th>Спеціалізація</th>
                  <th>Контакти</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {filteredList.map((d) => (
                  <tr key={d.id}>
                    <td>
                      <strong>
                        {d.firstName} {d.lastName}
                      </strong>
                    </td>
                    <td>{d.specialization}</td>
                    <td>
                      <div>{d.phone}</div>
                      <div className="text-small">{d.email}</div>
                    </td>
                    <td>
                      <div className="table-actions">
                        <button
                          className="btn-edit-sm"
                          onClick={() => editRow(d)}
                        >
                          ✏️
                        </button>
                        <button
                          className="btn-delete-sm"
                          onClick={() => remove(d.id)}
                        >
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
