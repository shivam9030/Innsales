import { useEffect, useState } from 'react';
import Navbar from './Navbar';
import {
  getBasketItems,
  updateBasketItem,
  removeBasketItem,
  checkoutBasket,
  applyPromoCode
} from '../services/basketService';
import { useNavigate } from 'react-router-dom';

export default function BasketPage() {
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [promoCode, setPromoCode] = useState('');
  const [appliedPromo, setAppliedPromo] = useState(null);
  const navigate = useNavigate();

  const fetchBasket = async () => {
    try {
      setLoading(true);
      const res = await getBasketItems();
      setCartItems(res.data.items);
    } catch (err) {
      console.error('Failed to fetch basket:', err.response?.data || err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBasket();
  }, []);

  const handleQuantityChange = async (item, newQuantity) => {
    try {
      if (newQuantity <= 0) {
        await removeBasketItem(item.id);
      } else {
        await updateBasketItem(item.id, newQuantity);
      }
      await fetchBasket();
    } catch (err) {
      console.error('Failed to update quantity:', err.response?.data || err.message);
    }
  };

  const handleRemoveItem = async (id) => {
    try {
      await removeBasketItem(id);
      await fetchBasket();
    } catch (err) {
      console.error('Failed to remove item:', err.response?.data || err.message);
    }
  };

  const handleCheckout = async () => {
    try {
      const res = await checkoutBasket();
      const order = res.data;
      console.log(order.id);
      navigate(`/checkout/${order.id}`); // Redirect to CheckoutPage
    } catch (err) {
      console.error('Checkout error:', err.response?.data || err.message);
      alert('Checkout failed. Please try again.');
    }
  };

  const handleApplyPromo = async () => {
    try {
      if (!promoCode.trim()) {
        alert('Please enter a promo code.');
        return;
      }
      const res = await applyPromoCode(promoCode); 
      if (res.data.success) {
        setAppliedPromo({ discount: res.data.discount });
        alert(`Promo applied! Discount: ₹${res.data.discount}`);
        await fetchBasket(); 
      } else {
        alert(res.data.message || 'Invalid promo code.');
      }
    } catch (err) {
      console.error('Failed to apply promo:', err.response?.data || err.message);
      alert('Error applying promo code.');
    }
  };

  const subtotal = cartItems.reduce(
    (sum, item) => sum + item.effectivePrice * item.quantity,
    0
  );
  
  // Example dummy tax logic for UI consistency if not from backend
  const estimatedTax = subtotal * 0.08; 
  const total = subtotal + estimatedTax;

  return (
    <>
      <Navbar />
      <main className="pt-32 pb-24 px-8 max-w-screen-2xl mx-auto min-h-screen">
        <header className="mb-12">
          <h1 className="text-5xl font-black font-headline tracking-tighter text-on-surface mb-2">My Cart</h1>
          <p className="text-on-surface-variant font-medium">You have {cartItems.length} items in your selection</p>
        </header>

        {loading ? (
           <div className="flex justify-center py-20">
             <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
           </div>
        ) : cartItems.length === 0 ? (
          <div className="py-20 text-center bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-6xl text-outline mb-4">production_quantity_limits</span>
            <p className="text-xl font-headline font-bold text-on-surface">Your basket is empty</p>
            <button 
              onClick={() => navigate('/dashboard')}
              className="mt-6 bg-primary text-white px-8 py-3 rounded-lg font-bold hover:bg-primary-container transition-all"
            >
              Continue Shopping
            </button>
          </div>
        ) : (
          <div className="flex flex-col lg:flex-row gap-16">
            
            {/* Line Items Area */}
            <div className="flex-grow space-y-8">
              {cartItems.map((item) => (
                <div key={item.id} className="group flex flex-col sm:flex-row gap-8 p-6 rounded-xl bg-surface-container-lowest transition-all duration-400 hover:shadow-2xl hover:shadow-blue-900/5">
                  <div className="w-full sm:w-48 h-48 rounded-lg overflow-hidden flex-shrink-0 bg-surface-container-low">
                    <img 
                      src={item.imageUrl || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30'} 
                      alt={item.productName} 
                      className="w-full h-full object-cover group-hover:scale-110 transition-all duration-400" 
                    />
                  </div>
                  
                  <div className="flex flex-col justify-between flex-grow py-2">
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="text-xl font-bold font-headline text-on-surface mb-1">{item.productName}</h3>
                        <p className="text-on-surface-variant text-sm">Product #{item.productId}</p>
                      </div>
                      <span className="text-xl font-bold font-headline text-primary">₹{item.effectivePrice}</span>
                    </div>
                    
                    <div className="flex items-center justify-between mt-6">
                      <div className="flex items-center bg-surface-container-low rounded-full px-4 py-1.5 space-x-4">
                        <button 
                          onClick={() => handleQuantityChange(item, item.quantity - 1)}
                          className="text-on-surface-variant hover:text-primary transition-colors"
                        >
                          <span className="material-symbols-outlined text-lg">remove</span>
                        </button>
                        <span className="font-bold text-on-surface">{item.quantity}</span>
                        <button 
                          onClick={() => handleQuantityChange(item, item.quantity + 1)}
                          className="text-on-surface-variant hover:text-primary transition-colors"
                        >
                          <span className="material-symbols-outlined text-lg">add</span>
                        </button>
                      </div>
                      
                      <button 
                        onClick={() => handleRemoveItem(item.id)}
                        className="text-on-surface-variant hover:text-error transition-all flex items-center space-x-1 opacity-60 hover:opacity-100"
                      >
                        <span className="material-symbols-outlined text-sm">delete</span>
                        <span className="text-sm font-semibold">Remove</span>
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Sticky Order Summary Sidebar */}
            <aside className="lg:w-[400px]">
              <div className="sticky top-32 space-y-8">
                <div className="bg-surface-container-low p-8 rounded-xl">
                  <h2 className="text-2xl font-black font-headline tracking-tight text-on-surface mb-8">Order Summary</h2>
                  
                  <div className="space-y-4 mb-8">
                    <div className="flex justify-between text-on-surface-variant">
                      <span>Subtotal</span>
                      <span className="font-bold text-on-surface">₹{subtotal.toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between text-on-surface-variant">
                      <span>Shipping</span>
                      <span className="font-bold text-on-surface">Calculated at checkout</span>
                    </div>
                    <div className="flex justify-between text-on-surface-variant">
                      <span>Estimated Tax</span>
                      <span className="font-bold text-on-surface">₹{estimatedTax.toFixed(2)}</span>
                    </div>
                  </div>
                  
                  {/* Promo Code Input */}
                  <div className="mb-8">
                    <label className="block text-xs font-bold uppercase tracking-widest text-on-surface-variant mb-2">Promo Code</label>
                    <div className="flex gap-2">
                      <input 
                        className="flex-grow bg-surface-container-lowest border-none rounded-lg focus:ring-2 focus:ring-primary/20 text-sm py-3 px-4 outline-none" 
                        placeholder="Enter code" 
                        type="text"
                        value={promoCode}
                        onChange={(e) => setPromoCode(e.target.value)}
                      />
                      <button 
                        onClick={handleApplyPromo}
                        className="bg-surface-container-high text-primary px-6 py-2 rounded-lg font-bold text-sm transition-colors hover:bg-primary hover:text-white"
                      >
                        Apply
                      </button>
                    </div>
                  </div>
                  
                  <div className="border-t border-outline-variant/20 pt-8 mb-8">
                    <div className="flex justify-between items-baseline">
                      <span className="text-xl font-bold font-headline">Total</span>
                      <span className="text-3xl font-black font-headline text-primary">₹{total.toFixed(2)}</span>
                    </div>
                  </div>
                  
                  {/* Primary Checkout Action */}
                  <button 
                    onClick={handleCheckout}
                    className="w-full bg-gradient-to-br from-primary to-primary-container text-white py-5 rounded-lg font-black font-headline flex items-center justify-center space-x-3 transition-transform hover:scale-[1.02] shadow-xl shadow-primary/20"
                  >
                    <span>Proceed to Checkout</span>
                    <span className="material-symbols-outlined">arrow_forward</span>
                  </button>
                  
                  {/* Trust Badge */}
                  <div className="mt-8 flex items-center justify-center space-x-2 text-on-surface-variant/60">
                    <span className="material-symbols-outlined text-sm">lock</span>
                    <span className="text-[11px] font-bold uppercase tracking-widest">Secure Encrypted Checkout</span>
                  </div>
                </div>
                
                {/* Additional Trust Info */}
                <div className="bg-surface-container-lowest p-6 rounded-xl flex items-start space-x-4">
                  <div className="bg-primary/10 p-2 rounded-lg text-primary">
                    <span className="material-symbols-outlined">local_shipping</span>
                  </div>
                  <div>
                    <h4 className="font-bold text-on-surface text-sm">Free Express Shipping</h4>
                    <p className="text-xs text-on-surface-variant mt-1">Orders over ₹500 qualify for complimentary courier delivery.</p>
                  </div>
                </div>
              </div>
            </aside>
            
          </div>
        )}
      </main>

      <footer className="w-full py-12 px-8 bg-surface-container-lowest border-t border-outline-variant/30 mt-12">
        <div className="max-w-screen-2xl mx-auto text-center md:text-left">
          <p className="text-on-surface-variant font-body text-sm">© 2024 InnSales Atelier. All rights reserved.</p>
        </div>
      </footer>
    </>
  );
}