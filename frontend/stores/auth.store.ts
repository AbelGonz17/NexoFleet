import { defineStore } from 'pinia'
import type { AuthenticatedUser } from '~/types/auth.types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthenticatedUser | null>(null)
  const isLoaded = ref(false)
  const isAuthenticated = computed(() => !!user.value)

  function setUser(newUser: AuthenticatedUser | null) {
    user.value = newUser
    isLoaded.value = true
  }

  function clearUser() {
    user.value = null
    isLoaded.value = true
  }

  return {
    user,
    isLoaded,
    isAuthenticated,
    setUser,
    clearUser
  }
})
