export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuth()

  if (!auth.isLoaded.value) {
    await auth.fetchCurrentUser()
  }

  if (!auth.isAuthenticated.value) {
    return navigateTo('/login')
  }
})
