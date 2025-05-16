<template>
  <v-container class="fill-height">
    <v-row justify="center" align="center">
      <v-col cols="12" sm="8" md="6">
        <v-card class="elevation-12 rounded-lg text-center">
          <v-card-title class="text-h4 pt-6 pb-2">
            <v-icon size="x-large" color="error" class="mb-2">mdi-alert-circle</v-icon>
            <div>404</div>
          </v-card-title>

          <v-card-subtitle class="text-h5 pb-4">
            Page Not Found
          </v-card-subtitle>

          <v-card-text>
            <p class="text-body-1 mb-6">
              The page you are looking for doesn't exist or has been moved.
            </p>

            <v-img src="https://cdn.vuetifyjs.com/images/illustrations/desert.svg"
                   height="200"
                   class="mb-6"
                   contain></v-img>

            <v-btn color="primary"
                   size="large"
                   variant="elevated"
                   to="/"
                   class="mb-4"
                   prepend-icon="mdi-home">
              Go to Home
            </v-btn>
          </v-card-text>

          <v-card-actions class="justify-center pb-6">
            <div class="text-body-2">
              Need help?
              <a href="mailto:support@university.edu" class="text-primary">Contact Support</a>
            </div>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts"></script>

<style scoped>
  
</style>
