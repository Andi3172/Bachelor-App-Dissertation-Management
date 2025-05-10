import apiClient from './axios';

// Types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  // Role is determined on the server side based on email
}

export interface AuthResponse {
  token: string;
}

export interface RegisterResponse {
  message: string;
  token: string;
}

// Auth Service
const authService = {
  /**
   * Login user with email and password
   * @param credentials User login credentials
   * @returns Promise with JWT token
   */
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post<AuthResponse>('/api/auth/login', credentials);
      // Store the token in localStorage
      if (response.data.token) {
        localStorage.setItem('auth_token', response.data.token);
        // Add token to axios default headers for future requests
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
      }
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data || 'Invalid credentials');
      }
      throw new Error('Network error. Please try again.');
    }
  },

  /**
   * Register new user
   * @param userData User registration data
   * @returns Promise with registration result and JWT token
   */
  register: async (userData: RegisterRequest): Promise<RegisterResponse> => {
    try {
      const response = await apiClient.post<RegisterResponse>('/api/auth/register', userData);
      // Store the token in localStorage
      if (response.data.token) {
        localStorage.setItem('auth_token', response.data.token);
        // Add token to axios default headers for future requests
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
      }
      return response.data;
    } catch (error: any) {
      if (error.response && error.response.data) {
        throw new Error(error.response.data);
      }
      throw new Error('Registration failed. Please try again.');
    }
  },

  /**
   * Logout user
   */
  logout: () => {
    localStorage.removeItem('auth_token');
    delete apiClient.defaults.headers.common['Authorization'];
  },

  /**
   * Check if user is authenticated
   * @returns boolean indicating if user has a stored token
   */
  isAuthenticated: (): boolean => {
    return !!localStorage.getItem('auth_token');
  },

  /**
   * Get the current auth token
   * @returns The JWT token or null if not authenticated
   */
  getToken: (): string | null => {
    return localStorage.getItem('auth_token');
  },

  /**
   * Initialize authentication from localStorage on app startup
   */
  initAuth: () => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    }
  }
};

export default authService;
