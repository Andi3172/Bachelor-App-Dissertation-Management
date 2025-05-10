<template>
  <v-container class="fill-height">
    <v-row justify="center" align="center">
      <v-col cols="12" sm="8" md="6">
        <v-card class="text-center elevation-5">
          <v-card-title class="text-h4 error--text pt-6">
            <v-icon color="error" size="x-large" class="mr-2">mdi-shield-alert</v-icon>
            Access Denied
          </v-card-title>

          <v-card-text class="text-body-1 pa-5">
            <p>You don't have permission to access this page.</p>
            <p class="mb-0">Please contact your administrator if you believe this is an error.</p>
          </v-card-text>

          <v-card-actions class="justify-center pb-6">
            <v-btn color="primary" prepend-icon="mdi-home" @click="$router.push('/')">
              Go to Home
            </v-btn>

            <v-btn v-if="!userStore.isAuthenticated"
                   color="secondary"
                   prepend-icon="mdi-login"
                   class="ml-4"
                   @click="$router.push('/login')">
              Login
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useUserStore } from '@/stores/userStore';

const userStore = useUserStore();
</script>
