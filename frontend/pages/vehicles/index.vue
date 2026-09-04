<script setup lang="ts">
import {
  Truck,
  Plus,
  Search,
  RefreshCw,
  Wrench,
  CheckCircle2,
  AlertTriangle,
  Users,
  ShieldCheck,
  Inbox
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import CreateVehicleModal from '~/components/vehicles/CreateVehicleModal.vue'
import type { VehicleResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Flota y Vehículos'
})

const api = useApi()
const toasts = useToasts()
const vehicles = ref<VehicleResponse[]>([])
const loading = ref(true)
const searchQuery = ref('')
const selectedStatus = ref('ALL')
const isCreateModalOpen = ref(false)

async function fetchVehicles() {
  loading.value = true
  try {
    const res = await api.get<VehicleResponse[]>('/v1/vehicles')
    vehicles.value = res || []
  } catch {
    vehicles.value = []
  } finally {
    loading.value = false
  }
}

const filteredVehicles = computed(() => {
  return vehicles.value.filter(v => {
    const matchSearch =
      searchQuery.value.trim() === '' ||
      v.licensePlate.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      v.make.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      v.model.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (v.color && v.color.toLowerCase().includes(searchQuery.value.toLowerCase()))

    const statusVal = v.status || v.operationalStatus || 'Operational'
    const matchStatus = selectedStatus.value === 'ALL' || statusVal === selectedStatus.value

    return matchSearch && matchStatus
  })
})

const kpiTotal = computed(() => vehicles.value.length)
const kpiOperational = computed(() => vehicles.value.filter(v => (v.status || v.operationalStatus) === 'Active' || (v.status || v.operationalStatus) === 'Operational').length)
const kpiMaintenance = computed(() => vehicles.value.filter(v => (v.status || v.operationalStatus) === 'Maintenance' || (v.status || v.operationalStatus) === 'InMaintenance').length)
const kpiCapacity = computed(() => {
  return vehicles.value.reduce((acc, v) => acc + (v.passengerCapacity || v.capacity || 0), 0)
})

onMounted(() => {
  fetchVehicles()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Truck class="w-3.5 h-3.5" />
            Flota de Transporte
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Vehículos y Unidades</h2>
        <p class="text-xs text-slate-400 mt-1">Control de unidades vehiculares, capacidades y estado operativo.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchVehicles">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>

        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Registrar Vehículo</span>
        </BaseButton>
      </div>
    </div>

    <!-- Operational KPIs -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Total Unidades</p>
            <p class="text-2xl font-black text-white mt-1">{{ kpiTotal }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400">
            <Truck class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Operativas / Activas</p>
            <p class="text-2xl font-black text-emerald-400 mt-1">{{ kpiOperational }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400">
            <CheckCircle2 class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">En Mantenimiento</p>
            <p class="text-2xl font-black text-amber-400 mt-1">{{ kpiMaintenance }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center text-amber-400">
            <Wrench class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Capacidad Total Pasajeros</p>
            <p class="text-2xl font-black text-cyan-400 mt-1">{{ kpiCapacity }} <span class="text-xs font-normal text-slate-400">asientos</span></p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400">
            <Users class="w-5 h-5" />
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
          placeholder="Buscar por placa, marca, modelo o color..."
          class="w-full bg-slate-950 border border-slate-800 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500"
        />
      </div>

      <div class="flex items-center gap-2">
        <select
          v-model="selectedStatus"
          class="bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-300 focus:outline-none focus:border-brand-500"
        >
          <option value="ALL">Todos los Estados</option>
          <option value="Active">Activo</option>
          <option value="Operational">Operativo</option>
          <option value="Maintenance">Mantenimiento</option>
        </select>
      </div>
    </div>

    <!-- Vehicles Table -->
    <BaseCard class="overflow-hidden border-slate-800">
      <div v-if="loading" class="p-12 text-center">
        <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
        <p class="text-sm font-semibold text-white">Consultando flota de vehículos...</p>
        <p class="text-xs text-slate-400 mt-1">Conectando con la base de datos de la empresa</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredVehicles.length === 0" class="p-16 text-center">
        <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
          <Inbox class="w-8 h-8" />
        </div>
        <h3 class="text-base font-bold text-white mb-1">No hay vehículos registrados</h3>
        <p class="text-xs text-slate-400 max-w-sm mx-auto mb-5">
          Registra las vans, cústers, buses o vehículos de tu empresa para asignarlos a despachos y rutas.
        </p>
        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Registrar Primer Vehículo</span>
        </BaseButton>
      </div>

      <!-- Table Content -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-300">
          <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
            <tr>
              <th class="px-4 py-3.5">Placa</th>
              <th class="px-4 py-3.5">Marca y Modelo</th>
              <th class="px-4 py-3.5">Año / Color</th>
              <th class="px-4 py-3.5">Tipo y Capacidad</th>
              <th class="px-4 py-3.5">Propiedad</th>
              <th class="px-4 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-for="v in filteredVehicles" :key="v.id" class="hover:bg-slate-900/40 transition-colors">
              <td class="px-4 py-3.5 font-mono font-bold text-white tracking-wider">
                {{ v.licensePlate }}
              </td>
              <td class="px-4 py-3.5 font-medium text-white">
                <div>{{ v.make }} {{ v.model }}</div>
              </td>
              <td class="px-4 py-3.5 text-slate-300">
                {{ v.manufactureYear || v.year }} · {{ v.color || 'No especificado' }}
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-1.5 text-slate-200">
                  <Users class="w-3.5 h-3.5 text-brand-400" />
                  <span>{{ v.passengerCapacity || v.capacity || 0 }} pasajeros</span>
                </div>
                <div class="text-[10px] text-slate-500 mt-0.5">{{ v.type }}</div>
              </td>
              <td class="px-4 py-3.5">
                <span v-if="v.ownershipType === 'CompanyOwned'" class="text-[11px] text-brand-300 bg-brand-500/10 px-2 py-0.5 rounded-md border border-brand-500/20">
                  Flota Propia
                </span>
                <span v-else class="text-[11px] text-purple-300 bg-purple-500/10 px-2 py-0.5 rounded-md border border-purple-500/20">
                  Tercerizado / Chofer
                </span>
              </td>
              <td class="px-4 py-3.5">
                <BaseBadge v-if="(v.status || v.operationalStatus) === 'Active' || (v.status || v.operationalStatus) === 'Operational'" variant="success">Operativo</BaseBadge>
                <BaseBadge v-else-if="(v.status || v.operationalStatus) === 'Maintenance'" variant="warning">Mantenimiento</BaseBadge>
                <BaseBadge v-else variant="default">{{ v.status || v.operationalStatus }}</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Create Vehicle Modal -->
    <CreateVehicleModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="fetchVehicles"
    />
  </div>
</template>
