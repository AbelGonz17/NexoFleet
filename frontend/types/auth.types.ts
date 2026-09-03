export interface AuthenticatedUser {
  id: string
  email: string
  fullName: string
  role: string
  companyId?: string | null
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
