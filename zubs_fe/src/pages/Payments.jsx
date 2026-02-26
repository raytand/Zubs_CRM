import { useState, useEffect } from "react";
import {
  getAllPayments,
  createPayment,
  updatePayment,
  deletePayment,
} from "../services/payments";
import { getAllPatients } from "../services/patients";
import { getAllAppointments } from "../services/appointments";
import "./styles/Payments.css";

const empty = {
  id: null,
  patientId: "",
  appointmentId: "",
  amount: 0,
  paidAt: new Date().toISOString().slice(0, 10),
  method: "Cash",
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= PAYMENT CARD =================
const PaymentCard = ({ payment, patients, appointments, onEdit, onDelete }) => {
  const patient = patients.find((p) => p.id === payment.patientId);
  const appointment = appointments.find((a) => a.id === payment.appointmentId);

  const formatPrice = (price) => {
    return new Intl.NumberFormat("uk-UA", {
      style: "currency",
      currency: "UAH",
      minimumFractionDigits: 2,
    }).format(price);
  };

  const getMethodIcon = (method) => {
    switch (method) {
      case 0:
        return "💵";
      case 1:
        return "💳";
      case 2:
        return "🌐";
      default:
        return "💰";
    }
  };

  const getMethodLabel = (method) => {
    switch (method) {
      case 0:
        return "Готівка";
      case 1:
        return "Картка";
      case 2:
        return "Онлайн";
      default:
        return method;
    }
  };

  return (
    <div className="payment-card">
      <div className="payment-header">
        <div className="payment-method-badge">
          <span className="method-icon">{getMethodIcon(payment.method)}</span>
          <span className="method-label">{getMethodLabel(payment.method)}</span>
        </div>
        <div className="payment-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(payment)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(payment.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      <div className="payment-amount">{formatPrice(payment.amount)}</div>

      <div className="payment-details">
        <div className="detail-row">
          <span className="detail-icon">👤</span>
          <span className="detail-value">
            {patient
              ? `${patient.firstName} ${patient.lastName}`
              : `ID: ${payment.patientId}`}
          </span>
        </div>

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
            {new Date(payment.paidAt).toLocaleDateString("uk-UA", {
              day: "2-digit",
              month: "2-digit",
              year: "numeric",
            })}
          </span>
        </div>
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function Payments() {
  const [list, setList] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [patients, setPatients] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterMethod, setFilterMethod] = useState("all");
  const [viewMode, setViewMode] = useState("cards");

  const load = async () => {
    try {
      const [paymentsData, patientsData, appointmentsData] = await Promise.all([
        getAllPayments(),
        getAllPatients(),
        getAllAppointments(),
      ]);

      setList(paymentsData || []);
      setPatients(patientsData || []);
      setAppointments(appointmentsData || []);

      console.log("Loaded data:", {
        payments: paymentsData?.length,
        patients: patientsData?.length,
        appointments: appointmentsData?.length,
      });
    } catch (error) {
      console.error("Error loading data:", error);
      alert("Помилка завантаження даних");
    }
  };

  useEffect(() => {
    load();
  }, []);

  const paymentMethodMap = {
    Cash: 0,
    Card: 1,
    Online: 2,
  };

  const submit = async (e) => {
    e.preventDefault();

    const dto = {
      patientId: form.patientId || null,
      appointmentId: form.appointmentId || null,
      amount: parseFloat(form.amount) || 0,
      paidAt: form.paidAt,
      method: paymentMethodMap[form.method],
    };

    try {
      if (edit) {
        await updatePayment(form.id, dto);
      } else {
        await createPayment(dto);
      }
      setForm(empty);
      setEdit(false);
      setShowModal(false);
      load();
    } catch (error) {
      console.error("Save error:", error);
      const errorMessage = error.response?.data?.errors
        ? Object.values(error.response.data.errors).flat().join(", ")
        : error.response?.data?.message || error.message || "Невідома помилка";
      alert("Помилка збереження: " + errorMessage);
    }
  };

  const editRow = (p) => {
    setForm({
      ...p,
      paidAt: p.paidAt?.slice(0, 10) || new Date().toISOString().slice(0, 10),
      // Keep IDs as they are (GUIDs are already strings)
      patientId: p.patientId || "",
      appointmentId: p.appointmentId || "",
    });
    setEdit(true);
    setShowModal(true);
  };

  const remove = async (id) => {
    if (!confirm("Видалити платіж?")) return;
    await deletePayment(id);
    setList(list.filter((x) => x.id !== id));
  };

  // Filter payments
  const filteredList = list.filter((p) => {
    // Filter by search term
    const patient = patients.find((pat) => pat.id === p.patientId);
    const matchesSearch = patient
      ? `${patient.firstName} ${patient.lastName}`
          .toLowerCase()
          .includes(searchTerm.toLowerCase())
      : true;

    // Filter by payment method
    const matchesMethod = filterMethod === "all" || p.method === filterMethod;

    return matchesSearch && matchesMethod;
  });

  // Sort by date (newest first)
  const sortedList = [...filteredList].sort(
    (a, b) => new Date(b.paidAt) - new Date(a.paidAt),
  );

  // Calculate statistics
  const stats = {
    total: list.reduce((sum, p) => sum + p.amount, 0),
    count: list.length,
    avgPayment:
      list.length > 0
        ? list.reduce((sum, p) => sum + p.amount, 0) / list.length
        : 0,
    todayTotal: list
      .filter(
        (p) => new Date(p.paidAt).toDateString() === new Date().toDateString(),
      )
      .reduce((sum, p) => sum + p.amount, 0),
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat("uk-UA", {
      style: "currency",
      currency: "UAH",
      minimumFractionDigits: 2,
    }).format(price);
  };

  // Get patient appointments for selector
  const getPatientAppointments = () => {
    if (!form.patientId) return [];
    // Compare as strings since IDs are GUIDs
    return appointments.filter((a) => a.patientId === form.patientId);
  };

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Платежі</h1>
            <p className="page-subtitle">
              Всього: {formatPrice(stats.total)} • Платежів: {stats.count} •
              Сьогодні: {formatPrice(stats.todayTotal)}
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
            + Додати платіж
          </button>
        </div>

        {/* Controls */}
        <div className="controls">
          <div className="search-box">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Пошук за пацієнтом..."
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
            <label>Метод:</label>
            <select
              value={filterMethod}
              onChange={(e) => setFilterMethod(e.target.value)}
            >
              <option value="all">Всі</option>
              <option value="Cash">Готівка</option>
              <option value="Card">Картка</option>
              <option value="Online">Онлайн</option>
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
            Знайдено: {sortedList.length} з {list.length}
          </p>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="payment-form" onSubmit={submit}>
              <h2>{edit ? "Редагувати платіж" : "Новий платіж"}</h2>

              <div className="form-group">
                <label>Пацієнт *</label>
                <select
                  required
                  value={form.patientId}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      patientId: e.target.value,
                      appointmentId: "",
                    })
                  }
                >
                  <option value="">Оберіть пацієнта</option>
                  {patients.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.firstName} {p.lastName}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>
                  Запис (опціонально)
                  {form.patientId && (
                    <span className="label-hint">
                      {" "}
                      • {getPatientAppointments().length} доступних
                    </span>
                  )}
                </label>
                <select
                  value={form.appointmentId}
                  onChange={(e) =>
                    setForm({ ...form, appointmentId: e.target.value })
                  }
                  disabled={!form.patientId}
                >
                  <option value="">
                    {!form.patientId
                      ? "Спочатку оберіть пацієнта"
                      : "Без прив'язки до запису"}
                  </option>
                  {getPatientAppointments().map((a) => (
                    <option key={a.id} value={a.id}>
                      {new Date(a.startTime).toLocaleDateString("uk-UA", {
                        day: "2-digit",
                        month: "2-digit",
                        year: "numeric",
                      })}{" "}
                      -{" "}
                      {new Date(a.startTime).toLocaleTimeString("uk-UA", {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Сума (грн) *</label>
                  <input
                    required
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    value={form.amount}
                    onChange={(e) =>
                      setForm({ ...form, amount: e.target.value })
                    }
                  />
                </div>

                <div className="form-group">
                  <label>Дата оплати *</label>
                  <input
                    required
                    type="date"
                    value={form.paidAt}
                    onChange={(e) =>
                      setForm({ ...form, paidAt: e.target.value })
                    }
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Спосіб оплати *</label>
                <select
                  required
                  value={form.method}
                  onChange={(e) => setForm({ ...form, method: e.target.value })}
                >
                  <option value="Cash">💵 Готівка</option>
                  <option value="Card">💳 Картка</option>
                  <option value="Online">🌐 Онлайн</option>
                </select>
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {edit ? "Зберегти зміни" : "Створити платіж"}
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
        {sortedList.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">💰</div>
            <h3>Платежів не знайдено</h3>
            <p>
              {searchTerm
                ? "Спробуйте змінити критерії пошуку"
                : "Додайте перший платіж для початку роботи"}
            </p>
          </div>
        ) : viewMode === "cards" ? (
          <div className="payments-grid">
            {sortedList.map((p) => (
              <PaymentCard
                key={p.id}
                payment={p}
                patients={patients}
                appointments={appointments}
                onEdit={editRow}
                onDelete={remove}
              />
            ))}
          </div>
        ) : (
          <div className="table-container">
            <table className="payments-table">
              <thead>
                <tr>
                  <th>Пацієнт</th>
                  <th>Запис</th>
                  <th>Сума</th>
                  <th>Дата оплати</th>
                  <th>Спосіб</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {sortedList.map((p) => {
                  const patient = patients.find(
                    (pat) => pat.id === p.patientId,
                  );
                  const appointment = appointments.find(
                    (a) => a.id === p.appointmentId,
                  );

                  return (
                    <tr key={p.id}>
                      <td>
                        <strong>
                          {patient
                            ? `${patient.firstName} ${patient.lastName}`
                            : `ID: ${p.patientId}`}
                        </strong>
                      </td>
                      <td>
                        {appointment ? (
                          <>
                            {new Date(appointment.startTime).toLocaleDateString(
                              "uk-UA",
                            )}
                            <div className="text-small">
                              {new Date(
                                appointment.startTime,
                              ).toLocaleTimeString("uk-UA", {
                                hour: "2-digit",
                                minute: "2-digit",
                              })}
                            </div>
                          </>
                        ) : (
                          "—"
                        )}
                      </td>
                      <td>
                        <strong className="price-highlight">
                          {formatPrice(p.amount)}
                        </strong>
                      </td>
                      <td>
                        {new Date(p.paidAt).toLocaleDateString("uk-UA", {
                          day: "2-digit",
                          month: "2-digit",
                          year: "numeric",
                        })}
                      </td>
                      <td>
                        <span className="method-badge method-{p.method.toLowerCase()}">
                          {p.method === "Cash"
                            ? "💵 Готівка"
                            : p.method === "Card"
                              ? "💳 Картка"
                              : "🌐 Онлайн"}
                        </span>
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
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
