export interface AuthenticatedUser {
  id: string
  email: string
  firstName?: string
  lastName?: string
  fullName: string
  roles: string[]
  requiresPasswordChange: boolean
  role?: string
  companyId?: string | null
  companyName?: string | null
}

export interface CsrfTokenResponse {
  token: string
  cookieName: string
  headerName: string
}

export interface ApiProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
  errors?: Record<string, string[]>
}
