import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import authService from './api/auth';
import vuetify from './plugins/vuetify';
import { useUserStore } from './stores/userStore';
import '@mdi/font/css/materialdesignicons.css';

import './assets/main.css';

//initialize services
authService.initAuth();

const app = createApp(App);
const pinia = createPinia();
app.use(pinia);
app.use(vuetify);

const userStore = useUserStore();
userStore.initializeFromToken().finally(() => {
  app.use(router);
  app.mount('#app');
});
