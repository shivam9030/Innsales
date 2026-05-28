import axiosClient from '../api/axiosClient';

export const getProductsByCategory = async (categoryId) => {
  return axiosClient.get(`/products/category/${categoryId}`);
};