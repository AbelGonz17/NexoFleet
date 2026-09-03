<script setup lang="ts">
import { Bell, Search, ShieldCheck } from 'lucide-vue-next'

const auth = useAuth()
</script>

<template>
  <header class="h-16 border-b border-slate-800 bg-slate-950/60 backdrop-blur-xl px-6 flex items-center justify-between sticky top-0 z-20">
    <!-- Search / Context -->
    <div class="flex items-center gap-4">
      <div class="relative w-72">
        <Search class="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
        <input
          type="text"
          placeholder="Buscar viajes, placas, rutas..."
          class="w-full bg-slate-900/80 border border-slate-800 rounded-xl pl-9 pr-3.5 py-1.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
        />
      </div>
    </div>

    <!-- Right Actions -->
    <div class="flex items-center gap-3">
      <!-- Tenant indicator -->
      <div class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-slate-900 border border-slate-800 text-xs text-slate-300">
        <ShieldCheck class="w-3.5 h-3.5 text-brand-400" />
        <span class="font-medium">NexoFleet Multi-Tenant</span>
      </div>

      <!-- Notifications -->
      <NuxtLink
        to="/notifications"
        class="relative p-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800/60 transition-colors border border-transparent hover:border-slate-700"
      >
        <Bell class="w-4 h-4" />
        <span class="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-brand-500 ring-2 ring-slate-950" />
      </NuxtLink>
    </div>
  </header>
</template>
