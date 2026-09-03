// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

  modules: [
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt'
  ],

  css: [
    '~/assets/css/main.css'
  ],

  app: {
    head: {
      title: 'NexoFleet - Plataforma Inteligente de Transporte',
      titleTemplate: '%s | NexoFleet',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Sistema integral de gestión de flotas, viajes, rutas y liquidaciones corporativas.' }
      ],
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@300;400;500;600;700;800&family=Inter:wght@400;500;600;700&display=swap' }
      ]
    }
  },

  routeRules: {
    '/api/**': {
      proxy: `${process.env.NUXT_BACKEND_URL || process.env.NUXT_PUBLIC_API_URL || 'http://localhost:5000'}/api/**`
    }
  },

  runtimeConfig: {
    public: {
      apiUrl: process.env.NUXT_PUBLIC_API_URL || 'http://localhost:5000'
    }
  },

  typescript: {
    strict: true,
    typeCheck: false
  }
})
