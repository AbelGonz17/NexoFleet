<script setup lang="ts">
import { Navigation, Plus, Filter } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { TripResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Viajes Operativos'
})

const api = useApi()
const trips = ref<TripResponse[]>([])
const loading = ref(true)

async function fetchTrips() {
  loading.value = true
  try {
    trips.value = await api.get<TripResponse[]>('/v1/trips')
  } catch {
    // Handled by useApi toast
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchTrips()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Despacho de Viajes</h2>
        <p class="text-xs text-slate-400 mt-1">Gestión integral de viajes planificados, imprevistos y ejecución en tiempo real.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Planificar Viaje</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Nº Viaje</th>
              <th class="px-6 py-3.5">Origen / Destino</th>
              <th class="px-6 py-3.5">Fecha Servicio</th>
              <th class="px-6 py-3.5">Monto Acordado</th>
              <th class="px-6 py-3.5">Estado</th>
              <th class="px-6 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="6" class="py-8 text-slate-500 font-medium">Cargando viajes operativos...</td>
            </tr>
            <tr v-else-if="trips.length === 0" class="text-center py-12">
              <td colspan="6" class="py-8 text-slate-500 font-medium">No hay viajes registrados en el sistema.</td>
            </tr>
            <tr v-for="trip in trips" :key="trip.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white">{{ trip.tripNumber }}</td>
              <td class="px-6 py-4">
                <p class="text-slate-200 font-medium">{{ trip.origin.address }}</p>
                <p class="text-[10px] text-slate-500">hacia {{ trip.destination.address }}</p>
              </td>
              <td class="px-6 py-4 text-slate-300">{{ trip.serviceDate }}</td>
              <td class="px-6 py-4 font-semibold text-slate-200">{{ trip.agreedAmount }} {{ trip.currency }}</td>
              <td class="px-6 py-4">
                <BaseBadge variant="primary" size="sm">{{ trip.status }}</BaseBadge>
              </td>
              <td class="px-6 py-4 text-right">
                <button class="text-brand-400 hover:text-brand-300 font-semibold text-xs">Detalle</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
