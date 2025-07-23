import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { useEffect } from 'react';
import { store } from './store/store';
import { useAuth } from './hooks/useAuth';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import './App.css';

// Components
import Navbar from './components/Navbar';
import ProtectedRoute from './components/ProtectedRoute';

// Pages
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import BookingsPage from './pages/BookingsPage';
import AddMoviePage from './pages/AddMoviePage';
import EditMoviePage from './pages/EditMoviePage';
import AddScreeningPage from './pages/AddScreeningPage';
import EditScreeningPage from './pages/EditScreeningPage';
import BookingPage from './pages/BookingPage';
import MovieDetailsPage from './pages/MovieDetailsPage';

// App content component that uses the useAuth hook
const AppContent = () => {
  const { initialize } = useAuth();

  useEffect(() => {
    initialize();
  }, [initialize]);

  return (
    <div className="app">
      <Navbar />
      <main className="py-4 mt-5 pt-5">
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/movie/:id" element={<MovieDetailsPage />} />
          
          {/* Protected routes (user) */}
          <Route element={<ProtectedRoute requireAdmin={false} />}>
            <Route path="/bookings" element={<BookingsPage />} />
            <Route path="/book/:id" element={<BookingPage />} />
          </Route>
          
          {/* Protected routes (admin) */}
          <Route element={<ProtectedRoute requireAdmin={true} />}>
            <Route path="/add-movie" element={<AddMoviePage />} />
            <Route path="/edit-movie/:id" element={<EditMoviePage />} />
            <Route path="/add-screening" element={<AddScreeningPage />} />
            <Route path="/edit-screening/:id" element={<EditScreeningPage />} />
          </Route>
        </Routes>
      </main>
    </div>
  );
};

function App() {
  return (
    <Provider store={store}>
      <Router>
        <AppContent />
      </Router>
    </Provider>
  );
}

export default App;
