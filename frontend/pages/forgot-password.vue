<script setup lang="ts">
import { Truck, ArrowRight, ArrowLeft } from 'lucide-vue-next'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseInput from '~/components/common/BaseInput.vue'

definePageMeta({
  layout: 'auth',
  middleware: 'guest'
})

useHead({
  title: 'Recuperar Contraseña'
})

const email = ref('')
const loading = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

async function handleSubmit() {
  if (!email.value) {
    errorMessage.value = 'Por favor ingresa tu correo electrónico.'
    return
  }

  loading.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    // TODO: Connect this with the actual useAuth() forgot password method when backend provides it
    // await api.post('/v1/auth/forgot-password', { email: email.value })
    
    // Simulating delay for now
    await new Promise(resolve => setTimeout(resolve, 1000))
    successMessage.value = 'Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña.'
  } catch (err: any) {
    errorMessage.value = err?.data?.detail || 'Ha ocurrido un error al intentar enviar el correo.'
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
      <h2 class="text-2xl font-extrabold text-white tracking-tight">Recuperar Contraseña</h2>
      <p class="text-xs text-slate-400 mt-1.5">Ingresa tu correo para recibir las instrucciones</p>
    </div>

    <!-- Error/Success alerts -->
    <div v-if="errorMessage" class="mb-5 p-3.5 rounded-xl bg-rose-950/60 border border-rose-500/30 text-xs text-rose-300">
      {{ errorMessage }}
    </div>
    
    <div v-if="successMessage" class="mb-5 p-3.5 rounded-xl bg-emerald-950/60 border border-emerald-500/30 text-xs text-emerald-300">
      {{ successMessage }}
    </div>

    <!-- Form -->
    <form class="space-y-4" @submit.prevent="handleSubmit" v-if="!successMessage">
      <BaseInput
        v-model="email"
        label="Correo Electrónico"
        type="email"
        placeholder="usuario@empresa.com"
        required
      />

      <div class="pt-3">
        <BaseButton
          type="submit"
          variant="primary"
          size="lg"
          block
          :loading="loading"
        >
          <span>Enviar instrucciones</span>
          <ArrowRight class="w-4 h-4" />
        </BaseButton>
      </div>
    </form>
    
    <div class="mt-6 text-center">
      <NuxtLink to="/login" class="inline-flex items-center gap-2 text-xs font-medium text-slate-400 hover:text-brand-400 transition-colors">
        <ArrowLeft class="w-4 h-4" />
        Volver a Iniciar Sesión
      </NuxtLink>
    </div>
  </div>
</template>
