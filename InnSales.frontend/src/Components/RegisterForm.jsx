import { useState } from 'react';
import { registerUser } from '../services/authService';
import { useNavigate, Link } from 'react-router-dom';

export default function RegisterForm() {
  const [form, setForm] = useState({
    email: '',
    password: '',
    displayName: '',
    department: '',
    officeLocation: ''
  });

  const navigate = useNavigate();

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await registerUser(form);
      navigate('/dashboard'); //  Redirect to dashboard
    } catch (err) {
      alert('Registration failed.');
      console.error(err.response?.data);
    }
  };

  return (
    <main className="min-h-screen w-full flex">
      {/* Left Side: The Artistic Anchor */}
      <section className="hidden lg:flex lg:w-1/2 relative overflow-hidden bg-primary-container items-center justify-center">
        <div className="absolute inset-0 z-0">
          <img 
            alt="Artistic curation visual" 
            className="w-full h-full object-cover mix-blend-overlay opacity-60" 
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuC5xq_AKfqsMPLA1JnwzN6f_bYHGGCcoKsceZ1s9vzR8MTwWd4NABLtrZGr5NUjdGE_No3dk27tmI0lXhxHSplzkxHsLh9nnxm4ypUrAB81beevXiCb1hK_KAny7klWgsZpxDaw1AENoMF4u6YxG7LnglzbE5-SFvDDbc43ckjNPBheslyNsjk_zlySM7TNrqHSq1MOpVkOhoBFgHNTHqvv4hY9idy-pnS_tTiBmg_sGSt_Jm-3Ydw9rxyb_H4lB3OeT7wJYsZIfV4"
          />
          <div className="absolute inset-0 bg-gradient-to-tr from-primary via-primary/40 to-transparent"></div>
        </div>
        <div className="relative z-10 px-16 max-w-2xl">
          <div className="flex flex-col gap-6">
            <div className="flex items-center gap-3">
              <span className="w-12 h-[2px] bg-on-primary-container"></span>
              <span className="font-headline font-bold text-on-primary-container tracking-widest text-sm uppercase">The Digital Atelier</span>
            </div>
            <h1 className="font-headline text-6xl font-extrabold text-white tracking-tighter leading-[0.9]">
              InnSales
            </h1>
            <p className="font-body text-2xl text-on-primary-container/90 font-light leading-relaxed">
              Artistry in every selection.
            </p>
            <div className="mt-8 flex gap-4">
              <div className="p-4 bg-white/70 backdrop-blur-md rounded-xl flex items-center gap-4">
                <div className="w-10 h-10 rounded-full bg-primary flex items-center justify-center text-white">
                  <span className="material-symbols-outlined">brush</span>
                </div>
                <div className="flex flex-col">
                  <span className="text-xs font-bold text-primary tracking-wide">CURATED</span>
                  <span className="text-sm text-on-surface">Bespoke UI Patterns</span>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="absolute bottom-12 left-16 flex items-center gap-4">
          <span className="text-on-primary-container/40 text-[10px] font-label tracking-[0.3em] uppercase">Est. 2024 Atelier Studio</span>
        </div>
      </section>

      {/* Right Side: The Form Canvas */}
      <section className="w-full lg:w-1/2 bg-surface-container-low flex items-center justify-center p-8 md:p-16 lg:p-24 overflow-y-auto">
        <div className="w-full max-w-md space-y-10">
          <div className="lg:hidden mb-8">
            <h2 className="font-headline text-3xl font-extrabold text-primary tracking-tight">InnSales</h2>
          </div>

          <header className="space-y-2">
            <h3 className="font-headline text-3xl font-bold text-on-surface tracking-tight">Create your account</h3>
            <p className="text-on-surface-variant font-body">Join the elite community of digital curators.</p>
          </header>

          <form onSubmit={handleSubmit} className="space-y-6">
            
            {/* Display Name */}
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
              <label className="block text-xs font-bold text-on-surface-variant uppercase ml-1 mb-1" htmlFor="displayName">Full Name</label>
              </div>
              <div className="relative group">
                <input 
                  type="text" 
                  name="displayName"
                  id="displayName"
                  value={form.displayName}
                  onChange={handleChange}
                  placeholder="Evelyn Thorne" 
                  className="w-full px-5 py-4 bg-surface-container-lowest text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary-container/20 focus:outline-none transition-all duration-300 placeholder:text-outline-variant"
                  required
                />
              </div>
            </div>

            {/* Email */}
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
              <label className="block text-xs font-bold text-on-surface-variant uppercase ml-1 mb-1" htmlFor="email">Email Address</label>
              </div>
              <div className="relative">
                <input 
                  type="email" 
                  name="email"
                  id="email"
                  value={form.email}
                  onChange={handleChange}
                  placeholder="evelyn@atelier.com" 
                  className="w-full px-5 py-4 bg-surface-container-lowest text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary-container/20 focus:outline-none transition-all duration-300 placeholder:text-outline-variant"
                  required
                />
              </div>
            </div>

            {/* Department */}
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
              <label className="block text-xs font-bold text-on-surface-variant uppercase ml-1 mb-1" htmlFor="department">Department</label>
              </div>
              <div className="relative group">
                <input 
                  type="text" 
                  name="department"
                  id="department"
                  value={form.department}
                  onChange={handleChange}
                  placeholder="Design Studio" 
                  className="w-full px-5 py-4 bg-surface-container-lowest text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary-container/20 focus:outline-none transition-all duration-300 placeholder:text-outline-variant"
                  required
                />
              </div>
            </div>

            {/* Office Location */}
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
              <label className="block text-xs font-bold text-on-surface-variant uppercase ml-1 mb-1" htmlFor="officeLocation">Office Location</label>
              </div>
              <div className="relative group">
                <input 
                  type="text" 
                  name="officeLocation"
                  id="officeLocation"
                  value={form.officeLocation}
                  onChange={handleChange}
                  placeholder="New York, NY" 
                  className="w-full px-5 py-4 bg-surface-container-lowest text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary-container/20 focus:outline-none transition-all duration-300 placeholder:text-outline-variant"
                  required
                />
              </div>
            </div>

            {/* Password */}
            <div className="space-y-2">
              <div className="flex justify-between items-center ml-1 mb-1">
              <label className="block text-xs font-bold text-on-surface-variant uppercase ml-1 mb-1" htmlFor="password">Password</label>
              </div>
              <div className="relative flex items-center">
                <input 
                  type="password" 
                  name="password"
                  id="password"
                  value={form.password}
                  onChange={handleChange}
                  placeholder="••••••••••••" 
                  className="w-full px-5 py-4 bg-surface-container-lowest text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary-container/20 focus:outline-none transition-all duration-300 placeholder:text-outline-variant"
                  required
                />
                <button type="button" className="absolute right-4 text-outline hover:text-primary transition-all duration-300">
                  <span className="material-symbols-outlined">visibility</span>
                </button>
              </div>
            </div>

            {/* Terms */}
            <div className="flex items-start gap-3 py-2">
              <div className="flex items-center h-5">
                <input type="checkbox" id="terms" required className="w-5 h-5 rounded border-outline-variant text-primary focus:ring-primary/20 bg-surface-container-lowest cursor-pointer" />
              </div>
              <label htmlFor="terms" className="text-sm text-on-surface-variant leading-tight">
                I agree to the <a href="#" className="text-primary font-medium hover:underline">Terms of Service</a> and <a href="#" className="text-primary font-medium hover:underline">Privacy Policy</a>.
              </label>
            </div>

            {/* Submit */}
            <div className="pt-4">
              <button type="submit" className="w-full py-4 bg-gradient-to-r from-primary to-primary-container text-white font-headline font-bold rounded-lg shadow-xl shadow-primary/10 hover:scale-[1.02] active:scale-95 transition-all duration-300">
                Create Account
              </button>
            </div>

            <p className="text-center text-sm text-on-surface-variant mt-8">
              Already a member?{' '}
              <Link to="/login" className="text-primary font-bold hover:underline ml-1">
                Log In
              </Link>
            </p>
          </form>

        </div>
      </section>
    </main>
  );
}