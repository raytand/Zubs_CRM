import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET /treatment-records/{id}
export const getTreatmentRecord = (id) => fetchGet(`/treatment-records/${id}`);

// GET /treatment-records/by-appointment/{appointmentId}
export const getTreatmentRecordsByAppointment = (appointmentId) =>
  fetchGet(`/treatment-records/by-appointment/${appointmentId}`);

// POST /treatment-records
export const createTreatmentRecord = (dto) =>
  fetchPost("/treatment-records", dto);

// PUT /treatment-records/{id}
export const updateTreatmentRecord = (id, dto) =>
  fetchPut(`/treatment-records/${id}`, dto);

// DELETE /treatment-records/{id}
export const deleteTreatmentRecord = (id) =>
  fetchDelete(`/treatment-records/${id}`);
