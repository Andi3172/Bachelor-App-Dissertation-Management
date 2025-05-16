<template>
  <v-container class="fill-height">
    <v-row justify="center" align="center">
      <v-col cols="12" sm="8" md="6" lg="4">
        <v-card class="elevation-12 rounded-lg">
          <v-card-title class="text-center text-h5 pt-6">Login</v-card-title>

          <v-card-text>
            <v-form @submit.prevent="handleLogin" v-model="isFormValid" lazy-validation>
              <v-text-field v-model="form.email"
                            :rules="emailRules"
                            label="Email"
                            prepend-inner-icon="mdi-email"
                            variant="outlined"
                            required
                            type="email"></v-text-field>

              <v-text-field v-model="form.password"
                            :rules="passwordRules"
                            label="Password"
                            prepend-inner-icon="mdi-lock"
                            variant="outlined"
                            required
                            :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
                            @click:append-inner="showPassword = !showPassword"
                            :type="showPassword ? 'text' : 'password'"></v-text-field>

              <v-alert v-if="error"
                       type="error"
                       variant="tonal"
                       density="compact"
                       class="mb-3">
                {{ error }}
              </v-alert>

              <v-btn color="primary"
                     variant="elevated"
                     size="large"
                     block
                     type="submit"
                     :loading="isLoading"
                     :disabled="!isFormValid"
                     class="mt-4">
                {{ isLoading ? 'Logging in...' : 'Login' }}
              </v-btn>

              <v-btn color="red"
                     variant="elevated"
                     size="large"
                     block
                     class="mt-2 mb-4"
                     @click="handleGoogleLogin">
                <v-icon left>mdi-google</v-icon>
                Login with Google
              </v-btn>
            </v-form>
          </v-card-text>

          <v-card-actions class="justify-center pb-6">
            <div class="text-body-2">
              Don't have an account?
              <router-link to="/register" class="text-primary">Register</router-link>
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
  import axios from '@/api/axios';
  import apiClient from '@/api/axios';

  const router = useRouter();
  const userStore = useUserStore();

  const form = ref({
    email: '',
    password: ''
  });

  const isFormValid = ref(false);
  const showPassword = ref(false);
  const isLoading = computed(() => userStore.isLoading);
  const error = computed(() => userStore.error);

  // Form validation rules
  const emailRules = [
    (v: string) => !!v || 'Email is required',
    (v: string) => /.+@.+\..+/.test(v) || 'Email must be valid'
  ];

  const passwordRules = [
    (v: string) => !!v || 'Password is required'
  ];

  const handleLogin = async () => {
    const success = await userStore.login({
      email: form.value.email,
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

  const handleGoogleLogin = () => {
    // Initialize Google Identity Services
    google.accounts.id.initialize({
      client_id: '212062145532-0f12s91iusq5gir9dniue9sj9kedr7rc.apps.googleusercontent.com',
      callback: async (response: any) => {
        console.log('Google login response:', response); // Debugging
        if (!response.credential) {
          console.error('No credential received from Google');
          userStore.setError('Google login failed: No credential received');
          return;
        }

        try {
          const res = await axios.post('/api/auth/google-login', {
            idToken: response.credential
          });
          console.log('Backend response:', res.data); // Debugging
          if (res.data.token) {
            console.log("a intrat in if", res.data.token);
            localStorage.setItem('auth_token', res.data.token);
            // Add token to axios default headers for future requests
            apiClient.defaults.headers.common['Authorization'] = `Bearer ${res.data.token}`;
          }
          console.log("111");
          userStore.initializeFromToken();
          console.log("22222");
          router.push('/student/dashboard'); // Redirect to dashboard
        } catch (err: any) {
          console.error('Google login failed:', err.response?.data || err.message); // Debugging
          userStore.setError('Google login failed');
        }
      }
    });
    google.accounts.id.prompt();
  };

</script>

<style scoped>
</style>
