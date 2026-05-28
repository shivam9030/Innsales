import axiosClient from '../api/axiosClient';

export const getAllCategories = async () => {
  return axiosClient.get('/categories');
};