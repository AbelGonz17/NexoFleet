<script setup lang="ts">
import { CheckCircle, AlertTriangle, AlertCircle, Info, X } from 'lucide-vue-next'

const toasts = useToasts()
</script>

<template>
  <div class="fixed bottom-5 right-5 z-50 flex flex-col gap-2.5 max-w-sm w-full pointer-events-none">
    <TransitionGroup
      enter-active-class="transform ease-out duration-300 transition"
      enter-from-class="translate-y-2 opacity-0 sm:translate-y-0 sm:translate-x-2"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
      leave-active-class="transition ease-in duration-100"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-for="toast in toasts.toasts.value"
        :key="toast.id"
        class="pointer-events-auto flex items-start gap-3 p-4 rounded-xl shadow-2xl border text-sm backdrop-blur-lg"
        :class="{
          'bg-emerald-950/90 border-emerald-500/30 text-emerald-200': toast.type === 'success',
          'bg-rose-950/90 border-rose-500/30 text-rose-200': toast.type === 'error',
          'bg-amber-950/90 border-amber-500/30 text-amber-200': toast.type === 'warning',
          'bg-slate-900/90 border-slate-700/60 text-slate-200': toast.type === 'info'
        }"
      >
        <div class="mt-0.5 shrink-0">
          <CheckCircle v-if="toast.type === 'success'" class="w-5 h-5 text-emerald-400" />
          <AlertCircle v-else-if="toast.type === 'error'" class="w-5 h-5 text-rose-400" />
          <AlertTriangle v-else-if="toast.type === 'warning'" class="w-5 h-5 text-amber-400" />
          <Info v-else class="w-5 h-5 text-sky-400" />
        </div>

        <div class="flex-1 min-w-0">
          <p class="font-semibold text-white leading-5">{{ toast.title }}</p>
          <p v-if="toast.message" class="mt-1 text-xs opacity-90 leading-relaxed break-words">{{ toast.message }}</p>
        </div>

        <button
          type="button"
          class="shrink-0 p-1 rounded-lg opacity-60 hover:opacity-100 transition-opacity"
          @click="toasts.remove(toast.id)"
        >
          <X class="w-4 h-4" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>
