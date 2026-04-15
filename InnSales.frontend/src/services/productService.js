
import axios from 'axios';

const API_BASE = 'http://localhost:5000/api/v1/products';

export const getProductsByCategory = async (categoryId) => {
  const token = localStorage.getItem('token');
  return axios.get(`${API_BASE}/category/${categoryId}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
};