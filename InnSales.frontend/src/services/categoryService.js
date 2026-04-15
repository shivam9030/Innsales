import axios from 'axios';

const API_BASE = 'http://localhost:5000/api/v1/categories';

export const getAllCategories = async () => {
  const token = localStorage.getItem('token');
  return axios.get(API_BASE, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
};