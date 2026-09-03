<script setup lang="ts">
import { Truck, Plus } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { VehicleResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Flota y Vehículos'
})

const api = useApi()
const vehicles = ref<VehicleResponse[]>([])
const loading = ref(true)

async function fetchVehicles() {
  loading.value = true
  try {
    vehicles.value = await api.get<VehicleResponse[]>('/v1/vehicles')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchVehicles()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Inventario de Flota</h2>
        <p class="text-xs text-slate-400 mt-1">Monitoreo de unidades propias y de terceros, estado operativo y documentos.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Registrar Vehículo</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Placa</th>
              <th class="px-6 py-3.5">Marca y Modelo</th>
              <th class="px-6 py-3.5">Tipo Propiedad</th>
              <th class="px-6 py-3.5">Capacidad</th>
              <th class="px-6 py-3.5">Estado Operativo</th>
              <th class="px-6 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="6" class="py-8 text-slate-500 font-medium">Cargando vehículos de la flota...</td>
            </tr>
            <tr v-else-if="vehicles.length === 0" class="text-center py-12">
              <td colspan="6" class="py-8 text-slate-500 font-medium">No hay vehículos registrados en la empresa.</td>
            </tr>
            <tr v-for="v in vehicles" :key="v.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white tracking-wider">{{ v.licensePlate }}</td>
              <td class="px-6 py-4 text-slate-200">{{ v.make }} {{ v.model }} ({{ v.year }})</td>
              <td class="px-6 py-4 text-slate-300">
                <span v-if="v.ownershipType === 'CompanyOwned'">Propio</span>
                <span v-else>Tercero (Conductor)</span>
              </td>
              <td class="px-6 py-4 text-slate-300">{{ v.capacity }} pasajeros</td>
              <td class="px-6 py-4">
                <BaseBadge
                  :variant="v.operationalStatus === 'Operational' ? 'success' : v.operationalStatus === 'Maintenance' ? 'warning' : 'danger'"
                  size="sm"
                >
                  {{ v.operationalStatus }}
                </BaseBadge>
              </td>
              <td class="px-6 py-4 text-right">
                <button class="text-brand-400 hover:text-brand-300 font-semibold text-xs">Ficha</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
