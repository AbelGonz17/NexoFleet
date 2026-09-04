<script setup lang="ts">
import {
  MapPin,
  Plus,
  Search,
  RefreshCw,
  Clock,
  Navigation,
  CheckCircle2,
  DollarSign,
  Compass,
  Inbox
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import CreateRouteModal from '~/components/routes/CreateRouteModal.vue'
import type { RouteResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Rutas Maestras y Recorridos Fijos'
})

const api = useApi()
const routes = ref<RouteResponse[]>([])
const loading = ref(true)
const searchQuery = ref('')
const selectedStatus = ref('ALL')
const isCreateModalOpen = ref(false)

async function fetchRoutes() {
  loading.value = true
  try {
    const res = await api.get<RouteResponse[]>('/v1/routes')
    routes.value = res || []
  } catch {
    routes.value = []
  } finally {
    loading.value = false
  }
}

const filteredRoutes = computed(() => {
  return routes.value.filter(r => {
    const matchSearch =
      searchQuery.value.trim() === '' ||
      r.routeCode.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.origin.address.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.destination.address.toLowerCase().includes(searchQuery.value.toLowerCase())

    const matchStatus =
      selectedStatus.value === 'ALL' || r.status === selectedStatus.value

    return matchSearch && matchStatus
  })
})

const kpiTotal = computed(() => routes.value.length)
const kpiActive = computed(() => routes.value.filter(r => r.status === 'Active').length)
const kpiAvgDuration = computed(() => {
  if (routes.value.length === 0) return 0
  const sum = routes.value.reduce((acc, r) => acc + (r.estimatedDurationMinutes || 0), 0)
  return Math.round(sum / routes.value.length)
})
const kpiAvgFare = computed(() => {
  if (routes.value.length === 0) return 0
  const sum = routes.value.reduce((acc, r) => acc + (r.referenceAmount || r.defaultBaseFare || 0), 0)
  return (sum / routes.value.length).toFixed(2)
})

onMounted(() => {
  fetchRoutes()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Compass class="w-3.5 h-3.5" />
            Catálogo Maestro
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Rutas Maestras Fijas</h2>
        <p class="text-xs text-slate-400 mt-1">Configuración de recorridos corporativos fijos, itinerarios estándar y tarifas base.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchRoutes">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>

        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Nueva Ruta Maestra</span>
        </BaseButton>
      </div>
    </div>

    <!-- KPIs -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Total Rutas</p>
            <p class="text-2xl font-black text-white mt-1">{{ kpiTotal }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400">
            <Compass class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Rutas Activas</p>
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
            <p class="text-xs text-slate-400 font-medium">Duración Promedio</p>
            <p class="text-2xl font-black text-cyan-400 mt-1">{{ kpiAvgDuration }} <span class="text-xs font-normal text-slate-400">min</span></p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400">
            <Clock class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Tarifa Referencial Prom.</p>
            <p class="text-2xl font-black text-purple-400 mt-1">{{ kpiAvgFare }} <span class="text-xs font-normal text-slate-400">PEN</span></p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-purple-500/10 border border-purple-500/20 flex items-center justify-center text-purple-400">
            <DollarSign class="w-5 h-5" />
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
          placeholder="Buscar por código de ruta, nombre, origen o destino..."
          class="w-full bg-slate-950 border border-slate-800 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500"
        />
      </div>

      <div class="flex items-center gap-2">
        <select
          v-model="selectedStatus"
          class="bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-300 focus:outline-none focus:border-brand-500"
        >
          <option value="ALL">Todos los Estados</option>
          <option value="Active">Activa</option>
          <option value="Inactive">Inactiva</option>
        </select>
      </div>
    </div>

    <!-- Routes Table -->
    <BaseCard class="overflow-hidden border-slate-800">
      <div v-if="loading" class="p-12 text-center">
        <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
        <p class="text-sm font-semibold text-white">Consultando rutas maestras...</p>
        <p class="text-xs text-slate-400 mt-1">Conectando con la base de datos de la empresa</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredRoutes.length === 0" class="p-16 text-center">
        <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
          <Inbox class="w-8 h-8" />
        </div>
        <h3 class="text-base font-bold text-white mb-1">No hay rutas maestras registradas</h3>
        <p class="text-xs text-slate-400 max-w-sm mx-auto mb-5">
          Crea tu primera ruta corporativa fija con puntos de partida y llegada para usarla en despachos y programaciones.
        </p>
        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Crear Primera Ruta</span>
        </BaseButton>
      </div>

      <!-- Table Content -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-300">
          <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
            <tr>
              <th class="px-4 py-3.5">Código</th>
              <th class="px-4 py-3.5">Nombre de la Ruta</th>
              <th class="px-4 py-3.5">Punto de Origen</th>
              <th class="px-4 py-3.5">Punto de Destino</th>
              <th class="px-4 py-3.5">Duración Est.</th>
              <th class="px-4 py-3.5">Tarifa Base</th>
              <th class="px-4 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-for="r in filteredRoutes" :key="r.id" class="hover:bg-slate-900/40 transition-colors">
              <td class="px-4 py-3.5 font-bold font-mono text-brand-400">
                {{ r.routeCode }}
              </td>
              <td class="px-4 py-3.5 font-bold text-white">
                <div>{{ r.name }}</div>
                <div v-if="r.instructions" class="text-[11px] text-slate-500 font-normal truncate max-w-xs mt-0.5">
                  {{ r.instructions }}
                </div>
              </td>
              <td class="px-4 py-3.5 max-w-xs text-slate-300">
                <div class="flex items-center gap-1.5 truncate">
                  <MapPin class="w-3.5 h-3.5 text-emerald-400 shrink-0" />
                  <span class="truncate">{{ r.origin.address }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5 max-w-xs text-slate-300">
                <div class="flex items-center gap-1.5 truncate">
                  <MapPin class="w-3.5 h-3.5 text-rose-400 shrink-0" />
                  <span class="truncate">{{ r.destination.address }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5 text-slate-200 font-medium">
                {{ r.estimatedDurationMinutes }} min
              </td>
              <td class="px-4 py-3.5 font-bold text-white">
                {{ r.referenceAmount ?? r.defaultBaseFare ?? 0 }} {{ r.referenceCurrency ?? r.currency ?? 'PEN' }}
              </td>
              <td class="px-4 py-3.5">
                <BaseBadge v-if="r.status === 'Active'" variant="success">Activa</BaseBadge>
                <BaseBadge v-else variant="danger">Inactiva</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Create Route Modal -->
    <CreateRouteModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="fetchRoutes"
    />
  </div>
</template>
