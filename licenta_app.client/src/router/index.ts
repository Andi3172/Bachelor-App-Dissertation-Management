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
          path: '/student/dashboard',
          name: 'student-dashboard',
          component: () => import('@/views/student/DashboardView.vue')
        },
        {
          path: '/student/profile',
          name: 'student-profile',
          component: () => import('@/views/student/ProfileView.vue')
        },
        {
          path: '/student/registration-requests',
          name: 'student-registration-requests',
          component: () => import('@/views/student/StudentRequestView.vue')
        },
        {
          path: '/student/my-requests',
          name: 'student-my-requests',
          component: () => import('@/views/student/StudentMyRequestsView.vue')
        }
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
        },
        {
          path: '/professor/profile',
          name: 'professor-profile',
          component: () => import('@/views/professor/ProfileView.vue')
        },
        {
          path: 'sessions',
          name: 'professor-sessions',
          component: () => import('@/views/professor/ProfessorSessionsView.vue')
        }
      ]
    },
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
        },
        {
          path: '/admin/profile',
          name: 'admin-profile',
          component: () => import('@/views/admin/ProfileView.vue')
        },
        {
          path: '/admin/users',
          name: 'admin-users',
          component: () => import('@/views/admin/AdminUsersView.vue'),
          meta: {
            requiresAuth: true,
            roles: ['Admin']
          }
        },
        {
          path: '/admin/departments',
          name: 'admin-departments',
          component: () => import('@/views/admin/AdminDepartmentsView.vue'),
          meta: {
            requiresAuth: true,
            roles: ['Admin']
          }
        }
        
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue')
    }
  ]
});

setupAuthGuards(router);

export default router;
