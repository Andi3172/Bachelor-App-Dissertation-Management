<template>
  <div class="admin-dashboard">
    <!-- System Overview -->
    <v-row>
      <v-col cols="12">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-monitor-dashboard</v-icon>
            System Overview
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col cols="12" sm="6" md="3">
                <v-card color="primary" variant="tonal" class="text-center pa-4">
                  <div class="text-h4 mb-1">{{ stats.usersCount || 0 }}</div>
                  <div class="text-subtitle-1">Total Users</div>
                  <v-icon size="large" class="mt-2">mdi-account-group</v-icon>
                </v-card>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <v-card color="success" variant="tonal" class="text-center pa-4">
                  <div class="text-h4 mb-1">{{ stats.studentCount || 0 }}</div>
                  <div class="text-subtitle-1">Students</div>
                  <v-icon size="large" class="mt-2">mdi-school</v-icon>
                </v-card>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <v-card color="info" variant="tonal" class="text-center pa-4">
                  <div class="text-h4 mb-1">{{ stats.professorCount || 0 }}</div>
                  <div class="text-subtitle-1">Professors</div>
                  <v-icon size="large" class="mt-2">mdi-teach</v-icon>
                </v-card>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <v-card color="secondary" variant="tonal" class="text-center pa-4">
                  <div class="text-h4 mb-1">{{ stats.departmentCount || 0 }}</div>
                  <div class="text-subtitle-1">Departments</div>
                  <v-icon size="large" class="mt-2">mdi-domain</v-icon>
                </v-card>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Quick Actions -->
    <v-row>
      <v-col cols="12" md="6">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-lightning-bolt</v-icon>
            Quick Actions
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col cols="6">
                <v-btn color="primary" block prepend-icon="mdi-account-plus" @click="$router.push('/admin/users')">
                  Manage Users
                </v-btn>
              </v-col>
              <v-col cols="6">
                <v-btn color="info" block prepend-icon="mdi-domain-plus" @click="$router.push('/admin/departments')">
                  Manage Departments
                </v-btn>
              </v-col>
              <v-col cols="6">
                <v-btn color="success" block prepend-icon="mdi-school" @click="$router.push('/admin/users?role=student')">
                  Manage Students
                </v-btn>
              </v-col>
              <v-col cols="6">
                <v-btn color="error" block prepend-icon="mdi-teach" @click="$router.push('/admin/users?role=professor')">
                  Manage Professors
                </v-btn>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>

      <!-- Admin Profile -->
      <v-col cols="12" md="6">
        <v-card class="mb-4">
          <v-card-title class="text-h6">
            <v-icon start>mdi-account-tie</v-icon>
            Admin Information
          </v-card-title>
          <v-card-text>
            <v-list>
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="error">mdi-account-circle</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.username }}</v-list-item-title>
                <v-list-item-subtitle>Username</v-list-item-subtitle>
              </v-list-item>

              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="error">mdi-email</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.email }}</v-list-item-title>
                <v-list-item-subtitle>Email</v-list-item-subtitle>
              </v-list-item>

              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="error">mdi-shield-account</v-icon>
                </template>
                <v-list-item-title>{{ userStore.user?.role }}</v-list-item-title>
                <v-list-item-subtitle>Role</v-list-item-subtitle>
              </v-list-item>
            </v-list>

            <div class="d-flex justify-end mt-2">
              <v-btn color="error" variant="outlined" prepend-icon="mdi-pencil" @click="editProfile">
                Edit Profile
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Recent Activities -->
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="text-h6">
            <v-icon start>mdi-clock-time-four</v-icon>
            Recent Registration Activities
          </v-card-title>
          <v-card-text>
            <v-alert v-if="loading" type="info" variant="tonal" class="mb-4">
              Loading recent activities...
              <v-progress-linear indeterminate color="primary" class="mt-2"></v-progress-linear>
            </v-alert>

            <template v-else>
              <v-list v-if="recentSessions.length > 0">
                <v-list-subheader>Recent Registration Sessions</v-list-subheader>
                <v-list-item v-for="session in recentSessions" :key="`session-${session.id}`">
                  <template v-slot:prepend>
                    <v-icon color="primary">mdi-calendar-clock</v-icon>
                  </template>
                  <v-list-item-title>
                    Session #{{ session.id }} by {{ session.professor?.user?.username || 'Unknown Professor' }}
                    <v-chip size="x-small" :color="getSessionStatusColor(session)" class="ml-2">
                      {{ getSessionStatus(session) }}
                    </v-chip>
                  </v-list-item-title>
                  <v-list-item-subtitle>
                    {{ formatDateRange(session.startDate, session.endDate) }} - Max Students: {{ session.maxStudents }}
                  </v-list-item-subtitle>
                </v-list-item>

                <v-divider class="my-2"></v-divider>

                <v-list-subheader>Recent Registration Requests</v-list-subheader>
                <v-list-item v-for="request in recentRequests" :key="`request-${request.id}`">
                  <template v-slot:prepend>
                    <v-icon :color="getRequestStatusColor(request)">mdi-clipboard-text</v-icon>
                  </template>
                  <v-list-item-title>
                    {{ request.student?.user?.username || 'Unknown Student' }} - {{ request.proposedTheme }}
                    <v-chip size="x-small" :color="getRequestStatusColor(request)" class="ml-2">
                      {{ request.status }}
                    </v-chip>
                  </v-list-item-title>
                </v-list-item>
              </v-list>
              <v-alert v-else type="info" variant="tonal">
                No recent registration activities found.
              </v-alert>
            </template>

            <v-btn color="primary" block class="mt-4" to="/admin/registrations">
              View All Registration Activity
            </v-btn>
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
const stats = ref({
  usersCount: 0,
  studentCount: 0,
  professorCount: 0,
  departmentCount: 0
});
const recentSessions = ref([]);
const recentRequests = ref([]);

// Fetch dashboard data
const fetchDashboardData = async () => {
  try {
    loading.value = true;

    // Fetch users count
    const usersResponse = await axios.get('/api/user');
    stats.value.usersCount = usersResponse.data.length || 0;

    // Fetch students
    const studentsResponse = await axios.get('/api/student');
    stats.value.studentCount = studentsResponse.data.length || 0;

    // Fetch professors
    const professorsResponse = await axios.get('/api/professor');
    stats.value.professorCount = professorsResponse.data.length || 0;

    // Fetch departments
    const departmentsResponse = await axios.get('/api/department');
    stats.value.departmentCount = departmentsResponse.data.length || 0;

    // Get recent registration sessions
    const sessionsResponse = await axios.get('/api/registrationsession');
    recentSessions.value = sessionsResponse.data.slice(0, 5); // Just take the first 5

    // For recent requests, you would need an endpoint that returns registration requests
    // This is a placeholder assuming such endpoint exists
    try {
      const requestsResponse = await axios.get('/api/registrationrequest');
      recentRequests.value = requestsResponse.data.slice(0, 5);
    } catch (e) {
      console.error('Could not fetch registration requests:', e);
      recentRequests.value = [];
    }

  } catch (error) {
    console.error('Error fetching admin dashboard data:', error);
  } finally {
    loading.value = false;
  }
};

const editProfile = () => {
  router.push('/admin/profile');
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

const getRequestStatusColor = (request) => {
  if (!request || !request.status) return 'grey';

  switch (request.status.toLowerCase()) {
    case 'approved':
      return 'success';
    case 'pending':
      return 'warning';
    case 'rejected':
      return 'error';
    default:
      return 'grey';
  }
};

const formatDateRange = (start, end) => {
  if (!start || !end) return 'Invalid date range';

  const startDate = new Date(start);
  const endDate = new Date(end);

  return `${startDate.toLocaleDateString()} - ${endDate.toLocaleDateString()}`;
};

onMounted(async () => {
  if (!userStore.user) {
    await userStore.initializeFromToken();
  }

  if (userStore.user?.role !== 'Admin') {
    router.push('/unauthorized');
    return;
  }

  fetchDashboardData();
});</script>

<style scoped>
  .admin-dashboard {
    padding: 8px;
  }
</style>
