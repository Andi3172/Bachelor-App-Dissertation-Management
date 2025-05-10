<template>
  <div class="professor-dashboard">
    <v-row>
      <v-col cols="12" md="6">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-account</v-icon>
            Professor Information
          </v-card-title>

          <v-card-text v-if="professorData">
            <v-list>
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="info">mdi-account-circle</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.username }}</v-list-item-title>
                <v-list-item-subtitle>Username</v-list-item-subtitle>
              </v-list-item>

              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="info">mdi-email</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.email }}</v-list-item-title>
                <v-list-item-subtitle>Email</v-list-item-subtitle>
              </v-list-item>

              <v-list-item v-if="departmentData">
                <template v-slot:prepend>
                  <v-icon color="info">mdi-domain</v-icon>
                </template>
                <v-list-item-title>{{ departmentData.departmentName || 'Not assigned' }}</v-list-item-title>
                <v-list-item-subtitle>Department</v-list-item-subtitle>
              </v-list-item>

              <v-list-item v-if="isHeadOfDepartment">
                <template v-slot:prepend>
                  <v-icon color="error">mdi-crown</v-icon>
                </template>
                <v-list-item-title>Head of Department</v-list-item-title>
                <v-list-item-subtitle>Administrator Role</v-list-item-subtitle>
              </v-list-item>
            </v-list>

            <v-btn color="info" variant="outlined" class="mt-4" prepend-icon="mdi-pencil" @click="editProfile">
              Edit Profile
            </v-btn>
          </v-card-text>

          <v-card-text v-else class="text-center">
            <v-progress-circular indeterminate color="info" v-if="loading"></v-progress-circular>
            <v-alert v-else type="warning" variant="tonal" text="Professor information not available"></v-alert>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-calendar-clock</v-icon>
            Registration Sessions
          </v-card-title>

          <v-card-text>
            <div v-if="registrationSessions && registrationSessions.length > 0">
              <v-list>
                <v-list-item v-for="session in registrationSessions" :key="session.id">
                  <template v-slot:prepend>
                    <v-icon :color="getSessionStatusColor(session)">
                      {{ getSessionStatusIcon(session) }}
                    </v-icon>
                  </template>

                  <v-list-item-title>
                    Session #{{ session.id }}
                    <v-chip size="x-small" :color="getSessionStatusColor(session)" class="ml-2">
                      {{ getSessionStatus(session) }}
                    </v-chip>
                  </v-list-item-title>

                  <v-list-item-subtitle>
                    {{ formatDateRange(session.startDate, session.endDate) }}
                  </v-list-item-subtitle>

                  <template v-slot:append>
                    <v-chip size="small" color="primary">
                      {{ session.maxStudents }} slots
                    </v-chip>
                  </template>
                </v-list-item>
              </v-list>
            </div>

            <v-alert v-else-if="!loading" type="info" variant="tonal" class="mb-4">
              You don't have any registration sessions yet. Create one to allow students to register for thesis supervision.
            </v-alert>

            <v-progress-circular v-else indeterminate color="info"></v-progress-circular>

            <div class="text-center mt-4">
              <v-btn color="info" prepend-icon="mdi-plus" @click="$router.push('/professor/sessions')">
                Manage Sessions
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="text-h6">
            <v-icon start>mdi-clipboard-list</v-icon>
            Recent Student Requests
          </v-card-title>

          <v-card-text>
            <v-alert v-if="loading" type="info" variant="tonal" class="mb-4">
              Loading student requests...
              <v-progress-linear indeterminate color="info" class="mt-2"></v-progress-linear>
            </v-alert>

            <v-alert v-else-if="!requests || requests.length === 0" type="info" variant="tonal">
              No pending student requests at this time.
            </v-alert>

            <div v-else>
              <!-- Student requests would be shown here -->
              <v-btn color="info" block class="mt-4" to="/professor/requests">
                View All Requests
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '@/stores/userStore';
import axios from '@/api/axios';

const router = useRouter();
const userStore = useUserStore();
const loading = ref(true);
const professorData = ref(null);
const departmentData = ref(null);
const registrationSessions = ref([]);
const requests = ref([]);
const isHeadOfDepartment = ref(false);

// Fetch professor data
const fetchProfessorData = async () => {
  try {
    loading.value = true;
    const userId = userStore.user?.id;
    if (!userId) return;

    // Get professor data
    const professorResponse = await axios.get(`/api/professor/${userId}`);
    professorData.value = professorResponse.data;

    // Get department data if professor has a department
    if (professorData.value?.departmentId) {
      const departmentResponse = await axios.get(`/api/professor/${userId}/department`);
      departmentData.value = departmentResponse.data;

      // Check if professor is head of department
      isHeadOfDepartment.value =
        departmentData.value?.headOfDepartmentId === professorData.value?.userId;
    }

    // Get registration sessions
    const sessionsResponse = await axios.get(`/api/professor/${userId}/registration-sessions`);
    registrationSessions.value = sessionsResponse.data;
  } catch (error) {
    console.error('Error fetching professor data:', error);
  } finally {
    loading.value = false;
  }
};

const editProfile = () => {
  router.push('/professor/profile');
};

// Helper functions for session display
const getSessionStatus = (session) => {
  const now = new Date();
  const startDate = new Date(session.startDate);
  const endDate = new Date(session.endDate);

  if (now < startDate) return 'Upcoming';
  if (now > endDate) return 'Closed';
  return 'Active';
};

const getSessionStatusColor = (session) => {
  const status = getSessionStatus(session);
  if (status === 'Active') return 'success';
  if (status === 'Upcoming') return 'info';
  return 'grey';
};

const getSessionStatusIcon = (session) => {
  const status = getSessionStatus(session);
  if (status === 'Active') return 'mdi-calendar-check';
  if (status === 'Upcoming') return 'mdi-calendar-clock';
  return 'mdi-calendar-remove';
};

const formatDateRange = (start, end) => {
  const startDate = new Date(start);
  const endDate = new Date(end);

  return `${startDate.toLocaleDateString()} - ${endDate.toLocaleDateString()}`;
};

onMounted(async () => {
  if (!userStore.user) {
    await userStore.initializeFromToken();
  }
  fetchProfessorData();
});</script>

<style scoped>
  .professor-dashboard {
    padding: 8px;
  }
</style>
