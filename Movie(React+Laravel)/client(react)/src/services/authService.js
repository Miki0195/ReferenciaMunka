import api from './api';

const AuthService = {
  register: async (userData) => {
    try {
      const response = await api.post('/register', userData);
      
      if (response.data.data?.token) {
        localStorage.setItem('token', response.data.data.token);
      } else if (response.data.token) {
        localStorage.setItem('token', response.data.token);
      }
      return response.data;
    } catch (error) {
      console.error('Register error:', error);
      throw error.response?.data || error;
    }
  },

  login: async (credentials) => {
    try {
      const response = await api.post('/login', credentials);
      
      if (response.data.data?.token) {
        localStorage.setItem('token', response.data.data.token);
        return {
          user: response.data.data.user,
          token: response.data.data.token
        };
      } else if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        return {
          user: response.data.user,
          token: response.data.token
        };
      } else {
        throw new Error('Authentication failed: No token received');
      }
    } catch (error) {
      console.error('Login error:', error);
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      } else if (error.message) {
        throw new Error(error.message);
      } else {
        throw new Error('Authentication failed. Please try again.');
      }
    }
  },

  logout: async () => {
    try {
      const response = await api.post('/logout');
      localStorage.removeItem('token');
      return response.data;
    } catch (error) {
      localStorage.removeItem('token');
      throw error.response?.data || error;
    }
  },

  getCurrentUser: async () => {
    try {
      const response = await api.get('/user');
      console.log('User data response:', response);
      return response.data.data || response.data;
    } catch (error) {
      console.error('Get user error:', error);
      throw error.response?.data || error;
    }
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('token');
  }
};

export default AuthService; 