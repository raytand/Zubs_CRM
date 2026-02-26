import { useEffect, useState } from "react";
import {
  getAllAppointments,
  createAppointment,
  updateAppointment,
  deleteAppointment,
} from "../services/appointments";
import { getAllPatients } from "../services/patients";
import { getAllDoctors } from "../services/doctors";
import { getAllServices } from "../services/services";

import "./styles/Appointments.css";

const empty = {
  id: null,
  patientId: "",
  doctorId: "",
  serviceId: "",
  startTime: "",
  endTime: "",
  status: 0,
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

// ================= MAIN COMPONENT =================
export default function Appointments() {
  const [appointments, setAppointments] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [patients, setPatients] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [services, setServices] = useState([]);
  const [startDate, setStartDate] = useState(
    new Date().toISOString().slice(0, 10),
  );
  const [showForm, setShowForm] = useState(false);

  // ================= LOAD DATA =================
  useEffect(() => {
    const loadData = async () => {
      setAppointments(await getAllAppointments());
      setPatients(await getAllPatients());
      setDoctors(await getAllDoctors());
      setServices(await getAllServices());
    };
    loadData();
  }, []);

  // ================= TIMEZONE UTILS =================
  // Convert local datetime-local input to UTC ISO string
  const toUtc = (localDatetimeString) => {
    if (!localDatetimeString) return null;
    // datetime-local gives us: "2024-02-08T14:30"
    // We need to interpret this as Kyiv time and convert to UTC
    const local = new Date(localDatetimeString);
    return local.toISOString();
  };

  // Convert UTC ISO string to local datetime-local format
  const toLocalDatetime = (utcString) => {
    if (!utcString) return "";
    const date = new Date(utcString);
    // Format: "2024-02-08T14:30"
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  // Get local date string from UTC
  const toLocalDate = (utcString) => {
    if (!utcString) return null;
    const date = new Date(utcString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  const submit = async (e) => {
    e.preventDefault();
    if (!form.startTime || !form.endTime) return alert("Select time");

    const dto = {
      ...form,
      patientId: form.patientId || null,
      doctorId: form.doctorId || null,
      serviceId: form.serviceId || null,
      startTime: toUtc(form.startTime),
      endTime: toUtc(form.endTime),
    };

    try {
      if (edit) await updateAppointment(form.id, dto);
      else await createAppointment(dto);

      setForm(empty);
      setEdit(false);
      setShowForm(false);

      const updatedList = await getAllAppointments();
      setAppointments(updatedList);
    } catch {
      alert("Save failed");
    }
  };

  const editRow = (a) => {
    setForm({
      ...a,
      startTime: toLocalDatetime(a.startTime),
      endTime: toLocalDatetime(a.endTime),
    });
    setEdit(true);
    setShowForm(true);
  };

  const deleteRow = async (id) => {
    await deleteAppointment(id);
    setAppointments(appointments.filter((a) => a.id !== id));
  };

  const patientMap = Object.fromEntries(
    patients.map((p) => [p.id, `${p.firstName} ${p.lastName}`]),
  );
  const doctorMap = Object.fromEntries(
    doctors.map((d) => [d.id, `${d.firstName} ${d.lastName}`]),
  );
  const serviceMap = Object.fromEntries(services.map((s) => [s.id, s.name]));
  const statusMap = ["Заплановано", "Виконано", "Скасовано"];

  // ================= GENERATE 4 DAYS =================
  const generateDays = () => {
    const days = [];
    const start = new Date(startDate);

    for (let i = 0; i < 4; i++) {
      const date = new Date(start);
      date.setDate(start.getDate() + i);
      const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;

      const dayAppointments = appointments
        .filter((a) => toLocalDate(a.startTime) === dateStr)
        .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

      days.push({
        date: dateStr,
        dateObj: date,
        dayName: date.toLocaleDateString("uk-UA", { weekday: "long" }),
        dayNumber: date.getDate(),
        monthName: date.toLocaleDateString("uk-UA", { month: "long" }),
        appointments: dayAppointments,
      });
    }

    return days;
  };

  const days = generateDays();

  const goToPrevious = () => {
    const newDate = new Date(startDate);
    newDate.setDate(newDate.getDate() - 4);
    const year = newDate.getFullYear();
    const month = String(newDate.getMonth() + 1).padStart(2, "0");
    const day = String(newDate.getDate()).padStart(2, "0");
    setStartDate(`${year}-${month}-${day}`);
  };

  const goToNext = () => {
    const newDate = new Date(startDate);
    newDate.setDate(newDate.getDate() + 4);
    const year = newDate.getFullYear();
    const month = String(newDate.getMonth() + 1).padStart(2, "0");
    const day = String(newDate.getDate()).padStart(2, "0");
    setStartDate(`${year}-${month}-${day}`);
  };

  const goToToday = () => {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, "0");
    const day = String(today.getDate()).padStart(2, "0");
    setStartDate(`${year}-${month}-${day}`);
  };

  // ================= TIME GRID SETUP =================
  const dayStart = 8; // 8:00
  const dayEnd = 20; // 20:00
  const hourHeight = 60; // pixels per hour
  const totalHeight = (dayEnd - dayStart) * hourHeight;

  const timeToPixels = (utcTimeString) => {
    const date = new Date(utcTimeString);
    const hours = date.getHours();
    const minutes = date.getMinutes();

    // If appointment is outside visible hours, clamp it
    if (hours < dayStart) return 0;
    if (hours >= dayEnd) return totalHeight - 10; // Show at bottom

    const minutesSinceDayStart = (hours - dayStart) * 60 + minutes;
    return (minutesSinceDayStart / 60) * hourHeight;
  };

  const durationPixels = (startUtc, endUtc) => {
    const start = new Date(startUtc);
    const end = new Date(endUtc);
    const durationMinutes = (end - start) / (1000 * 60);
    return Math.max((durationMinutes / 60) * hourHeight, 30); // Minimum 30px
  };

  // ================= OVERLAP DETECTION =================
  const calculateAppointmentPositions = (appointments) => {
    const positioned = [];

    appointments.forEach((apt) => {
      const aptStart = new Date(apt.startTime);
      const aptEnd = new Date(apt.endTime);

      const overlapping = positioned.filter((p) => {
        const pStart = new Date(p.startTime);
        const pEnd = new Date(p.endTime);
        return aptStart < pEnd && aptEnd > pStart;
      });

      const usedColumns = overlapping.map((p) => p.column);
      let column = 0;
      while (usedColumns.includes(column)) column++;

      const maxColumns = Math.max(
        column + 1,
        ...overlapping.map((p) => p.maxColumns),
      );

      overlapping.forEach((p) => (p.maxColumns = maxColumns));

      positioned.push({
        ...apt,
        column,
        maxColumns,
      });
    });

    return positioned;
  };

  return (
    <div className="page">
      <div className="content">
        <h1>Записи</h1>

        <div className="actions">
          <button
            onClick={() => {
              setForm(empty);
              setEdit(false);
              setShowForm(true);
            }}
          >
            + Створити запис
          </button>

          <div className="date-navigation">
            <button onClick={goToPrevious}>◀ Назад</button>
            <button onClick={goToToday}>Сьогодні</button>
            <button onClick={goToNext}>Вперед ▶</button>
          </div>
        </div>

        {/* ================= MODAL FORM ================= */}
        {showForm && (
          <Modal onClose={() => setShowForm(false)}>
            <form className="form" onSubmit={submit}>
              <h2>{edit ? "Редагувати запис" : "Створити запис"}</h2>

              <select
                value={form.patientId}
                onChange={(e) =>
                  setForm({ ...form, patientId: e.target.value })
                }
              >
                <option value="">Пацієнт</option>
                {patients.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.firstName} {p.lastName}
                  </option>
                ))}
              </select>

              <select
                value={form.doctorId}
                onChange={(e) => setForm({ ...form, doctorId: e.target.value })}
              >
                <option value="">Лікар</option>
                {doctors.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.firstName} {d.lastName}
                  </option>
                ))}
              </select>

              <select
                value={form.serviceId}
                onChange={(e) =>
                  setForm({ ...form, serviceId: e.target.value })
                }
              >
                <option value="">Послуга</option>
                {services.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name}
                  </option>
                ))}
              </select>

              <input
                type="datetime-local"
                value={form.startTime}
                onChange={(e) =>
                  setForm({ ...form, startTime: e.target.value })
                }
              />
              <input
                type="datetime-local"
                value={form.endTime}
                onChange={(e) => setForm({ ...form, endTime: e.target.value })}
              />

              <select
                value={form.status}
                onChange={(e) => setForm({ ...form, status: +e.target.value })}
              >
                <option value={0}>Заплановано</option>
                <option value={1}>Виконано</option>
                <option value={2}>Скасовано</option>
              </select>

              <textarea
                placeholder="Нотатки"
                value={form.notes}
                onChange={(e) => setForm({ ...form, notes: e.target.value })}
              />

              <div className="form-actions">
                <button type="submit">{edit ? "Оновити" : "Створити"}</button>
                <button type="button" onClick={() => setShowForm(false)}>
                  Закрити
                </button>
              </div>
            </form>
          </Modal>
        )}

        {/* ================= 4-DAY GRID WITH TIME ================= */}
        <div className="four-day-container">
          {/* Time column */}
          <div className="time-sidebar">
            <div className="time-header">Час</div>
            <div className="time-labels" style={{ height: `${totalHeight}px` }}>
              {Array.from({ length: dayEnd - dayStart + 1 }, (_, i) => (
                <div
                  key={i}
                  className="time-label"
                  style={{ top: `${i * hourHeight}px` }}
                >
                  {String(dayStart + i).padStart(2, "0")}:00
                </div>
              ))}
            </div>
          </div>

          {/* Day columns */}
          <div className="days-grid">
            {days.map((day) => {
              const positionedAppointments = calculateAppointmentPositions(
                day.appointments,
              );

              return (
                <div key={day.date} className="day-column">
                  <div className="day-header">
                    <div className="day-name">{day.dayName}</div>
                    <div className="day-date">
                      {day.dayNumber} {day.monthName}
                    </div>
                    <div className="day-count">
                      {day.appointments.length}{" "}
                      {day.appointments.length === 1 ? "запис" : "записів"}
                    </div>
                  </div>

                  <div
                    className="day-timeline"
                    style={{ height: `${totalHeight}px` }}
                  >
                    {/* Hour grid lines */}
                    {Array.from({ length: dayEnd - dayStart + 1 }, (_, i) => (
                      <div
                        key={`grid-${i}`}
                        className="grid-line"
                        style={{ top: `${i * hourHeight}px` }}
                      />
                    ))}

                    {/* Appointments */}
                    {positionedAppointments.map((a) => {
                      const top = timeToPixels(a.startTime);
                      const height = durationPixels(a.startTime, a.endTime);
                      const width =
                        a.maxColumns > 1
                          ? `calc(${100 / a.maxColumns}% - 4px)`
                          : "calc(100% - 8px)";
                      const left =
                        a.maxColumns > 1
                          ? `calc(${(a.column * 100) / a.maxColumns}% + 2px)`
                          : "4px";

                      return (
                        <div
                          key={a.id}
                          className={`appointment-card status-${a.status}`}
                          style={{
                            top: `${top}px`,
                            height: `${height}px`,
                            left: left,
                            width: width,
                          }}
                          onClick={() => editRow(a)}
                        >
                          <div className="apt-time">
                            {new Date(a.startTime).toLocaleTimeString([], {
                              hour: "2-digit",
                              minute: "2-digit",
                            })}
                            {" - "}
                            {new Date(a.endTime).toLocaleTimeString([], {
                              hour: "2-digit",
                              minute: "2-digit",
                            })}
                          </div>

                          <div className="apt-patient">
                            <strong>{patientMap[a.patientId] || "—"}</strong>
                          </div>

                          {doctorMap[a.doctorId] && (
                            <div className="apt-doctor">
                              👨‍⚕️ {doctorMap[a.doctorId]}
                            </div>
                          )}

                          {serviceMap[a.serviceId] && (
                            <div className="apt-service">
                              📋 {serviceMap[a.serviceId]}
                            </div>
                          )}

                          {a.notes && (
                            <div className="apt-notes">{a.notes}</div>
                          )}

                          <div className="apt-actions">
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                editRow(a);
                              }}
                              title="Редагувати"
                            >
                              ✏️
                            </button>
                            <button
                              className="danger"
                              onClick={(e) => {
                                e.stopPropagation();
                                deleteRow(a.id);
                              }}
                              title="Видалити"
                            >
                              🗑️
                            </button>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
