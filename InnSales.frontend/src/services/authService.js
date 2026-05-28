import axiosClient from '../api/axiosClient';

export const registerUser = async (data) => {
  return axiosClient.post('/auth/register', data);
};

export const loginUser = async (data) => {
  return axiosClient.post('/auth/login', data);
};

export const getCurrentUser = async () => {
  return axiosClient.get('/auth/me');
};