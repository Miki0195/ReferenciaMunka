import { useSelector, useDispatch } from 'react-redux';
import { useCallback } from 'react';
import {
  initializeAuth,
  loginUser,
  registerUser,
  logoutUser,
  clearError,
  selectCurrentUser,
  selectAuthLoading,
  selectAuthError,
  selectIsAuthenticated,
  selectIsAdmin,
} from '../store/slices/authSlice';
import AuthService from '../services/authService';

export const useAuth = () => {
  const dispatch = useDispatch();
  const currentUser = useSelector(selectCurrentUser);
  const loading = useSelector(selectAuthLoading);
  const error = useSelector(selectAuthError);
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const isAdminState = useSelector(selectIsAdmin);

  const initialize = useCallback(() => {
    dispatch(initializeAuth());
  }, [dispatch]);

  const login = useCallback(async (credentials) => {
    const result = await dispatch(loginUser(credentials));
    if (loginUser.fulfilled.match(result)) {
      return result.payload;
    } else {
      throw new Error(result.payload || 'Failed to login');
    }
  }, [dispatch]);

  const register = useCallback(async (userData) => {
    const result = await dispatch(registerUser(userData));
    if (registerUser.fulfilled.match(result)) {
      return result.payload;
    } else {
      throw new Error(result.payload || 'Failed to register');
    }
  }, [dispatch]);

  const logout = useCallback(async () => {
    await dispatch(logoutUser());
  }, [dispatch]);

  const clearAuthError = useCallback(() => {
    dispatch(clearError());
  }, [dispatch]);

  // Make isAdmin a function to maintain compatibility with existing components
  const isAdmin = useCallback(() => {
    return isAdminState;
  }, [isAdminState]);

  return {
    currentUser,
    loading,
    error,
    isAuthenticated,
    isAdmin, // This is now a function that returns the boolean
    initialize,
    login,
    register,
    logout,
    clearError: clearAuthError,
    // Keep the same interface as the old AuthContext
    isAuthenticated: AuthService.isAuthenticated,
  };
}; 