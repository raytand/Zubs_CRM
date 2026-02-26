import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET api/doctors
export const getAllDoctors = () => fetchGet("/doctors");

// GET api/doctors/{id}
export const getDoctorById = (id) => fetchGet(`/doctors/${id}`);

// POST api/doctors
export const createDoctor = (dto) => fetchPost("/doctors", dto);

// PUT api/doctors/{id}
export const updateDoctor = (id, dto) => fetchPut(`/doctors/${id}`, dto);

// DELETE api/doctors/{id}
export const deleteDoctor = (id) => fetchDelete(`/doctors/${id}`);
