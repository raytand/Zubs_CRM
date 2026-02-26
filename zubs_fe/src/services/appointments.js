import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET api/appointments
export const getAllAppointments = () => fetchGet("/appointments");

// GET api/appointments/doctor/{id}
export const getAppointmentsByDoctor = (doctorId) =>
  fetchGet(`/appointments/doctor/${doctorId}`);

// GET api/appointments/patient/{id}
export const getAppointmentsByPatient = (patientId) =>
  fetchGet(`/appointments/patient/${patientId}`);

// POST api/appointments
export const createAppointment = (dto) => fetchPost("/appointments", dto);

// PUT api/appointments/{id}
export const updateAppointment = (id, dto) =>
  fetchPut(`/appointments/${id}`, dto);

// DELETE api/appointments/{id}
export const deleteAppointment = (id) => fetchDelete(`/appointments/${id}`);
