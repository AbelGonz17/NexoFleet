<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral'
    size?: 'sm' | 'md'
    dot?: boolean
  }>(),
  {
    variant: 'neutral',
    size: 'md',
    dot: false
  }
)

const variants = {
  primary: 'bg-brand-500/10 text-brand-300 border-brand-500/20',
  success: 'bg-emerald-500/10 text-emerald-300 border-emerald-500/20',
  warning: 'bg-amber-500/10 text-amber-300 border-amber-500/20',
  danger: 'bg-rose-500/10 text-rose-300 border-rose-500/20',
  info: 'bg-sky-500/10 text-sky-300 border-sky-500/20',
  neutral: 'bg-slate-800/60 text-slate-300 border-slate-700/60'
}

const dotColors = {
  primary: 'bg-brand-400',
  success: 'bg-emerald-400',
  warning: 'bg-amber-400',
  danger: 'bg-rose-400',
  info: 'bg-sky-400',
  neutral: 'bg-slate-400'
}

const sizes = {
  sm: 'px-2 py-0.5 text-xs',
  md: 'px-2.5 py-1 text-xs font-medium'
}
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 rounded-lg border leading-none select-none"
    :class="[variants[props.variant], sizes[props.size]]"
  >
    <span v-if="dot" class="w-1.5 h-1.5 rounded-full shrink-0 animate-pulse" :class="dotColors[props.variant]" />
    <slot />
  </span>
</template>
