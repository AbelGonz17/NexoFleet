<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    modelValue: string | number
    label?: string
    type?: string
    placeholder?: string
    error?: string
    disabled?: boolean
    required?: boolean
    id?: string
  }>(),
  {
    type: 'text',
    placeholder: '',
    error: '',
    disabled: false,
    required: false
  }
)

defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const inputId = props.id || `input-${Math.random().toString(36).slice(2, 7)}`
</script>

<template>
  <div class="flex flex-col gap-1.5 w-full">
    <label v-if="label" :for="inputId" class="text-xs font-semibold text-slate-300">
      {{ label }}
      <span v-if="required" class="text-rose-400">*</span>
    </label>

    <div class="relative rounded-xl shadow-sm">
      <input
        :id="inputId"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        class="block w-full rounded-xl bg-slate-900/90 border px-3.5 py-2.5 text-sm text-white placeholder-slate-500 transition-colors focus:outline-none focus:ring-2 disabled:bg-slate-900/50 disabled:opacity-50"
        :class="error
          ? 'border-rose-500/60 focus:border-rose-500 focus:ring-rose-500/20'
          : 'border-slate-700/80 focus:border-brand-500 focus:ring-brand-500/20'"
        @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      />
    </div>

    <p v-if="error" class="text-xs text-rose-400 font-medium mt-0.5">
      {{ error }}
    </p>
  </div>
</template>
