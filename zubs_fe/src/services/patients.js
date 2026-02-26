import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET api/patients
export const getAllPatients = () => fetchGet("/patients");

// GET api/patients/{id}
export const getPatientById = (id) => fetchGet(`/patients/${id}`);

// POST api/patients
export const createPatient = (dto) => fetchPost("/patients", dto);

// PUT api/patients/{id}
export const updatePatient = (id, dto) => fetchPut(`/patients/${id}`, dto);

// DELETE api/patients/{id}
export const deletePatient = (id) => fetchDelete(`/patients/${id}`);
