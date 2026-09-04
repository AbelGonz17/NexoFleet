<script setup lang="ts">
import { AlertTriangle, AlertCircle, CheckCircle, HelpCircle } from 'lucide-vue-next'
import BaseModal from './BaseModal.vue'
import BaseButton from './BaseButton.vue'

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    title: string
    message: string
    confirmText?: string
    cancelText?: string
    variant?: 'danger' | 'warning' | 'primary' | 'success'
    loading?: boolean
  }>(),
  {
    confirmText: 'Confirmar',
    cancelText: 'Cancelar',
    variant: 'danger',
    loading: false
  }
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()

const iconComponent = computed(() => {
  switch (props.variant) {
    case 'danger':
      return AlertCircle
    case 'warning':
      return AlertTriangle
    case 'success':
      return CheckCircle
    default:
      return HelpCircle
  }
})

const iconColorClasses = computed(() => {
  switch (props.variant) {
    case 'danger':
      return 'text-rose-400 bg-rose-500/10 border-rose-500/20'
    case 'warning':
      return 'text-amber-400 bg-amber-500/10 border-amber-500/20'
    case 'success':
      return 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20'
    default:
      return 'text-brand-400 bg-brand-500/10 border-brand-500/20'
  }
})

const buttonVariant = computed(() => {
  switch (props.variant) {
    case 'danger':
      return 'danger'
    case 'warning':
      return 'danger'
    case 'success':
      return 'primary'
    default:
      return 'primary'
  }
})

function close() {
  emit('update:modelValue', false)
  emit('cancel')
}

function handleConfirm() {
  emit('confirm')
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    max-width="sm"
    :persistent="loading"
    @update:model-value="emit('update:modelValue', $event)"
    @close="close"
  >
    <div class="flex flex-col items-center text-center p-2">
      <div
        class="w-12 h-12 rounded-2xl border flex items-center justify-center mb-4 shadow-inner"
        :class="iconColorClasses"
      >
        <component :is="iconComponent" class="w-6 h-6" />
      </div>

      <h3 class="text-base font-bold text-white mb-2">
        {{ title }}
      </h3>

      <p class="text-xs text-slate-300 leading-relaxed max-w-xs">
        {{ message }}
      </p>
    </div>

    <template #footer>
      <div class="flex items-center justify-end gap-2.5 w-full">
        <BaseButton
          variant="secondary"
          size="sm"
          :disabled="loading"
          @click="close"
        >
          {{ cancelText }}
        </BaseButton>

        <BaseButton
          :variant="buttonVariant"
          size="sm"
          :loading="loading"
          @click="handleConfirm"
        >
          {{ confirmText }}
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>
