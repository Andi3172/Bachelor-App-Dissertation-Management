<template>
  <div class="professor-sessions">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-calendar-clock</v-icon>
        My Registration Sessions
      </v-card-title>
      <v-card-text>
        <v-btn color="primary" class="mb-4" @click="openCreateDialog">
          <v-icon start>mdi-plus</v-icon>
          New Session
        </v-btn>
        <v-data-table :headers="headers"
                      :items="sessions"
                      :items-per-page="10"
                      class="elevation-1">
          <template v-slot:item.actions="{ item }">
            <v-btn icon color="info" @click="openEditDialog(item)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
            <v-btn icon color="error" @click="confirmDelete(item.id)">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Create/Edit Dialog -->
    <v-dialog v-model="dialog" max-width="500">
      <v-card>
        <v-card-title>
          <span v-if="editing">Edit Session</span>
          <span v-else>New Session</span>
        </v-card-title>
        <v-card-text>
          <v-menu v-model="startDateMenu"
                  :close-on-content-click="false"
                  transition="scale-transition"
                  offset-y
                  max-width="290px"
                  min-width="290px">
            <template #activator="{ props }">
              <v-text-field v-model="form.startDate"
                            label="Start Date"
                            readonly
                            v-bind="props"></v-text-field>
            </template>
            <v-date-picker v-model="form.startDate"
                           @input="startDateMenu = false"
                           scrollable></v-date-picker>
          </v-menu>

          <v-menu v-model="endDateMenu"
                  :close-on-content-click="false"
                  transition="scale-transition"
                  offset-y
                  max-width="290px"
                  min-width="290px">
            <template #activator="{ props }">
              <v-text-field v-model="form.endDate"
                            label="End Date"
                            readonly
                            v-bind="props"></v-text-field>
            </template>
            <v-date-picker v-model="form.endDate"
                           @input="endDateMenu = false"
                           scrollable></v-date-picker>
          </v-menu>


          <v-text-field v-if="isHeadOfDepartment || userStore.isAdmin"
                        v-model.number="form.maxStudents"
                        label="Max Students"
                        type="number"
                        min="5"
                        max="500"
                        required />
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn text @click="dialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="saveSession">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this session?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteSession">Delete</v-btn>
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
  const isHeadOfDepartment = ref(false);
  const sessions = ref([]);
  const dialog = ref(false);
  const deleteDialog = ref(false);
  const editing = ref(false);
  const startDateMenu = ref(false);
  const endDateMenu = ref(false);
  const form = ref({
    startDate: '',
    endDate: '',
    maxStudents: 1
  });
  const selectedSessionId = ref<number | null>(null);

  const headers = [
    { text: 'ID', value: 'id' },
    { text: 'Start Date', value: 'startDate' },
    { text: 'End Date', value: 'endDate' },
    { text: 'Max Students', value: 'maxStudents' },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

  const fetchSessions = async () => {
    try {
      const professorId = userStore.user?.id;
      if (!professorId) return;
      const response = await axios.get(`/api/registrationsession/by-professor/${professorId}`);
      sessions.value = response.data;
    } catch (error) {
      alert('Failed to fetch sessions.');
    }
  };

  const openCreateDialog = () => {
    form.value = {
      startDate: '',
      endDate: '',
      maxStudents: 5 // default value
    };
    editing.value = false;
    dialog.value = true;
  };

  const openEditDialog = (session) => {
    form.value = {
      startDate: session.startDate.slice(0, 16),
      endDate: session.endDate.slice(0, 16),
      maxStudents: session.maxStudents
    };
    selectedSessionId.value = session.id;
    editing.value = true;
    dialog.value = true;
  };

  const saveSession = async () => {
    try {
      const professorId = userStore.user?.id;
      if (!professorId) return;

      const payload = {
        startDate: new Date(form.value.startDate).toISOString(),
        endDate: new Date(form.value.endDate).toISOString(),
        professorId: Number(professorId)
      };

      if (isHeadOfDepartment.value || userStore.isAdmin) {
        payload.maxStudents = form.value.maxStudents;
      }

      if (editing.value) {
        await axios.put(`/api/registrationsession/${selectedSessionId.value}`, payload);
        alert('Session updated successfully.');
      } else {
        await axios.post('/api/registrationsession', payload);
        alert('Session created successfully.');
      }
      dialog.value = false;
      fetchSessions();
    } catch (error) {
      alert('Failed to save session.');
    }
  };

  const confirmDelete = (id) => {
    selectedSessionId.value = id;
    deleteDialog.value = true;
  };

  const deleteSession = async () => {
    try {
      await axios.delete(`/api/registrationsession/${selectedSessionId.value}`);
      alert('Session deleted successfully.');
      deleteDialog.value = false;
      fetchSessions();
    } catch (error) {
      alert('Failed to delete session.');
    }
  };

  const fetchProfessorDetails = async () => {
    try {
      const professorId = userStore.user?.id;
      if (!professorId) return;

      const response = await axios.get(`/api/professor/${professorId}`);
      const professor = response.data;

      console.log('Professor details: ', professor);

      isHeadOfDepartment.value = professor.department?.headOfDepartmentId === Number(professorId);
    } catch (error) {
      console.error('Failed to fetch professor details: ', error);
    }
  };

  onMounted(() => {
    fetchSessions();
    fetchProfessorDetails();
  });</script>

<style scoped>
  .professor-sessions {
    padding: 16px;
  }
</style>
