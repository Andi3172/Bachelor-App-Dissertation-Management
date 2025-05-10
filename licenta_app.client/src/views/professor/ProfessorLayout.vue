<template>
  <div class="professor-layout">
    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <v-card class="mb-4">
            <v-card-title class="text-h5">
              <v-icon class="mr-2">mdi-teach</v-icon>
              Professor Portal
            </v-card-title>
            <v-card-subtitle>
              Manage thesis supervision and registration sessions
            </v-card-subtitle>

            <v-tabs v-model="activeTab" bg-color="info" align-tabs="center">
              <v-tab value="dashboard" to="/professor/dashboard">
                <v-icon start>mdi-view-dashboard</v-icon>
                Dashboard
              </v-tab>
              <v-tab value="profile" to="/professor/profile">
                <v-icon start>mdi-account</v-icon>
                Profile
              </v-tab>
              <v-tab value="sessions" to="/professor/sessions">
                <v-icon start>mdi-calendar-clock</v-icon>
                Registration Sessions
              </v-tab>
              <v-tab value="requests" to="/professor/requests">
                <v-icon start>mdi-clipboard-list</v-icon>
                Student Requests
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
  } else if (path.includes('/sessions')) {
    activeTab.value = 'sessions';
  } else if (path.includes('/requests')) {
    activeTab.value = 'requests';
  }

  // Ensure the user data is loaded
  if (!userStore.user) {
    userStore.initializeFromToken();
  }
});</script>

<style scoped>
  .professor-layout {
    padding: 16px;
  }
</style>
