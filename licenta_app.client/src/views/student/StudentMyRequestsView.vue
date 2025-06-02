<template>
  <div class="student-my-requests">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-file-document"></v-icon>
        My Registration Requests
      </v-card-title>
      <v-card-text>
        <!-- Data Table -->
        <v-data-table :headers="requestHeaders"
                      :items="requests"
                      :items-per-page="10"
                      class="elevation-1">
          <!-- Status Column with Chips -->
          <template v-slot:item.status="{ item }">
            <v-chip :color="getStatusColor(item.status)" outlined>
              {{ item.status }}
            </v-chip>
          </template>
          <!-- Professor Name Column -->
          <template v-slot:item.professorName="{ item }">
            {{ item.professorName }}
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import axios from '@/api/axios';
  import { useUserStore } from '@/stores/userStore';

  const userStore = useUserStore();
  const requests = ref([]);

  const requestHeaders = [
    { text: 'ID', value: 'id' },
    { text: 'Proposed Theme', value: 'proposedTheme' },
    { text: 'Status', value: 'status' },
    { text: 'Professor', value: 'professorName' }
  ];

  const fetchRequests = async () => {
    try {
      const studentId = userStore.user?.id;
      if (!studentId) return;

      const response = await axios.get(`/api/registrationrequest/by-student/${studentId}`);
      requests.value = response.data;
    } catch (error) {
      console.error('Failed to fetch requests:', error);
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Pending': return 'warning';
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
      default: return 'info';
    }
  };

  onMounted(() => {
    fetchRequests();
  });
</script>

<style scoped>
  .student-my-requests {
    padding: 16px;
  }
</style>


