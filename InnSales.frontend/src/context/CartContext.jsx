import React, { createContext, useState, useEffect, useContext, useCallback, useMemo } from 'react';
import { getBasketItems, addToBasket, updateBasketItem, removeBasketItem, checkoutBasket } from '../services/basketService';
import { useAuth } from './AuthContext';

const CartContext = createContext();

export const CartProvider = ({ children }) => {
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const { isAuthenticated } = useAuth();

  const fetchCart = useCallback(async () => {
    if (!isAuthenticated) {
      setCartItems([]);
      return;
    }
    try {
      setLoading(true);
      const res = await getBasketItems();
      setCartItems(res.data.items || []);
    } catch (err) {
      console.error('Failed to fetch cart:', err);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  const addItem = useCallback(async (productId, quantity) => {
    try {
      await addToBasket(productId, quantity);
      await fetchCart();
      return { success: true };
    } catch (err) {
      console.error('Failed to add item:', err);
      return { success: false, error: err };
    }
  }, [fetchCart]);

  const updateQuantity = useCallback(async (itemId, quantity) => {
    try {
      if (quantity <= 0) {
        await removeBasketItem(itemId);
      } else {
        await updateBasketItem(itemId, quantity);
      }
      await fetchCart();
    } catch (err) {
      console.error('Failed to update quantity:', err);
    }
  }, [fetchCart]);

  const removeItem = useCallback(async (itemId) => {
    try {
      await removeBasketItem(itemId);
      await fetchCart();
    } catch (err) {
      console.error('Failed to remove item:', err);
    }
  }, [fetchCart]);

  const cartCount = useMemo(() => {
    return cartItems.reduce((sum, item) => sum + item.quantity, 0);
  }, [cartItems]);

  const subtotal = useMemo(() => {
    return cartItems.reduce((sum, item) => sum + (item.effectivePrice * item.quantity), 0);
  }, [cartItems]);

  const value = useMemo(() => ({
    cartItems,
    loading,
    addItem,
    updateQuantity,
    removeItem,
    fetchCart,
    cartCount,
    subtotal
  }), [cartItems, loading, addItem, updateQuantity, removeItem, fetchCart, cartCount, subtotal]);

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => useContext(CartContext);
