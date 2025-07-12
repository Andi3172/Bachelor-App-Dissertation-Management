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
          <template v-slot:item.actions="{ item }">
            <v-btn icon color="error" @click="deleteRequest(item.id)">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
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
    { text: 'Professor', value: 'professorName' },
    { text: 'Actions', value: 'actions', sortable: false }
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

  const deleteRequest = async (id) => {
    if (!confirm('Are you sure you want to delete this request?')) return;
    try {
      await axios.delete(`/api/registrationrequest/${id}`);
      fetchRequests();
    } catch (error) {
      if (error.response && error.response.status === 404) {
        alert('Request not found. It may have already been deleted.');
      } else {
        alert('Failed to delete request.');
      }
      console.error('Failed to delete request:', error);
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


