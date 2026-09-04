import { useAuthStore } from '~/stores/auth.store'

export function usePermissions() {
  const store = useAuthStore()

  const user = computed(() => store.user)
  const roles = computed<string[]>(() => store.user?.roles || (store.user?.role ? [store.user.role] : []))
  
  const isSuperAdmin = computed(() => roles.value.includes('SuperAdmin'))
  const isCompanyAdmin = computed(() => roles.value.includes('Administrator'))
  const isEmployee = computed(() => roles.value.includes('Employee'))

  const primaryRole = computed(() => {
    if (isSuperAdmin.value) return 'SuperAdmin'
    if (isCompanyAdmin.value) return 'Administrator'
    if (isEmployee.value) return 'Employee'
    return roles.value[0] || 'Guest'
  })

  const roleLabel = computed(() => {
    if (isSuperAdmin.value) return 'Super Administrador'
    if (isCompanyAdmin.value) return 'Admin Empresa'
    if (isEmployee.value) return 'Conductor / Personal'
    return 'Usuario'
  })

  const companyId = computed(() => store.user?.companyId || null)
  const companyName = computed(() => store.user?.companyName || null)

  function hasRole(requiredRoles: string | string[]): boolean {
    const list = Array.isArray(requiredRoles) ? requiredRoles : [requiredRoles]
    return list.some(r => roles.value.includes(r))
  }

  function canAccess(path: string): boolean {
    if (!store.isAuthenticated) return false
    if (isSuperAdmin.value) return true // SuperAdmin has access to inspect all

    if (path.startsWith('/companies') && !path.startsWith('/companies/my-company')) {
      return isSuperAdmin.value
    }

    if (path.startsWith('/audit-logs')) {
      return isSuperAdmin.value || isCompanyAdmin.value
    }

    if (path.startsWith('/employees') || path.startsWith('/clients') || path.startsWith('/payments') || path.startsWith('/schedules') || path.startsWith('/routes')) {
      return isCompanyAdmin.value || isSuperAdmin.value
    }

    return true
  }

  return {
    user,
    roles,
    primaryRole,
    roleLabel,
    isSuperAdmin,
    isCompanyAdmin,
    isEmployee,
    companyId,
    companyName,
    hasRole,
    canAccess
  }
}
