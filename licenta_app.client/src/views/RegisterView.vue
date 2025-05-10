<template>
  <v-container class="fill-height">
    <v-row justify="center" align="center">
      <v-col cols="12" sm="10" md="8" lg="6">
        <v-card class="elevation-12 rounded-lg">
          <v-card-title class="text-center text-h5 pt-6">Create an Account</v-card-title>

          <v-card-text>
            <v-form @submit.prevent="handleRegister" v-model="isFormValid" lazy-validation>
              <!-- Email field -->
              <v-text-field v-model="form.email"
                            :rules="emailRules"
                            label="Email"
                            prepend-inner-icon="mdi-email"
                            variant="outlined"
                            required
                            type="email"
                            hint="Your role will be determined by your email domain"
                            persistent-hint></v-text-field>

              <!-- Role information chip group -->
              <div class="d-flex flex-wrap gap-2 mb-4">
                <v-chip size="small" color="primary" variant="outlined">
                  <v-icon start size="small">mdi-school</v-icon>
                  stud.ase.ro: Student
                </v-chip>
                <v-chip size="small" color="error" variant="outlined">
                  <v-icon start size="small">mdi-shield</v-icon>
                  admin: Admin
                </v-chip>
                <v-chip size="small" color="info" variant="outlined">
                  <v-icon start size="small">mdi-teach</v-icon>
                  Other: Professor
                </v-chip>
              </div>

              <!-- Username field -->
              <v-text-field v-model="form.username"
                            :rules="usernameRules"
                            label="Username"
                            prepend-inner-icon="mdi-account"
                            variant="outlined"
                            required></v-text-field>

              <!-- Password field -->
              <v-text-field v-model="form.password"
                            :rules="passwordRules"
                            label="Password"
                            prepend-inner-icon="mdi-lock"
                            variant="outlined"
                            required
                            :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
                            @click:append-inner="showPassword = !showPassword"
                            :type="showPassword ? 'text' : 'password'"></v-text-field>

              <!-- Confirm Password field -->
              <v-text-field v-model="form.confirmPassword"
                            :rules="confirmPasswordRules"
                            label="Confirm Password"
                            prepend-inner-icon="mdi-lock-check"
                            variant="outlined"
                            required
                            :append-inner-icon="showConfirmPassword ? 'mdi-eye-off' : 'mdi-eye'"
                            @click:append-inner="showConfirmPassword = !showConfirmPassword"
                            :type="showConfirmPassword ? 'text' : 'password'"></v-text-field>

              <!-- Error alert -->
              <v-alert v-if="userStore.error"
                       type="error"
                       variant="tonal"
                       density="compact"
                       class="mb-3">
                {{ userStore.error }}
              </v-alert>

              <!-- Submit button -->
              <v-btn color="primary"
                     variant="elevated"
                     size="large"
                     block
                     type="submit"
                     :loading="userStore.isLoading"
                     :disabled="!isFormValid"
                     class="mt-4">
                {{ userStore.isLoading ? 'Registering...' : 'Register' }}
              </v-btn>
            </v-form>
          </v-card-text>

          <v-card-actions class="justify-center pb-6">
            <div class="text-body-2">
              Already have an account?
              <router-link to="/login" class="text-primary">Login</router-link>
            </div>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '@/stores/userStore';

const router = useRouter();
const userStore = useUserStore();

const form = ref({
  email: '',
  username: '',
  password: '',
  confirmPassword: ''
});

const isFormValid = ref(false);
const showPassword = ref(false);
const showConfirmPassword = ref(false);

// Form validation rules
const emailRules = [
  (v: string) => !!v || 'Email is required',
  (v: string) => /.+@.+\..+/.test(v) || 'Email must be valid'
];

const usernameRules = [
  (v: string) => !!v || 'Username is required',
  (v: string) => v.length >= 3 || 'Username must be at least 3 characters'
];

const passwordRules = [
  (v: string) => !!v || 'Password is required',
  (v: string) => v.length >= 6 || 'Password must be at least 6 characters'
];

const confirmPasswordRules = [
  (v: string) => !!v || 'Please confirm your password',
  (v: string) => v === form.value.password || "Passwords don't match"
];

const handleRegister = async () => {
  const success = await userStore.register({
    email: form.value.email,
    username: form.value.username,
    password: form.value.password
  });

  if (success) {
    // Redirect based on user role
    const role = userStore.user?.role;

    if (role === 'Admin') {
      router.push('/admin/dashboard');
    } else if (role === 'Professor') {
      router.push('/professor/dashboard');
    } else {
      router.push('/student/dashboard');
    }
  }
};
</script>

<style scoped>
  /* Any additional custom styles if needed */
</style>
