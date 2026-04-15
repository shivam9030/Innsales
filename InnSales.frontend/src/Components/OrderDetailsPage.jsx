import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import Navbar from './Navbar';

export default function OrderDetailsPage() {
  const { orderId } = useParams();
  const navigate = useNavigate();

  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const paymentStatusMap = {
    0: 'Pending',
    1: 'Success',
    2: 'Failed',
    3: 'Refunded',
    4: 'Cancelled'
  };

  const orderStatusMap = {
    0: 'Pending',
    1: 'Confirmed',
    2: 'Shipped',
    3: 'Delivered',
    4: 'Cancelled'
  };

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          setError('No access token found. Please login again.');
          navigate('/login');
          return;
        }

        const res = await axios.get(`http://localhost:5000/api/v1/orders/${orderId}`, {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
          withCredentials: false,
        });

        setOrder(res.data);
      } catch (err) {
        const message = err.response?.data?.message || err.response?.data || err.message || 'Could not load order details.';
        setError(typeof message === 'string' ? message : 'Could not load order details.');
        console.error('Failed to fetch order:', err.response?.data || err.message);

        if (err.response?.status === 401) navigate('/login');
        if (err.response?.status === 403) alert('Access denied.');
      } finally {
        setLoading(false);
      }
    };

    fetchOrder();
  }, [orderId, navigate]);

  const handlePaymentRedirect = () => {
    if (!order?.id) return;
    navigate(`/payment/${order.id}`);
  };

  const handleCancelOrder = async () => {
    const confirmCancel = window.confirm('Are you sure you want to cancel this order?');
    if (!confirmCancel || !order?.id) return;

    try {
      const token = localStorage.getItem('token');
      if (!token) {
        alert('No access token found. Please login again.');
        navigate('/login');
        return;
      }

      await axios.post(
        `http://localhost:5000/api/v1/orders/cancel/${order.id}`,
        null,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        }
      );

      alert('Order cancelled successfully.');
      navigate('/orders');
    } catch (err) {
      if (err.response?.status === 401) {
        alert('Session expired. Please login again.');
        navigate('/login');
      } else {
        alert(err.response?.data?.message || 'Could not cancel the order.');
      }
    }
  };

  if (loading) return (
    <div className="bg-surface min-h-screen">
       <Navbar />
       <div className="flex justify-center pt-32 pb-20"><div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div></div>
    </div>
  );

  if (error || !order) return (
    <div className="bg-surface min-h-screen">
       <Navbar />
       <div className="max-w-screen-xl mx-auto px-8 pt-32 text-center">
         <p className="text-xl text-error font-bold">{error || 'Order not found.'}</p>
         <button onClick={() => navigate('/orders')} className="mt-8 bg-surface-container-low px-6 py-2 rounded-lg font-bold hover:bg-surface-container-high transition-colors text-on-surface">Go Back to Orders</button>
       </div>
    </div>
  );

  return (
    <div className="bg-surface font-body text-on-surface min-h-screen">
      <Navbar />
      
      <main className="pt-32 pb-20 px-8 max-w-screen-xl mx-auto">
        <div className="flex items-center gap-4 mb-8">
           <button onClick={() => navigate('/orders')} className="flex items-center justify-center p-2 rounded-full hover:bg-surface-container-low transition-colors text-on-surface-variant">
             <span className="material-symbols-outlined">arrow_back</span>
           </button>
           <h1 className="text-3xl md:text-4xl font-black font-headline tracking-tighter text-on-surface">
             Order {order.readableOrderId}
           </h1>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
          
          {/* Left Column: Items and Info */}
          <div className="lg:col-span-8 flex flex-col gap-8">
            <section className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-outline-variant/10">
              <h2 className="text-xl font-bold font-headline mb-6 border-b border-outline-variant/10 pb-4">Order Items</h2>
              <div className="space-y-4">
                {(order.orderItems ?? []).map((item, idx) => {
                  const isPromo = item.productName.toLowerCase().includes('promo');
                  return (
                    <div key={idx} className="flex items-center gap-4 group">
                       <div className="w-16 h-16 bg-surface-container rounded-lg overflow-hidden flex-shrink-0 flex items-center justify-center border border-outline-variant/10">
                          {item.imageUrl ? (
                            <img src={item.imageUrl} alt={item.productName} className="w-full h-full object-cover" />
                          ) : (
                            <span className="material-symbols-outlined text-outline text-3xl">{isPromo ? 'loyalty' : 'category'}</span>
                          )}
                       </div>
                       <div className="flex-grow">
                          <p className="font-bold text-on-surface text-lg font-headline">{item.productName}</p>
                          {!isPromo && <p className="text-sm text-on-surface-variant font-medium mt-1">Quantity: {item.quantity}</p>}
                       </div>
                       <div className="text-right">
                          <p className="font-bold text-primary text-lg">₹{Number(item.unitPrice ?? 0).toFixed(2)}</p>
                       </div>
                    </div>
                  );
                })}
              </div>
            </section>

            <section className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-outline-variant/10">
              <h2 className="text-xl font-bold font-headline mb-6 border-b border-outline-variant/10 pb-4">Information</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div>
                   <p className="text-xs font-bold text-on-surface-variant uppercase mb-1">Date Placed</p>
                   <p className="font-medium">{new Date(order.orderDate).toLocaleString()}</p>
                </div>
                <div>
                   <p className="text-xs font-bold text-on-surface-variant uppercase mb-1">Customer</p>
                   <p className="font-medium">{order.customerName || 'N/A'}</p>
                </div>
              </div>
            </section>
          </div>

          {/* Right Column: Status and Actions */}
          <aside className="lg:col-span-4 flex flex-col gap-8">
            <div className="bg-surface-container-low p-6 md:p-8 rounded-2xl border border-primary/10">
               <h3 className="text-xl font-black font-headline tracking-tight text-on-surface mb-6 border-b border-outline-variant/10 pb-4">Summary</h3>
               
               <div className="space-y-4 mb-8">
                 <div className="flex justify-between items-center text-sm font-medium">
                   <span className="text-on-surface-variant">Order Status</span>
                   <span className="font-bold bg-surface-container px-3 py-1 rounded-full">{orderStatusMap[order.orderStatus] ?? 'Unknown'}</span>
                 </div>
                 <div className="flex justify-between items-center text-sm font-medium">
                   <span className="text-on-surface-variant">Payment Status</span>
                   <span className={`font-bold px-3 py-1 rounded-full ${order.paymentStatus === 1 ? 'bg-[#34A853]/10 text-[#34A853]' : 'bg-error-container text-error'}`}>{paymentStatusMap[order.paymentStatus] ?? 'Unknown'}</span>
                 </div>
               </div>
               
               <div className="border-t border-outline-variant/10 pt-6 mb-8">
                 <div className="flex justify-between items-baseline mb-2">
                   <span className="text-lg font-bold text-on-surface-variant">Total</span>
                   <span className="text-3xl font-black font-headline text-primary">₹{Number(order.totalAmount ?? 0).toFixed(2)}</span>
                 </div>
               </div>

               {/* Action Buttons */}
               <div className="space-y-3">
                 {order.paymentStatus === 0 && (
                    <button
                      onClick={handlePaymentRedirect}
                      className="w-full bg-primary hover:bg-primary-container text-white py-4 rounded-lg font-bold flex flex-col items-center justify-center transition-all duration-300 shadow-md hover:shadow-lg"
                    >
                      <span className="flex items-center gap-2"><span className="material-symbols-outlined text-[20px]">credit_card</span> Complete Payment</span>
                    </button>
                  )}

                  {order.orderStatus === 0 && (
                    <button
                      onClick={handleCancelOrder}
                      className="w-full bg-transparent hover:bg-error-container text-error border border-error/50 hover:border-error py-4 rounded-lg font-bold flex items-center justify-center transition-all duration-300"
                    >
                      Cancel Order
                    </button>
                  )}
               </div>

            </div>
          </aside>
          
        </div>
      </main>

    </div>
  );
}
