import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { HttpError } from '../../shared/api/httpClient'
import * as authApi from './authApi'
import type { AuthenticatedUser, LoginRequest, UserRole } from './types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthenticatedUser | null>(null)
  const initialized = ref(false)
  const loading = ref(false)

  const isAuthenticated = computed(() => user.value !== null)

  async function initialize() {
    if (initialized.value) return

    try {
      user.value = await authApi.getCurrentUser()
    } catch (error) {
      if (!(error instanceof HttpError) || error.status !== 401) throw error
      user.value = null
    } finally {
      initialized.value = true
    }
  }

  async function login(request: LoginRequest) {
    loading.value = true

    try {
      user.value = await authApi.login(request)
      initialized.value = true
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    try {
      await authApi.logout()
    } finally {
      user.value = null
      initialized.value = true
    }
  }

  function hasAnyRole(roles: UserRole[]) {
    return roles.some((role) => user.value?.roles.includes(role))
  }

  return { user, initialized, loading, isAuthenticated, initialize, login, logout, hasAnyRole }
})
