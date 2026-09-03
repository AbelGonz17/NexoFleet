export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const backendBase = process.env.NUXT_BACKEND_URL || config.public.apiUrl || 'http://localhost:5000'
  const normalizedBase = backendBase.replace(/\/+$/, '')

  // event.path is the incoming path with query params, e.g. /api/v1/auth/me
  const targetUrl = `${normalizedBase}${event.path}`

  return proxyRequest(event, targetUrl)
})
