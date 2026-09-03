<script setup lang="ts">
import { Calendar, Plus } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Programaciones Recurrentes'
})

const api = useApi()
const schedules = ref<any[]>([])
const loading = ref(true)

async function fetchSchedules() {
  loading.value = true
  try {
    schedules.value = await api.get<any[]>('/v1/route-schedules')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchSchedules()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Programaciones de Rutas</h2>
        <p class="text-xs text-slate-400 mt-1">Horarios recurrentes, turnos y asignación de choferes y unidades.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Nueva Programación</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">ID Programación</th>
              <th class="px-6 py-3.5">Hora Inicio</th>
              <th class="px-6 py-3.5">Turno</th>
              <th class="px-6 py-3.5">Días</th>
              <th class="px-6 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="5" class="py-8 text-slate-500 font-medium">Cargando programaciones...</td>
            </tr>
            <tr v-else-if="schedules.length === 0" class="text-center py-12">
              <td colspan="5" class="py-8 text-slate-500 font-medium">No hay programaciones registradas.</td>
            </tr>
            <tr v-for="s in schedules" :key="s.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-mono font-bold text-white">{{ s.id.slice(0, 8) }}</td>
              <td class="px-6 py-4 font-semibold text-slate-200">{{ s.startTime }}</td>
              <td class="px-6 py-4 text-slate-300">{{ s.shift }}</td>
              <td class="px-6 py-4 text-slate-400">{{ s.daysOfWeek?.join(', ') || 'N/A' }}</td>
              <td class="px-6 py-4">
                <BaseBadge :variant="s.status === 'Active' ? 'success' : 'neutral'" size="sm">{{ s.status }}</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
