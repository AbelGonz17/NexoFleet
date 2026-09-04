<script setup lang="ts">
import { X } from 'lucide-vue-next'

const props = withDefaults(
  defineProps<{
    modelValue?: boolean
    isOpen?: boolean
    title?: string
    description?: string
    maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl'
    persistent?: boolean
  }>(),
  {
    modelValue: false,
    isOpen: undefined,
    title: '',
    description: '',
    maxWidth: 'md',
    persistent: false
  }
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'close'): void
}>()

const isVisible = computed(() => {
  if (props.isOpen !== undefined) return props.isOpen
  return props.modelValue
})

const maxWidthClass = computed(() => {
  switch (props.maxWidth) {
    case 'sm':
      return 'max-w-sm'
    case 'md':
      return 'max-w-md'
    case 'lg':
      return 'max-w-lg'
    case 'xl':
      return 'max-w-xl'
    case '2xl':
      return 'max-w-2xl'
    default:
      return 'max-w-md'
  }
})

function close() {
  emit('update:modelValue', false)
  emit('close')
}

function handleBackdropClick() {
  if (!props.persistent) {
    close()
  }
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape' && isVisible.value && !props.persistent) {
    close()
  }
}

watch(
  () => isVisible.value,
  (val) => {
    if (typeof document !== 'undefined') {
      if (val) {
        document.body.style.overflow = 'hidden'
      } else {
        document.body.style.overflow = ''
      }
    }
  }
)

onMounted(() => {
  if (typeof window !== 'undefined') {
    window.addEventListener('keydown', onKeydown)
  }
})

onUnmounted(() => {
  if (typeof window !== 'undefined') {
    window.removeEventListener('keydown', onKeydown)
  }
  if (typeof document !== 'undefined') {
    document.body.style.overflow = ''
  }
})
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="isVisible"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-slate-950/75 backdrop-blur-md"
        @click.self="handleBackdropClick"
      >
        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 scale-95 translate-y-2"
          enter-to-class="opacity-100 scale-100 translate-y-0"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 scale-100 translate-y-0"
          leave-to-class="opacity-0 scale-95 translate-y-2"
        >
          <div
            v-if="isVisible"
            class="relative w-full rounded-2xl bg-slate-900 border border-slate-800 shadow-2xl shadow-black/80 flex flex-col max-h-[90vh] overflow-hidden"
            :class="maxWidthClass"
          >
            <!-- Header -->
            <div class="flex items-start justify-between px-6 py-5 border-b border-slate-800/80 bg-slate-900/50">
              <slot name="header">
                <div>
                  <h3 v-if="title" class="text-lg font-bold text-white tracking-tight">
                    {{ title }}
                  </h3>
                  <p v-if="description" class="text-xs text-slate-400 mt-1">
                    {{ description }}
                  </p>
                </div>
              </slot>

              <button
                type="button"
                class="rounded-lg p-1.5 text-slate-400 hover:text-white hover:bg-slate-800 transition-colors focus:outline-none focus:ring-2 focus:ring-brand-500"
                @click="close"
              >
                <X class="w-5 h-5" />
                <span class="sr-only">Cerrar</span>
              </button>
            </div>

            <!-- Body -->
            <div class="px-6 py-5 overflow-y-auto space-y-4">
              <slot />
            </div>

            <!-- Footer -->
            <div
              v-if="$slots.footer"
              class="px-6 py-4 border-t border-slate-800/80 bg-slate-950/40 flex items-center justify-end gap-3"
            >
              <slot name="footer" />
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
