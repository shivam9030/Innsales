import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosClient from '../api/axiosClient';
import Navbar from './Navbar';
import toast from 'react-hot-toast';

const OrdersPage = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const orderStatusMap = {
    0: 'Pending',
    1: 'Confirmed',
    2: 'Shipped',
    3: 'Delivered',
    4: 'Cancelled',
  };

  const getStatusColorClass = (status) => {
    switch(status) {
      case 0: return 'bg-surface-container border-outline-variant text-on-surface-variant';
      case 1: return 'bg-primary-container/20 border-primary-container text-primary';
      case 2: return 'bg-secondary-container/30 border-secondary text-secondary';
      case 3: return 'bg-[#34A853]/10 border-[#34A853]/30 text-[#34A853]'; // Greenish 
      case 4: return 'bg-error-container border-error/20 text-error';
      default: return 'bg-surface-container border-outline-variant text-on-surface-variant';
    }
  };

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const res = await axiosClient.get('/orders');
        setOrders(res.data || []);
      } catch (err) {
        const message = err.response?.data?.message || err.response?.data || err.message;
        setError(typeof message === 'string' ? message : 'Failed to fetch orders');
        console.error('Failed to fetch orders:', err.response?.data || err.message);

        if (err.response?.status === 403) {
          toast.error('Access denied. You might need Employee role for this page.');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, [navigate]);

  return (
    <div className="bg-surface font-body text-on-surface min-h-screen">
      <Navbar />
      <main className="pt-32 pb-20 px-8 max-w-screen-xl mx-auto">
        <header className="mb-12">
          <h1 className="text-4xl font-black font-headline tracking-tighter text-on-surface mb-2">Order History</h1>
          <p className="text-on-surface-variant font-medium">Review your past purchases and current shipments.</p>
        </header>

        {loading ? (
          <div className="flex justify-center py-20">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
          </div>
        ) : error ? (
           <div className="py-20 text-center bg-error-container rounded-xl">
            <span className="material-symbols-outlined text-4xl text-error mb-2">error</span>
            <p className="font-headline font-bold text-error">{error}</p>
          </div>
        ) : orders.length === 0 ? (
          <div className="py-20 text-center bg-surface-container-lowest rounded-xl border border-outline-variant/10 shadow-sm">
            <span className="material-symbols-outlined text-6xl text-outline mb-4">receipt_long</span>
            <p className="text-xl font-headline font-bold text-on-surface">No orders found</p>
            <button 
              onClick={() => navigate('/dashboard')}
              className="mt-6 bg-primary text-white px-8 py-3 rounded-lg font-bold hover:bg-primary-container transition-all"
            >
              Start Shopping
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            {orders.map((order) => (
               <div 
                 key={order.id} 
                 className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl p-6 md:p-8 flex flex-col md:flex-row justify-between items-start md:items-center gap-6 shadow-sm hover:shadow-md transition-shadow cursor-pointer group"
                 onClick={() => navigate(`/orders/${order.id}`)}
               >
                 <div className="grid grid-cols-2 md:grid-cols-4 gap-6 flex-grow w-full">
                    <div>
                      <p className="text-xs font-bold text-on-surface-variant uppercase mb-1">Order Number</p>
                      <p className="font-headline font-bold text-lg">{order.readableOrderId}</p>
                    </div>
                    <div>
                      <p className="text-xs font-bold text-on-surface-variant uppercase mb-1">Date</p>
                      <p className="font-headline font-bold text-lg">{new Date(order.orderDate).toLocaleDateString()}</p>
                    </div>
                    <div>
                      <p className="text-xs font-bold text-on-surface-variant uppercase mb-1">Total</p>
                      <p className="font-headline font-bold text-lg">₹{Number(order.totalAmount || 0).toFixed(2)}</p>
                    </div>
                    <div>
                      <p className="text-xs font-bold text-on-surface-variant uppercase mb-1">Status</p>
                      <span className={`inline-block px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider border ${getStatusColorClass(order.orderStatus)}`}>
                        {orderStatusMap[order.orderStatus] ?? 'Unknown'}
                      </span>
                    </div>
                 </div>

                 <div className="flex-shrink-0 flex items-center gap-2 text-primary font-bold group-hover:translate-x-1 transition-transform">
                   <span className="hidden md:inline">View Details</span>
                   <span className="material-symbols-outlined">chevron_right</span>
                 </div>
               </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
};

export default OrdersPage;
