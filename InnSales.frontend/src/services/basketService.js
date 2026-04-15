

import axios from 'axios';

const API_BASE = 'http://localhost:5000/api/v1/basket';

const authHeader = () => ({
  Authorization: `Bearer ${localStorage.getItem('token')}`,
  'Content-Type': 'application/json',
});

//  1. Get all basket items for current user
export const getBasketItems = async () => {
  try {
    const response = await axios.get(`${API_BASE}`, {
      headers: authHeader(),
    });
    return response;
  } catch (error) {
    console.error('Error fetching basket items:', error.response?.data || error.message);
    throw error;
  }
};

//  2. Add item to basket
export const addToBasket = async (productId, quantity) => {
  const dto = { productId, quantity };

  try {
    const response = await axios.post(`${API_BASE}/add`, dto, {
      headers: authHeader(),
    });
    return response;
  } catch (error) {
    console.error('Error adding to basket:', error.response?.data || error.message);
    throw error;
  }
};

//  3. Update quantity of a basket item
export const updateBasketItem = async (id, quantity) => {
  try {
    const response = await axios.put(`${API_BASE}/update/${id}`, quantity, {
      headers: authHeader(),
    });
    return response;
  } catch (error) {
    console.error('Error updating basket item:', error.response?.data || error.message);
    throw error;
  }
};

// 4. Remove item from basket
export const removeBasketItem = async (id) => {
  try {
    const response = await axios.delete(`${API_BASE}/remove/${id}`, {
      headers: authHeader(),
    });
    return response;
  } catch (error) {
    console.error('Error removing basket item:', error.response?.data || error.message);
    throw error;
  }
};

// 5. Checkout basket
export const checkoutBasket = async () => {
  try {
    const response = await axios.post(`${API_BASE}/checkout`, null, {
      headers: authHeader(),
    });
    return response;
  } catch (error) {
    console.error('Error during checkout:', error.response?.data || error.message);
    throw error;
  }
};

// 6. Apply promo code
export const applyPromoCode = async (promoCode) => {
  try {
    const response = await axios.post(
      `${API_BASE}/apply-promo`,
      JSON.stringify(promoCode), // Send raw string
      {
        headers: {
          ...authHeader(),
          'Content-Type': 'application/json',
        },
      }
    );
    return response;
  } catch (error) {
    console.error('Error applying promo code:', error.response?.data || error.message);
    throw error;
  }
};