<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import axios from '@/api/axios';
  import { useUserStore } from '@/stores/userStore';

  const userStore = useUserStore();
  const loading = ref(false);
  const requests = ref<any[]>([]);

  const dialog = ref(false);
  const dialogAction = ref<'Approved' | 'Rejected'>('Approved');
  const selectedRequest = ref<any>(null);
  const statusJustification = ref('');

  const headers = [
    { text: 'ID', value: 'id' },
    { text: 'Student', value: 'studentName' },
    { text: 'Theme', value: 'proposedTheme' },
    { text: 'Status', value: 'status' },
    { text: 'Session', value: 'sessionId' },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

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

  const fetchRequests = async () => {
    loading.value = true;
    try {
      const professorId = userStore.user?.id;
      if (!professorId) return;

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
              sessionId: session.id,
              registrationSessionId: r.registrationSessionId,
              studentId: r.student?.userId
            }))
          );
        } catch (err) {
          // If no requests for this session, skip
        }
      }
      requests.value = allRequests;
    } catch (error) {
      requests.value = [];
    } finally {
      loading.value = false;
    }
  };

  const openDialog = (req: any, action: 'Approved' | 'Rejected') => {
    selectedRequest.value = req;
    dialogAction.value = action;
    statusJustification.value = '';
    dialog.value = true;
  };

  const statusToEnum = (status: 'Approved' | 'Rejected') => {
    switch (status) {
      case 'Approved': return 1;
      case 'Rejected': return 2;
      default: return 0;
    }
  };

  const submitStatusUpdate = async () => {
    if (!statusJustification.value || !selectedRequest.value) return;
    try {
      await axios.put(`/api/registrationrequest/${selectedRequest.value.id}`, {
        id: selectedRequest.value.id,
        studentId: selectedRequest.value.studentId,
        registrationSessionId: selectedRequest.value.registrationSessionId,
        proposedTheme: selectedRequest.value.proposedTheme,
        status: statusToEnum(dialogAction.value), // send as integer
        statusJustification: statusJustification.value
      });
      dialog.value = false;
      fetchRequests();
    } catch (error) {
      alert('Failed to update request.');
    }
  };


  onMounted(() => {
    fetchRequests();
  });
</script>

<template>
  <div class="professor-requests-view">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-account-multiple</v-icon>
        Registration Requests for My Active Sessions
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="requests"
                      :loading="loading"
                      class="elevation-1"
                      item-key="id">
          <template v-slot:item.status="{ item }">
            <v-chip :color="getStatusColor(item.status)" outlined>
              {{ getStatusText(item.status) }}
            </v-chip>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn color="success" variant="text" @click="openDialog(item, 'Approved')" :disabled="getStatusText(item.status) !== 'Pending'">
              Approve
            </v-btn>
            <v-btn color="error" variant="text" @click="openDialog(item, 'Rejected')" :disabled="getStatusText(item.status) !== 'Pending'">
              Deny
            </v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Approve/Reject Dialog -->
    <v-dialog v-model="dialog" max-width="500">
      <v-card>
        <v-card-title>
          {{ dialogAction === 'Approved' ? 'Approve' : 'Reject' }} Request
        </v-card-title>
        <v-card-text>
          <div>
            <strong>Student:</strong> {{ selectedRequest?.studentName }}<br>
            <strong>Theme:</strong> {{ selectedRequest?.proposedTheme }}
          </div>
          <v-textarea v-model="statusJustification"
                      label="Status Justification"
                      rows="3"
                      auto-grow
                      class="mt-4"
                      :rules="[v => !!v || 'Justification is required']"
                      required />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="dialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="submitStatusUpdate">Submit</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
  .professor-requests-view {
    padding: 16px;
  }
</style>
