import { useState, useEffect } from "react";
import { getAllAppointments } from "../services/appointments";
import { getAllServices } from "../services/services";
import { getAllPatients } from "../services/patients";
import {
  getTreatmentRecordsByAppointment,
  createTreatmentRecord,
  updateTreatmentRecord,
  deleteTreatmentRecord,
} from "../services/treatmentRecords";
import "./styles/TreatmentRecords.css";

const empty = {
  id: null,
  appointmentId: "",
  serviceId: "",
  notes: "",
  performedAt: new Date().toISOString().slice(0, 10),
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= TREATMENT CARD =================
const TreatmentCard = ({
  treatment,
  services,
  appointments,
  patients,
  onEdit,
  onDelete,
}) => {
  const service = services.find((s) => s.id === treatment.serviceId);
  const appointment = appointments.find(
    (a) => a.id === treatment.appointmentId,
  );
  const patient = appointment
    ? patients.find((p) => p.id === appointment.patientId)
    : null;

  const formatPrice = (price) => {
    return new Intl.NumberFormat("uk-UA", {
      style: "currency",
      currency: "UAH",
      minimumFractionDigits: 2,
    }).format(price);
  };

  return (
    <div className="treatment-card">
      <div className="treatment-header">
        <div className="treatment-icon">🦷</div>
        <div className="treatment-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(treatment)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(treatment.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      {service && <h3 className="service-name">{service.name}</h3>}

      <div className="treatment-details">
        {patient && (
          <div className="detail-row">
            <span className="detail-icon">👤</span>
            <span className="detail-value">
              {patient.firstName} {patient.lastName}
            </span>
          </div>
        )}

        {appointment && (
          <div className="detail-row">
            <span className="detail-icon">📅</span>
            <span className="detail-value">
              {new Date(appointment.startTime).toLocaleDateString("uk-UA", {
                day: "2-digit",
                month: "2-digit",
                year: "numeric",
              })}
            </span>
          </div>
        )}

        <div className="detail-row">
          <span className="detail-icon">🕐</span>
          <span className="detail-value">
            {new Date(treatment.performedAt).toLocaleDateString("uk-UA", {
              day: "2-digit",
              month: "2-digit",
              year: "numeric",
            })}
          </span>
        </div>

        {service && (
          <div className="detail-row">
            <span className="detail-icon">💰</span>
            <span className="detail-value price">
              {formatPrice(service.price)}
            </span>
          </div>
        )}
      </div>

      {treatment.notes && (
        <div className="treatment-notes">
          <strong>Примітки:</strong>
          <p>{treatment.notes}</p>
        </div>
      )}
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function TreatmentRecords() {
  const [appointments, setAppointments] = useState([]);
  const [services, setServices] = useState([]);
  const [patients, setPatients] = useState([]);
  const [selectedAppointment, setSelectedAppointment] = useState("");
  const [list, setList] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [viewMode, setViewMode] = useState("cards");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [appointmentsData, servicesData, patientsData] = await Promise.all([
        getAllAppointments(),
        getAllServices(),
        getAllPatients(),
      ]);

      setAppointments(appointmentsData || []);
      setServices(servicesData || []);
      setPatients(patientsData || []);
    } catch (error) {
      console.error("Помилка завантаження даних:", error);
    }
  };

  const load = async () => {
    if (!selectedAppointment) {
      setList([]);
      return;
    }

    try {
      const data = await getTreatmentRecordsByAppointment(selectedAppointment);
      setList(data || []);
    } catch (error) {
      console.error("Помилка завантаження записів:", error);
      alert("Помилка завантаження записів лікування");
    }
  };

  useEffect(() => {
    if (selectedAppointment) {
      load();
    }
  }, [selectedAppointment]);

  const submit = async (e) => {
    e.preventDefault();

    const dto = {
      appointmentId: form.appointmentId,
      serviceId: form.serviceId,
      notes: form.notes || "",
      performedAt: form.performedAt,
    };

    try {
      if (edit) {
        await updateTreatmentRecord(form.id, { ...dto, id: form.id });
      } else {
        await createTreatmentRecord(dto);
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
      ...r,
      performedAt:
        r.performedAt?.slice(0, 10) || new Date().toISOString().slice(0, 10),
    });
    setEdit(true);
    setShowModal(true);
  };

  const remove = async (id) => {
    if (!confirm("Видалити запис лікування?")) return;

    try {
      await deleteTreatmentRecord(id);
      setList(list.filter((x) => x.id !== id));
    } catch (error) {
      console.error("Помилка видалення:", error);
      alert("Помилка видалення запису");
    }
  };

  const selectedAppointmentData = appointments.find(
    (a) => a.id === selectedAppointment,
  );
  const selectedPatient = selectedAppointmentData
    ? patients.find((p) => p.id === selectedAppointmentData.patientId)
    : null;

  // Statistics
  const stats = {
    total: list.length,
    totalCost: list.reduce((sum, t) => {
      const service = services.find((s) => s.id === t.serviceId);
      return sum + (service?.price || 0);
    }, 0),
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat("uk-UA", {
      style: "currency",
      currency: "UAH",
      minimumFractionDigits: 2,
    }).format(price);
  };

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Записи лікування</h1>
            <p className="page-subtitle">
              {selectedPatient
                ? `Пацієнт: ${selectedPatient.firstName} ${selectedPatient.lastName} • Процедур: ${stats.total} • Вартість: ${formatPrice(stats.totalCost)}`
                : "Оберіть запис для перегляду лікування"}
            </p>
          </div>
          {selectedAppointment && (
            <button
              className="btn-primary"
              onClick={() => {
                setForm({ ...empty, appointmentId: selectedAppointment });
                setEdit(false);
                setShowModal(true);
              }}
            >
              + Додати процедуру
            </button>
          )}
        </div>

        {/* Appointment Selector */}
        <div className="patient-selector-container">
          <label className="selector-label">Запис:</label>
          <select
            className="patient-selector"
            value={selectedAppointment}
            onChange={(e) => setSelectedAppointment(e.target.value)}
          >
            <option value="">Оберіть запис...</option>
            {appointments.map((a) => {
              const patient = patients.find((p) => p.id === a.patientId);
              return (
                <option key={a.id} value={a.id}>
                  {patient
                    ? `${patient.firstName} ${patient.lastName}`
                    : "Пацієнт"}{" "}
                  - {new Date(a.startTime).toLocaleDateString("uk-UA")}{" "}
                  {new Date(a.startTime).toLocaleTimeString("uk-UA", {
                    hour: "2-digit",
                    minute: "2-digit",
                  })}
                </option>
              );
            })}
          </select>
        </div>

        {selectedAppointment ? (
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
                <div className="empty-icon">🦷</div>
                <h3>Записів лікування не знайдено</h3>
                <p>Додайте процедури, які були виконані під час цього візиту</p>
                <button
                  className="btn-primary"
                  onClick={() => {
                    setForm({ ...empty, appointmentId: selectedAppointment });
                    setEdit(false);
                    setShowModal(true);
                  }}
                >
                  + Додати процедуру
                </button>
              </div>
            ) : viewMode === "cards" ? (
              <div className="treatments-grid">
                {list.map((t) => (
                  <TreatmentCard
                    key={t.id}
                    treatment={t}
                    services={services}
                    appointments={appointments}
                    patients={patients}
                    onEdit={editRow}
                    onDelete={remove}
                  />
                ))}
              </div>
            ) : (
              <div className="table-container">
                <table className="treatments-table">
                  <thead>
                    <tr>
                      <th>Послуга</th>
                      <th>Примітки</th>
                      <th>Дата виконання</th>
                      <th>Вартість</th>
                      <th>Дії</th>
                    </tr>
                  </thead>
                  <tbody>
                    {list.map((t) => {
                      const service = services.find(
                        (s) => s.id === t.serviceId,
                      );
                      return (
                        <tr key={t.id}>
                          <td>
                            <strong>
                              {service?.name || `Service ID: ${t.serviceId}`}
                            </strong>
                          </td>
                          <td>{t.notes || "—"}</td>
                          <td>
                            {new Date(t.performedAt).toLocaleDateString(
                              "uk-UA",
                              {
                                day: "2-digit",
                                month: "2-digit",
                                year: "numeric",
                              },
                            )}
                          </td>
                          <td>
                            <strong className="price-highlight">
                              {service ? formatPrice(service.price) : "—"}
                            </strong>
                          </td>
                          <td>
                            <div className="table-actions">
                              <button
                                className="btn-edit-sm"
                                onClick={() => editRow(t)}
                              >
                                ✏️
                              </button>
                              <button
                                className="btn-delete-sm"
                                onClick={() => remove(t.id)}
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
          </>
        ) : (
          <div className="empty-state">
            <div className="empty-icon">📅</div>
            <h3>Оберіть запис</h3>
            <p>Виберіть запис зі списку для перегляду процедур лікування</p>
          </div>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="treatment-form" onSubmit={submit}>
              <h2>{edit ? "Редагувати процедуру" : "Нова процедура"}</h2>

              <div className="form-group">
                <label>Запис *</label>
                <select
                  required
                  value={form.appointmentId}
                  onChange={(e) =>
                    setForm({ ...form, appointmentId: e.target.value })
                  }
                  disabled={!!selectedAppointment && !edit}
                >
                  <option value="">Оберіть запис</option>
                  {appointments.map((a) => {
                    const patient = patients.find((p) => p.id === a.patientId);
                    return (
                      <option key={a.id} value={a.id}>
                        {patient
                          ? `${patient.firstName} ${patient.lastName}`
                          : "Пацієнт"}{" "}
                        - {new Date(a.startTime).toLocaleDateString("uk-UA")}
                      </option>
                    );
                  })}
                </select>
              </div>

              <div className="form-group">
                <label>Послуга *</label>
                <select
                  required
                  value={form.serviceId}
                  onChange={(e) =>
                    setForm({ ...form, serviceId: e.target.value })
                  }
                >
                  <option value="">Оберіть послугу</option>
                  {services.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name} - {formatPrice(s.price)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Дата виконання *</label>
                <input
                  required
                  type="date"
                  value={form.performedAt}
                  onChange={(e) =>
                    setForm({ ...form, performedAt: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Примітки</label>
                <textarea
                  rows="4"
                  placeholder="Опишіть деталі виконаної процедури..."
                  value={form.notes}
                  onChange={(e) => setForm({ ...form, notes: e.target.value })}
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
