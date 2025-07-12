<template>
  <v-container fluid>
    <v-row>
      <!-- Registration Sessions -->
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>
            <v-icon start>mdi-calendar-multiselect</v-icon>
            Registration Sessions
          </v-card-title>
          <v-card-text>
            <v-data-table :headers="sessionHeaders"
                          :items="sessions"
                          :items-per-page="8"
                          class="elevation-1"
                          item-value="id">
              <template v-slot:item.status="{ item }">
                <v-chip :color="isSessionExpired(item) ? 'error' : 'success'" small>
                  {{ isSessionExpired(item) ? 'Expired' : 'Active' }}
                </v-chip>
              </template>
              <template v-slot:item.actions="{ item }">
                <v-btn color="error" variant="outlined" @click="confirmDeleteSession(item)">
                  Delete
                </v-btn>
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>

      <!-- Registration Requests -->
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>
            <v-icon start>mdi-file-document-multiple</v-icon>
            Registration Requests
          </v-card-title>
          <v-card-text>
            <v-data-table :headers="requestHeaders"
                          :items="requests"
                          :items-per-page="8"
                          class="elevation-1"
                          item-value="id">
              <template v-slot:item.status="{ item }">
                <v-chip :color="statusColor(item.status)" small>
                  {{ statusLabel(item.status) }}
                </v-chip>
              </template>
              <template v-slot:item.actions="{ item }">
                <v-btn color="info" variant="outlined" @click="openRequestDetails(item.id)">
                  Details
                </v-btn>
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Request Details Dialog -->
    <v-dialog v-model="detailsDialog" max-width="750">
      <v-card>
        <v-card-title>
          <v-icon start color="primary">mdi-information</v-icon>
          Registration Request Details
        </v-card-title>
        <v-divider />
        <v-card-text>
          <template v-if="selectedRequestDetails">
            <v-row>
              <v-col cols="12" md="6">
                <h4 class="mb-2">Request Info</h4>
                <v-list dense>
                  <v-list-item>
                    <v-list-item-title>ID</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.id }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Status</v-list-item-title>
                    <v-list-item-subtitle>
                      <v-chip :color="statusColor(selectedRequestDetails.status)" small>
                        {{ statusLabel(selectedRequestDetails.status) }}
                      </v-chip>
                    </v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Proposed Theme</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.proposedTheme }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Status Justification</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.statusJustification || '-' }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item v-if="selectedRequestDetails.createdAt">
                    <v-list-item-title>Created At</v-list-item-title>
                    <v-list-item-subtitle>{{ formatDate(selectedRequestDetails.createdAt) }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item v-if="selectedRequestDetails.updatedAt">
                    <v-list-item-title>Updated At</v-list-item-title>
                    <v-list-item-subtitle>{{ formatDate(selectedRequestDetails.updatedAt) }}</v-list-item-subtitle>
                  </v-list-item>
                </v-list>
              </v-col>
              <v-col cols="12" md="6">
                <h4 class="mb-2">Student Info</h4>
                <v-list dense>
                  <v-list-item>
                    <v-list-item-title>Username</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.student?.user?.username }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Email</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.student?.user?.email }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Student Number</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.student?.studentNumber }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Department</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.student?.department }}</v-list-item-subtitle>
                  </v-list-item>
                </v-list>
                <h4 class="mb-2 mt-4">Professor Info</h4>
                <v-list dense>
                  <v-list-item>
                    <v-list-item-title>Username</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.registrationSession?.professor?.user?.username }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Email</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.registrationSession?.professor?.user?.email }}</v-list-item-subtitle>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-title>Department ID</v-list-item-title>
                    <v-list-item-subtitle>{{ selectedRequestDetails.registrationSession?.professor?.departmentId }}</v-list-item-subtitle>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
            <v-divider class="my-4" />
            <h4 class="mb-2">Uploaded Documents</h4>
            <v-simple-table dense>
              <thead>
                <tr>
                  <th>Type</th>
                  <th>Status</th>
                  <th>File Name</th>
                  <th>Download</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="type in fileTypes" :key="type">
                  <td>{{ fileTypeLabels[type] }}</td>
                  <td>
                    <v-icon color="success" v-if="requestFiles && requestFiles[type]">mdi-check-circle</v-icon>
                    <v-icon color="error" v-else>mdi-close-circle</v-icon>
                  </td>
                  <td>
                    <span v-if="requestFiles && requestFiles[type]">
                      {{ requestFiles[type].fileName }}
                    </span>
                    <span v-else>
                      Not uploaded
                    </span>
                  </td>
                  <td>
                    <v-btn v-if="requestFiles && requestFiles[type]" size="x-small" icon="mdi-download" @click="downloadFile(requestFiles[type].id)" />
                  </td>
                </tr>
              </tbody>
            </v-simple-table>
            <div v-if="!requestFiles" class="mt-2">
              <v-progress-circular indeterminate color="primary" size="20" />
            </div>
          </template>
          <template v-else>
            <v-row align="center" justify="center">
              <v-col class="text-center">
                <v-progress-circular indeterminate color="primary" />
                <div>Loading details...</div>
              </v-col>
            </v-row>
          </template>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="detailsDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Session Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="600">
      <v-card>
        <v-card-title class="red--text">
          <v-icon start color="error">mdi-alert</v-icon>
          Confirm Delete Session
        </v-card-title>
        <v-divider />
        <v-card-text>
          <div v-if="linkedRequests.length">
            <v-alert type="warning" class="mb-4">
              <strong>This session has registration requests linked to it.</strong><br>
              Deleting the session will also remove these requests:
            </v-alert>
            <v-simple-table dense>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Student</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="req in linkedRequests" :key="req.id">
                  <td>{{ req.id }}</td>
                  <td>{{ req.student?.user?.username }}</td>
                  <td>
                    <v-chip :color="statusColor(req.status)" small>
                      {{ statusLabel(req.status) }}
                    </v-chip>
                  </td>
                </tr>
              </tbody>
            </v-simple-table>
            <v-alert type="error" class="mt-4">
              Are you sure you want to <strong>delete this session and all its requests</strong>?
            </v-alert>
          </div>
          <div v-else>
            <v-alert type="warning" class="mb-0">
              Are you sure you want to delete this session?
            </v-alert>
          </div>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteSessionConfirmed" variant="elevated">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import axios from '@/api/axios';

  const sessions = ref([]);
  const requests = ref([]);
  const detailsDialog = ref(false);
  const selectedRequestId = ref<number | null>(null);
  const selectedRequestDetails = ref<any>(null);
  const requestFiles = ref<Record<string, any> | null>(null);

  const deleteDialog = ref(false);
  const sessionToDelete = ref<any>(null);
  const linkedRequests = ref<any[]>([]);

  const fileTypes = ['academic-default', 'academic-signed', 'progress'];
  const fileTypeLabels = {
    'academic-default': 'Academic Default',
    'academic-signed': 'Academic Signed',
    'progress': 'Progress'
  };

  const sessionHeaders = [
    { text: 'ID', value: 'id' },
    { text: 'Professor', value: 'professor.user.username' },
    { text: 'Start Date', value: 'startDate' },
    { text: 'End Date', value: 'endDate' },
    { text: 'Status', value: 'status', sortable: false },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

  const requestHeaders = [
    { text: 'ID', value: 'id' },
    { text: 'Student', value: 'student.user.username' },
    { text: 'Professor', value: 'registrationSession.professor.user.username' },
    { text: 'Status', value: 'status', sortable: false },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

  function isSessionExpired(session: any) {
    return new Date(session.endDate) < new Date();
  }

  function statusLabel(status: any) {
    if (typeof status === 'string') return status;
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Approved';
      case 2: return 'Rejected';
      default: return status;
    }
  }
  function statusColor(status: any) {
    if (typeof status === 'string') {
      if (status === 'Pending') return 'warning';
      if (status === 'Approved') return 'success';
      if (status === 'Rejected') return 'error';
      return 'primary';
    }
    switch (status) {
      case 0: return 'warning';
      case 1: return 'success';
      case 2: return 'error';
      default: return 'primary';
    }
  }

  function formatDate(dateStr: string) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return d.toLocaleString();
  }

  const fetchSessions = async () => {
    const res = await axios.get('/api/registrationsession');
    sessions.value = res.data;
  };
  const fetchRequests = async () => {
    const res = await axios.get('/api/registrationrequest');
    requests.value = res.data;
  };

  const confirmDeleteSession = async (session: any) => {
    sessionToDelete.value = session;
    // Fetch linked requests for this session
    const res = await axios.get(`/api/registrationrequest/by-session/${session.id}`);
    linkedRequests.value = res.data;
    deleteDialog.value = true;
  };

  const deleteSessionConfirmed = async () => {
    if (!sessionToDelete.value) return;
    await axios.delete(`/api/registrationsession/${sessionToDelete.value.id}`);
    deleteDialog.value = false;
    sessionToDelete.value = null;
    linkedRequests.value = [];
    fetchSessions();
    fetchRequests();
  };

  const openRequestDetails = async (id: number) => {
    selectedRequestId.value = id;
    selectedRequestDetails.value = null;
    requestFiles.value = null;
    detailsDialog.value = true;
    // Fetch full request details
    const res = await axios.get(`/api/registrationrequest/${id}`);
    selectedRequestDetails.value = res.data;
    // Fetch files for this request
    const filesRes = await axios.get(`/api/fileupload/by-request/${id}`);
    requestFiles.value = filesRes.data.files;
  };

  const downloadFile = async (fileId: number) => {
    const res = await axios.get(`/api/fileupload/download/${fileId}`, { responseType: 'blob' });
    const url = window.URL.createObjectURL(new Blob([res.data]));
    const link = document.createElement('a');
    let fileName = 'file';
    if (requestFiles.value) {
      for (const type of fileTypes) {
        if (requestFiles.value[type]?.id === fileId) {
          fileName = requestFiles.value[type].fileName;
          break;
        }
      }
    }
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
  };

  onMounted(() => {
    fetchSessions();
    fetchRequests();
  });
</script>

<style scoped>
  .v-card {
    margin-bottom: 24px;
  }

  h4 {
    font-weight: 600;
    color: #1976d2;
  }
</style>
