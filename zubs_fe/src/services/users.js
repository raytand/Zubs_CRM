import { fetchGet, fetchPost, fetchPut, fetchDelete } from "./api";

export const getAllUsers = () => fetchGet("/users");
export const getUserById = (id) => fetchGet(`/users/${id}`);
export const getUserByUsername = (username) =>
  fetchGet(`/users/by-username/${username}`);
export const createUser = (dto) => fetchPost("/users", dto);
export const updateUser = (id, dto) => fetchPut(`/users/${id}`, dto);
export const deleteUser = (id) => fetchDelete(`/users/${id}`);
