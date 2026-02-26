import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET /services
export const getAllServices = () => fetchGet("/services");

// GET /services/{id}
export const getServiceById = (id) => fetchGet(`/services/${id}`);

// POST /services
export const createService = (dto) => fetchPost("/services", dto);

// PUT /services/{id}
export const updateService = (id, dto) => fetchPut(`/services/${id}`, dto);

// DELETE /services/{id}
export const deleteService = (id) => fetchDelete(`/services/${id}`);
