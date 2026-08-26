<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '../../modules/auth/authStore'

const router = useRouter()
const authStore = useAuthStore()

async function logout() {
  await authStore.logout()
  await router.replace('/login')
}
</script>

<template>
  <div class="shell">
    <aside class="sidebar">
      <RouterLink class="brand" to="/superadmin">NexoFleet</RouterLink>
      <nav>
        <RouterLink to="/superadmin">Inicio</RouterLink>
        <RouterLink to="/superadmin/companies">Empresas</RouterLink>
      </nav>
    </aside>
    <main>
      <header class="topbar">
        <span>Panel del SuperAdmin</span>
        <div class="session">
          <span>{{ authStore.user?.firstName }} {{ authStore.user?.lastName }}</span>
          <button type="button" @click="logout">Cerrar sesión</button>
        </div>
      </header>
      <div class="content"><slot /></div>
    </main>
  </div>
</template>

<style scoped>
.shell { min-height: 100vh; display: grid; grid-template-columns: 220px minmax(0, 1fr); }
.sidebar { padding: 24px 16px; background: #ffffff; border-right: 1px solid #d5dfe6; }
.brand { display: block; margin-bottom: 28px; font-weight: 700; text-decoration: none; }
nav { display: grid; gap: 6px; }
nav a { padding: 10px 12px; border-radius: 8px; color: #657887; text-decoration: none; }
nav a.router-link-active { background: #e1f1ef; color: #183247; }
.topbar { min-height: 68px; display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 0 24px; background: #ffffff; border-bottom: 1px solid #d5dfe6; }
.session { display: flex; align-items: center; gap: 14px; }
.session button { border: 0; background: transparent; color: #176f6a; cursor: pointer; font: inherit; }
.content { padding: 24px; }
@media (max-width: 700px) {
  .shell { grid-template-columns: 1fr; }
  .sidebar { border-right: 0; border-bottom: 1px solid #d5dfe6; }
  nav { grid-template-columns: 1fr 1fr; }
}
</style>
