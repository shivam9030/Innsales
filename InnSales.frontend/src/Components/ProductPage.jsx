import React, { useEffect, useState, useCallback, memo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import Navbar from './Navbar';
import { useCart } from '../context/CartContext';

const ProductPage = memo(() => {
  const { productId } = useParams();
  const navigate = useNavigate();
  const { addItem } = useCart();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Custom states for the mocked detailed page
  const [selectedSize, setSelectedSize] = useState('M');
  const [addingToCart, setAddingToCart] = useState(false);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        setLoading(true);
        const res = await axios.get(`http://localhost:5000/api/v1/product/all`); 
        const found = res.data?.find(p => p.id === parseInt(productId));
        
        if (found) {
          setProduct(found);
        } else {
          setError("Product not found in available listings.");
        }
      } catch (err) {
        console.error(err);
        setError("Failed to load product details.");
      } finally {
        setLoading(false);
      }
    };
    
    fetchProduct();
  }, [productId]);

  const handleAddToCart = useCallback(async () => {
    try {
      setAddingToCart(true);
      const result = await addItem(product.id, 1);
      if (result.success) {
        alert(`${product.name} added to your basket!`);
      } else {
        alert('Could not add item to basket.');
      }
    } catch (err) {
      console.error(err);
      alert('Could not add item to basket.');
    } finally {
      setAddingToCart(false);
    }
  }, [addItem, product]);

  if (loading) {
    return (
      <div className="bg-surface min-h-screen">
         <Navbar />
         <div className="flex justify-center pt-32 pb-20"><div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div></div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="bg-surface min-h-screen">
         <Navbar />
         <div className="max-w-screen-xl mx-auto px-8 pt-32 text-center">
           <span className="material-symbols-outlined text-error text-6xl mb-4">error</span>
           <p className="text-xl text-error font-bold">{error || 'Product not found.'}</p>
           <button onClick={() => navigate('/dashboard')} className="mt-8 bg-surface-container-low px-6 py-2 rounded-lg font-bold hover:bg-surface-container-high transition-colors text-on-surface">Go Back to Collections</button>
         </div>
      </div>
    );
  }

  return (
    <div className="bg-surface font-body text-on-surface antialiased min-h-screen pb-20">
      <Navbar />
      
      <main className="pt-24 px-8 max-w-screen-2xl mx-auto">
        {/* Product Detail Section */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 lg:gap-20">
          
          {/* Gallery Section */}
          <div className="lg:col-span-7 space-y-6">
            <div className="aspect-[4/5] bg-surface-container-low rounded-xl overflow-hidden group border border-outline-variant/10">
              <img 
                src={product.imageUrl || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30'} 
                alt={product.name} 
                className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" 
              />
            </div>
          </div>
          
          {/* Product Info Section */}
          <div className="lg:col-span-5 flex flex-col justify-start space-y-10 pt-4">
            <header className="space-y-4">
              <button onClick={() => navigate(-1)} className="inline-flex items-center text-sm font-bold text-on-surface-variant hover:text-primary mb-2 transition-colors">
                <span className="material-symbols-outlined text-sm mr-1">arrow_back</span>
                Back
              </button>
              
              <div className="flex items-center justify-between">
                <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold tracking-widest bg-secondary-container text-on-secondary-container uppercase">
                  Atelier Collection
                </span>
                <button className="p-2 hover:bg-surface-container-high rounded-full transition-colors">
                  <span className="material-symbols-outlined text-outline">favorite</span>
                </button>
              </div>
              
              <h1 className="text-4xl lg:text-5xl font-headline font-black text-on-surface -tracking-[0.03em] leading-tight">
                {product.name}
              </h1>
              
              <div className="flex items-baseline space-x-4">
                <span className="text-3xl font-headline font-bold text-primary">₹{(product.price || 0).toFixed(2)}</span>
                <span className="text-sm font-label text-on-surface-variant italic">Inclusive of taxes</span>
              </div>
            </header>

            <div className="space-y-8">
              {/* Size Selection Mockup */}
              <div className="space-y-3">
                <div className="flex justify-between items-center">
                   <span className="text-xs font-bold tracking-widest uppercase text-on-surface-variant font-label">Select Size</span>
                </div>
                <div className="grid grid-cols-4 gap-3">
                  {['XS', 'S', 'M', 'L'].map(size => (
                    <button 
                      key={size}
                      onClick={() => setSelectedSize(size)}
                      className={`py-3 text-sm font-bold rounded-lg transition-all ${
                        selectedSize === size 
                          ? 'border-2 border-primary bg-primary-fixed text-on-primary-fixed'
                          : 'border border-outline-variant bg-surface-container-lowest hover:bg-surface-container-low text-on-surface'
                      }`}
                    >
                      {size}
                    </button>
                  ))}
                </div>
              </div>

              {/* Actions */}
              <div className="pt-4 space-y-4">
                <button 
                  onClick={handleAddToCart}
                  disabled={addingToCart}
                  className="w-full py-5 bg-gradient-to-r from-primary to-primary-container text-white rounded-lg font-headline font-bold text-lg shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-95 transition-all flex items-center justify-center space-x-3 disabled:opacity-70 disabled:pointer-events-none"
                >
                  {addingToCart ? (
                     <div className="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full"></div>
                  ) : (
                     <span className="material-symbols-outlined">shopping_bag</span>
                  )}
                  <span>{addingToCart ? 'Adding...' : 'Add to Shopping Bag'}</span>
                </button>
                <p className="text-center text-xs font-label text-on-surface-variant">
                  <span className="material-symbols-outlined text-[14px] align-middle mr-1">bolt</span>
                  Estimated delivery: 2-4 business days
                </p>
              </div>
            </div>

            {/* Accordions */}
            <div className="border-t border-outline-variant/30 pt-4 space-y-2">
              <div className="group border-b border-outline-variant/20">
                <details className="w-full" open>
                  <summary className="flex items-center justify-between w-full py-4 font-headline font-bold hover:text-primary transition-colors cursor-pointer list-none [&::-webkit-details-marker]:hidden">
                    <span>Product Description</span>
                    <span className="material-symbols-outlined transition-transform duration-300 group-open:rotate-180">expand_more</span>
                  </summary>
                  <div className="pb-4 text-sm text-on-surface-variant leading-relaxed font-body">
                    {product.description || "An essential piece crafted uniquely for the bespoke collector."}
                  </div>
                </details>
              </div>
              <div className="group border-b border-outline-variant/20">
                <details className="w-full">
                  <summary className="flex items-center justify-between w-full py-4 font-headline font-bold hover:text-primary transition-colors cursor-pointer list-none [&::-webkit-details-marker]:hidden">
                    <span>Materials & Care</span>
                    <span className="material-symbols-outlined transition-transform duration-300 group-open:rotate-180">expand_more</span>
                  </summary>
                  <ul className="pb-4 text-sm text-on-surface-variant leading-relaxed list-disc list-inside">
                    <li>100% Premium Materials</li>
                    <li>Sustainably sourced components</li>
                    <li>Dry clean or spot clean only</li>
                    <li>Crafted with precision</li>
                  </ul>
                </details>
              </div>
              <div className="group border-b border-outline-variant/20">
                <details className="w-full">
                  <summary className="flex items-center justify-between w-full py-4 font-headline font-bold hover:text-primary transition-colors cursor-pointer list-none [&::-webkit-details-marker]:hidden">
                    <span>Shipping & Returns</span>
                    <span className="material-symbols-outlined transition-transform duration-300 group-open:rotate-180">expand_more</span>
                  </summary>
                  <div className="pb-4 text-sm text-on-surface-variant leading-relaxed font-body">
                    Complimentary express shipping on all orders over ₹5000. Returns are accepted within 14 days of receipt for a full refund or exchange.
                  </div>
                </details>
              </div>
            </div>
            
          </div>
        </div>
      </main>
      
      {/* Footer */}
      <footer className="w-full py-12 px-8 bg-surface-container-lowest border-t border-outline-variant/30 mt-32">
        <div className="max-w-screen-2xl mx-auto text-center md:text-left">
          <p className="text-on-surface-variant font-body text-sm">© 2024 InnSales Atelier. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
});

export default ProductPage;
