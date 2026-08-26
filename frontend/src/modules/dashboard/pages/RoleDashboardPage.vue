<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../auth/authStore'

const router = useRouter()
const authStore = useAuthStore()

async function logout() {
  await authStore.logout()
  await router.replace('/login')
}
</script>

<template>
  <main class="role-dashboard">
    <section>
      <strong>NexoFleet</strong>
      <h1>Hola, {{ authStore.user?.firstName }}</h1>
      <p>Tu acceso está configurado correctamente. Este panel se completará en su módulo correspondiente.</p>
      <button class="button button--primary" type="button" @click="logout">Cerrar sesión</button>
    </section>
  </main>
</template>

<style scoped>
.role-dashboard { min-height: 100vh; display: grid; place-items: center; padding: 24px; }
section { width: min(560px, 100%); padding: 32px; border: 1px solid #d5dfe6; border-radius: 14px; background: #fff; }
h1 { margin-top: 24px; }
p { margin: 12px 0 24px; color: #657887; }
</style>
