<template>
  <div class="admin-layout">
    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <v-card class="mb-4">
            <v-card-title class="text-h5">
              <v-icon class="mr-2">mdi-shield-account</v-icon>
              Administration Panel
            </v-card-title>
            <v-card-subtitle>
              Manage users, departments, and registration sessions
            </v-card-subtitle>

            <v-tabs v-model="activeTab" bg-color="error" align-tabs="center">
              <v-tab value="dashboard" to="/admin/dashboard">
                <v-icon start>mdi-view-dashboard</v-icon>
                Dashboard
              </v-tab>
              <v-tab value="users" to="/admin/users">
                <v-icon start>mdi-account-group</v-icon>
                Users
              </v-tab>
              <v-tab value="departments" to="/admin/departments">
                <v-icon start>mdi-domain</v-icon>
                Departments
              </v-tab>
              <v-tab value="registrations" to="/admin/registrations">
                <v-icon start>mdi-clipboard-text</v-icon>
                Registrations
              </v-tab>
            </v-tabs>
          </v-card>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <!-- Router view for nested routes -->
          <router-view></router-view>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script setup lang="ts">import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useUserStore } from '@/stores/userStore';

const route = useRoute();
const userStore = useUserStore();
const activeTab = ref(null);

onMounted(() => {
  // Set active tab based on current route
  const path = route.path;
  if (path.includes('/dashboard')) {
    activeTab.value = 'dashboard';
  } else if (path.includes('/users')) {
    activeTab.value = 'users';
  } else if (path.includes('/departments')) {
    activeTab.value = 'departments';
  } else if (path.includes('/registrations')) {
    activeTab.value = 'registrations';
  }

  // Ensure the user data is loaded
  if (!userStore.user) {
    userStore.initializeFromToken();
  }
});</script>

<style scoped>
  .admin-layout {
    padding: 16px;
  }
</style>
