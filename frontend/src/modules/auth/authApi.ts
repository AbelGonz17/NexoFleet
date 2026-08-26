import { apiFetch } from '../../shared/api/httpClient'
import type { AuthenticatedUser, LoginRequest } from './types'

let csrfToken: string | null = null

async function getCsrfToken() {
  if (csrfToken) return csrfToken

  const response = await apiFetch<{ token: string }>('/auth/csrf')
  csrfToken = response.token
  return csrfToken
}

export async function login(request: LoginRequest): Promise<AuthenticatedUser> {
  const token = await getCsrfToken()
  return apiFetch<AuthenticatedUser>('/auth/login', {
    method: 'POST',
    headers: { 'X-XSRF-TOKEN': token },
    body: JSON.stringify(request),
  })
}

export function getCurrentUser(): Promise<AuthenticatedUser> {
  return apiFetch<AuthenticatedUser>('/auth/me')
}

export async function logout(): Promise<void> {
  const token = await getCsrfToken()
  await apiFetch<void>('/auth/logout', {
    method: 'POST',
    headers: { 'X-XSRF-TOKEN': token },
  })
}
