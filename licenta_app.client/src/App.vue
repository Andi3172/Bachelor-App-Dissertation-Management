<template>
  <v-app>
    <v-app-bar app
               color="primary"
               dark
               v-if="userStore.isAuthenticated">
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>

      <v-app-bar-title>
        University App
      </v-app-bar-title>

      <v-spacer></v-spacer>

      <v-btn icon @click="userStore.logout(); $router.push('/login')">
        <v-icon>mdi-logout</v-icon>
      </v-btn>
    </v-app-bar>

    <v-navigation-drawer v-model="drawer"
                         app
                         temporary
                         v-if="userStore.isAuthenticated">
      <v-list>
        <v-list-item prepend-avatar="https://randomuser.me/api/portraits/lego/1.jpg"
                     :title="userStore.user?.username || ''"
                     :subtitle="userStore.user?.email || ''">
          <template v-slot:append>
            <v-chip :color="roleColor"
                    size="small">
              {{ userStore.user?.role }}
            </v-chip>
          </template>
        </v-list-item>
      </v-list>

      <v-divider></v-divider>

      <!-- Admin Navigation -->
      <v-list v-if="userStore.isAdmin">
        <v-list-item to="/admin/dashboard" prepend-icon="mdi-view-dashboard">
          Dashboard
        </v-list-item>
        <!-- Add more admin menu items -->
      </v-list>

      <!-- Professor Navigation -->
      <v-list v-else-if="userStore.isProfessor">
        <v-list-item to="/professor/dashboard" prepend-icon="mdi-view-dashboard">
          Dashboard
        </v-list-item>
        <!-- Add more professor menu items -->
      </v-list>

      <!-- Student Navigation -->
      <v-list v-else-if="userStore.isStudent">
        <v-list-item to="/student/dashboard" prepend-icon="mdi-view-dashboard">
          Dashboard
        </v-list-item>
        <!-- Add more student menu items -->
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>

    <v-footer app>
      <span class="text-caption">&copy; {{ new Date().getFullYear() }} University App</span>
    </v-footer>
  </v-app>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useUserStore } from './stores/userStore';

  const router = useRouter();
  const userStore = useUserStore();
  const drawer = ref(false);

  onMounted(() => {
    userStore.initializeFromToken();
  });

  const roleColor = computed(() => {
    switch (userStore.user?.role) {
      case 'Admin':
        return 'error';
      case 'Professor':
        return 'info';
      case 'Student':
        return 'primary';
      default:
        return 'gray';
    }
  });
</script>

<style>
</style>
