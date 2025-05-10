<!-- src/views/HomeView.vue -->
<template>
  <v-container class="fill-height">
    <v-row justify="center" align="center">
      <v-col cols="12" sm="8" md="6" lg="4">
        <v-card class="elevation-12 rounded-lg">
          <v-card-title class="text-center text-h5 pt-6">
            <v-icon size="x-large" class="mr-2">mdi-school</v-icon>
            University Dissertation Management
          </v-card-title>

          <v-card-text>
            <p class="text-center mb-6">
              Manage your thesis registration and supervision
            </p>

            <v-alert type="info"
                     variant="tonal"
                     density="comfortable"
                     class="mb-6">
              <template #prepend>
                <v-icon color="info">mdi-information</v-icon>
              </template>
              <div>Welcome to the University Dissertation Management System.</div>
              <div class="text-caption mt-1">
                Please log in to access your personalized dashboard based on your role.
              </div>
            </v-alert>

            <v-row dense class="mb-6">
              <v-col cols="12" sm="6">
                <v-btn color="primary"
                       block
                       to="/login"
                       prepend-icon="mdi-login">
                  Login
                </v-btn>
              </v-col>
              <v-col cols="12" sm="6">
                <v-btn color="secondary"
                       block
                       to="/register"
                       prepend-icon="mdi-account-plus">
                  Register
                </v-btn>
              </v-col>
            </v-row>
          </v-card-text>

          <v-card-actions class="justify-center pb-6">
            <div class="text-body-2">
              &copy; {{ new Date().getFullYear() }} University Dissertation Management System
            </div>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
  import { onMounted } from 'vue'
  import { useUserStore } from '@/stores/userStore'
  import { useRouter } from 'vue-router'

  const userStore = useUserStore()
  const router = useRouter()

  onMounted(() => {
    if (userStore.isAuthenticated) {
      const role = userStore.user?.role
      if (role === 'Admin') router.replace('/admin/dashboard')
      else if (role === 'Professor') router.replace('/professor/dashboard')
      else router.replace('/student/dashboard')
    }
  })
</script>

<style scoped>
  p {
    margin: 0;
  }
</style>
