import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import authService, { type LoginRequest, type RegisterRequest } from '@/api/auth';
import { jwtDecode } from 'jwt-decode';
import axios from '../api/axios';

interface UserState {
  id?: string;
  email?: string;
  role?: string;
  username?: string;
  studentNumber?: string;
  department?: string;
}

interface JwtPayload {
  sub: string; // userId
  email: string;
  role: string;
  exp: number;
  // Add any other claims your JWT contains
}

export const useUserStore = defineStore('user', () => {
  const user = ref<UserState | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Computed properties
  const isAuthenticated = computed(() => !!user.value);
  const isAdmin = computed(() => user.value?.role === 'Admin');
  const isStudent = computed(() => user.value?.role === 'Student');
  const isProfessor = computed(() => user.value?.role === 'Professor');

  // Actions
  async function login(credentials: LoginRequest) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await authService.login(credentials);
      const decoded = jwtDecode<JwtPayload>(response.token);

      user.value = {
        id: decoded.sub,
        email: decoded.email,
        role: decoded.role
      };

      return true;
    } catch (err: any) {
      error.value = err.response?.data || 'Login failed';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function register(userData: RegisterRequest) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await authService.register(userData);
      const decoded = jwtDecode<JwtPayload>(response.token);

      user.value = {
        id: decoded.sub,
        email: decoded.email,
        role: decoded.role
      };

      return true;
    } catch (err: any) {
      error.value = err.response?.data || 'Registration failed';
      return false;
    } finally {
      isLoading.value = false;
    }
  }
  async function initializeFromToken() {
    const token = authService.getToken();
    if (!token) return;

    const decoded = jwtDecode<JwtPayload>(token);
    const now = Date.now() / 1000;
    if (decoded.exp < now) {
      authService.logout();
      return;
    }

    // 1) fetch the core User record
    const userRes = await axios.get(`/api/user/${decoded.sub}`);
    const core = userRes.data as { id: number, username: string, email: string, role: string };

    // 2) if student, also fetch student details
    if (core.role === 'Student') {
      const studRes = await axios.get(`/api/student/${core.id}`);
      user.value = {
        id: String(core.id),
        username: core.username,
        email: core.email,
        role: core.role,
        studentNumber: studRes.data.studentNumber,
        department: studRes.data.department
      };
    } else {
      user.value = {
        id: String(core.id),
        username: core.username,
        email: core.email,
        role: core.role
      };
    }
  }
  function logout() {
    authService.logout();
    user.value = null;
  }

  

  return {
    user,
    isLoading,
    error,
    isAuthenticated,
    isAdmin,
    isStudent,
    isProfessor,
    login,
    register,
    logout,
    initializeFromToken
  };
});
