<script setup lang="ts">
import ToastContainer from '~/components/common/ToastContainer.vue'
</script>

<template>
  <div class="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
    <!-- Background atmospheric glow -->
    <div class="absolute -top-40 -left-40 w-96 h-96 bg-brand-600/20 rounded-full blur-[128px] pointer-events-none" />
    <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-indigo-600/20 rounded-full blur-[128px] pointer-events-none" />

    <div class="w-full max-w-md z-10">
      <slot />
    </div>

    <ToastContainer />
  </div>
</template>
