<template>
  <div class="professor-dashboard">
    <v-row class="equal-height-row">
      <!-- Professor Information -->
      <v-col cols="12" md="6" class="d-flex">
        <v-card class="mb-4 flex-grow-1 equal-card">
          <v-card-title class="text-h6">
            <v-icon start>mdi-account-tie</v-icon>
            Professor Information
          </v-card-title>

          <v-card-text v-if="professorData" class="flex-grow-1 d-flex flex-column justify-space-between">
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
                  <v-icon color="primary">mdi-domain</v-icon>
                </template>
                <v-list-item-title>{{ professorData.department || 'Not assigned' }}</v-list-item-title>
                <v-list-item-subtitle>Department</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-book-education</v-icon>
                </template>
                <v-list-item-title>{{ professorData.specialization || 'Not set' }}</v-list-item-title>
                <v-list-item-subtitle>Specialization</v-list-item-subtitle>
              </v-list-item>
            </v-list>
            <div class="edit-profile-btn-container">
              <v-btn color="primary" variant="outlined" prepend-icon="mdi-pencil" @click="editProfile">
                Edit Profile
              </v-btn>
            </div>
          </v-card-text>

          <v-card-text v-else class="text-center flex-grow-1 d-flex flex-column justify-center align-center">
            <v-progress-circular indeterminate color="primary" v-if="loading"></v-progress-circular>
            <v-alert v-else type="warning" variant="tonal" text="Professor information not available"></v-alert>
          </v-card-text>
        </v-card>
      </v-col>

      <!-- Thesis Supervision with Requests Preview -->
      <v-col cols="12" md="6" class="d-flex">
        <v-card class="mb-4 flex-grow-1 equal-card">
          <v-card-title class="text-h6">
            <v-icon start>mdi-account-multiple-check</v-icon>
            Thesis Supervision
          </v-card-title>

          <v-card-text class="flex-grow-1 d-flex flex-column justify-space-between">
            <div>
              <v-alert type="info" variant="tonal" class="mb-4 text-center">
                View and manage your supervised students and their thesis progress.
              </v-alert>
              <div v-if="requestsLoading" class="text-center">
                <v-progress-circular indeterminate color="primary"></v-progress-circular>
              </div>
              <div v-else>
                <v-table density="compact" v-if="requestsPreview.length">
                  <thead>
                    <tr>
                      <th>Student</th>
                      <th>Theme</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="req in requestsPreview" :key="req.id">
                      <td>{{ req.studentName }}</td>
                      <td>{{ req.proposedTheme }}</td>
                      <td>
                        <v-chip :color="getStatusColor(req.status)" size="small" outlined>
                          {{ getStatusText(req.status) }}
                        </v-chip>
                      </td>
                    </tr>
                  </tbody>
                </v-table>
                <div v-else class="text-center text-medium-emphasis">
                  No recent requests.
                </div>
              </div>
            </div>
            <div class="mt-auto">
              <v-btn color="primary" class="mt-4" block prepend-icon="mdi-account-supervisor" @click="$router.push('/professor/professor-requests')">
                Manage Supervision
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Important Dates -->
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

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useUserStore } from '@/stores/userStore';
  import axios from '@/api/axios';

  const router = useRouter();
  const userStore = useUserStore();
  const loading = ref(true);
  const professorData = ref(null);
  const importantDates = ref([
    { title: 'Proposal Review Period', date: 'June 5 – June 20, 2025' },
    { title: 'Thesis Submission Deadline', date: 'July 10, 2025' },
    { title: 'Defense Evaluation Week', date: 'July 25 – July 30, 2025' }
  ]);

  const requestsPreview = ref<any[]>([]);
  const requestsLoading = ref(false);

  const fetchRequestsPreview = async () => {
    requestsLoading.value = true;
    try {
      const professorId = userStore.user?.id;
      if (!professorId) return;

      // Get active sessions for this professor
      const sessionsRes = await axios.get(`/api/registrationsession/by-professor/${professorId}`);
      const now = new Date();
      const activeSessions = sessionsRes.data.filter((s: any) =>
        new Date(s.startDate) <= now && new Date(s.endDate) >= now
      );

      let allRequests: any[] = [];
      for (const session of activeSessions) {
        try {
          const res = await axios.get(`/api/registrationrequest/by-session/${session.id}`);
          allRequests = allRequests.concat(
            res.data.map((r: any) => ({
              id: r.id,
              studentName: r.student?.user?.username || r.student?.user?.email || 'Unknown',
              proposedTheme: r.proposedTheme,
              status: r.status,
            }))
          );
        } catch { }
      }
      // Sort by id descending (latest first), take top 3
      requestsPreview.value = allRequests.sort((a, b) => b.id - a.id).slice(0, 3);
    } finally {
      requestsLoading.value = false;
    }
  };

  const getStatusColor = (status: string | number) => {
    switch (getStatusText(status)) {
      case 'Pending': return 'warning';
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
      default: return 'info';
    }
  };

  const getStatusText = (status: string | number) => {
    if (typeof status === 'string') return status;
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Approved';
      case 2: return 'Rejected';
      default: return status;
    }
  };

  const fetchProfessorData = async () => {
    try {
      loading.value = true;
      const userId = userStore.user?.id;
      if (!userId) return;

      const response = await axios.get(`/api/professor/${userId}`);
      const professor = response.data;

      professorData.value = {
        username: userStore.user?.username,
        email: userStore.user?.email,
        department: professor.department?.departmentName || 'Not assigned',
        specialization: professor.specialization || 'Not set'
      };
    } catch (error) {
      console.error('Error fetching professor data:', error);
    } finally {
      loading.value = false;
    }
  };

  const editProfile = () => {
    router.push('/professor/profile');
  };

  onMounted(async () => {
    if (userStore.user) {
      await userStore.initializeFromToken();
    }
    fetchProfessorData();
    fetchRequestsPreview();
  });
</script>

<style scoped>
  .professor-dashboard {
    padding: 8px;
  }

  .equal-height-row {
    align-items: stretch;
  }

  .equal-card {
    display: flex;
    flex-direction: column;
    height: 100%;
    min-height: 420px;
  }

  .edit-profile-btn-container {
    display: flex;
    justify-content: center;
    margin-top: 32px;
    margin-bottom: 8px;
  }
</style>
