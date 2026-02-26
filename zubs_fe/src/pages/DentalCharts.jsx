import { useEffect, useState } from "react";
import { getAllPatients } from "../services/patients";
import {
  getDentalChartsByPatient,
  createDentalChart,
  updateDentalChart,
  deleteDentalChart,
} from "../services/dentalCharts";
import "./styles/DentalCharts.css";

const emptyForm = { toothNumber: "", status: "", notes: "" };

// Tooth status options
const STATUSES = [
  { value: "Healthy", label: "Здоровий", color: "#28a745" },
  { value: "Cavity", label: "Карієс", color: "#ffc107" },
  { value: "Filled", label: "Запломбований", color: "#17a2b8" },
  { value: "Crown", label: "Коронка", color: "#6f42c1" },
  { value: "Missing", label: "Відсутній", color: "#dc3545" },
  { value: "Root Canal", label: "Канал", color: "#fd7e14" },
  { value: "Implant", label: "Імплант", color: "#20c997" },
];

// Standard teeth numbering (adult: 1-32)
const TEETH_NUMBERS = {
  upper: {
    right: [18, 17, 16, 15, 14, 13, 12, 11],
    left: [21, 22, 23, 24, 25, 26, 27, 28],
  },
  lower: {
    left: [48, 47, 46, 45, 44, 43, 42, 41],
    right: [31, 32, 33, 34, 35, 36, 37, 38],
  },
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= TOOTH DIAGRAM =================
const ToothDiagram = ({ charts, onToothClick }) => {
  const getToothStatus = (toothNumber) => {
    const chart = charts.find((c) => c.toothNumber === toothNumber.toString());
    return chart ? chart.status : null;
  };

  const getToothColor = (status) => {
    const statusObj = STATUSES.find((s) => s.value === status);
    return statusObj ? statusObj.color : "#e1ecf7";
  };

  const Tooth = ({ number, status }) => (
    <div
      className="tooth"
      style={{ backgroundColor: getToothColor(status) }}
      onClick={() => onToothClick(number)}
      title={`Зуб ${number}${status ? ` - ${STATUSES.find((s) => s.value === status)?.label}` : ""}`}
    >
      {number}
    </div>
  );

  return (
    <div className="tooth-diagram">
      {/* Upper jaw */}
      <div className="jaw upper-jaw">
        <div className="jaw-side right">
          {TEETH_NUMBERS.upper.right.map((num) => (
            <Tooth key={num} number={num} status={getToothStatus(num)} />
          ))}
        </div>
        <div className="jaw-divider"></div>
        <div className="jaw-side left">
          {TEETH_NUMBERS.upper.left.map((num) => (
            <Tooth key={num} number={num} status={getToothStatus(num)} />
          ))}
        </div>
      </div>

      {/* Lower jaw */}
      <div className="jaw lower-jaw">
        <div className="jaw-side right">
          {TEETH_NUMBERS.lower.right.map((num) => (
            <Tooth key={num} number={num} status={getToothStatus(num)} />
          ))}
        </div>
        <div className="jaw-divider"></div>
        <div className="jaw-side left">
          {TEETH_NUMBERS.lower.left.map((num) => (
            <Tooth key={num} number={num} status={getToothStatus(num)} />
          ))}
        </div>
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function DentalCharts() {
  const [patients, setPatients] = useState([]);
  const [selectedPatient, setSelectedPatient] = useState("");
  const [charts, setCharts] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    loadPatients();
  }, []);

  const loadPatients = async () => {
    try {
      const data = await getAllPatients();
      setPatients(data || []);
    } catch (e) {
      console.error("Error loading patients:", e);
    }
  };

  const loadCharts = async (patientId) => {
    if (!patientId) {
      setCharts([]);
      return;
    }

    try {
      const data = await getDentalChartsByPatient(patientId);
      setCharts(data || []);
    } catch (e) {
      console.error("Error loading charts:", e);
      setError("Помилка завантаження даних");
    }
  };

  useEffect(() => {
    if (selectedPatient) {
      loadCharts(selectedPatient);
    }
  }, [selectedPatient]);

  const submitForm = async (e) => {
    e.preventDefault();
    setError("");

    try {
      const dto = {
        toothNumber: form.toothNumber,
        status: form.status,
        notes: form.notes,
        patientId: selectedPatient,
      };

      if (editingId) {
        await updateDentalChart({ ...dto, id: editingId });
      } else {
        await createDentalChart(dto);
      }

      setForm(emptyForm);
      setEditingId(null);
      setShowModal(false);
      loadCharts(selectedPatient);
    } catch (err) {
      console.error("Save error:", err);
      setError(
        err?.response?.data?.message || err?.title || "Помилка збереження",
      );
    }
  };

  const startEdit = (chart) => {
    setForm({
      toothNumber: chart.toothNumber,
      status: chart.status || "",
      notes: chart.notes || "",
    });
    setEditingId(chart.id);
    setShowModal(true);
  };

  const handleToothClick = (toothNumber) => {
    const existingChart = charts.find(
      (c) => c.toothNumber === toothNumber.toString(),
    );

    if (existingChart) {
      startEdit(existingChart);
    } else {
      setForm({ ...emptyForm, toothNumber: toothNumber.toString() });
      setEditingId(null);
      setShowModal(true);
    }
  };

  const removeChart = async (id) => {
    if (!confirm("Видалити запис?")) return;

    try {
      await deleteDentalChart(id);
      loadCharts(selectedPatient);
    } catch (err) {
      console.error("Delete error:", err);
      setError("Помилка видалення");
    }
  };

  const selectedPatientData = patients.find((p) => p.id === selectedPatient);

  // Get status statistics
  const statusStats = STATUSES.map((status) => ({
    ...status,
    count: charts.filter((c) => c.status === status.value).length,
  })).filter((s) => s.count > 0);

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Зубні карти</h1>
            <p className="page-subtitle">
              {selectedPatientData
                ? `Пацієнт: ${selectedPatientData.firstName} ${selectedPatientData.lastName}`
                : "Оберіть пацієнта для перегляду карти"}
            </p>
          </div>
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

        {error && (
          <div className="alert alert-error">
            <span className="alert-icon">⚠️</span>
            <span>{error}</span>
          </div>
        )}

        {selectedPatient ? (
          <>
            {/* Statistics */}
            {statusStats.length > 0 && (
              <div className="status-stats">
                {statusStats.map((stat) => (
                  <div
                    key={stat.value}
                    className="stat-item"
                    style={{ borderLeftColor: stat.color }}
                  >
                    <span className="stat-label">{stat.label}</span>
                    <span className="stat-count">{stat.count}</span>
                  </div>
                ))}
              </div>
            )}

            {/* Tooth Diagram */}
            <div className="diagram-container">
              <h2>Зубна формула</h2>
              <p className="diagram-hint">
                Клацніть на зуб для додавання або редагування
              </p>
              <ToothDiagram charts={charts} onToothClick={handleToothClick} />
            </div>

            {/* Legend */}
            <div className="legend">
              <h3>Легенда:</h3>
              <div className="legend-items">
                {STATUSES.map((status) => (
                  <div key={status.value} className="legend-item">
                    <div
                      className="legend-color"
                      style={{ backgroundColor: status.color }}
                    ></div>
                    <span>{status.label}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Charts List */}
            {charts.length > 0 && (
              <div className="charts-list">
                <h2>Детальна інформація</h2>
                <div className="table-container">
                  <table className="charts-table">
                    <thead>
                      <tr>
                        <th>Зуб</th>
                        <th>Статус</th>
                        <th>Примітки</th>
                        <th>Дії</th>
                      </tr>
                    </thead>
                    <tbody>
                      {charts.map((c) => {
                        const statusObj = STATUSES.find(
                          (s) => s.value === c.status,
                        );
                        return (
                          <tr key={c.id}>
                            <td>
                              <strong>Зуб {c.toothNumber}</strong>
                            </td>
                            <td>
                              <span
                                className="status-badge"
                                style={{ backgroundColor: statusObj?.color }}
                              >
                                {statusObj?.label || c.status}
                              </span>
                            </td>
                            <td>{c.notes || "—"}</td>
                            <td>
                              <div className="table-actions">
                                <button
                                  className="btn-edit-sm"
                                  onClick={() => startEdit(c)}
                                >
                                  ✏️
                                </button>
                                <button
                                  className="btn-delete-sm"
                                  onClick={() => removeChart(c.id)}
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
              </div>
            )}
          </>
        ) : (
          <div className="empty-state">
            <div className="empty-icon">🦷</div>
            <h3>Оберіть пацієнта</h3>
            <p>Виберіть пацієнта зі списку для перегляду зубної карти</p>
          </div>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="dental-form" onSubmit={submitForm}>
              <h2>{editingId ? "Редагувати запис" : "Новий запис"}</h2>

              <div className="form-group">
                <label>Номер зуба *</label>
                <input
                  required
                  type="number"
                  min="1"
                  max="32"
                  placeholder="1-32"
                  value={form.toothNumber}
                  onChange={(e) =>
                    setForm({ ...form, toothNumber: e.target.value })
                  }
                  disabled={!!editingId}
                />
              </div>

              <div className="form-group">
                <label>Статус *</label>
                <select
                  required
                  value={form.status}
                  onChange={(e) => setForm({ ...form, status: e.target.value })}
                >
                  <option value="">Оберіть статус</option>
                  {STATUSES.map((status) => (
                    <option key={status.value} value={status.value}>
                      {status.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Примітки</label>
                <textarea
                  rows="3"
                  placeholder="Додаткова інформація..."
                  value={form.notes}
                  onChange={(e) => setForm({ ...form, notes: e.target.value })}
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {editingId ? "Зберегти зміни" : "Створити запис"}
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
