import { useUserStore } from '@/stores/userStore';
import type { NavigationGuardNext, RouteLocationNormalized } from 'vue-router';

export const authGuard = (to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) => {
  const userStore = useUserStore();

  // Initialize user if not already done
  if (!userStore.user) {
    userStore.initializeFromToken();

  }

  // Check if route requires authentication
  if (to.meta.requiresAuth && !userStore.isAuthenticated) {

    next({ path: '/login', query: { redirect: to.fullPath } });
    return;
  }
  // Check role-based access
  if (to.meta.roles && userStore.user?.role) {
    const authorizedRoles = to.meta.roles as Array<string>;
    if (!authorizedRoles.includes(userStore.user.role)) {
      next({ path: '/unauthorized' });
      return;
    }
  }

  next();
};

export const setupAuthGuards = (router: any) => {
  router.beforeEach(authGuard);
};
