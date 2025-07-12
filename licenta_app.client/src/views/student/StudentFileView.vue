<template>
  <v-container>
    <v-card>
      <v-card-title>
        Supervision Professor: {{ professorInfo?.username }}
        <span v-if="requestId" class="ml-2">(Request #{{ requestId }})</span>
      </v-card-title>
      <v-card-text>
        <div v-if="professorInfo">
          <strong>Professor Name:</strong> {{ professorInfo.username }}<br>
          <strong>Email:</strong> {{ professorInfo.email }}
        </div>
        <div v-else>
          <v-alert type="warning" variant="tonal" class="mt-2">
            No approved supervision found.
          </v-alert>
        </div>
      </v-card-text>
    </v-card>

    <!-- Disclaimer -->
    <v-alert type="info"
             variant="tonal"
             class="mt-6 mb-2 text-center disclaimer-alert"
             border="start"
             color="primary"
             icon="mdi-information">
      <strong>Note:</strong> Uploading a new file will <u>overwrite</u> the previous file of the same type.
    </v-alert>

    <v-row class="mt-4" v-if="requestId">
      <v-col cols="12"
             md="4"
             v-for="type in fileTypes"
             :key="type"
             class="d-flex">
        <v-card class="file-section-card flex-grow-1 d-flex flex-column justify-space-between">
          <div class="file-section-title"
               :class="fileTypeColorClass[type]">
            <span>{{ fileTypeLabels[type] }}</span>
          </div>
          <v-card-text class="file-section-content text-center flex-grow-1 d-flex flex-column justify-center align-center">
            <div class="flex-grow-1 d-flex flex-column justify-center align-center">
              <div v-if="files[type]">
                <v-icon color="primary" large>mdi-file</v-icon>
                <div class="file-name mt-2">{{ files[type].fileName }}</div>
              </div>
              <div v-else>
                <v-alert type="info" variant="tonal" class="my-2">No file uploaded</v-alert>
              </div>
            </div>
            <div class="button-stack mt-4">
              <v-btn v-if="files[type]"
                     color="primary"
                     @click="downloadFile(files[type].id)"
                     class="download-btn mb-2"
                     block>
                <v-icon start>mdi-download</v-icon>
                Download
              </v-btn>
              <v-btn v-if="type !== 'academic-default'"
                     color="success"
                     class="upload-btn"
                     block
                     @click="triggerFileInput(type)">
                <v-icon start>mdi-paperclip</v-icon>
                Upload New
              </v-btn>
              <input v-if="type !== 'academic-default'"
                     :ref="setFileInputRef(type)"
                     type="file"
                     accept=".pdf,.docx"
                     style="display: none"
                     @change="onFileSelected($event, type)" />
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-snackbar v-model="showSnackbar" color="success" timeout="3000">
      {{ snackbarMessage }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useUserStore } from '@/stores/userStore';
  import axios from '@/api/axios';

  const showSnackbar = ref(false);
  const snackbarMessage = ref('');

  const userStore = useUserStore();
  const files = ref<Record<string, any>>({});
  const professorInfo = ref<any>(null);
  const requestId = ref<number | null>(null);

  const fileTypes = ['academic-default', 'academic-signed', 'progress'];
  const fileTypeLabels = {
    'academic-default': 'Academic Default',
    'academic-signed': 'Academic Signed',
    'progress': 'Progress'
  };
  const fileTypeColorClass = {
    'academic-default': 'strip-default',
    'academic-signed': 'strip-signed',
    'progress': 'strip-progress'
  };
  const uploadFiles = ref<Record<string, File | null>>({
    'academic-default': null,
    'academic-signed': null,
    'progress': null
  });
  const fileInputRefs = ref<Record<string, HTMLInputElement | null>>({
    'academic-signed': null,
    'progress': null
  });
  const setFileInputRef = (type: string) => (el: HTMLInputElement | null) => {
    if (type !== 'academic-default') fileInputRefs.value[type] = el;
  };

  const fetchRequestAndProfessor = async () => {
    const studentId = userStore.user?.id;
    if (!studentId) return;

    const reqRes = await axios.get(`/api/registrationrequest/by-student/${studentId}`);

    const approvedRequests = reqRes.data.filter((r: any) => r.status === 1 || r.status === "Approved");
    if (!approvedRequests.length) {
      professorInfo.value = null;
      files.value = {};
      requestId.value = null;
      return;
    }
    const latestApproved = approvedRequests.sort((a: any, b: any) => b.id - a.id)[0];
    requestId.value = latestApproved.id;

    const fullReqRes = await axios.get(`/api/registrationrequest/${requestId.value}`);
    const regReq = fullReqRes.data;
    professorInfo.value = {
      username: regReq.registrationSession?.professor?.user?.username,
      email: regReq.registrationSession?.professor?.user?.email
    };

    const filesRes = await axios.get(`/api/fileupload/by-request/${requestId.value}`);
    files.value = filesRes.data.files;
  };

  const downloadFile = async (fileId: number) => {
    const res = await axios.get(`/api/fileupload/download/${fileId}`, { responseType: 'blob' });
    const url = window.URL.createObjectURL(new Blob([res.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', files.value[fileTypes.find(type => files.value[type]?.id === fileId)]?.fileName || 'file');
    document.body.appendChild(link);
    link.click();
    link.remove();
  };

  const triggerFileInput = (type: string) => {
    fileInputRefs.value[type]?.click();
  };

  const onFileSelected = async (event: Event, type: string) => {
    const input = event.target as HTMLInputElement;
    if (!input.files || !input.files[0]) return;
    const file = input.files[0];
    await uploadNewFile(type, file);
    input.value = '';
  };

  const uploadNewFile = async (type: string, file: File) => {
    if (!file || !requestId.value) return;
    const formData = new FormData();
    formData.append('file', file);
    formData.append('requestId', String(requestId.value));
    formData.append('uploadedBy', userStore.user?.username || 'student');
    formData.append('fileType', type);

    await axios.post('/api/fileupload/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    snackbarMessage.value = 'File uploaded successfully!';
    showSnackbar.value = true;
    await fetchRequestAndProfessor();
  };

  onMounted(fetchRequestAndProfessor);
</script>

<style scoped>
  .file-section-card {
    min-height: 340px;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }

  .file-section-title {
    width: 100%;
    padding: 12px 0;
    text-align: center;
    font-weight: bold;
    font-size: 1.1rem;
    letter-spacing: 0.5px;
    color: #fff;
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
  }

  .strip-default {
    background: #1976d2;
  }

  .strip-signed {
    background: #43a047;
  }

  .strip-progress {
    background: #fbc02d;
    color: #333;
  }

  .file-section-content {
    flex: 1 1 auto;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
  }

  .file-name {
    font-size: 1rem;
    font-weight: 500;
    margin-top: 4px;
    word-break: break-all;
  }

  .button-stack {
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    margin-top: 16px;
  }

  .download-btn,
  .upload-btn {
    width: 90%;
    margin: 0 auto 8px auto;
    font-weight: 500;
    letter-spacing: 0.5px;
  }

  .upload-btn {
    color: #fff;
  }

    .upload-btn .v-icon {
      color: #fff;
    }

  .disclaimer-alert {
    font-size: 1rem;
    font-weight: 500;
    border-radius: 8px;
    max-width: 600px;
    margin-left: auto;
    margin-right: auto;
  }
</style>
