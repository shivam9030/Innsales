import React from 'react';

const OrderSummaryWidget = ({ order, buttonText, onAction, disableButton, children }) => {
  if (!order) return null;
  const discountTotal = order.discount || 0;

  return (
    <aside className="lg:col-span-4 sticky top-28">
      <div className="bg-surface-container-lowest p-8 rounded-2xl shadow-sm space-y-8 border border-outline-variant/10">
        <h3 className="text-xl font-black font-headline tracking-tight text-on-surface">Order Summary</h3>
        
        {/* Item List */}
        <div className="space-y-4 max-h-96 overflow-y-auto pr-2 no-scrollbar">
          {order.orderItems?.map((item, idx) => {
            const isPromo = item.productName.toLowerCase().includes('promo');
            return (
              <div key={idx} className="flex items-center space-x-4">
                <div className="w-16 h-16 bg-surface-container rounded-lg overflow-hidden flex-shrink-0 flex items-center justify-center">
                  {isPromo ? (
                     <span className="material-symbols-outlined text-primary text-3xl">loyalty</span>
                  ) : (
                     <span className="material-symbols-outlined text-outline text-3xl">inventory_2</span>
                  )}
                </div>
                <div className="flex-grow">
                  <p className="text-sm font-bold text-on-surface line-clamp-1">{item.productName}</p>
                  {!isPromo && <p className="text-xs text-on-surface-variant">Qty: {item.quantity}</p>}
                </div>
                <p className="text-sm font-bold whitespace-nowrap">
                  {isPromo ? '₹0' : `₹${(item.unitPrice * item.quantity).toFixed(2)}`}
                </p>
              </div>
            );
          })}
        </div>

        <div className="border-t border-surface-container-high pt-6 space-y-3">
          <div className="flex justify-between text-sm text-on-surface-variant">
            <span>Subtotal</span>
            <span className="text-on-surface font-semibold">₹{(order.subtotal || 0).toFixed(2)}</span>
          </div>
          {discountTotal > 0 && (
            <div className="flex justify-between text-sm text-primary">
              <span>Discount</span>
              <span className="font-semibold">- ₹{discountTotal.toFixed(2)}</span>
            </div>
          )}
          <div className="flex justify-between text-sm text-on-surface-variant">
            <span>Tax</span>
            <span className="text-on-surface font-semibold">₹{(order.tax || 0).toFixed(2)}</span>
          </div>
          
          <div className="flex justify-between text-lg font-black font-headline pt-4 border-t border-surface-container-high text-on-surface">
            <span>Total</span>
            <span className="text-primary">₹{(order.totalAmount || 0).toFixed(2)}</span>
          </div>
        </div>

        {children ? children : (
          order.paymentStatus === 0 ? (
            <button 
              onClick={onAction}
              disabled={disableButton}
              className="w-full bg-primary hover:bg-primary-container text-white py-5 rounded-lg font-bold text-lg flex items-center justify-center space-x-2 transition-all duration-400 group scale-100 active:scale-95 shadow-lg shadow-primary/20 disabled:opacity-50 disabled:pointer-events-none"
            >
              <span>{buttonText || 'Complete Purchase'}</span>
              <span className="opacity-70 group-hover:translate-x-1 transition-transform font-mono">• ₹{(order.totalAmount || 0).toFixed(2)}</span>
            </button>
          ) : (
            <div className="w-full bg-surface-container-low text-primary py-5 rounded-lg font-bold text-lg flex items-center justify-center space-x-2">
              <span className="material-symbols-outlined">check_circle</span>
              <span>Payment Completed</span>
            </div>
          )
        )}

        <p className="text-[10px] text-center text-on-surface-variant leading-relaxed">
          By completing your purchase, you agree to our Terms of Service and Privacy Policy. Secure transaction powered by InnSales.
        </p>
      </div>

      {/* Trust Badges */}
      <div className="mt-6 flex justify-center space-x-8 text-on-surface-variant/40">
        <span className="material-symbols-outlined scale-125">lock</span>
        <span className="material-symbols-outlined scale-125">verified_user</span>
        <span className="material-symbols-outlined scale-125">shield</span>
      </div>
    </aside>
  );
};

export default OrderSummaryWidget;
