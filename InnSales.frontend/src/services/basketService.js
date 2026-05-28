import axiosClient from '../api/axiosClient';

//  1. Get all basket items for current user
export const getBasketItems = async () => {
  try {
    const response = await axiosClient.get('/basket');
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
    const response = await axiosClient.post('/basket/add', dto);
    return response;
  } catch (error) {
    console.error('Error adding to basket:', error.response?.data || error.message);
    throw error;
  }
};

//  3. Update quantity of a basket item
export const updateBasketItem = async (id, quantity) => {
  try {
    const response = await axiosClient.put(`/basket/update/${id}`, quantity);
    return response;
  } catch (error) {
    console.error('Error updating basket item:', error.response?.data || error.message);
    throw error;
  }
};

// 4. Remove item from basket
export const removeBasketItem = async (id) => {
  try {
    const response = await axiosClient.delete(`/basket/remove/${id}`);
    return response;
  } catch (error) {
    console.error('Error removing basket item:', error.response?.data || error.message);
    throw error;
  }
};

// 5. Checkout basket
export const checkoutBasket = async () => {
  try {
    const response = await axiosClient.post('/basket/checkout', null);
    return response;
  } catch (error) {
    console.error('Error during checkout:', error.response?.data || error.message);
    throw error;
  }
};

// 6. Apply promo code
export const applyPromoCode = async (promoCode) => {
  try {
    const response = await axiosClient.post(
      '/basket/apply-promo',
      JSON.stringify(promoCode)
    );
    return response;
  } catch (error) {
    console.error('Error applying promo code:', error.response?.data || error.message);
    throw error;
  }
};