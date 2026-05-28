import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosClient from '../api/axiosClient';
import Navbar from './Navbar';
import OrderSummaryWidget from './OrderSummaryWidget';

const CheckoutPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        const res = await axiosClient.get(`/orders/${orderId}`);
        setOrder(res.data);
        if (res.data.paymentToken) {
          localStorage.setItem('paymentToken', res.data.paymentToken);
        }
      } catch (err) {
        setError('Failed to load order');
        console.error(err.response?.data || err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchOrder();
  }, [orderId]);

  const handleRedirectToPayment = () => {
    navigate(`/payment/${order.id}`);
  };

  if (loading) {
    return (
      <>
        <Navbar />
        <div className="min-h-screen flex items-center justify-center bg-surface pt-20">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
        </div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <Navbar />
        <div className="min-h-screen flex flex-col items-center justify-center bg-surface pt-20">
          <span className="material-symbols-outlined text-error text-6xl mb-4">error</span>
          <p className="text-xl font-body text-error font-bold">{error}</p>
          <button onClick={() => navigate('/dashboard')} className="mt-6 bg-primary text-white px-6 py-2 rounded-lg font-bold">Return Home</button>
        </div>
      </>
    );
  }

  return (
    <div className="bg-surface font-body text-on-surface antialiased min-h-screen">
      <Navbar />

      <main className="pt-32 pb-20 px-8 max-w-screen-2xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 items-start">
          
          {/* Left Column: Checkout Process */}
          <div className="lg:col-span-8 space-y-12">
            
            {/* Progress Stepper */}
            <div className="flex items-center justify-start space-x-4 py-4">
              <div className="flex items-center space-x-3 opacity-40">
                <span className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center text-sm font-bold">1</span>
                <span className="font-headline font-bold text-on-surface">Cart</span>
              </div>
              <div className="w-12 h-[2px] bg-surface-container-high"></div>
              <div className="flex items-center space-x-3">
                <span className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center text-sm font-bold">2</span>
                <span className="font-headline font-bold text-on-surface">Shipping</span>
              </div>
              <div className="w-12 h-[2px] bg-surface-container-high"></div>
              <div className="flex items-center space-x-3 opacity-40">
                <span className="w-8 h-8 rounded-full border-2 border-outline-variant flex items-center justify-center text-sm font-bold">3</span>
                <span className="font-headline font-bold text-on-surface">Payment</span>
              </div>
            </div>

            {/* Express Checkout */}
            <section className="space-y-6">
              <h2 className="text-sm font-bold uppercase tracking-widest text-on-surface-variant font-headline ml-1">Express Checkout</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <button className="bg-black text-white h-14 rounded-lg flex items-center justify-center space-x-2 hover:scale-[1.02] transition-transform duration-400">
                  <span className="font-bold">Apple Pay</span>
                </button>
                <button className="bg-white border border-outline-variant h-14 rounded-lg flex items-center justify-center space-x-2 hover:bg-surface-container-low transition-colors duration-400">
                  <span className="font-bold">Google Pay</span>
                </button>
              </div>
              <div className="relative flex items-center py-4">
                <div className="flex-grow border-t border-outline-variant/30"></div>
                <span className="flex-shrink mx-4 text-sm text-on-surface-variant font-medium">OR CONTINUE WITH SHIPPING</span>
                <div className="flex-grow border-t border-outline-variant/30"></div>
              </div>
            </section>

            {/* Shipping Form Section */}
            <section className="space-y-8">
              <h2 className="text-2xl font-black font-headline tracking-tight text-on-surface ml-1">Shipping Address</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <div className="flex justify-between items-center ml-1 mb-1">
                    <label className="block text-xs font-bold text-on-surface-variant uppercase">First Name</label>
                  </div>
                  <input className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant" placeholder="Julian" type="text" />
                </div>
                <div className="space-y-2">
                  <div className="flex justify-between items-center ml-1 mb-1">
                    <label className="block text-xs font-bold text-on-surface-variant uppercase">Last Name</label>
                  </div>
                  <input className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant" placeholder="Atelier" type="text" />
                </div>
                <div className="md:col-span-2 space-y-2">
                  <div className="flex justify-between items-center ml-1 mb-1">
                    <label className="block text-xs font-bold text-on-surface-variant uppercase">Street Address</label>
                  </div>
                  <input className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant" placeholder="123 Artisan Way, Studio 4B" type="text" />
                </div>
                <div className="space-y-2">
                  <div className="flex justify-between items-center ml-1 mb-1">
                    <label className="block text-xs font-bold text-on-surface-variant uppercase">City</label>
                  </div>
                  <input className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant" placeholder="New York" type="text" />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <div className="flex justify-between items-center ml-1 mb-1">
                      <label className="block text-xs font-bold text-on-surface-variant uppercase">State</label>
                    </div>
                    <input className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant" placeholder="NY" type="text" />
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between items-center ml-1 mb-1">
                      <label className="block text-xs font-bold text-on-surface-variant uppercase">Zip Code</label>
                    </div>
                    <input className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant" placeholder="10001" type="text" />
                  </div>
                </div>
              </div>
            </section>

            {/* Shipping Method */}
            <section className="space-y-6">
              <h2 className="text-2xl font-black font-headline tracking-tight text-on-surface ml-1">Shipping Method</h2>
              <div className="space-y-4">
                <label className="group relative block cursor-pointer">
                  <input defaultChecked className="peer sr-only" name="shipping" type="radio" value="standard" />
                  <div className="p-6 bg-surface-container-lowest rounded-xl border-2 border-transparent transition-all duration-400 peer-checked:border-primary-container peer-checked:bg-primary-fixed/20 group-hover:bg-surface-container-low flex justify-between items-center">
                    <div className="flex items-center space-x-4">
                      <span className="material-symbols-outlined text-primary">local_shipping</span>
                      <div>
                        <p className="font-bold text-on-surface">Standard Shipping</p>
                        <p className="text-sm text-on-surface-variant">3-5 business days</p>
                      </div>
                    </div>
                    <span className="font-bold text-primary">Free</span>
                  </div>
                </label>
                <label className="group relative block cursor-pointer">
                  <input className="peer sr-only" name="shipping" type="radio" value="express" />
                  <div className="p-6 bg-surface-container-lowest rounded-xl border-2 border-transparent transition-all duration-400 peer-checked:border-primary-container peer-checked:bg-primary-fixed/20 group-hover:bg-surface-container-low flex justify-between items-center">
                    <div className="flex items-center space-x-4">
                      <span className="material-symbols-outlined text-primary">bolt</span>
                      <div>
                        <p className="font-bold text-on-surface">Express Courier</p>
                        <p className="text-sm text-on-surface-variant">Next day delivery</p>
                      </div>
                    </div>
                    <span className="font-bold text-primary">₹250.00</span>
                  </div>
                </label>
              </div>
            </section>
            
          </div>

          <OrderSummaryWidget 
            order={order} 
            buttonText="Review & Proceed to Payment" 
            onAction={handleRedirectToPayment} 
          />
          
        </div>
      </main>

      {/* Footer */}
      <footer className="w-full py-12 px-8 bg-surface-container-lowest border-t border-outline-variant/30 mt-12">
        <div className="max-w-screen-2xl mx-auto text-center md:text-left">
          <p className="text-on-surface-variant font-body text-sm">© 2024 InnSales Atelier. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
};

export default CheckoutPage;