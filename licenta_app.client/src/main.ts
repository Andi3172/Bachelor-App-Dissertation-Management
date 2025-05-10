import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import authService from './api/auth';
import vuetify from './plugins/vuetify';
import '@mdi/font/css/materialdesignicons.css';

import './assets/main.css';

//initialize services
authService.initAuth();


const app = createApp(App)

app.use(createPinia())
app.use(vuetify)
app.use(router)
app.mount('#app')
