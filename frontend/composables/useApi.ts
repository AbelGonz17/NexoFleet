import type { ApiProblemDetails, CsrfTokenResponse } from '~/types/auth.types'

let cachedCsrfToken: string | null = null

export function useApi() {
  const config = useRuntimeConfig()
  const toasts = useToasts()

  async function getCsrfToken(): Promise<string> {
    if (cachedCsrfToken) return cachedCsrfToken

    try {
      const response = await $fetch<CsrfTokenResponse>('/v1/auth/csrf', {
        baseURL: '/api',
        credentials: 'include'
      })
      cachedCsrfToken = response.token
      return response.token
    } catch {
      return ''
    }
  }

  async function request<T>(
    endpoint: string,
    options: {
      method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
      body?: any
      query?: Record<string, any>
      headers?: Record<string, string>
      suppressErrorToast?: boolean
    } = {}
  ): Promise<T> {
    const method = options.method || 'GET'
    const headers: Record<string, string> = { ...options.headers }

    if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method)) {
      const token = await getCsrfToken()
      if (token) {
        headers['X-XSRF-TOKEN'] = token
      }
    }

    try {
      return await $fetch<T>(endpoint, {
        baseURL: '/api',
        method,
        body: options.body,
        params: options.query,
        headers,
        credentials: 'include'
      })
    } catch (err: any) {
      const problem = err.data as ApiProblemDetails | undefined

      if (!options.suppressErrorToast) {
        if (problem?.errors) {
          const firstErrorKey = Object.keys(problem.errors)[0]
          const firstErrorMsg = problem.errors[firstErrorKey]?.[0] || problem.detail || 'Error de validación'
          toasts.error(problem.title || 'Solicitud incorrecta', firstErrorMsg)
        } else if (problem?.detail) {
          toasts.error(problem.title || 'Error', problem.detail)
        } else if (err.status === 401) {
          toasts.warning('Sesión expirada', 'Por favor inicia sesión nuevamente.')
        } else if (err.status === 403) {
          toasts.error('Acceso denegado', 'No tienes permisos suficientes para realizar esta acción.')
        } else {
          toasts.error('Error de comunicación', err.message || 'No se pudo conectar con el servidor.')
        }
      }

      throw err
    }
  }

  return {
    get: <T>(endpoint: string, query?: Record<string, any>) => request<T>(endpoint, { method: 'GET', query }),
    post: <T>(endpoint: string, body?: any, query?: Record<string, any>) => request<T>(endpoint, { method: 'POST', body, query }),
    put: <T>(endpoint: string, body?: any, query?: Record<string, any>) => request<T>(endpoint, { method: 'PUT', body, query }),
    delete: <T>(endpoint: string, query?: Record<string, any>) => request<T>(endpoint, { method: 'DELETE', query }),
    request,
    getCsrfToken
  }
}
