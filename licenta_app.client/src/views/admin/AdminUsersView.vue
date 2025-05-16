<template>
  <div class="admin-users">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-account-group</v-icon>
        Manage Users
      </v-card-title>

      <v-card-text>
        <!-- Search Box -->
        <v-text-field v-model="searchQuery"
                      label="Search by User ID or Username"
                      prepend-icon="mdi-magnify"
                      clearable
                      class="mb-4" />

        <!-- Users Table -->
        <v-data-table :headers="headers"
                      :items="filteredUsers"
                      :items-per-page="10"
                      class="elevation-1"
                      item-value="id">
          <template v-slot:top>
            <v-toolbar flat>
              <v-spacer></v-spacer>
              <v-btn color="primary" @click="fetchUsers" prepend-icon="mdi-refresh">
                Refresh
              </v-btn>
            </v-toolbar>
          </template>

          <template v-slot:item.actions="{ item }">
            <v-btn color="info" variant="outlined" @click="openDetails(item.id)">
              Details
            </v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Details Dialog -->
    <v-dialog v-model="detailsDialog" max-width="600">
      <v-card>
        <v-card-title>
          <v-icon start>mdi-account</v-icon>
          User Details
        </v-card-title>
        <v-card-text>
          <template v-if="selectedUserDetails">
            <v-list dense>
              <v-list-item>
                <v-list-item-title>ID</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.id }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Username</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.username }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Email</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.email }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Role</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.role }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Created At</v-list-item-title>
                <v-list-item-subtitle>{{ formatDate(selectedUserDetails.createdAt) }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Updated At</v-list-item-title>
                <v-list-item-subtitle>{{ formatDate(selectedUserDetails.updatedAt) }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item v-if="selectedUserDetails.student">
                <v-list-item-title>Student Number</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.student.studentNumber }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item v-if="selectedUserDetails.student">
                <v-list-item-title>Student Department</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.student.department }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item v-if="selectedUserDetails.professor">
                <v-list-item-title>Professor Department ID</v-list-item-title>
                <v-list-item-subtitle>{{ selectedUserDetails.professor.departmentId }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
            <v-btn color="error" block class="mt-4" @click="deleteUser(selectedUserDetails.id)">
              Delete User
            </v-btn>
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
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import axios from '@/api/axios';

  const users = ref([]);
  const searchQuery = ref('');
  const detailsDialog = ref(false);
  const selectedUserId = ref<number | null>(null);
  const selectedUserDetails = ref<any>(null);

  const headers = [
    { text: 'ID', value: 'id' },
    { text: 'Username', value: 'username' },
    { text: 'Email', value: 'email' },
    { text: 'Role', value: 'role' },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

  const fetchUsers = async () => {
    try {
      const response = await axios.get('/api/user');
      users.value = response.data;
    } catch (error) {
      alert('An error occurred while fetching users.');
    }
  };

  const filteredUsers = computed(() => {
    if (!searchQuery.value) return users.value;
    return users.value.filter(
      user =>
        user.id.toString().includes(searchQuery.value) ||
        user.username.toLowerCase().includes(searchQuery.value.toLowerCase())
    );
  });

  function formatDate(dateStr: string) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return d.toLocaleString();
  }

  const openDetails = async (id: number) => {
    selectedUserId.value = id;
    selectedUserDetails.value = null;
    detailsDialog.value = true;
    try {
      const response = await axios.get(`/api/user/${id}`);
      selectedUserDetails.value = response.data;
    } catch {
      selectedUserDetails.value = null;
      alert('Failed to fetch user details.');
    }
  };

  const deleteUser = async (id: number) => {
    if (!confirm('Are you sure you want to delete this user?')) return;
    try {
      await axios.delete(`/api/user/${id}`);
      alert('User deleted successfully.');
      detailsDialog.value = false;
      fetchUsers();
    } catch (error) {
      alert('An error occurred while deleting the user.');
    }
  };

  onMounted(fetchUsers);
</script>

<style scoped>
  .admin-users {
    padding: 16px;
  }
</style>
