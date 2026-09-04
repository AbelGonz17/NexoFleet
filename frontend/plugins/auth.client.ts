export default defineNuxtPlugin(async () => {
  const auth = useAuth()
  if (!auth.isLoaded.value) {
    await auth.fetchCurrentUser()
  }
})
