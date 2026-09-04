<script setup lang="ts">
import { Bell, Search, Building2, Globe, Truck, Sparkles } from 'lucide-vue-next'

const auth = useAuth()
const permissions = usePermissions()

const activeContextLabel = computed(() => {
  if (permissions.isSuperAdmin.value) {
    return 'Consola Global Multi-Tenant'
  }
  if (permissions.companyName.value) {
    return permissions.companyName.value
  }
  if (permissions.isCompanyAdmin.value) {
    return 'Empresa Asignada'
  }
  return 'Personal Operativo'
})
</script>

<template>
  <header class="h-16 border-b border-slate-800 bg-slate-950/60 backdrop-blur-xl px-6 flex items-center justify-between sticky top-0 z-20">
    <!-- Search / Context -->
    <div class="flex items-center gap-4">
      <div class="relative w-72">
        <Search class="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
        <input
          type="text"
          :placeholder="permissions.isSuperAdmin.value ? 'Buscar empresas, auditoría, usuarios...' : 'Buscar viajes, vehículos, rutas...'"
          class="w-full bg-slate-900/80 border border-slate-800 rounded-xl pl-9 pr-3.5 py-1.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
        />
      </div>
    </div>

    <!-- Right Actions -->
    <div class="flex items-center gap-3">
      <!-- Tenant / Role indicator -->
      <div
        class="flex items-center gap-2 px-3.5 py-1.5 rounded-xl border text-xs font-semibold shadow-sm transition-all"
        :class="permissions.isSuperAdmin.value
          ? 'bg-purple-500/10 border-purple-500/20 text-purple-300'
          : permissions.isCompanyAdmin.value
          ? 'bg-brand-500/10 border-brand-500/20 text-brand-300'
          : 'bg-emerald-500/10 border-emerald-500/20 text-emerald-300'"
      >
        <Globe v-if="permissions.isSuperAdmin.value" class="w-3.5 h-3.5 text-purple-400 shrink-0" />
        <Building2 v-else-if="permissions.isCompanyAdmin.value" class="w-3.5 h-3.5 text-brand-400 shrink-0" />
        <Truck v-else class="w-3.5 h-3.5 text-emerald-400 shrink-0" />

        <span class="max-w-[200px] truncate" :title="activeContextLabel">
          {{ activeContextLabel }}
        </span>

        <span
          v-if="permissions.isSuperAdmin.value"
          class="text-[10px] font-bold bg-purple-500/20 text-purple-200 px-1.5 py-0.2 rounded"
        >
          SuperAdmin
        </span>
        <span
          v-else-if="permissions.companyName.value"
          class="text-[10px] font-bold bg-brand-500/20 text-brand-200 px-1.5 py-0.2 rounded"
        >
          Activa
        </span>
      </div>

      <!-- Notifications Link -->
      <NuxtLink
        to="/notifications"
        class="relative p-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800/60 transition-colors border border-transparent hover:border-slate-700"
        title="Centro de Notificaciones"
      >
        <Bell class="w-4 h-4" />
        <span class="absolute top-1 right-1 w-2 h-2 rounded-full bg-brand-500 ring-2 ring-slate-950 animate-pulse" />
      </NuxtLink>
    </div>
  </header>
</template>

