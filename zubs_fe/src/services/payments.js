import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

// GET /payments
export const getAllPayments = () => fetchGet("/payments");

// GET /payments/{id}
export const getPaymentById = (id) => fetchGet(`/payments/${id}`);

// POST /payments
export const createPayment = (dto) => fetchPost("/payments", dto);

// PUT /payments/{id}
export const updatePayment = (id, dto) => fetchPut(`/payments/${id}`, dto);

// DELETE /payments/{id}
export const deletePayment = (id) => fetchDelete(`/payments/${id}`);
