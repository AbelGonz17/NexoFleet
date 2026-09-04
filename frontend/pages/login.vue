<script setup lang="ts">
import { Truck, ArrowRight } from 'lucide-vue-next'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseInput from '~/components/common/BaseInput.vue'

definePageMeta({
  layout: 'auth',
  middleware: 'guest'
})

useHead({
  title: 'Iniciar Sesión'
})

const auth = useAuth()
const api = useApi()
const email = ref('')
const password = ref('')
const rememberMe = ref(true)
const loading = ref(false)
const errorMessage = ref('')

onMounted(() => {
  api.clearCsrfToken()
})

async function handleSubmit() {
  if (!email.value || !password.value) {
    errorMessage.value = 'Por favor ingresa tu correo y contraseña.'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    await auth.login({
      email: email.value,
      password: password.value,
      rememberMe: rememberMe.value
    })
  } catch (err: any) {
    errorMessage.value = err?.data?.detail || 'Credenciales inválidas o servidor no disponible.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="rounded-3xl border border-slate-800/80 bg-slate-900/70 backdrop-blur-2xl p-8 shadow-2xl">
    <!-- Header -->
    <div class="text-center mb-8">
      <div class="w-14 h-14 rounded-2xl bg-gradient-to-tr from-brand-600 to-indigo-400 mx-auto flex items-center justify-center shadow-xl shadow-brand-500/25 mb-4">
        <Truck class="w-8 h-8 text-white" />
      </div>
      <h2 class="text-2xl font-extrabold text-white tracking-tight">NexoFleet</h2>
      <p class="text-xs text-slate-400 mt-1.5">Ingresa tus credenciales para acceder a la consola</p>
    </div>

    <!-- Error alert -->
    <div v-if="errorMessage" class="mb-5 p-3.5 rounded-xl bg-rose-950/60 border border-rose-500/30 text-xs text-rose-300">
      {{ errorMessage }}
    </div>

    <!-- Form -->
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <BaseInput
        v-model="email"
        label="Correo Electrónico"
        type="email"
        placeholder="usuario@empresa.com"
        required
      />

      <BaseInput
        v-model="password"
        label="Contraseña"
        type="password"
        placeholder="••••••••••••"
        required
      />

      <div class="flex items-center justify-between text-xs pt-1">
        <label class="flex items-center gap-2 cursor-pointer text-slate-400 select-none">
          <input
            v-model="rememberMe"
            type="checkbox"
            class="rounded bg-slate-900 border-slate-700 text-brand-600 focus:ring-brand-500 focus:ring-offset-0"
          />
          <span>Recordar sesión</span>
        </label>

        <a href="#" class="text-brand-400 hover:text-brand-300 font-medium">¿Olvidaste tu clave?</a>
      </div>

      <div class="pt-3">
        <BaseButton
          type="submit"
          variant="primary"
          size="lg"
          block
          :loading="loading"
        >
          <span>Iniciar Sesión</span>
          <ArrowRight class="w-4 h-4" />
        </BaseButton>
      </div>
    </form>
  </div>
</template>
