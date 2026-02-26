import { useState, useEffect } from "react";
import { getAllPatients } from "../services/patients";
import {
  getMedicalRecordsByPatient,
  createMedicalRecord,
  updateMedicalRecord,
  deleteMedicalRecord,
} from "../services/medicalRecords";
import "./styles/MedicalRecords.css";

const empty = {
  id: null,
  allergies: "",
  medications: "",
  conditions: "",
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= RECORD CARD =================
const RecordCard = ({ record, onEdit, onDelete }) => {
  return (
    <div className="record-card">
      <div className="record-header">
        <div className="record-icon">📋</div>
        <div className="record-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(record)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(record.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      <div className="record-sections">
        {record.allergies && (
          <div className="record-section">
            <h4 className="section-title">🚨 Алергії</h4>
            <p className="section-content">{record.allergies}</p>
          </div>
        )}

        {record.medications && (
          <div className="record-section">
            <h4 className="section-title">💊 Медикаменти</h4>
            <p className="section-content">{record.medications}</p>
          </div>
        )}

        {record.conditions && (
          <div className="record-section">
            <h4 className="section-title">🩺 Захворювання</h4>
            <p className="section-content">{record.conditions}</p>
          </div>
        )}
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function MedicalRecords() {
  const [patients, setPatients] = useState([]);
  const [selectedPatient, setSelectedPatient] = useState("");
  const [list, setList] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [viewMode, setViewMode] = useState("cards");

  useEffect(() => {
    loadPatients();
  }, []);

  const loadPatients = async () => {
    try {
      const data = await getAllPatients();
      setPatients(data || []);
    } catch (error) {
      console.error("Помилка завантаження пацієнтів:", error);
    }
  };

  const load = async () => {
    if (!selectedPatient) {
      setList([]);
      return;
    }

    try {
      const data = await getMedicalRecordsByPatient(selectedPatient);
      setList(data || []);
    } catch (error) {
      console.error("Помилка завантаження записів:", error);
      alert("Помилка завантаження медичних записів");
    }
  };

  useEffect(() => {
    if (selectedPatient) {
      load();
    }
  }, [selectedPatient]);

  const submit = async (e) => {
    e.preventDefault();

    const dto = {
      allergies: form.allergies || "",
      medications: form.medications || "",
      conditions: form.conditions || "",
    };

    try {
      if (edit) {
        await updateMedicalRecord(form.id, { ...dto, id: form.id });
      } else {
        await createMedicalRecord({ ...dto, patientId: selectedPatient });
      }

      setForm(empty);
      setEdit(false);
      setShowModal(false);
      load();
    } catch (error) {
      console.error("Помилка збереження:", error);
      alert(
        "Помилка збереження: " +
          (error.response?.data?.message ||
            error.message ||
            "Невідома помилка"),
      );
    }
  };

  const editRow = (r) => {
    setForm({
      id: r.id,
      allergies: r.allergies || "",
      medications: r.medications || "",
      conditions: r.conditions || "",
    });
    setEdit(true);
    setShowModal(true);
  };

  const remove = async (id) => {
    if (!confirm("Видалити медичний запис?")) return;

    try {
      await deleteMedicalRecord(id);
      setList((x) => x.filter((r) => r.id !== id));
    } catch (error) {
      console.error("Помилка видалення:", error);
      alert("Помилка видалення запису");
    }
  };

  const selectedPatientData = patients.find((p) => p.id === selectedPatient);

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Медичні записи</h1>
            <p className="page-subtitle">
              {selectedPatientData
                ? `Пацієнт: ${selectedPatientData.firstName} ${selectedPatientData.lastName}`
                : "Оберіть пацієнта для перегляду медичних записів"}
            </p>
          </div>
          {selectedPatient && (
            <button
              className="btn-primary"
              onClick={() => {
                setForm(empty);
                setEdit(false);
                setShowModal(true);
              }}
            >
              + Додати запис
            </button>
          )}
        </div>

        {/* Patient Selector */}
        <div className="patient-selector-container">
          <label className="selector-label">Пацієнт:</label>
          <select
            className="patient-selector"
            value={selectedPatient}
            onChange={(e) => setSelectedPatient(e.target.value)}
          >
            <option value="">Оберіть пацієнта...</option>
            {patients.map((p) => (
              <option key={p.id} value={p.id}>
                {p.firstName} {p.lastName}
              </option>
            ))}
          </select>
        </div>

        {selectedPatient ? (
          <>
            {/* View Toggle */}
            {list.length > 0 && (
              <div className="controls">
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
            )}

            {/* Data Display */}
            {list.length === 0 ? (
              <div className="empty-state">
                <div className="empty-icon">📋</div>
                <h3>Медичних записів не знайдено</h3>
                <p>Додайте перший медичний запис для цього пацієнта</p>
                <button
                  className="btn-primary"
                  onClick={() => {
                    setForm(empty);
                    setEdit(false);
                    setShowModal(true);
                  }}
                >
                  + Додати запис
                </button>
              </div>
            ) : viewMode === "cards" ? (
              <div className="records-grid">
                {list.map((r) => (
                  <RecordCard
                    key={r.id}
                    record={r}
                    onEdit={editRow}
                    onDelete={remove}
                  />
                ))}
              </div>
            ) : (
              <div className="table-container">
                <table className="records-table">
                  <thead>
                    <tr>
                      <th>Алергії</th>
                      <th>Медикаменти</th>
                      <th>Захворювання</th>
                      <th>Дії</th>
                    </tr>
                  </thead>
                  <tbody>
                    {list.map((r) => (
                      <tr key={r.id}>
                        <td>{r.allergies || "—"}</td>
                        <td>{r.medications || "—"}</td>
                        <td>{r.conditions || "—"}</td>
                        <td>
                          <div className="table-actions">
                            <button
                              className="btn-edit-sm"
                              onClick={() => editRow(r)}
                            >
                              ✏️
                            </button>
                            <button
                              className="btn-delete-sm"
                              onClick={() => remove(r.id)}
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
          </>
        ) : (
          <div className="empty-state">
            <div className="empty-icon">👤</div>
            <h3>Оберіть пацієнта</h3>
            <p>Виберіть пацієнта зі списку для перегляду медичних записів</p>
          </div>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="medical-form" onSubmit={submit}>
              <h2>{edit ? "Редагувати запис" : "Новий медичний запис"}</h2>

              <div className="form-group">
                <label>🚨 Алергії</label>
                <textarea
                  rows="3"
                  placeholder="Опишіть алергії пацієнта..."
                  value={form.allergies}
                  onChange={(e) =>
                    setForm({ ...form, allergies: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>💊 Медикаменти</label>
                <textarea
                  rows="3"
                  placeholder="Перелічіть медикаменти, які приймає пацієнт..."
                  value={form.medications}
                  onChange={(e) =>
                    setForm({ ...form, medications: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>🩺 Захворювання</label>
                <textarea
                  rows="3"
                  placeholder="Опишіть хронічні захворювання та стан здоров'я..."
                  value={form.conditions}
                  onChange={(e) =>
                    setForm({ ...form, conditions: e.target.value })
                  }
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {edit ? "Зберегти зміни" : "Створити запис"}
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
