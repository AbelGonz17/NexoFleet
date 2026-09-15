<script setup lang="ts">
import { User, Mail, Phone, CreditCard, Calendar, AlertTriangle } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { CreateEmployeeRequest } from '~/types/api.types'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'created'): void
}>()

const api = useApi()
const toasts = useToasts()
const isSubmitting = ref(false)
const errorMessage = ref('')

const form = ref<CreateEmployeeRequest>({
  employeeCode: '',
  firstName: '',
  lastName: '',
  identityDocument: '',
  phone: '',
  email: '',
  hireDate: new Date().toISOString().split('T')[0],
  role: ''
})

function resetForm() {
  form.value = {
    employeeCode: `EMP-${Math.floor(100 + Math.random() * 900)}`,
    firstName: '',
    lastName: '',
    identityDocument: '',
    phone: '+51 9',
    email: '',
    hireDate: new Date().toISOString().split('T')[0],
    role: ''
  }
  errorMessage.value = ''
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) resetForm()
})

async function handleSubmit() {
  errorMessage.value = ''
  if (!form.value.firstName.trim() || !form.value.lastName.trim()) {
    errorMessage.value = 'Nombres y apellidos son requeridos.'
    return
  }
  if (!form.value.identityDocument.trim() || !form.value.email.trim()) {
    errorMessage.value = 'Documento de identidad y correo electrónico son obligatorios.'
    return
  }

  isSubmitting.value = true
  try {
    await api.post('/v1/employees', form.value)
    toasts.success(`Empleado ${form.value.firstName} ${form.value.lastName} registrado exitosamente.`)
    emit('created')
    emit('close')
  } catch (e: any) {
    const errorData = e?.response?.data || e?.data || e
    errorMessage.value = errorData?.detail || errorData?.message || 'Ocurrió un error inesperado al registrar el empleado.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Registrar Conductor / Empleado" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div v-if="errorMessage" class="p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 flex items-start gap-2.5 text-rose-400">
        <AlertTriangle class="w-4 h-4 shrink-0 mt-0.5" />
        <span class="text-xs font-semibold leading-relaxed">{{ errorMessage }}</span>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.employeeCode"
          label="Código de Empleado"
          placeholder="ej. EMP-101"
          required
        />
        <BaseInput
          v-model="form.identityDocument"
          label="DNI / Carné Extranjería"
          placeholder="ej. 45678912"
          required
        >
          <template #prefix>
            <CreditCard class="w-4 h-4 text-brand-400" />
          </template>
        </BaseInput>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.firstName"
          label="Nombres"
          placeholder="ej. Carlos Alberto"
          required
        />
        <BaseInput
          v-model="form.lastName"
          label="Apellidos"
          placeholder="ej. Mendoza Quispe"
          required
        />
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.email"
          type="email"
          label="Correo Electrónico"
          placeholder="ej. cmendoza@empresa.pe"
          required
        >
          <template #prefix>
            <Mail class="w-4 h-4 text-slate-400" />
          </template>
        </BaseInput>

        <BaseInput
          v-model="form.phone"
          label="Teléfono Móvil"
          placeholder="+51 987 654 321"
          required
        >
          <template #prefix>
            <Phone class="w-4 h-4 text-slate-400" />
          </template>
        </BaseInput>
      </div>

        <BaseInput
          v-model="form.hireDate"
          type="date"
          label="Fecha de Contratación / Ingreso"
          required
        />

      <div class="flex flex-col gap-1.5 w-full">
        <label for="roleSelect" class="text-xs font-semibold text-slate-300">
          Rol de Sistema
          <span class="text-rose-400">*</span>
        </label>
        <select
          id="roleSelect"
          v-model="form.role"
          required
          class="block w-full rounded-xl bg-slate-900/90 border border-slate-700/80 px-3.5 py-2.5 text-sm text-white focus:outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20"
        >
          <option value="" disabled>Seleccione un rol</option>
          <option value="Driver">Conductor</option>
          <option value="Employee">Personal / Operador</option>
        </select>
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Registrar Personal
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
