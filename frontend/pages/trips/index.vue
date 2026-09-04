<script setup lang="ts">
import {
  Navigation,
  Plus,
  Search,
  RefreshCw,
  Clock,
  MapPin,
  CheckCircle2,
  AlertCircle,
  Truck,
  User,
  DollarSign,
  Play,
  CheckSquare,
  XCircle,
  ThumbsUp,
  Inbox
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import PlanTripModal from '~/components/trips/PlanTripModal.vue'
import type { TripResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Despacho de Viajes Operativos'
})

const api = useApi()
const toasts = useToasts()
const trips = ref<TripResponse[]>([])
const loading = ref(true)
const searchQuery = ref('')
const selectedStatus = ref('ALL')
const isPlanModalOpen = ref(false)

// Action modal states
const actionLoading = ref(false)
const isCompleteModalOpen = ref(false)
const isCancelModalOpen = ref(false)
const isRejectModalOpen = ref(false)
const selectedTrip = ref<TripResponse | null>(null)
const completeAmount = ref<number>(0)
const completeCurrency = ref('PEN')
const cancelReason = ref('')
const rejectReason = ref('')

async function fetchTrips() {
  loading.value = true
  try {
    const res = await api.get<TripResponse[]>('/v1/trips')
    trips.value = res || []
  } catch {
    trips.value = []
  } finally {
    loading.value = false
  }
}

const filteredTrips = computed(() => {
  return trips.value.filter(t => {
    const matchSearch =
      searchQuery.value.trim() === '' ||
      t.tripNumber.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      t.origin.address.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      t.destination.address.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (t.currentAssignment?.employeeName && t.currentAssignment.employeeName.toLowerCase().includes(searchQuery.value.toLowerCase())) ||
      (t.currentAssignment?.licensePlate && t.currentAssignment.licensePlate.toLowerCase().includes(searchQuery.value.toLowerCase()))

    const matchStatus =
      selectedStatus.value === 'ALL' || t.status === selectedStatus.value

    return matchSearch && matchStatus
  })
})

const kpiPlanned = computed(() => trips.value.filter(t => t.status === 'Planned' || t.status === 'Assigned').length)
const kpiInProgress = computed(() => trips.value.filter(t => t.status === 'InProgress').length)
const kpiCompleted = computed(() => trips.value.filter(t => t.status === 'Completed').length)
const kpiPendingApproval = computed(() => trips.value.filter(t => t.status === 'PendingApproval').length)

// Actions
async function startTrip(trip: TripResponse) {
  if (!trip.currentAssignment?.employeeId) {
    toasts.warning('El viaje debe tener un conductor asignado para ser iniciado.')
    return
  }
  actionLoading.value = true
  try {
    await api.post(`/v1/trips/${trip.id}/start?employeeId=${trip.currentAssignment.employeeId}`, {})
    toasts.success(`Viaje ${trip.tripNumber} iniciado en ruta.`)
    await fetchTrips()
  } catch {
    // Handled
  } finally {
    actionLoading.value = false
  }
}

function openCompleteModal(trip: TripResponse) {
  selectedTrip.value = trip
  completeAmount.value = trip.agreedAmount || 0
  completeCurrency.value = trip.currency || 'PEN'
  isCompleteModalOpen.value = true
}

async function handleCompleteTrip() {
  if (!selectedTrip.value) return
  const empId = selectedTrip.value.currentAssignment?.employeeId
  if (!empId) {
    toasts.warning('El viaje requiere un conductor asignado para ser completado.')
    return
  }
  actionLoading.value = true
  try {
    await api.post(`/v1/trips/${selectedTrip.value.id}/complete?employeeId=${empId}`, {
      finalAmount: completeAmount.value,
      currency: completeCurrency.value
    })
    toasts.success(`Viaje ${selectedTrip.value.tripNumber} completado exitosamente.`)
    isCompleteModalOpen.value = false
    await fetchTrips()
  } catch {
    // Handled
  } finally {
    actionLoading.value = false
  }
}

function openCancelModal(trip: TripResponse) {
  selectedTrip.value = trip
  cancelReason.value = ''
  isCancelModalOpen.value = true
}

async function handleCancelTrip() {
  if (!selectedTrip.value) return
  if (!cancelReason.value.trim()) {
    toasts.warning('Debes ingresar el motivo de la cancelación.')
    return
  }
  actionLoading.value = true
  try {
    await api.post(`/v1/trips/${selectedTrip.value.id}/cancel`, {
      reason: cancelReason.value.trim()
    })
    toasts.success(`Viaje ${selectedTrip.value.tripNumber} cancelado.`)
    isCancelModalOpen.value = false
    await fetchTrips()
  } catch {
    // Handled
  } finally {
    actionLoading.value = false
  }
}

async function approveTrip(trip: TripResponse) {
  actionLoading.value = true
  try {
    await api.post(`/v1/trips/${trip.id}/approve`, {
      comments: 'Aprobado administrativamente desde el panel de control.'
    })
    toasts.success(`Viaje ${trip.tripNumber} aprobado.`)
    await fetchTrips()
  } catch {
    // Handled
  } finally {
    actionLoading.value = false
  }
}

function openRejectModal(trip: TripResponse) {
  selectedTrip.value = trip
  rejectReason.value = ''
  isRejectModalOpen.value = true
}

async function handleRejectTrip() {
  if (!selectedTrip.value) return
  if (!rejectReason.value.trim()) {
    toasts.warning('Debes especificar el motivo del rechazo.')
    return
  }
  actionLoading.value = true
  try {
    await api.post(`/v1/trips/${selectedTrip.value.id}/reject`, {
      reason: rejectReason.value.trim()
    })
    toasts.success(`Viaje ${selectedTrip.value.tripNumber} rechazado.`)
    isRejectModalOpen.value = false
    await fetchTrips()
  } catch {
    // Handled
  } finally {
    actionLoading.value = false
  }
}

onMounted(() => {
  fetchTrips()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Navigation class="w-3.5 h-3.5" />
            Despachos y Rutas
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Despacho de Viajes</h2>
        <p class="text-xs text-slate-400 mt-1">Gestión integral de viajes planificados, asignación de unidades y monitoreo en tiempo real.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchTrips">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>

        <BaseButton variant="primary" size="md" @click="isPlanModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Planificar Viaje</span>
        </BaseButton>
      </div>
    </div>

    <!-- Operational KPIs -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Total Registrados</p>
            <p class="text-2xl font-black text-white mt-1">{{ trips.length }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400">
            <Navigation class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">En Curso</p>
            <p class="text-2xl font-black text-emerald-400 mt-1">{{ kpiInProgress }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400">
            <Clock class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Planificados / Listos</p>
            <p class="text-2xl font-black text-cyan-400 mt-1">{{ kpiPlanned }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400">
            <CheckCircle2 class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Por Aprobar</p>
            <p class="text-2xl font-black text-amber-400 mt-1">{{ kpiPendingApproval }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center text-amber-400">
            <AlertCircle class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filter & Search Bar -->
    <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-slate-900/60 border border-slate-800/80 rounded-2xl p-3">
      <div class="relative flex-1">
        <Search class="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por nº de viaje, origen, destino, conductor o placa..."
          class="w-full bg-slate-950 border border-slate-800 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500"
        />
      </div>

      <div class="flex items-center gap-2">
        <select
          v-model="selectedStatus"
          class="bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-300 focus:outline-none focus:border-brand-500"
        >
          <option value="ALL">Todos los Estados</option>
          <option value="Planned">Planificado</option>
          <option value="Assigned">Asignado</option>
          <option value="InProgress">En Curso</option>
          <option value="Completed">Completado</option>
          <option value="PendingApproval">Por Aprobar</option>
          <option value="Cancelled">Cancelado</option>
        </select>
      </div>
    </div>

    <!-- Trips Table -->
    <BaseCard class="overflow-hidden border-slate-800">
      <div v-if="loading" class="p-12 text-center">
        <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
        <p class="text-sm font-semibold text-white">Consultando viajes operativos...</p>
        <p class="text-xs text-slate-400 mt-1">Conectando con la base de datos de la empresa</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredTrips.length === 0" class="p-16 text-center">
        <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
          <Inbox class="w-8 h-8" />
        </div>
        <h3 class="text-base font-bold text-white mb-1">No hay viajes registrados</h3>
        <p class="text-xs text-slate-400 max-w-sm mx-auto mb-5">
          Comienza a registrar la operación planificando el primer viaje de tu empresa de transporte.
        </p>
        <BaseButton variant="primary" size="md" @click="isPlanModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Planificar Primer Viaje</span>
        </BaseButton>
      </div>

      <!-- Table Content -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-300">
          <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
            <tr>
              <th class="px-4 py-3.5">Nº Viaje</th>
              <th class="px-4 py-3.5">Itinerario (Origen -> Destino)</th>
              <th class="px-4 py-3.5">Conductor y Unidad</th>
              <th class="px-4 py-3.5">Fecha</th>
              <th class="px-4 py-3.5">Monto Acordado</th>
              <th class="px-4 py-3.5">Estado</th>
              <th class="px-4 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-for="t in filteredTrips" :key="t.id" class="hover:bg-slate-900/40 transition-colors">
              <td class="px-4 py-3.5 font-bold text-white">
                <div>{{ t.tripNumber }}</div>
                <div class="text-[10px] text-slate-500 font-mono mt-0.5">{{ t.source }}</div>
              </td>
              <td class="px-4 py-3.5 max-w-xs">
                <div class="flex items-center gap-1.5 text-slate-200 truncate">
                  <MapPin class="w-3.5 h-3.5 text-emerald-400 shrink-0" />
                  <span class="truncate">{{ t.origin.address }}</span>
                </div>
                <div class="flex items-center gap-1.5 text-slate-400 truncate mt-1">
                  <MapPin class="w-3.5 h-3.5 text-rose-400 shrink-0" />
                  <span class="truncate">{{ t.destination.address }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5">
                <div v-if="t.currentAssignment" class="space-y-0.5">
                  <div class="flex items-center gap-1.5 text-white font-medium">
                    <User class="w-3.5 h-3.5 text-brand-400" />
                    <span>{{ t.currentAssignment.employeeName || 'Conductor Asignado' }}</span>
                  </div>
                  <div class="text-[11px] text-slate-400 flex items-center gap-1.5">
                    <Truck class="w-3 h-3 text-slate-500" />
                    <span>{{ t.currentAssignment.licensePlate || 'Unidad Asignada' }}</span>
                  </div>
                </div>
                <span v-else class="text-slate-500 italic text-[11px]">Sin asignar</span>
              </td>
              <td class="px-4 py-3.5 font-mono text-slate-300">
                {{ t.serviceDate }}
              </td>
              <td class="px-4 py-3.5 font-bold text-white">
                {{ t.finalAmount ?? t.agreedAmount }} {{ t.currency }}
              </td>
              <td class="px-4 py-3.5">
                <BaseBadge v-if="t.status === 'Completed'" variant="success">Completado</BaseBadge>
                <BaseBadge v-else-if="t.status === 'InProgress'" variant="brand">En Curso</BaseBadge>
                <BaseBadge v-else-if="t.status === 'Planned' || t.status === 'Assigned'" variant="default">Planificado</BaseBadge>
                <BaseBadge v-else-if="t.status === 'PendingApproval'" variant="warning">Por Aprobar</BaseBadge>
                <BaseBadge v-else-if="t.status === 'Cancelled'" variant="danger">Cancelado</BaseBadge>
                <BaseBadge v-else variant="default">{{ t.status }}</BaseBadge>
              </td>
              <td class="px-4 py-3.5 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <!-- Start button for Planned / Assigned -->
                  <button
                    v-if="(t.status === 'Planned' || t.status === 'Assigned') && t.currentAssignment"
                    class="p-1.5 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 rounded-lg transition-colors"
                    title="Iniciar Viaje"
                    @click="startTrip(t)"
                  >
                    <Play class="w-3.5 h-3.5" />
                  </button>

                  <!-- Complete button for InProgress -->
                  <button
                    v-if="t.status === 'InProgress'"
                    class="p-1.5 bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-400 rounded-lg transition-colors"
                    title="Completar Viaje"
                    @click="openCompleteModal(t)"
                  >
                    <CheckSquare class="w-3.5 h-3.5" />
                  </button>

                  <!-- Approve / Reject for PendingApproval -->
                  <template v-if="t.status === 'PendingApproval'">
                    <button
                      class="p-1.5 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 rounded-lg transition-colors"
                      title="Aprobar Viaje"
                      @click="approveTrip(t)"
                    >
                      <ThumbsUp class="w-3.5 h-3.5" />
                    </button>
                    <button
                      class="p-1.5 bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 rounded-lg transition-colors"
                      title="Rechazar Viaje"
                      @click="openRejectModal(t)"
                    >
                      <XCircle class="w-3.5 h-3.5" />
                    </button>
                  </template>

                  <!-- Cancel button for Planned / PendingApproval -->
                  <button
                    v-if="t.status === 'Planned' || t.status === 'Assigned'"
                    class="p-1.5 bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 rounded-lg transition-colors"
                    title="Cancelar Viaje"
                    @click="openCancelModal(t)"
                  >
                    <XCircle class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Plan Trip Modal -->
    <PlanTripModal
      :is-open="isPlanModalOpen"
      @close="isPlanModalOpen = false"
      @created="fetchTrips"
    />

    <!-- Complete Trip Modal -->
    <BaseModal
      :is-open="isCompleteModalOpen"
      title="Completar y Liquidar Viaje"
      @close="isCompleteModalOpen = false"
    >
      <form class="space-y-4" @submit.prevent="handleCompleteTrip">
        <p class="text-xs text-slate-300">
          Registra el monto final devengado para el viaje
          <strong class="text-white">{{ selectedTrip?.tripNumber }}</strong>.
        </p>
        <div class="grid grid-cols-2 gap-3">
          <BaseInput
            v-model.number="completeAmount"
            type="number"
            step="0.01"
            label="Monto Final Devengado"
            required
          />
          <div class="space-y-1">
            <label class="block text-xs font-semibold text-slate-300">Moneda</label>
            <select
              v-model="completeCurrency"
              class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none"
            >
              <option value="PEN">Soles (PEN S/.)</option>
              <option value="USD">Dólares (USD $)</option>
            </select>
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
          <BaseButton type="button" variant="secondary" size="md" @click="isCompleteModalOpen = false">
            Cancelar
          </BaseButton>
          <BaseButton type="submit" variant="primary" size="md" :loading="actionLoading">
            Confirmar Finalización
          </BaseButton>
        </div>
      </form>
    </BaseModal>

    <!-- Cancel Trip Modal -->
    <BaseModal
      :is-open="isCancelModalOpen"
      title="Cancelar Viaje"
      @close="isCancelModalOpen = false"
    >
      <form class="space-y-4" @submit.prevent="handleCancelTrip">
        <p class="text-xs text-slate-300">
          ¿Estás seguro de que deseas cancelar el viaje
          <strong class="text-white">{{ selectedTrip?.tripNumber }}</strong>?
        </p>
        <BaseInput
          v-model="cancelReason"
          label="Motivo de Cancelación"
          placeholder="ej. Cancelación por parte del cliente / Cambio de turno"
          required
        />
        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
          <BaseButton type="button" variant="secondary" size="md" @click="isCancelModalOpen = false">
            Regresar
          </BaseButton>
          <BaseButton type="submit" variant="danger" size="md" :loading="actionLoading">
            Cancelar Viaje
          </BaseButton>
        </div>
      </form>
    </BaseModal>

    <!-- Reject Trip Modal -->
    <BaseModal
      :is-open="isRejectModalOpen"
      title="Rechazar Viaje Imprevisto"
      @close="isRejectModalOpen = false"
    >
      <form class="space-y-4" @submit.prevent="handleRejectTrip">
        <p class="text-xs text-slate-300">
          Indica la justificación para rechazar el viaje imprevisto
          <strong class="text-white">{{ selectedTrip?.tripNumber }}</strong>.
        </p>
        <BaseInput
          v-model="rejectReason"
          label="Motivo del Rechazo"
          placeholder="ej. No autorizado por la gerencia de operaciones"
          required
        />
        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
          <BaseButton type="button" variant="secondary" size="md" @click="isRejectModalOpen = false">
            Volver
          </BaseButton>
          <BaseButton type="submit" variant="danger" size="md" :loading="actionLoading">
            Rechazar Viaje
          </BaseButton>
        </div>
      </form>
    </BaseModal>
  </div>
</template>
