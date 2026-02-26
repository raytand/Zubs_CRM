import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET api/medical-records/patient/{patientId}
export const getMedicalRecordsByPatient = (patientId) =>
  fetchGet(`/medical-records/patient/${patientId}`);

// POST api/medical-records
export const createMedicalRecord = (dto) => fetchPost("/medical-records", dto);

// PUT api/medical-records/{id}
export const updateMedicalRecord = (id, dto) =>
  fetchPut(`/medical-records/${id}`, dto);

// DELETE api/medical-records/{id}
export const deleteMedicalRecord = (id) =>
  fetchDelete(`/medical-records/${id}`);
