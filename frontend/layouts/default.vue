<script setup lang="ts">
import AppSidebar from '~/components/layout/AppSidebar.vue'
import AppNavbar from '~/components/layout/AppNavbar.vue'
import ToastContainer from '~/components/common/ToastContainer.vue'
</script>

<template>
  <div class="min-h-screen bg-slate-950 flex text-slate-100">
    <AppSidebar />
    <div class="flex-1 flex flex-col min-w-0">
      <AppNavbar />
      <main class="flex-1 p-8 overflow-y-auto">
        <slot />
      </main>
    </div>
    <ToastContainer />
  </div>
</template>
