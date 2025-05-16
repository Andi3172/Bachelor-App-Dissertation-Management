<template>
  <div class="professor-profile">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-account-edit</v-icon>
        Edit Profile
      </v-card-title>

      <v-card-text>
        <v-form @submit.prevent="saveProfile" ref="formRef" v-model="formValid">
          <v-text-field label="Username"
                        v-model="form.username"
                        :rules="[v => !!v || 'Username is required']"
                        prepend-icon="mdi-account"
                        required />
          <v-text-field label="Email"
                        v-model="form.email"
                        type="email"
                        :rules="[v => !!v || 'Email is required']"
                        prepend-icon="mdi-email"
                        required />
          <v-text-field label="Department ID"
                        v-model.number="form.departmentId"
                        prepend-icon="mdi-domain"
                        type="number" />
          <v-text-field label="New Password (optional)"
                        v-model="form.password"
                        type="password"
                        prepend-icon="mdi-lock" />
          <v-text-field label="Confirm Your Password"
                        v-model="form.confirmPassword"
                        type="password"
                        :rules="[v => !!v || 'You must confirm your password to save changes']"
                        prepend-icon="mdi-lock-check"
                        required />

          <v-btn type="submit" color="primary" class="mt-4" :loading="saving">
            Save Changes
          </v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import axios from '@/api/axios';
  import { useUserStore } from '@/stores/userStore';
  import { useRouter } from 'vue-router';

  const userStore = useUserStore();
  const router = useRouter();

  const formRef = ref();
  const formValid = ref(true);
  const saving = ref(false);

  const form = ref({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    departmentId: 0
  });

  const loadProfile = () => {
    const user = userStore.user;
    if (!user) return;

    form.value.username = user.username || '';
    form.value.email = user.email || '';
    form.value.departmentId = user.departmentId || 0;
  };

  const saveProfile = async () => {
    const userId = userStore.user?.id;
    if (!userId || !formRef.value?.validate()) return;

    saving.value = true;

    try {
      const userPayload: any = {
        username: form.value.username,
        email: form.value.email,
        role: userStore.user?.role,
        password: form.value.confirmPassword
      };

      if (form.value.password?.length > 0) {
        userPayload.newPassword = form.value.password;
      }

      await axios.put(`/api/user/${userId}`, userPayload);


      const professorPayload = {
        userId: userId,
        departmentId: form.value.departmentId
      };
      await axios.put(`/api/professor/${userId}`, professorPayload);
      

      await userStore.initializeFromToken();
      router.push('/professor/dashboard');
    } catch (error) {
      console.error('Failed to save profile:', error);
      alert('An error occurred while saving. Please try again.');
    } finally {
      saving.value = false;
    }
  };

  onMounted(async () => {
    if (!userStore.user) {
      await userStore.initializeFromToken();
    }
    loadProfile();
  });
</script>

<style scoped>
  .professor-profile {
    padding: 8px;
  }
</style>
