import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7193',
  withCredentials: false, // Changed to false as we're using JWT Bearer token
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

// Request interceptor for adding auth token
apiClient.interceptors.request.use(
  config => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Response interceptor for handling common errors
apiClient.interceptors.response.use(
  response => response,
  error => {
    const { response } = error;

    // Handle unauthorized errors (401)
    if (response && response.status === 401) {
      localStorage.removeItem('auth_token');
      // Optional: Redirect to login page
      // window.location.href = '/login';
    }

    // Handle forbidden errors (403)
    if (response && response.status === 403) {
      console.error('Permission denied');
      // Optional: Redirect to access denied page
    }

    return Promise.reject(error);
  }
);

export default apiClient;
