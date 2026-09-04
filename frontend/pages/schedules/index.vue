<script setup lang="ts">
import {
  Calendar,
  Plus,
  Search,
  RefreshCw,
  Clock,
  Compass,
  CheckCircle2,
  Users,
  Truck,
  User,
  Power,
  Inbox
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import CreateScheduleModal from '~/components/schedules/CreateScheduleModal.vue'
import type { RouteScheduleResponse, RouteResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Programaciones y Turnos'
})

const api = useApi()
const toasts = useToasts()
const schedules = ref<RouteScheduleResponse[]>([])
const routesMap = ref<Record<string, RouteResponse>>({})
const loading = ref(true)
const actionLoading = ref(false)
const searchQuery = ref('')
const selectedShift = ref('ALL')
const isCreateModalOpen = ref(false)

const dayLabels: Record<number, string> = {
  1: 'Lun',
  2: 'Mar',
  3: 'Mié',
  4: 'Jue',
  5: 'Vie',
  6: 'Sáb',
  0: 'Dom'
}

async function fetchSchedules() {
  loading.value = true
  try {
    const [schedRes, routesRes] = await Promise.allSettled([
      api.get<RouteScheduleResponse[]>('/v1/route-schedules'),
      api.get<RouteResponse[]>('/v1/routes')
    ])

    if (routesRes.status === 'fulfilled' && routesRes.value) {
      const map: Record<string, RouteResponse> = {}
      routesRes.value.forEach(r => { map[r.id] = r })
      routesMap.value = map
    }

    if (schedRes.status === 'fulfilled' && schedRes.value) {
      schedules.value = schedRes.value
    } else {
      schedules.value = []
    }
  } catch {
    schedules.value = []
  } finally {
    loading.value = false
  }
}

const filteredSchedules = computed(() => {
  return schedules.value.filter(s => {
    const route = routesMap.value[s.routeId]
    const routeName = route ? `${route.routeCode} ${route.name}` : ''
    const matchSearch =
      searchQuery.value.trim() === '' ||
      routeName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      s.shift.toLowerCase().includes(searchQuery.value.toLowerCase())

    const matchShift =
      selectedShift.value === 'ALL' || s.shift.toLowerCase() === selectedShift.value.toLowerCase()

    return matchSearch && matchShift
  })
})

const kpiTotal = computed(() => schedules.value.length)
const kpiActive = computed(() => schedules.value.filter(s => s.status === 'Active').length)
const kpiMorning = computed(() => schedules.value.filter(s => s.shift === 'Morning' || s.shift === '0').length)
const kpiNight = computed(() => schedules.value.filter(s => s.shift === 'Night' || s.shift === '2').length)

async function toggleScheduleStatus(schedule: RouteScheduleResponse) {
  actionLoading.value = true
  try {
    if (schedule.status === 'Active') {
      await api.post(`/v1/route-schedules/${schedule.id}/deactivate`, {})
      toasts.info('Programación desactivada.')
    } else {
      await api.post(`/v1/route-schedules/${schedule.id}/activate`, {})
      toasts.success('Programación activada.')
    }
    await fetchSchedules()
  } catch {
    // Handled
  } finally {
    actionLoading.value = false
  }
}

function formatDays(days: any[]): string {
  if (!days || days.length === 0) return 'Sin recurrencia'
  return days.map(d => {
    if (typeof d === 'number') return dayLabels[d] || String(d)
    if (typeof d === 'string') {
      const num = parseInt(d)
      if (!isNaN(num)) return dayLabels[num] || d
      return d.substring(0, 3)
    }
    return String(d)
  }).join(', ')
}

onMounted(() => {
  fetchSchedules()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Calendar class="w-3.5 h-3.5" />
            Turnos y Recurrencias
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Programaciones y Horarios</h2>
        <p class="text-xs text-slate-400 mt-1">Horarios de despacho fijo, turnos rotativos y asignación recurrente de recursos.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchSchedules">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>

        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Nueva Programación</span>
        </BaseButton>
      </div>
    </div>

    <!-- KPIs -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Total Programaciones</p>
            <p class="text-2xl font-black text-white mt-1">{{ kpiTotal }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400">
            <Calendar class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Activas</p>
            <p class="text-2xl font-black text-emerald-400 mt-1">{{ kpiActive }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400">
            <CheckCircle2 class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Turnos Mañana</p>
            <p class="text-2xl font-black text-amber-400 mt-1">{{ kpiMorning }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center text-amber-400">
            <Clock class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Turnos Noche</p>
            <p class="text-2xl font-black text-purple-400 mt-1">{{ kpiNight }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-purple-500/10 border border-purple-500/20 flex items-center justify-center text-purple-400">
            <Clock class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filter & Search -->
    <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-slate-900/60 border border-slate-800/80 rounded-2xl p-3">
      <div class="relative flex-1">
        <Search class="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por ruta maestra, turno o código..."
          class="w-full bg-slate-950 border border-slate-800 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500"
        />
      </div>

      <div class="flex items-center gap-2">
        <select
          v-model="selectedShift"
          class="bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-300 focus:outline-none focus:border-brand-500"
        >
          <option value="ALL">Todos los Turnos</option>
          <option value="Morning">Mañana</option>
          <option value="Afternoon">Tarde</option>
          <option value="Night">Noche</option>
        </select>
      </div>
    </div>

    <!-- Schedules Table -->
    <BaseCard class="overflow-hidden border-slate-800">
      <div v-if="loading" class="p-12 text-center">
        <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
        <p class="text-sm font-semibold text-white">Consultando programaciones de rutas...</p>
        <p class="text-xs text-slate-400 mt-1">Conectando con la base de datos de la empresa</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredSchedules.length === 0" class="p-16 text-center">
        <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
          <Inbox class="w-8 h-8" />
        </div>
        <h3 class="text-base font-bold text-white mb-1">No hay programaciones registradas</h3>
        <p class="text-xs text-slate-400 max-w-sm mx-auto mb-5">
          Configura despachos automáticos y turnos recurrentes vinculados a tus rutas maestras.
        </p>
        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Crear Primera Programación</span>
        </BaseButton>
      </div>

      <!-- Table Content -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-300">
          <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
            <tr>
              <th class="px-4 py-3.5">Ruta Maestra</th>
              <th class="px-4 py-3.5">Turno y Horario</th>
              <th class="px-4 py-3.5">Días Recurrentes</th>
              <th class="px-4 py-3.5">Vigencia</th>
              <th class="px-4 py-3.5">Tarifa Servicio</th>
              <th class="px-4 py-3.5">Estado</th>
              <th class="px-4 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-for="s in filteredSchedules" :key="s.id" class="hover:bg-slate-900/40 transition-colors">
              <td class="px-4 py-3.5 font-bold text-white max-w-xs">
                <div>{{ routesMap[s.routeId]?.name || s.routeName || 'Ruta Registrada' }}</div>
                <div class="text-[10px] text-brand-400 font-mono mt-0.5">
                  {{ routesMap[s.routeId]?.routeCode || 'RT' }}
                </div>
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-1.5 text-white font-medium">
                  <Clock class="w-3.5 h-3.5 text-brand-400" />
                  <span>{{ s.startTime }} <span v-if="s.endTime">a {{ s.endTime }}</span></span>
                </div>
                <div class="text-[10px] text-slate-500 mt-0.5 font-semibold uppercase">
                  Turno {{ s.shift }}
                </div>
              </td>
              <td class="px-4 py-3.5">
                <span class="inline-flex items-center px-2 py-0.5 rounded-md bg-slate-800 text-slate-300 font-medium text-[11px]">
                  {{ formatDays(s.days) }}
                </span>
              </td>
              <td class="px-4 py-3.5 font-mono text-slate-300 text-[11px]">
                {{ s.effectiveFrom }}
              </td>
              <td class="px-4 py-3.5 font-bold text-white">
                {{ s.defaultAmount ?? 0 }} {{ s.defaultCurrency || 'PEN' }}
              </td>
              <td class="px-4 py-3.5">
                <BaseBadge v-if="s.status === 'Active'" variant="success">Activa</BaseBadge>
                <BaseBadge v-else variant="danger">Inactiva</BaseBadge>
              </td>
              <td class="px-4 py-3.5 text-right">
                <button
                  class="p-1.5 rounded-lg transition-colors"
                  :class="s.status === 'Active'
                    ? 'bg-rose-500/10 hover:bg-rose-500/20 text-rose-400'
                    : 'bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400'"
                  :title="s.status === 'Active' ? 'Desactivar Programación' : 'Activar Programación'"
                  :disabled="actionLoading"
                  @click="toggleScheduleStatus(s)"
                >
                  <Power class="w-3.5 h-3.5" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Create Schedule Modal -->
    <CreateScheduleModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="fetchSchedules"
    />
  </div>
</template>
