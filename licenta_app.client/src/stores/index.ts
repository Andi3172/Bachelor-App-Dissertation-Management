import { defineStore } from 'pinia'

export const useMainStore = defineStore('main', {
  state: () => ({
    user: null as null | { username: string; token: string }
  }),
  actions: {
    setUser(user: { username: string; token: string }) {
      this.user = user
    },
    logout() {
      this.user = null
    }
  }
})
