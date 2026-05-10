import React, { useState, memo } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

const Navbar = memo(function Navbar() {
  const { user, logout } = useAuth();
  const { cartCount } = useCart();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  return (
    <nav className="fixed top-0 w-full z-50 bg-white/70 backdrop-blur-xl shadow-sm transition-all duration-300">
      <div className="flex justify-between items-center px-8 py-4 w-full max-w-screen-2xl mx-auto">
        <Link to="/dashboard" className="text-2xl font-black text-primary font-headline tracking-tight hover:opacity-80 transition-opacity">
          InnSales
        </Link>
        <div className="hidden md:flex space-x-8">
          <Link to="/dashboard" className="text-primary border-b-2 border-primary pb-1 font-headline tracking-tight font-bold transition-colors">
            Shop
          </Link>
          <Link to="/orders" className="text-on-surface-variant hover:text-primary transition-colors font-headline tracking-tight font-bold">
            Orders
          </Link>
        </div>
        <div className="flex items-center space-x-6">
          <div className="flex items-center space-x-4">
            
            <div className="relative">
              <button 
                onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                className="flex items-center gap-2 hover:scale-105 transition-transform duration-300 text-primary"
              >
                <span className="material-symbols-outlined">person</span>
                <span className="hidden sm:inline-block font-label text-sm font-semibold">{user?.displayName || 'User'}</span>
              </button>
              
              {isDropdownOpen && (
                <div className="absolute right-0 mt-2 w-48 bg-surface-container-lowest rounded-lg shadow-lg py-2 border border-outline-variant/20">
                  <span className="block px-4 py-2 text-xs text-on-surface-variant uppercase tracking-wider font-bold border-b border-outline-variant/10 mb-1">
                    Account
                  </span>
                  <button 
                    onClick={logout} 
                    className="w-full text-left px-4 py-2 text-sm text-error hover:bg-error/10 font-label transition-colors"
                  >
                    Logout
                  </button>
                </div>
              )}
            </div>

            <Link to="/basket" className="hover:scale-105 transition-transform duration-300 text-primary flex items-center relative">
              <span className="material-symbols-outlined">shopping_cart</span>
              {cartCount > 0 && (
                <span className="absolute -top-2 -right-2 bg-primary text-white text-[10px] font-bold rounded-full h-4 w-4 flex items-center justify-center">
                  {cartCount}
                </span>
              )}
            </Link>
            
          </div>
        </div>
      </div>
    </nav>
  );
});

export default Navbar;