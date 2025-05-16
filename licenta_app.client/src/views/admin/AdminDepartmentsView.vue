<template>
  <div class="admin-departments">
    <v-card>
      <v-card-title class="text-h6">
        <v-icon start>mdi-domain</v-icon>
        Manage Departments
      </v-card-title>

      <v-card-text>
        <v-btn color="primary" class="mb-4" @click="openCreateDialog">
          <v-icon start>mdi-plus</v-icon>
          Add Department
        </v-btn>

        <v-data-table :headers="headers"
                      :items="departments"
                      :items-per-page="10"
                      class="elevation-1">
          <template v-slot:item.headOfDepartment="{ item }">
            <span v-if="item.headOfDepartment && item.headOfDepartment.user">
              {{ item.headOfDepartment.user.username }}
            </span>
            <span v-else>
              <em>Not Assigned</em>
            </span>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn icon color="info" @click="openEditDialog(item)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
            <v-btn icon color="error" @click="confirmDelete(item.departmentId)">
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
          <span v-if="editing">Edit Department</span>
          <span v-else>Create Department</span>
        </v-card-title>
        <v-card-text>
          <v-text-field v-model="form.departmentName"
                        label="Department Name"
                        required />
          <v-autocomplete v-model="form.headOfDepartmentId"
                          :items="professors"
                          item-value="userId"
                          item-title="displayName"
                          label="Head of Department"
                          clearable />

        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="dialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="saveDepartment">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this department?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteDepartment">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import axios from '@/api/axios';

  const departments = ref([]);
  const professors = ref([]);
  const dialog = ref(false);
  const deleteDialog = ref(false);
  const editing = ref(false);
  const form = ref({ departmentName: '', headOfDepartmentId: null });
  const selectedDepartmentId = ref<number | null>(null);

  const headers = [
    { text: 'ID', value: 'departmentId' },
    { text: 'Name', value: 'departmentName' },
    { text: 'Head of Department', value: 'headOfDepartment' },
    { text: 'Actions', value: 'actions', sortable: false }
  ];

  const fetchDepartments = async () => {
    try {
      const response = await axios.get('/api/department');
      departments.value = response.data;
    } catch (error) {
      alert('Failed to fetch departments.');
    }
  };

  const fetchProfessors = async () => {
    try {
      const response = await axios.get('/api/professor');
      professors.value = response.data.map((prof) => ({
        userId: prof.userId,
        displayName: prof.user && prof.user.username
          ? prof.user.username
          : `Professor #${prof.userId}`
      }));
    } catch (error) {
      alert('Failed to fetch professors.');
    }
  };


  const openCreateDialog = () => {
    form.value = { departmentName: '', headOfDepartmentId: null };
    editing.value = false;
    dialog.value = true;
  };

  const openEditDialog = (department) => {
    form.value = {
      departmentName: department.departmentName,
      headOfDepartmentId: department.headOfDepartmentId ?? null
    };
    selectedDepartmentId.value = department.departmentId;
    editing.value = true;
    dialog.value = true;
  };

  const saveDepartment = async () => {
    try {
      if (editing.value) {
        await axios.put(`/api/department/${selectedDepartmentId.value}`, form.value);
        alert('Department updated successfully.');
      } else {
        await axios.post('/api/department', form.value);
        alert('Department created successfully.');
      }
      dialog.value = false;
      fetchDepartments();
    } catch (error) {
      alert('Failed to save department.');
    }
  };

  const confirmDelete = (id) => {
    selectedDepartmentId.value = id;
    deleteDialog.value = true;
  };

  const deleteDepartment = async () => {
    try {
      await axios.delete(`/api/department/${selectedDepartmentId.value}`);
      alert('Department deleted successfully.');
      deleteDialog.value = false;
      fetchDepartments();
    } catch (error) {
      alert('Failed to delete department.');
    }
  };

  onMounted(() => {
    fetchDepartments();
    fetchProfessors();
  });
</script>

<style scoped>
  .admin-departments {
    padding: 16px;
  }
</style>
