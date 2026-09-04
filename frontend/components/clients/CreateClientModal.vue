<script setup lang="ts">
import { Building2, Mail, Phone, CreditCard, User } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { CreateClientRequest } from '~/types/api.types'

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

const form = ref<CreateClientRequest>({
  clientCode: '',
  name: '',
  taxIdentification: '',
  contactName: '',
  phone: '+51 9',
  email: ''
})

function resetForm() {
  form.value = {
    clientCode: `CLI-${Math.floor(100 + Math.random() * 900)}`,
    name: '',
    taxIdentification: '20',
    contactName: '',
    phone: '+51 9',
    email: ''
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) resetForm()
})

async function handleSubmit() {
  if (!form.value.clientCode.trim() || !form.value.name.trim()) {
    toasts.warning('El código y la razón social / nombre del cliente son obligatorios.')
    return
  }

  isSubmitting.value = true
  try {
    await api.post('/v1/clients', form.value)
    toasts.success(`Cliente ${form.value.name} registrado exitosamente.`)
    emit('created')
    emit('close')
  } catch {
    // Handled by useApi
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Registrar Cliente Corporativo" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.clientCode"
          label="Código de Cliente"
          placeholder="ej. CLI-101"
          required
        />
        <BaseInput
          v-model="form.taxIdentification"
          label="RUC / Identificación Fiscal"
          placeholder="ej. 20609876543"
        >
          <template #prefix>
            <CreditCard class="w-4 h-4 text-brand-400" />
          </template>
        </BaseInput>
      </div>

      <BaseInput
        v-model="form.name"
        label="Razón Social / Nombre Comercial"
        placeholder="ej. Minera Las Bambas S.A."
        required
      >
        <template #prefix>
          <Building2 class="w-4 h-4 text-brand-400" />
        </template>
      </BaseInput>

      <BaseInput
        v-model="form.contactName"
        label="Persona de Contacto / Coordinador"
        placeholder="ej. Lic. Roberto Gómez (Logística)"
      >
        <template #prefix>
          <User class="w-4 h-4 text-slate-400" />
        </template>
      </BaseInput>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.email"
          type="email"
          label="Correo Corporativo"
          placeholder="logistica@empresa.com"
        >
          <template #prefix>
            <Mail class="w-4 h-4 text-slate-400" />
          </template>
        </BaseInput>

        <BaseInput
          v-model="form.phone"
          label="Teléfono / Central"
          placeholder="+51 1 456 7890"
        >
          <template #prefix>
            <Phone class="w-4 h-4 text-slate-400" />
          </template>
        </BaseInput>
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Registrar Cliente
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
