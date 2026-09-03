<script setup lang="ts">
withDefaults(
  defineProps<{
    title?: string
    subtitle?: string
    padding?: 'none' | 'sm' | 'md' | 'lg'
  }>(),
  {
    padding: 'md'
  }
)

const paddingClasses = {
  none: '',
  sm: 'p-4',
  md: 'p-6',
  lg: 'p-8'
}
</script>

<template>
  <div class="rounded-2xl border border-slate-800 bg-slate-900/60 backdrop-blur-xl shadow-xl overflow-hidden transition-all duration-200">
    <div v-if="title || $slots.header" class="px-6 py-4 border-b border-slate-800/80 flex items-center justify-between">
      <div>
        <h3 v-if="title" class="text-base font-semibold text-white tracking-tight">{{ title }}</h3>
        <p v-if="subtitle" class="text-xs text-slate-400 mt-0.5">{{ subtitle }}</p>
      </div>
      <slot name="header" />
    </div>

    <div :class="paddingClasses[padding]">
      <slot />
    </div>

    <div v-if="$slots.footer" class="px-6 py-3.5 bg-slate-950/40 border-t border-slate-800/80 flex items-center justify-end gap-3">
      <slot name="footer" />
    </div>
  </div>
</template>
