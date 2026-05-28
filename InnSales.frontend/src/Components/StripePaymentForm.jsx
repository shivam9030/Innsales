import React, { useState } from 'react';
import { useStripe, useElements, CardElement } from '@stripe/react-stripe-js';
import axiosClient from '../api/axiosClient';
import { useNavigate } from 'react-router-dom';

const CARD_ELEMENT_OPTIONS = {
  style: {
    base: {
      color: '#0b1c30',
      fontFamily: '"Inter", sans-serif',
      fontSmoothing: 'antialiased',
      fontSize: '16px',
      '::placeholder': {
        color: '#c4c5d5',
      },
      iconColor: '#00288e',
    },
    invalid: {
      color: '#ba1a1a',
      iconColor: '#ba1a1a',
    },
  },
};

const StripePaymentForm = ({ orderId }) => {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const [cardHolderName, setCardHolderName] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    if (!stripe || !elements) {
      setMessage('Stripe not ready.');
      setLoading(false);
      return;
    }

    const cardElement = elements.getElement(CardElement);
    const { token: stripeToken, error } = await stripe.createToken(cardElement, {
      name: cardHolderName,
    });

    if (error) {
      setMessage(error.message);
      setLoading(false);
      return;
    }

    const paymentToken = localStorage.getItem('paymentToken');

    if (!paymentToken) {
      setMessage('Payment token missing.');
      setLoading(false);
      return;
    }

    try {
      await axiosClient.post(
        '/payment/process', 
        {
          orderId,
          token: stripeToken.id,
          currency: 'INR',
          paymentToken: paymentToken,
          cardHolderName,
        }
      );
      setMessage('Payment successful');
      setTimeout(() => {
        navigate(`/orders/${orderId}`);
      }, 1500);
    } catch (err) {
      setMessage(err.response?.data?.message || 'Payment failed.');
    }

    setLoading(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-2">
        <div className="flex justify-between items-center ml-1 mb-1">
          <label className="block text-xs font-bold text-on-surface-variant uppercase">Name on Card</label>
        </div>

        <input
          type="text"
          placeholder="Card Holder Name"
          value={cardHolderName}
          onChange={(e) => setCardHolderName(e.target.value)}
          className="w-full bg-surface-container-lowest border-none rounded-lg p-4 text-on-surface placeholder:text-outline-variant focus:ring-2 focus:ring-primary/20 outline-none transition-all"
          required
        />
      </div>
      
      <div className="space-y-2">
        <div className="flex justify-between items-center ml-1 mb-1">
          <label className="block text-xs font-bold text-on-surface-variant uppercase">Card Details</label>
        </div>

        <div className="bg-surface-container-lowest rounded-lg p-4 transition-all focus-within:ring-2 focus-within:ring-primary/20">
          <CardElement options={CARD_ELEMENT_OPTIONS} />
        </div>
      </div>

      <button
        type="submit"
        disabled={!stripe || loading}
        className="w-full bg-primary hover:bg-primary-container text-white py-5 rounded-lg font-bold text-lg flex items-center justify-center space-x-2 transition-all duration-400 disabled:opacity-50 disabled:pointer-events-none group scale-100 active:scale-95 shadow-lg shadow-primary/20 mt-8"
      >
        <span>{loading ? 'Processing...' : 'Complete Payment'}</span>
        {!loading && <span className="material-symbols-outlined group-hover:translate-x-1 transition-transform">arrow_forward</span>}
      </button>

      {message && (
        <div className={`p-4 rounded-lg mt-4 text-sm font-bold flex items-center justify-center space-x-2 ${message.toLowerCase().includes('success') ? 'bg-[#34A853]/10 text-[#34A853]' : 'bg-error-container text-error'}`}>
          <span className="material-symbols-outlined">{message.toLowerCase().includes('success') ? 'check_circle' : 'error'}</span>
          <span>{message}</span>
        </div>
      )}
    </form>
  );
};

export default StripePaymentForm;
