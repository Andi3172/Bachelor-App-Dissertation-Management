<template>
  <div class="student-request-view">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-calendar-clock</v-icon>
        Available Registration Sessions
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="sessionHeaders"
                      :items="sessions"
                      :items-per-page="10"
                      class="elevation-1">
          <template v-slot:item.actions="{ item }">
            <v-btn color="primary" @click="openRequestDialog(item)">
              <v-icon start>mdi-plus</v-icon>
              Request
            </v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <v-btn color="primary" class="mt-4" @click="$router.push('/student/my-requests')">
      <v-icon start>mdi-file-document"></v-icon>
      View My Requests
    </v-btn>

    <!-- Request Dialog -->
    <v-dialog v-model="requestDialog" max-width="500">
      <v-card>
        <v-card-title>
          Submit Registration Request
        </v-card-title>
        <v-card-text>
          <v-text-field v-model="form.proposedTheme" label="Proposed Theme" required />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="requestDialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="submitRequest">Submit</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import axios from '@/api/axios';
  import { useUserStore } from '@/stores/userStore';

  const userStore = useUserStore();
  const sessions = ref([]);
  const requestDialog = ref(false);
  const form = ref({
    registrationSessionId: null,
    proposedTheme: ''
  });

  const sessionHeaders = [
    { text: 'ID', value: 'id' },
    { text: 'Start Date', value: 'startDate' },
    { text: 'End Date', value: 'endDate' },
    { text: 'Professor', value: 'professorName' },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

  const fetchSessions = async () => {
    try {
      const response = await axios.get('/api/registrationsession/active');
      sessions.value = response.data.map(session => ({
        ...session,
        professorName: session.professor?.user?.username || 'Unknown'
      }));
    } catch (error) {
      console.error('Failed to fetch sessions:', error);
    }
  };

  const openRequestDialog = (session) => {
    form.value.registrationSessionId = session.id;
    form.value.proposedTheme = '';
    requestDialog.value = true;
  };

  const submitRequest = async () => {
    try {
      const studentId = userStore.user?.id;
      if (!studentId) return;

      const payload = {
        studentId: studentId,
        registrationSessionId: form.value.registrationSessionId,
        proposedTheme: form.value.proposedTheme || 'Default Theme',
        //status: 'Pending',
        statusJustification: ''
      };

      await axios.post('/api/registrationrequest', payload);
      alert('Request submitted successfully.');
      requestDialog.value = false;
    } catch (error) {
      console.error('Failed to submit request:', error);
      alert('Failed to submit request.');
    }
  };

  onMounted(() => {
    fetchSessions();
  });
</script>

<style scoped>
  .student-request-view {
    padding: 16px;
  }
</style>
