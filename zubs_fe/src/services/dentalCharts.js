import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

export const getDentalChartsByPatient = (patientId) =>
  fetchGet(`/dental-charts/patient/${patientId}`);
export const getDentalChartById = (id) => fetchGet(`/dental-charts/${id}`);
export const createDentalChart = (dto) => fetchPost("/dental-charts", dto);
export const updateDentalChart = (dto) => fetchPut("/dental-charts", dto);
export const deleteDentalChart = (id) => fetchDelete(`/dental-charts/${id}`);
