export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuth()
  const permissions = usePermissions()

  if (!auth.isLoaded.value) {
    await auth.fetchCurrentUser()
  }

  if (!auth.isAuthenticated.value) {
    return navigateTo('/login')
  }

  // Enforce role-based access control on routes
  if (!permissions.canAccess(to.path)) {
    return navigateTo('/')
  }
})
