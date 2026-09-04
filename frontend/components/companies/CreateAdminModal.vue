<script setup lang="ts">
import { UserCheck, ShieldCheck, Mail, Lock, User, KeyRound, CheckCircle2 } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { CompanyResponse, CreateCompanyAdminRequest, CompanyAdminUser } from '~/types/api.types'

const props = defineProps<{
  modelValue: boolean
  company: CompanyResponse | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'admin-created', user: CompanyAdminUser): void
}>()

const api = useApi()
const toasts = useToasts()

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const errors = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const loading = ref(false)

function resetForm() {
  form.firstName = ''
  form.lastName = ''
  form.email = ''
  form.password = ''
  form.confirmPassword = ''

  errors.firstName = ''
  errors.lastName = ''
  errors.email = ''
  errors.password = ''
  errors.confirmPassword = ''
}

watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      resetForm()
      if (props.company) {
        // Pre-fill email domain hint or clean email
        form.email = ''
      }
    }
  }
)

function validate(): boolean {
  let valid = true

  errors.firstName = ''
  errors.lastName = ''
  errors.email = ''
  errors.password = ''
  errors.confirmPassword = ''

  if (!form.firstName.trim()) {
    errors.firstName = 'El nombre es obligatorio.'
    valid = false
  }

  if (!form.lastName.trim()) {
    errors.lastName = 'El apellido es obligatorio.'
    valid = false
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!form.email.trim()) {
    errors.email = 'El correo electrónico es obligatorio.'
    valid = false
  } else if (!emailRegex.test(form.email.trim())) {
    errors.email = 'Introduce un formato de correo válido.'
    valid = false
  }

  if (!form.password) {
    errors.password = 'La contraseña es obligatoria.'
    valid = false
  } else if (form.password.length < 6) {
    errors.password = 'La contraseña debe tener al menos 6 caracteres.'
    valid = false
  }

  if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Las contraseñas no coinciden.'
    valid = false
  }

  return valid
}

async function handleSubmit() {
  if (!props.company) return
  if (!validate()) return

  loading.value = true
  try {
    const payload: CreateCompanyAdminRequest = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      password: form.password
    }

    const createdUser = await api.post<CompanyAdminUser>(`/v1/companies/${props.company.id}/admins`, payload)
    toasts.success(
      'Administrador creado con éxito',
      `El usuario "${createdUser.email}" ahora puede iniciar sesión como Administrador de "${props.company.name}".`
    )
    emit('admin-created', createdUser)
    emit('update:modelValue', false)
  } catch {
    // Error handled by useApi toasts
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    title="Crear Administrador de Empresa"
    description="Registra la cuenta de usuario principal que administrará las operaciones de esta empresa."
    max-width="md"
    :persistent="loading"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div v-if="company" class="space-y-4">
      <!-- Company Banner -->
      <div class="p-3.5 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center gap-3">
        <div class="w-9 h-9 rounded-lg bg-brand-500/20 text-brand-400 flex items-center justify-center font-bold text-sm shrink-0">
          {{ company.name.charAt(0).toUpperCase() }}
        </div>
        <div>
          <h4 class="text-xs font-bold text-white leading-snug">{{ company.name }}</h4>
          <p class="text-[11px] text-slate-400 font-mono">{{ company.taxIdentification }} • {{ company.city }}</p>
        </div>
      </div>

      <form class="space-y-3.5" @submit.prevent="handleSubmit">
        <!-- First & Last Name -->
        <div class="grid grid-cols-2 gap-3">
          <BaseInput
            v-model="form.firstName"
            label="Nombre"
            placeholder="Ej. Juan"
            required
            :error="errors.firstName"
            :disabled="loading"
          />

          <BaseInput
            v-model="form.lastName"
            label="Apellido"
            placeholder="Ej. Pérez"
            required
            :error="errors.lastName"
            :disabled="loading"
          />
        </div>

        <!-- Email -->
        <BaseInput
          v-model="form.email"
          label="Correo Electrónico (Usuario de Acceso)"
          type="email"
          placeholder="admin@empresa.com"
          required
          :error="errors.email"
          :disabled="loading"
        />

        <!-- Password -->
        <BaseInput
          v-model="form.password"
          label="Contraseña"
          type="password"
          placeholder="••••••••"
          required
          :error="errors.password"
          :disabled="loading"
        />

        <!-- Confirm Password -->
        <BaseInput
          v-model="form.confirmPassword"
          label="Confirmar Contraseña"
          type="password"
          placeholder="••••••••"
          required
          :error="errors.confirmPassword"
          :disabled="loading"
        />

        <div class="p-3 rounded-xl bg-slate-950/60 border border-slate-800 text-[11px] text-slate-400 flex items-start gap-2">
          <ShieldCheck class="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
          <span>
            Este usuario tendrá rol <strong class="text-white">Administrator</strong> con acceso completo a los módulos operativos y financieros de esta empresa.
          </span>
        </div>
      </form>
    </div>

    <template #footer>
      <div class="flex items-center justify-end gap-2.5 w-full">
        <BaseButton
          variant="secondary"
          size="sm"
          :disabled="loading"
          @click="emit('update:modelValue', false)"
        >
          Cancelar
        </BaseButton>

        <BaseButton
          variant="primary"
          size="sm"
          :loading="loading"
          @click="handleSubmit"
        >
          <UserCheck class="w-4 h-4 mr-1.5" />
          Crear Administrador
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>
