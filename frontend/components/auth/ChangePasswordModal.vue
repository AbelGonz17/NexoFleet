<script setup lang="ts">
import { ref } from 'vue'
import { Key } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  (e: 'success'): void
}>()

const api = useApi()
const toasts = useToasts()
const isSubmitting = ref(false)

const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

async function handleSubmit() {
  if (newPassword.value !== confirmPassword.value) {
    toasts.warning('Las contraseñas no coinciden', 'La confirmación debe ser igual a la nueva contraseña.')
    return
  }

  if (newPassword.value.length < 8) {
    toasts.warning('Contraseña muy corta', 'La nueva contraseña debe tener al menos 8 caracteres.')
    return
  }

  isSubmitting.value = true
  try {
    await api.post('/v1/auth/change-password', {
      currentPassword: currentPassword.value,
      newPassword: newPassword.value
    })
    toasts.success('Contraseña actualizada', 'Tu contraseña ha sido cambiada correctamente.')
    emit('success')
  } catch (e: any) {
    toasts.error('Error al cambiar contraseña', e?.data?.detail || 'Revisa tu contraseña actual e intenta nuevamente.')
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Cambio de Contraseña Obligatorio" @close="() => {}">
    <div class="mb-4 p-3 bg-brand-950/40 border border-brand-500/20 rounded-xl flex gap-3 text-sm text-brand-200">
      <Key class="w-5 h-5 shrink-0 text-brand-400" />
      <p>Por políticas de seguridad, es necesario que actualices la contraseña temporal que se te ha asignado antes de continuar.</p>
    </div>

    <form class="space-y-4" @submit.prevent="handleSubmit">
      <BaseInput
        v-model="currentPassword"
        type="password"
        label="Contraseña Temporal (Actual)"
        placeholder="Ingresa tu contraseña actual"
        required
      />

      <BaseInput
        v-model="newPassword"
        type="password"
        label="Nueva Contraseña"
        placeholder="Mínimo 8 caracteres"
        required
      />

      <BaseInput
        v-model="confirmPassword"
        type="password"
        label="Confirmar Nueva Contraseña"
        placeholder="Repite tu nueva contraseña"
        required
      />

      <div class="flex justify-end pt-4 border-t border-slate-800">
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Actualizar y Continuar
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
