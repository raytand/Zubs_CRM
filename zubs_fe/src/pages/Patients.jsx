import { useEffect, useState } from "react";
import {
  getAllPatients,
  createPatient,
  updatePatient,
  deletePatient,
} from "../services/patients";
import "./styles/Patients.css";

const empty = {
  id: null,
  firstName: "",
  lastName: "",
  birthDate: "",
  phone: "",
  email: "",
  address: "",
  gender: "",
  notes: "",
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= PATIENT CARD =================
const PatientCard = ({ patient, onEdit, onDelete }) => {
  // Calculate age from birthDate
  let age = null;
  if (patient.birthDate) {
    try {
      const birthDate = new Date(patient.birthDate);
      const today = new Date();
      age = today.getFullYear() - birthDate.getFullYear();
      const monthDiff = today.getMonth() - birthDate.getMonth();
      if (
        monthDiff < 0 ||
        (monthDiff === 0 && today.getDate() < birthDate.getDate())
      ) {
        age--;
      }
    } catch (e) {
      console.error("Age calculation error:", e);
    }
  }

  const genderLabel =
    patient.gender === 0 ? "Чоловік" : patient.gender === 1 ? "Жінка" : "—";

  return (
    <div className="patient-card">
      <div className="patient-header">
        <div className="patient-avatar">
          {patient.gender === 0 ? "👨" : patient.gender === 1 ? "👩" : "👤"}
        </div>
        <div className="patient-info">
          <h3 className="patient-name">
            {patient.firstName} {patient.lastName}
          </h3>
          <div className="patient-meta">
            {age && <span className="meta-item">📅 {age} років</span>}
            <span className="meta-item">⚧ {genderLabel}</span>
          </div>
        </div>
        <div className="patient-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(patient)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(patient.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      <div className="patient-details">
        {patient.phone && (
          <div className="detail-row">
            <span className="detail-icon">📞</span>
            <span className="detail-value">{patient.phone}</span>
          </div>
        )}
        {patient.email && (
          <div className="detail-row">
            <span className="detail-icon">📧</span>
            <span className="detail-value">{patient.email}</span>
          </div>
        )}
        {patient.address && (
          <div className="detail-row">
            <span className="detail-icon">📍</span>
            <span className="detail-value">{patient.address}</span>
          </div>
        )}
        {patient.notes && (
          <div className="detail-row notes">
            <span className="detail-icon">📝</span>
            <span className="detail-value">{patient.notes}</span>
          </div>
        )}
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function Patients() {
  const [list, setList] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [viewMode, setViewMode] = useState("cards"); // "cards" or "table"

  const load = async () => setList(await getAllPatients());

  useEffect(() => {
    load();
  }, []);

  const submit = async (e) => {
    e.preventDefault();

    // Prepare birthDate - ensure it's either a valid date string or null
    let birthDateToSend = null;
    if (form.birthDate && form.birthDate.trim() !== "") {
      // Send as YYYY-MM-DD format (no time component)
      birthDateToSend = form.birthDate;
    }

    const dto = {
      ...form,
      birthDate: birthDateToSend,
      gender: form.gender === "" ? null : +form.gender,
    };

    try {
      edit ? await updatePatient(form.id, dto) : await createPatient(dto);
      setForm(empty);
      setEdit(false);
      setShowModal(false);
      load();
    } catch (error) {
      console.error("Save error:", error);
      alert(
        "Помилка збереження: " +
          (error.response?.data?.message ||
            error.message ||
            "Невідома помилка"),
      );
    }
  };

  const editRow = (p) => {
    // Handle birthDate properly - convert from ISO string to YYYY-MM-DD format
    let birthDateValue = "";
    if (p.birthDate) {
      try {
        const date = new Date(p.birthDate);
        // Format as YYYY-MM-DD for date input
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        birthDateValue = `${year}-${month}-${day}`;
      } catch (e) {
        console.error("Date parsing error:", e);
        birthDateValue = "";
      }
    }

    setForm({
      ...p,
      birthDate: birthDateValue,
      gender: p.gender ?? "",
    });
    setEdit(true);
    setShowModal(true);
  };

  const remove = async (id) => {
    if (!confirm("Видалити пацієнта?")) return;
    await deletePatient(id);
    setList((x) => x.filter((p) => p.id !== id));
  };

  // Filter patients
  const filteredList = list.filter((p) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      p.firstName.toLowerCase().includes(searchLower) ||
      p.lastName.toLowerCase().includes(searchLower) ||
      p.phone?.toLowerCase().includes(searchLower) ||
      p.email?.toLowerCase().includes(searchLower)
    );
  });

  // Calculate statistics
  const stats = {
    total: list.length,
    male: list.filter((p) => p.gender === 0).length,
    female: list.filter((p) => p.gender === 1).length,
  };

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Пацієнти</h1>
            <p className="page-subtitle">
              Загалом: {stats.total} • Чоловіків: {stats.male} • Жінок:{" "}
              {stats.female}
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
            + Додати пацієнта
          </button>
        </div>

        {/* Search and View Controls */}
        <div className="controls">
          <div className="search-box">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Пошук за ім'ям, телефоном, email..."
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
            <form className="patient-form" onSubmit={submit}>
              <h2>{edit ? "Редагувати пацієнта" : "Новий пацієнт"}</h2>

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

              <div className="form-row">
                <div className="form-group">
                  <label>Дата народження</label>
                  <input
                    type="date"
                    value={form.birthDate}
                    onChange={(e) =>
                      setForm({ ...form, birthDate: e.target.value })
                    }
                  />
                </div>

                <div className="form-group">
                  <label>Стать</label>
                  <select
                    value={form.gender}
                    onChange={(e) =>
                      setForm({ ...form, gender: e.target.value })
                    }
                  >
                    <option value="">Не вказано</option>
                    <option value="0">Чоловік</option>
                    <option value="1">Жінка</option>
                  </select>
                </div>
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

              <div className="form-group">
                <label>Адреса</label>
                <input
                  placeholder="Вулиця, місто"
                  value={form.address}
                  onChange={(e) =>
                    setForm({ ...form, address: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Нотатки</label>
                <textarea
                  rows="3"
                  placeholder="Додаткова інформація"
                  value={form.notes}
                  onChange={(e) => setForm({ ...form, notes: e.target.value })}
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {edit ? "Зберегти зміни" : "Створити пацієнта"}
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
            <div className="empty-icon">👥</div>
            <h3>Пацієнтів не знайдено</h3>
            <p>
              {searchTerm
                ? "Спробуйте змінити критерії пошуку"
                : "Додайте першого пацієнта для початку роботи"}
            </p>
          </div>
        ) : viewMode === "cards" ? (
          <div className="patients-grid">
            {filteredList.map((p) => (
              <PatientCard
                key={p.id}
                patient={p}
                onEdit={editRow}
                onDelete={remove}
              />
            ))}
          </div>
        ) : (
          <div className="table-container">
            <table className="patients-table">
              <thead>
                <tr>
                  <th>Пацієнт</th>
                  <th>Дата народження</th>
                  <th>Контакти</th>
                  <th>Адреса</th>
                  <th>Стать</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {filteredList.map((p) => (
                  <tr key={p.id}>
                    <td>
                      <strong>
                        {p.firstName} {p.lastName}
                      </strong>
                    </td>
                    <td>
                      {p.birthDate
                        ? (() => {
                            try {
                              const date = new Date(p.birthDate);
                              return date.toLocaleDateString("uk-UA", {
                                year: "numeric",
                                month: "2-digit",
                                day: "2-digit",
                              });
                            } catch (e) {
                              return "—";
                            }
                          })()
                        : "—"}
                    </td>
                    <td>
                      <div>{p.phone}</div>
                      <div className="text-small">{p.email}</div>
                    </td>
                    <td>{p.address}</td>
                    <td>
                      {p.gender === 0
                        ? "Чоловік"
                        : p.gender === 1
                          ? "Жінка"
                          : "—"}
                    </td>
                    <td>
                      <div className="table-actions">
                        <button
                          className="btn-edit-sm"
                          onClick={() => editRow(p)}
                        >
                          ✏️
                        </button>
                        <button
                          className="btn-delete-sm"
                          onClick={() => remove(p.id)}
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
