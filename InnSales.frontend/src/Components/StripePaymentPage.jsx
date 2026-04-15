import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import axios from 'axios';
import StripePaymentForm from './StripePaymentForm';
import Navbar from './Navbar';
import OrderSummaryWidget from './OrderSummaryWidget';

const stripePromise = loadStripe('pk_test_51QTow3CWSF4SmdBm0QLWUmxNIO8QIZIoRKfjQpaMh9vhkY5SfaUJykOG0oA7sukCvxJsdEYscihC5qWH5ST3HBzH00t5nCdGSZ');

const StripePaymentPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        const res = await axios.get(`http://localhost:5000/api/v1/orders/${orderId}`, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`,
          },
        });
        setOrder(res.data);
      } catch (err) {
        setError('Failed to load order');
        console.error(err.response?.data || err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchOrder();
  }, [orderId]);

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

  if (error || !order) {
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
              <div className="flex items-center space-x-3 opacity-40">
                <span className="w-8 h-8 rounded-full border-2 border-outline-variant flex items-center justify-center text-sm font-bold">2</span>
                <span className="font-headline font-bold text-on-surface">Shipping</span>
              </div>
              <div className="w-12 h-[2px] bg-surface-container-high"></div>
              <div className="flex items-center space-x-3">
                <span className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center text-sm font-bold">3</span>
                <span className="font-headline font-bold text-on-surface">Payment</span>
              </div>
            </div>

            {/* Payment Section */}
            <section className="space-y-8 bg-surface-container-low p-8 rounded-2xl">
              <div className="flex items-center justify-between">
                <h2 className="text-2xl font-black font-headline tracking-tight text-on-surface">Payment Information</h2>
                <div className="flex space-x-2 grayscale opacity-50">
                  <span className="material-symbols-outlined">credit_card</span>
                  <span className="material-symbols-outlined">account_balance_wallet</span>
                </div>
              </div>
              <Elements stripe={stripePromise}>
                <StripePaymentForm orderId={orderId} />
              </Elements>
            </section>
            
          </div>

          <OrderSummaryWidget order={order}>
            {/* The actual payment button will be inside the StripePaymentForm so it can submit the stripe element,
                but we can place an informational block here or disable the button */}
             <div className="w-full bg-surface-container-low text-primary py-5 rounded-lg font-bold text-sm flex flex-col items-center justify-center space-y-1 text-center">
                <span className="material-symbols-outlined">lock</span>
                <span>Complete payment securely on the left.</span>
             </div>
          </OrderSummaryWidget>
          
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

export default StripePaymentPage;