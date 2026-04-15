import axios from 'axios';

const API_BASE = 'http://localhost:5000/api/v1/auth';

export const registerUser = async (data) => {
  return axios.post(`${API_BASE}/register`, data);
};

export const loginUser = async (data) => {
  return axios.post(`${API_BASE}/login`, data);
};

export const getCurrentUser = async () => {
  const token = localStorage.getItem('token');
  return axios.get(`${API_BASE}/me`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
};