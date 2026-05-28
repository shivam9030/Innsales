import React, { useEffect, useState, useCallback, memo } from 'react';
import { Link } from 'react-router-dom';
import { getAllCategories } from '../services/categoryService';
import { getProductsByCategory } from '../services/productService';
import { useCart } from '../context/CartContext';
import Navbar from './Navbar';

const DashboardPage = memo(function DashboardPage() {
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [quantities, setQuantities] = useState({});
  const [loading, setLoading] = useState(false);
  const [toastMessage, setToastMessage] = useState(null);
  const { addItem } = useCart();

  const showToast = useCallback((message, isError = false) => {
    setToastMessage({ message, isError });
    setTimeout(() => {
      setToastMessage(null);
    }, 3000);
  }, []);

  const [error, setError] = useState('');

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const catRes = await getAllCategories();
        setCategories(catRes.data);
      } catch (err) {
        console.error('Error fetching categories:', err.response?.data || err.message);
        setError('Failed to load categories.');
      }
    };

    fetchCategories();
  }, []);

  const handleCategoryClick = useCallback(async (categoryId) => {
    setSelectedCategory(categoryId);
    setLoading(true);
    try {
      const res = await getProductsByCategory(categoryId);
      setProducts(res.data);

      const initialQuantities = {};
      res.data.forEach((product) => {
        initialQuantities[product.id] = 1;
      });
      setQuantities(initialQuantities);
    } catch (err) {
      console.error('Failed to fetch products:', err.response?.data || err.message);
      setError('Failed to load products.');
    } finally {
      setLoading(false);
    }
  }, []);

  const handleQuantityChange = useCallback((productId, value) => {
    const parsed = parseInt(value);
    setQuantities((prev) => ({
      ...prev,
      [productId]: parsed > 0 ? parsed : 1,
    }));
  }, []);

  const handleAddToCart = useCallback(async (product) => {
    try {
      const quantity = quantities[product.id] || 1;
      const result = await addItem(product.id, quantity);
      if (result.success) {
        showToast(`${product.name} (x${quantity}) added to basket!`);
      } else {
        showToast('Could not add item to basket.', true);
      }
    } catch (err) {
      console.error('Error adding to basket:', err.response?.data || err.message);
      showToast('Could not add item to basket.', true);
    }
  }, [addItem, quantities, showToast]);

  const getCategoryClass = useCallback((index) => {
    if (index === 0) return "md:col-span-8 group relative overflow-hidden rounded-xl bg-surface-container-low transition-all h-[700px] cursor-pointer";
    return "group relative overflow-hidden rounded-xl bg-surface-container-low transition-all h-[338px] cursor-pointer"; 
  }, []);

  return (
    <>
      <Navbar />

      <main className="pt-20">
        
        {/* Hero Section */}
        <section className="relative h-[600px] md:h-[921px] w-full overflow-hidden">
          <div 
            className="absolute inset-0 bg-cover bg-center" 
            style={{ backgroundImage: "url('https://lh3.googleusercontent.com/aida-public/AB6AXuBpBlAZrxV7TBa8lyWI2REKbWhtiBTQQaOTlqbPXeO7ww3iT4O7z0YJqro0vCZT6nTsj2kam8QrHY7dQAHm1Fij1JyUN65szDa9VRhS7pHpmo2CoQYtAY2afzY4berxvlHZevSi3-qor18QqC3NyvT0hOj0YCnBaNOTurWyYOPrOPr_4kSBVWpJA6gbr0ZC1MJWtL43xVaBo1jAAQBC_U3lif4BeiQ2nRDS6l5N13aVde-RuMk5NYdLg3kSulbZBO3W6DL1JqTCKQU')" }}
          ></div>
          <div className="absolute inset-0 bg-gradient-to-r from-on-surface/40 to-transparent"></div>
          <div className="relative h-full max-w-screen-2xl mx-auto px-8 flex flex-col justify-center items-start">
            <div className="max-w-2xl bg-white/10 backdrop-blur-md p-8 md:p-12 rounded-xl border border-white/20">
              <h1 className="text-white font-headline text-5xl md:text-8xl font-extrabold tracking-tighter leading-[1.1] mb-6">
                Timeless<br/>Precision.
              </h1>
              <p className="text-white/90 text-lg md:text-xl font-body mb-8 max-w-md">
                Experience the fusion of artisan craftsmanship and modern engineering. Curated for the discerning eye.
              </p>
              <div className="flex space-x-4">
                <button 
                  className="bg-gradient-to-br from-primary to-primary-container text-white px-8 py-4 rounded-lg font-bold hover:scale-105 transition-all duration-300 shadow-lg"
                  onClick={() => window.scrollTo({ top: 800, behavior: 'smooth' })}
                >
                  Shop the Collection
                </button>
              </div>
            </div>
          </div>
        </section>

        {/* Featured Categories */}
        <section className="py-24 px-8 max-w-screen-2xl mx-auto" id="categories">
          <div className="flex justify-between items-end mb-12">
            <div>
              <span className="text-primary font-bold tracking-widest uppercase text-xs">Curated Selections</span>
              <h2 className="text-4xl font-headline font-extrabold text-on-surface mt-2">The Pillars of Style</h2>
            </div>
          </div>

          {error && <p className="text-error font-semibold mb-4">{error}</p>}

          {!error && categories.length === 0 && (
            <p className="text-on-surface-variant italic">No categories available at the moment.</p>
          )}

          {categories.length > 0 && (
            <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
              
              {/* If there's 1st category */}
              {categories[0] && (
                <div 
                  className={getCategoryClass(0)}
                  onClick={() => handleCategoryClick(categories[0].id)}
                >
                  <img 
                    alt={categories[0].name} 
                    className="absolute inset-0 w-full h-full object-cover group-hover:scale-105 transition-all duration-500" 
                    src={categories[0].imageUrl || 'https://images.unsplash.com/photo-1542291026-7eec264c27ff'} 
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-on-surface/80 via-transparent to-transparent"></div>
                  <div className="absolute bottom-0 left-0 p-8">
                    <h3 className="text-white text-3xl font-headline font-bold">{categories[0].name}</h3>
                    <p className="text-white/70 mt-2">{categories[0].description}</p>
                    {selectedCategory === categories[0].id && (
                       <span className="mt-4 inline-block bg-primary text-white px-3 py-1 text-xs uppercase font-bold rounded-full">Viewing</span>
                    )}
                  </div>
                </div>
              )}

              {/* Smaller Categories Column (for index 1, 2) */}
              <div className="md:col-span-4 flex flex-col gap-6">
                {categories.slice(1, 3).map((cat, idx) => (
                  <div 
                    key={cat.id} 
                    className={getCategoryClass(idx + 1)}
                    onClick={() => handleCategoryClick(cat.id)}
                  >
                    <img 
                      alt={cat.name} 
                      className="absolute inset-0 w-full h-full object-cover group-hover:scale-105 transition-all duration-500" 
                      src={cat.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e'} 
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-on-surface/80 via-transparent to-transparent"></div>
                    <div className="absolute bottom-0 left-0 p-6">
                      <h3 className="text-white text-xl font-headline font-bold">{cat.name}</h3>
                      {selectedCategory === cat.id && (
                        <span className="mt-2 inline-block bg-primary text-white px-2 py-0.5 text-[10px] uppercase font-bold rounded-full">Viewing</span>
                      )}
                    </div>
                  </div>
                ))}
              </div>
              
              {/* Additional Categories if there are more than 3 */}
              {categories.slice(3).map((cat) => (
                 <div 
                 key={cat.id} 
                 className="md:col-span-4 group relative overflow-hidden rounded-xl bg-surface-container-low transition-all h-[338px] cursor-pointer"
                 onClick={() => handleCategoryClick(cat.id)}
               >
                 <img 
                   alt={cat.name} 
                   className="absolute inset-0 w-full h-full object-cover group-hover:scale-105 transition-all duration-500" 
                   src={cat.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e'} 
                 />
                 <div className="absolute inset-0 bg-gradient-to-t from-on-surface/80 via-transparent to-transparent"></div>
                 <div className="absolute bottom-0 left-0 p-6">
                   <h3 className="text-white text-xl font-headline font-bold">{cat.name}</h3>
                   {selectedCategory === cat.id && (
                     <span className="mt-2 inline-block bg-primary text-white px-2 py-0.5 text-[10px] uppercase font-bold rounded-full">Viewing</span>
                   )}
                 </div>
               </div>
              ))}

            </div>
          )}
        </section>

        {/* Selected Category Products */}
        {selectedCategory && (
          <section className="bg-surface-container-low py-24">
            <div className="max-w-screen-2xl mx-auto px-8">
              <div className="flex flex-col items-center text-center mb-16">
                <span className="text-primary font-bold tracking-widest uppercase text-xs">Selected Portfolio</span>
                <h2 className="text-4xl font-headline font-extrabold text-on-surface mt-2">Atelier Offerings</h2>
              </div>

              {loading ? (
                <div className="flex justify-center h-32 items-center">
                  <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary"></div>
                </div>
              ) : products.length === 0 ? (
                <p className="text-center text-on-surface-variant font-body">No products found in this category.</p>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
                  {products.map((product) => (
                    <Link to={`/product/${product.id}`} key={product.id} className="group bg-surface-container-lowest rounded-xl p-4 flex flex-col h-full shadow-sm hover:shadow-md transition-shadow">
                      <div className="relative aspect-[4/5] overflow-hidden rounded-lg bg-surface-container-lowest mb-4">
                        <img 
                          alt={product.name} 
                          className="w-full h-full object-cover group-hover:scale-110 transition-all duration-500" 
                          src={product.imageUrl || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30'} 
                        />
                        <button 
                          onClick={(e) => {
                            e.preventDefault();
                            handleAddToCart(product);
                          }}
                          className="absolute bottom-4 right-4 bg-white text-primary p-3 rounded-full shadow-lg opacity-0 group-hover:opacity-100 translate-y-2 group-hover:translate-y-0 transition-all duration-300 hover:text-white hover:bg-primary"
                        >
                          <span className="material-symbols-outlined">add_shopping_cart</span>
                        </button>
                      </div>
                      <h3 className="text-on-surface font-headline font-bold text-lg leading-tight mt-2">{product.name}</h3>
                      <p className="text-on-surface-variant font-body text-sm mb-4 line-clamp-2 mt-1 flex-grow">{product.description}</p>
                      
                      <div className="flex items-center justify-between mt-auto">
                        <p className="text-primary font-extrabold font-headline">₹{product.price}</p>
                        <input
                          type="number"
                          min="1"
                          value={quantities[product.id] || 1}
                          onClick={(e) => e.preventDefault()}
                          onChange={(e) => {
                            e.preventDefault();
                            handleQuantityChange(product.id, e.target.value);
                          }}
                          className="w-16 px-2 py-1 bg-surface-container-low text-on-surface border-none rounded text-center text-sm font-label focus:ring-1 focus:ring-primary outline-none"
                        />
                      </div>
                    </Link>
                  ))}
                </div>
              )}
            </div>
          </section>
        )}

      </main>

      {/* Footer */}
      <footer className="bg-surface-container-lowest w-full py-12 px-8 mt-12 border-t border-outline-variant/30">
        <div className="max-w-screen-2xl mx-auto flex flex-col md:flex-row justify-between items-center text-center md:text-left gap-4">
          <div>
            <div className="text-xl font-bold font-headline text-primary opacity-80 mb-2">InnSales</div>
            <p className="text-on-surface-variant text-sm font-body">Defining the intersection of artisan legacy and digital commerce.</p>
          </div>
          <p className="text-on-surface-variant font-label text-sm">
            © 2024 InnSales Atelier. All rights reserved.
          </p>
        </div>
      </footer>
      {toastMessage && (
        <div className={`fixed top-24 right-8 z-[100] p-4 rounded-xl shadow-xl border min-w-[300px] flex items-center space-x-3 transition-all duration-300 ${toastMessage.isError ? 'bg-error-container text-error border-error/50' : 'bg-primary text-white border-primary/20 shadow-primary/20'}`}>
          <span className="material-symbols-outlined">{toastMessage.isError ? 'error' : 'check_circle'}</span>
          <span className="font-bold text-sm tracking-wide">{toastMessage.message}</span>
        </div>
      )}
    </>
  );
});

export default DashboardPage;
;
