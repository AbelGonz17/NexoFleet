<script setup lang="ts">
import { reactive } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const form = reactive({ email: '', password: '' })

async function submit() {
  await router.push('/superadmin')
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
      <button class="button button--primary" type="submit">Ingresar</button>
    </form>
  </main>
</template>

<style scoped>
.login-page { min-height: 100vh; display: grid; grid-template-columns: 1.05fr 0.95fr; background: #ffffff; }
.intro { display: flex; flex-direction: column; justify-content: space-between; padding: 48px; background: #176f6a; color: #ffffff; }
.login-form { width: min(420px, 100%); margin: auto; padding: 32px; display: grid; gap: 18px; }
.login-form p { color: #657887; }
@media (max-width: 700px) {
  .login-page { grid-template-columns: 1fr; }
  .intro { min-height: 230px; padding: 28px; }
}
</style>

