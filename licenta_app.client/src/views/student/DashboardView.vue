<template>
  <div class="student-dashboard">
    <v-row>
      <v-col cols="12" md="6">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-account</v-icon>
            Student Information
          </v-card-title>

          <v-card-text v-if="studentData">
            <v-list>
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-account-circle</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.username }}</v-list-item-title>
                <v-list-item-subtitle>Username</v-list-item-subtitle>
              </v-list-item>

              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-email</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.email }}</v-list-item-title>
                <v-list-item-subtitle>Email</v-list-item-subtitle>
              </v-list-item>

              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-card-account-details</v-icon>
                </template>
                <v-list-item-title>{{ studentData.studentNumber || 'Not set' }}</v-list-item-title>
                <v-list-item-subtitle>Student Number</v-list-item-subtitle>
              </v-list-item>

              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-domain</v-icon>
                </template>
                <v-list-item-title>{{ studentData.department || 'Not assigned' }}</v-list-item-title>
                <v-list-item-subtitle>Department</v-list-item-subtitle>
              </v-list-item>
            </v-list>

            <v-btn color="primary" variant="outlined" class="mt-4" prepend-icon="mdi-pencil" @click="editProfile">
              Edit Profile
            </v-btn>
          </v-card-text>

          <v-card-text v-else class="text-center">
            <v-progress-circular indeterminate color="primary" v-if="loading"></v-progress-circular>
            <v-alert v-else type="warning" variant="tonal" text="Student information not available"></v-alert>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-book-open-page-variant</v-icon>
            Thesis Registration Status
          </v-card-title>

          <v-card-text class="text-center">
            <v-alert type="info" variant="tonal" class="mb-4">
              Check active registration sessions to register for your thesis.
            </v-alert>

            <v-btn color="primary" prepend-icon="mdi-clipboard-text" @click="$router.push('/student/registration')">
              View Registration Options
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="text-h6">
            <v-icon start>mdi-calendar</v-icon>
            Important Dates
          </v-card-title>

          <v-card-text>
            <v-list>
              <v-list-item v-for="(date, index) in importantDates" :key="index">
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-calendar-clock</v-icon>
                </template>
                <v-list-item-title>{{ date.title }}</v-list-item-title>
                <v-list-item-subtitle>{{ date.date }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '@/stores/userStore';
import axios from '@/api/axios';

const router = useRouter();
const userStore = useUserStore();
const loading = ref(true);
const studentData = ref(null);
const importantDates = ref([
  { title: 'Thesis Registration Deadline', date: 'June 15, 2025' },
  { title: 'Thesis Submission', date: 'July 10, 2025' },
  { title: 'Final Presentation', date: 'July 25, 2025' }
]);

const fetchStudentData = async () => {
  try {
    loading.value = true;
    const userId = userStore.user?.id;
    if (!userId) return;

    const response = await axios.get(`/api/student/${userId}`);
    studentData.value = response.data;
  } catch (error) {
    console.error('Error fetching student data:', error);
  } finally {
    loading.value = false;
  }
};

const editProfile = () => {
  router.push('/student/profile');
};

onMounted(async () => {
  if (!userStore.user) {
    await userStore.initializeFromToken();
  }
  fetchStudentData();
});</script>

<style scoped>
  .student-dashboard {
    padding: 8px;
  }
</style>
