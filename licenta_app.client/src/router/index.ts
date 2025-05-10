import { createRouter, createWebHistory } from 'vue-router';
import { setupAuthGuards } from './auth-guard';
import HomeView from '@/views/HomeView.vue';
import LoginView from '@/views/LoginView.vue';
import RegisterView from '@/views/RegisterView.vue';
import UnauthorizedView from '@/views/UnauthorizedView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView
    },
    {
      path: '/unauthorized',
      name: 'unauthorized',
      component: UnauthorizedView
    },
    // Student routes
    {
      path: '/student',
      name: 'student',
      component: () => import('@/views/student/StudentLayout.vue'),
      meta: {
        requiresAuth: true,
        roles: ['Student']
      },
      children: [
        {
          path: 'dashboard',
          name: 'student-dashboard',
          component: () => import('@/views/student/DashboardView.vue')
        },
        {
          path: '/student/profile',
          name: 'student-profile',
          component: () => import('@/views/student/ProfileView.vue')
        }
        // Add more student-specific routes as needed
      ]
    },
    // Professor routes
    {
      path: '/professor',
      name: 'professor',
      component: () => import('@/views/professor/ProfessorLayout.vue'),
      meta: {
        requiresAuth: true,
        roles: ['Professor']
      },
      children: [
        {
          path: 'dashboard',
          name: 'professor-dashboard',
          component: () => import('@/views/professor/DashboardView.vue')
        }
        // Add more professor-specific routes as needed
      ]
    },
    // Admin routes
    {
      path: '/admin',
      name: 'admin',
      component: () => import('@/views/admin/AdminLayout.vue'),
      meta: {
        requiresAuth: true,
        roles: ['Admin']
      },
      children: [
        {
          path: 'dashboard',
          name: 'admin-dashboard',
          component: () => import('@/views/admin/DashboardView.vue')
        }
        // Add more admin-specific routes as needed
      ]
    },
    // Catch-all route for 404
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue')
    }
  ]
});

// Set up authentication guards
setupAuthGuards(router);

export default router;
