import { useAuthStore } from '~/stores/auth.store'
import type { AuthenticatedUser } from '~/types/auth.types'

export function useAuth() {
  const store = useAuthStore()
  const api = useApi()
  const toasts = useToasts()
  const router = useRouter()

  const user = computed(() => store.user)
  const isAuthenticated = computed(() => store.isAuthenticated)
  const isLoaded = computed(() => store.isLoaded)

  async function fetchCurrentUser(): Promise<AuthenticatedUser | null> {
    try {
      const data = await api.request<AuthenticatedUser>('/v1/auth/me', {
        method: 'GET',
        suppressErrorToast: true
      })
      store.setUser(data)
      return data
    } catch {
      store.clearUser()
      return null
    }
  }

  async function login(credentials: { email: string; password: string; rememberMe?: boolean }) {
    const data = await api.post<AuthenticatedUser>('/v1/auth/login', credentials)
    store.setUser(data)
    toasts.success('¡Bienvenido!', `Has iniciado sesión como ${data.fullName}`)
    router.push('/')
    return data
  }

  async function logout() {
    try {
      await api.post('/v1/auth/logout')
    } finally {
      store.clearUser()
      toasts.info('Sesión cerrada', 'Has salido del sistema de manera segura.')
      router.push('/login')
    }
  }

  return {
    user,
    isAuthenticated,
    isLoaded,
    fetchCurrentUser,
    login,
    logout
  }
}
