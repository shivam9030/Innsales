


import './App.css';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import RegisterForm from './Components/RegisterForm';
import LoginForm from './Components/LoginForm';
import DashboardPage from './Components/DashBoardPage';
import BasketPage from './Components/BasketPage';
import CheckoutPage from './Components/CheckoutPage'; // Order Summary Page
import StripePaymentPage from './Components/StripePaymentPage'; // Card Input Page
import OrdersPage from './Components/OrdersPage'; // All Orders
import OrderDetailsPage from './Components/OrderDetailsPage'; // Single Order Details
import ProductPage from './Components/ProductPage'; // Single Product Display

function AppRoutes() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="flex justify-center items-center h-screen bg-surface">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <Routes>
      {/* Default route redirects to Register */}
      <Route path="/" element={<Navigate to="/register" />} />
      <Route path="/register" element={<RegisterForm />} />
      <Route path="/login" element={<LoginForm />} />
      <Route
        path="/dashboard"
        element={isAuthenticated ? <DashboardPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/basket"
        element={isAuthenticated ? <BasketPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/checkout/:orderId"
        element={isAuthenticated ? <CheckoutPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/payment/:orderId"
        element={isAuthenticated ? <StripePaymentPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/orders"
        element={isAuthenticated ? <OrdersPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/orders/:orderId"
        element={isAuthenticated ? <OrderDetailsPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/product/:productId"
        element={isAuthenticated ? <ProductPage /> : <Navigate to="/login" />}
      />
    </Routes>
  );
}

import { Toaster } from 'react-hot-toast';

function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <Router>
          <AppRoutes />
          <Toaster position="bottom-center" />
        </Router>
      </CartProvider>
    </AuthProvider>
  );
}

export default App;

