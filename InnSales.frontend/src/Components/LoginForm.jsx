import React, { useState, useCallback, memo } from 'react';
import { loginUser } from '../services/authService';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import toast from 'react-hot-toast';

const LoginForm = memo(function LoginForm() {
  const [form, setForm] = useState({ email: '', password: '' });
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleChange = useCallback((e) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  }, []);

  const handleSubmit = useCallback(async (e) => {
    e.preventDefault();
    try {
      const res = await loginUser(form);
      const token = res.data.token;
      console.log('Login successful, token:', token);
      login(token);
      toast.success('Successfully logged in!');
      navigate('/dashboard'); 
    } catch (err) {
      toast.error(err.response?.data?.message || err.response?.data || 'Login failed. Please check your credentials.');
      console.error(err.response?.data);
    }
  }, [form, login, navigate]);

  return (
    <main className="min-h-screen flex flex-col md:flex-row">
      <section className="hidden md:flex w-1/2 relative overflow-hidden items-center justify-center p-16">
        <div className="absolute inset-0 z-0">
          <img 
            alt="Artisan workshop with precise tools" 
            className="w-full h-full object-cover" 
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuCPNnRX-Wp8xB1SPcX9QTn6XY5ZbuXyql-PedAYMVOtJqpJ9G5svwSCB-BqsUDAHGYdIgqK3fzELa47m0yhl_bZup8JSw0lHjTTVOgbsYDDj9gUT7AeRJt282ik7Fl2dzlqrGMQKKaqgrMBcjLvS7i2tw_e03PQMUJ6W8ZmkTTKtk3GQEC-DYKhErUwNZh4mLyzzKP07qDSdIXxDHIGR39p8QOo1QqtWD6iWfgeutwYFD1Bzz5PZZ7Bif9w47uxYr5mcf91IxZze9A"
          />
          <div className="absolute inset-0 bg-gradient-to-br from-primary/60 to-on-surface/80 mix-blend-multiply"></div>
        </div>
        <div className="relative z-10 w-full max-w-xl">
          <div className="mb-12">
            <h1 className="font-headline text-5xl font-extrabold text-white tracking-[-0.02em] mb-4">InnSales</h1>
            <div className="h-1 w-24 bg-primary-container rounded-full"></div>
          </div>
          <h2 className="font-headline text-6xl font-bold text-white tracking-[-0.02em] leading-[1.1]">
            Elevate your <br/>standard.
          </h2>
          <p className="mt-8 text-white/70 text-lg max-w-md font-body">
            The digital atelier for those who believe business is an art form. Manage your commerce with bespoke precision.
          </p>
        </div>
        <div className="absolute bottom-12 left-16 z-10">
          <p className="text-white/40 text-sm font-label tracking-widest uppercase">© 2024 InnSales Atelier</p>
        </div>
      </section>

      <section className="w-full md:w-1/2 flex items-center justify-center p-8 md:p-24 bg-surface-bright">
        <div className="w-full max-w-md">
          <div className="md:hidden mb-12 flex items-center gap-3">
            <span className="material-symbols-outlined text-primary text-3xl">precision_manufacturing</span>
            <h1 className="font-headline text-2xl font-bold text-on-surface tracking-tight">InnSales</h1>
          </div>
          
          <div className="mb-10">
            <h3 className="font-headline text-3xl font-bold text-on-surface tracking-[-0.02em] mb-2">Welcome Back</h3>
            <p className="text-on-surface-variant font-body">Please enter your credentials to access the atelier.</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
              <label className="block text-xs font-bold text-on-surface-variant uppercase " htmlFor="email">Email Address</label>
              </div>
              <input 
                id="email" 
                name="email" 
                type="email" 
                value={form.email}
                onChange={handleChange}
                placeholder="curator@innsales.com" 
                className="w-full px-5 py-4 bg-surface-container-lowest border-0 rounded-lg focus:ring-2 focus:ring-primary-container/20 text-on-surface transition-all duration-300 placeholder:text-outline-variant shadow-sm"
                required
              />
            </div>
            
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
                <label className="block text-xs font-bold text-on-surface-variant uppercase" htmlFor="password">Password</label>
                <a className="text-sm font-medium text-primary hover:text-primary-container transition-all duration-300" href="#">Forgot Password?</a>
              </div>
              <div className="relative">
                <input 
                  id="password" 
                  name="password" 
                  type="password" 
                  value={form.password}
                  onChange={handleChange}
                  placeholder="••••••••" 
                  className="w-full px-5 py-4 bg-surface-container-lowest border-0 rounded-lg focus:ring-2 focus:ring-primary-container/20 text-on-surface transition-all duration-300 placeholder:text-outline-variant shadow-sm"
                  required
                />
                <button type="button" className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-all duration-300">
                  <span className="material-symbols-outlined text-[20px]">visibility</span>
                </button>
              </div>
            </div>

            <div className="flex items-center">
              <input type="checkbox" id="keep-signed-in" name="keep-signed-in" className="h-5 w-5 rounded border-outline-variant text-primary-container focus:ring-primary-container/20 transition-all duration-300" />
              <label htmlFor="keep-signed-in" className="ml-3 text-sm text-on-surface-variant font-body">Keep me signed in</label>
            </div>
            
            <button type="submit" className="w-full py-4 px-6 bg-gradient-to-r from-primary to-primary-container text-white font-headline font-bold rounded-lg shadow-lg hover:scale-[1.02] active:scale-[0.98] transition-all duration-300">
              Sign In to InnSales
            </button>
          </form>

          <div className="relative my-10">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-outline-variant/30"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-4 bg-surface-bright text-on-surface-variant font-label">Or continue with</span>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <button className="flex items-center justify-center gap-3 py-3 px-4 bg-surface-container-lowest rounded-lg border border-outline-variant/10 hover:bg-surface-container-low transition-all duration-300 hover:scale-[1.01]">
              <svg className="w-5 h-5" viewBox="0 0 24 24">
                <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"></path>
                <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"></path>
                <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05"></path>
                <path d="M12 5.38c1.62 0 3.06.56 4.21 1.66l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"></path>
              </svg>
              <span className="font-label text-sm font-semibold text-on-surface">Google</span>
            </button>
            <button className="flex items-center justify-center gap-3 py-3 px-4 bg-surface-container-lowest rounded-lg border border-outline-variant/10 hover:bg-surface-container-low transition-all duration-300 hover:scale-[1.01]">
              <svg className="w-5 h-5" viewBox="0 0 24 24">
                <path d="M17.05 20.28c-.98.95-2.05 1.71-3.21 1.71-1.13 0-1.5-.68-2.84-.68-1.35 0-1.78.66-2.84.68-1.1.02-2.04-.63-3.07-1.63C3.01 18.3 1.5 15.11 1.5 12.18c0-3.09 2.01-4.73 3.96-4.73 1.04 0 1.88.37 2.68.37.77 0 1.34-.37 2.58-.37 1.2 0 2.21.51 2.92 1.39-2.58 1.13-2.15 4.63.47 5.76-.71 1.72-1.64 3.43-2.56 4.68zM12.03 7.25c-.02-2.23 1.83-4.08 4.04-4.25.17 2.45-2.22 4.49-4.04 4.25z" fill="currentColor"></path>
              </svg>
              <span className="font-label text-sm font-semibold text-on-surface">Apple</span>
            </button>
          </div>

          <div className="mt-12 text-center">
            <p className="text-on-surface-variant text-sm font-body">
              Don't have an account yet?{' '}
              <Link to="/register" className="text-primary font-bold hover:underline ml-1">
                Join the InnSales
              </Link>
            </p>
          </div>
        </div>
      </section>
    </main>
  );
});

export default LoginForm;