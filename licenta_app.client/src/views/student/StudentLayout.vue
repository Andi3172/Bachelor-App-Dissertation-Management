<template>
  <div class="student-layout">
    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <v-card class="mb-4">
            <v-card-title class="text-h5">
              <v-icon class="mr-2">mdi-school</v-icon>
              Student Portal
            </v-card-title>
            <v-card-subtitle>
              Access your academic information and manage your thesis registration
            </v-card-subtitle>

            <v-tabs v-model="activeTab" bg-color="primary" align-tabs="center">
              <v-tab value="dashboard" to="/student/dashboard">
                <v-icon start>mdi-view-dashboard</v-icon>
                Dashboard
              </v-tab>
              <v-tab value="profile" to="/student/profile">
                <v-icon start>mdi-account</v-icon>
                Profile
              </v-tab>
              <v-tab value="registration" to="/student/registration">
                <v-icon start>mdi-clipboard-text</v-icon>
                Thesis Registration
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
  } else if (path.includes('/profile')) {
    activeTab.value = 'profile';
  } else if (path.includes('/registration')) {
    activeTab.value = 'registration';
  }

  // Ensure the user data is loaded
  if (!userStore.user) {
    userStore.initializeFromToken();
  }
});</script>

<style scoped>
  .student-layout {
    padding: 16px;
  }
</style>
