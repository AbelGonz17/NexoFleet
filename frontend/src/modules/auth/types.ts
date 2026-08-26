export const userRoles = {
  superAdmin: 'SuperAdmin',
  administrator: 'Administrator',
  employee: 'Employee',
} as const

export type UserRole = (typeof userRoles)[keyof typeof userRoles]

export interface AuthenticatedUser {
  id: string
  email: string
  firstName: string
  lastName: string
  companyId: string | null
  roles: UserRole[]
}

export interface LoginRequest {
  email: string
  password: string
}
