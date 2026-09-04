<script setup lang="ts">
import { User, Mail, Phone, CreditCard, Calendar } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { EmployeeResponse, UpdateEmployeeRequest } from '~/types/api.types'

const props = defineProps<{
  isOpen: boolean
  employee: EmployeeResponse | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'updated'): void
}>()

const api = useApi()
const toasts = useToasts()
const isSubmitting = ref(false)

const form = ref<UpdateEmployeeRequest>({
  employeeCode: '',
  firstName: '',
  lastName: '',
  identityDocument: '',
  phone: '',
  email: '',
  hireDate: ''
})

watch(() => props.employee, (emp) => {
  if (emp) {
    let fName = emp.firstName || ''
    let lName = emp.lastName || ''
    if (!fName && !lName && typeof emp.fullName === 'string') {
      const parts = emp.fullName.split(' ')
      fName = parts.slice(0, Math.ceil(parts.length / 2)).join(' ')
      lName = parts.slice(Math.ceil(parts.length / 2)).join(' ')
    }

    form.value = {
      employeeCode: emp.employeeCode || '',
      firstName: fName,
      lastName: lName,
      identityDocument: emp.identityDocument || '',
      phone: emp.phone || '',
      email: emp.email || '',
      hireDate: emp.hireDate || emp.hiredOn || new Date().toISOString().split('T')[0]
    }
  }
}, { immediate: true })

async function handleSubmit() {
  if (!props.employee) return
  if (!form.value.firstName.trim() || !form.value.lastName.trim()) {
    toasts.warning('Nombres y apellidos son requeridos.')
    return
  }
  if (!form.value.identityDocument.trim() || !form.value.email.trim()) {
    toasts.warning('Documento de identidad y correo electrónico son obligatorios.')
    return
  }

  isSubmitting.value = true
  try {
    await api.put(`/v1/employees/${props.employee.id}`, {
      employeeCode: form.value.employeeCode.trim(),
      firstName: form.value.firstName.trim(),
      lastName: form.value.lastName.trim(),
      identityDocument: form.value.identityDocument.trim(),
      phone: form.value.phone.trim(),
      email: form.value.email.trim(),
      hireDate: form.value.hireDate
    })
    toasts.success('Empleado actualizado', `Los datos de ${form.value.firstName} ${form.value.lastName} se actualizaron correctamente.`)
    emit('updated')
    emit('close')
  } catch {
    // Handled by useApi
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Editar Conductor / Empleado" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
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

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Guardar Cambios
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
