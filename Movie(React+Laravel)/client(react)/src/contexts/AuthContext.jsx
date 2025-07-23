import { createContext, useContext, useState, useEffect } from 'react';
import AuthService from '../services/authService';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchUser = async () => {
      try {
        if (AuthService.isAuthenticated()) {
          const userData = await AuthService.getCurrentUser();
          setCurrentUser(userData);
        } else {
          console.log('No authentication token found');
        }
      } catch (err) {
        console.error("Failed to fetch user:", err);
        localStorage.removeItem('token');
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, []);

  const login = async (credentials) => {
    setError(null);
    try {
      const data = await AuthService.login(credentials);
      
      if (data && data.user) {
        setCurrentUser(data.user);
        return data;
      } else {
        throw new Error('Invalid login response format');
      }
    } catch (err) {
      setError(err.message || 'Failed to login');
      throw err;
    }
  };

  const register = async (userData) => {
    setError(null);
    try {
      const data = await AuthService.register(userData);
      
      if (data && (data.user || data.data?.user)) {
        const user = data.user || data.data?.user;
        setCurrentUser(user);
        return data;
      } else {
        throw new Error('Invalid register response format');
      }
    } catch (err) {
      setError(err.message || 'Failed to register');
      throw err;
    }
  };

  const logout = async () => {
    try {
      await AuthService.logout();
      setCurrentUser(null);
    } catch (err) {
      console.error("Logout error:", err);
    }
  };

  const isAdmin = () => {
    return currentUser?.role === 'admin';
  };

  const value = {
    currentUser,
    loading,
    error,
    login,
    register,
    logout,
    isAdmin,
    isAuthenticated: AuthService.isAuthenticated
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext; 