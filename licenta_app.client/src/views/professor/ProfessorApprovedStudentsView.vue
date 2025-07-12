<template>
  <v-container>
    <v-card>
      <v-card-title>My Approved Students</v-card-title>
      <v-data-table :headers="headers"
                    :items="requests"
                    :loading="loading"
                    class="elevation-1">
        <template #item.student="{ item }">
          {{ item.student?.user?.username }}
        </template>
        <template #item.actions="{ item }">
          <v-btn color="primary" @click="goToRequest(item.id)">View</v-btn>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">import { ref, onMounted } from 'vue';
import { useUserStore } from '@/stores/userStore';
import axios from '@/api/axios';
import { useRouter } from 'vue-router';

const router = useRouter();
const userStore = useUserStore();
const requests = ref([]);
const loading = ref(true);

const headers = [
  { text: 'Request ID', value: 'id' },
  { text: 'Student', value: 'student' },
  { text: 'Theme', value: 'proposedTheme' },
  { text: 'Status', value: 'status' },
  { text: 'Actions', value: 'actions', sortable: false }
];

const fetchRequests = async () => {
  loading.value = true;
  const professorId = userStore.user?.id;
  if (!professorId) return;
  const res = await axios.get(`/api/registrationrequest/approved-by-professor/${professorId}`);
  requests.value = res.data;
  loading.value = false;
};

const goToRequest = (id: number) => {
  router.push(`/professor/approved-students/${id}`);
};

onMounted(fetchRequests);</script>
