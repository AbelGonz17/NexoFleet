<script setup lang="ts">
import { MapPin, Plus } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { RouteResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Rutas Maestras'
})

const api = useApi()
const routes = ref<RouteResponse[]>([])
const loading = ref(true)

async function fetchRoutes() {
  loading.value = true
  try {
    routes.value = await api.get<RouteResponse[]>('/v1/routes')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchRoutes()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Rutas Maestras</h2>
        <p class="text-xs text-slate-400 mt-1">Configuración de trayectos, paradas intermedias, tiempos estimados y tarifas base.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Nueva Ruta</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Código</th>
              <th class="px-6 py-3.5">Nombre de Ruta</th>
              <th class="px-6 py-3.5">Origen & Destino</th>
              <th class="px-6 py-3.5">Duración Estimada</th>
              <th class="px-6 py-3.5">Tarifa Base</th>
              <th class="px-6 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="6" class="py-8 text-slate-500 font-medium">Cargando rutas...</td>
            </tr>
            <tr v-else-if="routes.length === 0" class="text-center py-12">
              <td colspan="6" class="py-8 text-slate-500 font-medium">No hay rutas configuradas.</td>
            </tr>
            <tr v-for="r in routes" :key="r.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white">{{ r.routeCode }}</td>
              <td class="px-6 py-4 font-semibold text-slate-200">{{ r.name }}</td>
              <td class="px-6 py-4">
                <p class="text-slate-300 font-medium">{{ r.origin.address }}</p>
                <p class="text-[10px] text-slate-500">hacia {{ r.destination.address }}</p>
              </td>
              <td class="px-6 py-4 text-slate-300">{{ r.estimatedDurationMinutes }} min</td>
              <td class="px-6 py-4 font-semibold text-slate-200">{{ r.defaultBaseFare }} {{ r.currency }}</td>
              <td class="px-6 py-4">
                <BaseBadge :variant="r.status === 'Active' ? 'success' : 'neutral'" size="sm">{{ r.status }}</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
