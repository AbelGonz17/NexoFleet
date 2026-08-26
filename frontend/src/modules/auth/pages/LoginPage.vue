<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { HttpError } from '../../../shared/api/httpClient'
import { useAuthStore } from '../authStore'
import { userRoles } from '../types'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const form = reactive({ email: '', password: '' })
const errorMessage = ref('')

function defaultDestination() {
  if (authStore.user?.roles.includes(userRoles.superAdmin)) return '/superadmin'
  if (authStore.user?.roles.includes(userRoles.administrator)) return '/admin'
  return '/employee'
}

async function submit() {
  errorMessage.value = ''

  try {
    await authStore.login(form)
    const redirect = typeof route.query.redirect === 'string'
      ? route.query.redirect
      : defaultDestination()
    await router.replace(redirect)
  } catch (error) {
    errorMessage.value = error instanceof HttpError && error.status === 423
      ? 'Tu cuenta está bloqueada temporalmente. Inténtalo nuevamente más tarde.'
      : 'No pudimos iniciar sesión. Revisa tu correo y contraseña.'
  }
}
</script>

<template>
  <main class="login-page">
    <section class="intro">
      <strong>NexoFleet</strong>
      <div>
        <h1>Operación de transporte conectada</h1>
        <p>Empresas, rutas, viajes y reportes en un solo lugar.</p>
      </div>
    </section>
    <form class="login-form" @submit.prevent="submit">
      <div><h1>Iniciar sesión</h1><p>Ingresa a tu espacio de NexoFleet.</p></div>
      <div class="field"><label for="email">Correo electrónico</label><input id="email" v-model="form.email" type="email" autocomplete="email" required></div>
      <div class="field"><label for="password">Contraseña</label><input id="password" v-model="form.password" type="password" autocomplete="current-password" required></div>
      <p v-if="errorMessage" class="form-error" role="alert">{{ errorMessage }}</p>
      <button class="button button--primary" type="submit" :disabled="authStore.loading">
        {{ authStore.loading ? 'Ingresando…' : 'Ingresar' }}
      </button>
    </form>
  </main>
</template>

<style scoped>
.login-page { min-height: 100vh; display: grid; grid-template-columns: 1.05fr 0.95fr; background: #ffffff; }
.intro { display: flex; flex-direction: column; justify-content: space-between; padding: 48px; background: #176f6a; color: #ffffff; }
.login-form { width: min(420px, 100%); margin: auto; padding: 32px; display: grid; gap: 18px; }
.login-form p { color: #657887; }
.form-error { padding: 10px 12px; border-radius: 8px; background: #fff0f0; color: #a33030 !important; }
.button:disabled { cursor: wait; opacity: 0.65; }
@media (max-width: 700px) {
  .login-page { grid-template-columns: 1fr; }
  .intro { min-height: 230px; padding: 28px; }
}
</style>
