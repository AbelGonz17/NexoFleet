<script setup lang="ts">
import { Navigation, Calendar, DollarSign, MapPin, Building2, User, Truck, Route as RouteIcon } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { RouteResponse, VehicleResponse, EmployeeResponse, ClientResponse, TripResponse } from '~/types/api.types'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'created'): void
}>()

const api = useApi()
const toasts = useToasts()
const isSubmitting = ref(false)
const isLoadingCatalogs = ref(false)

const routes = ref<RouteResponse[]>([])
const vehicles = ref<VehicleResponse[]>([])
const employees = ref<EmployeeResponse[]>([])
const clients = ref<ClientResponse[]>([])

const selectedRouteId = ref<string>('')
const selectedClientId = ref<string>('')
const selectedEmployeeId = ref<string>('')
const selectedVehicleId = ref<string>('')

const form = ref({
  tripNumber: '',
  serviceDate: new Date().toISOString().split('T')[0],
  originAddress: '',
  originLat: -12.046374,
  originLng: -77.042793,
  destinationAddress: '',
  destinationLat: -12.091234,
  destinationLng: -77.031543,
  agreedAmount: 180,
  currency: 'PEN'
})

function resetForm() {
  form.value = {
    tripNumber: `VIAJE-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`,
    serviceDate: new Date().toISOString().split('T')[0],
    originAddress: '',
    originLat: -12.046374,
    originLng: -77.042793,
    destinationAddress: '',
    destinationLat: -12.091234,
    destinationLng: -77.031543,
    agreedAmount: 180,
    currency: 'PEN'
  }
  selectedRouteId.value = ''
  selectedClientId.value = ''
  selectedEmployeeId.value = ''
  selectedVehicleId.value = ''
}

async function loadCatalogs() {
  isLoadingCatalogs.value = true
  try {
    const [r, v, e, c] = await Promise.allSettled([
      api.get<RouteResponse[]>('/v1/routes'),
      api.get<VehicleResponse[]>('/v1/vehicles'),
      api.get<EmployeeResponse[]>('/v1/employees'),
      api.get<ClientResponse[]>('/v1/clients')
    ])
    if (r.status === 'fulfilled' && r.value) routes.value = r.value
    if (v.status === 'fulfilled' && v.value) vehicles.value = v.value
    if (e.status === 'fulfilled' && e.value) employees.value = e.value
    if (c.status === 'fulfilled' && c.value) clients.value = c.value
  } finally {
    isLoadingCatalogs.value = false
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    resetForm()
    loadCatalogs()
  }
})

function handleRouteChange() {
  if (!selectedRouteId.value) return
  const route = routes.value.find(r => r.id === selectedRouteId.value)
  if (route) {
    form.value.originAddress = route.origin.address
    form.value.originLat = route.origin.latitude
    form.value.originLng = route.origin.longitude
    form.value.destinationAddress = route.destination.address
    form.value.destinationLat = route.destination.latitude
    form.value.destinationLng = route.destination.longitude
    if (route.defaultBaseFare) {
      form.value.agreedAmount = route.defaultBaseFare
    }
    if (route.currency) {
      form.value.currency = route.currency
    }
    if (route.clientId) {
      selectedClientId.value = route.clientId
    }
  }
}

async function handleSubmit() {
  if (!form.value.tripNumber.trim()) {
    toasts.warning('El código de viaje es requerido.')
    return
  }
  if (!form.value.originAddress.trim() || !form.value.destinationAddress.trim()) {
    toasts.warning('El origen y destino son obligatorios.')
    return
  }

  isSubmitting.value = true
  try {
    const payload = {
      tripNumber: form.value.tripNumber.trim(),
      serviceDate: form.value.serviceDate,
      origin: {
        address: form.value.originAddress.trim(),
        latitude: form.value.originLat,
        longitude: form.value.originLng
      },
      destination: {
        address: form.value.destinationAddress.trim(),
        latitude: form.value.destinationLat,
        longitude: form.value.destinationLng
      },
      clientId: selectedClientId.value || null,
      routeId: selectedRouteId.value || null,
      agreedAmount: form.value.agreedAmount,
      currency: form.value.currency
    }

    const createdTrip = await api.post<TripResponse>('/v1/trips/planned', payload)

    // If driver is selected, automatically assign
    if (createdTrip && createdTrip.id && selectedEmployeeId.value) {
      try {
        await api.post(`/v1/trips/${createdTrip.id}/assign`, {
          employeeId: selectedEmployeeId.value,
          vehicleId: selectedVehicleId.value || null
        })
      } catch {
        toasts.warning('El viaje fue creado pero ocurrió un detalle al asignar la unidad.')
      }
    }

    toasts.success(`Viaje ${form.value.tripNumber} planificado exitosamente.`)
    emit('created')
    emit('close')
  } catch {
    // Handled by useApi toast
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Planificar Nuevo Viaje" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.tripNumber"
          label="Nº de Viaje / Despacho"
          placeholder="ej. VIAJE-2026-101"
          required
        />
        <BaseInput
          v-model="form.serviceDate"
          type="date"
          label="Fecha de Servicio"
          required
        />
      </div>

      <!-- Quick Template / Route Selection -->
      <div v-if="routes.length > 0" class="space-y-1">
        <label class="block text-xs font-semibold text-slate-300">
          Cargar desde Ruta Maestra (Opcional)
        </label>
        <select
          v-model="selectedRouteId"
          class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          @change="handleRouteChange"
        >
          <option value="">-- Ingresar direcciones manualmente --</option>
          <option v-for="r in routes" :key="r.id" :value="r.id">
            {{ r.routeCode }} - {{ r.name }}
          </option>
        </select>
      </div>

      <!-- Origin & Destination -->
      <div class="space-y-3">
        <BaseInput
          v-model="form.originAddress"
          label="Punto de Origen"
          placeholder="ej. Sede Central - Av. Argentina 1230, Lima"
          required
        >
          <template #prefix>
            <MapPin class="w-4 h-4 text-emerald-400" />
          </template>
        </BaseInput>

        <BaseInput
          v-model="form.destinationAddress"
          label="Punto de Destino"
          placeholder="ej. Planta Sur - Carretera Panamericana Sur Km 24"
          required
        >
          <template #prefix>
            <MapPin class="w-4 h-4 text-rose-400" />
          </template>
        </BaseInput>
      </div>

      <!-- Client & Resource Assignments -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Cliente Corporativo</label>
          <select
            v-model="selectedClientId"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="">Sin cliente asociado</option>
            <option v-for="c in clients" :key="c.id" :value="c.id">
              {{ c.name }}
            </option>
          </select>
        </div>

        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Conductor / Chofer</label>
          <select
            v-model="selectedEmployeeId"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="">Sin asignar por ahora</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">
              {{ e.fullName || `${e.firstName} ${e.lastName}` }} ({{ e.employeeCode }})
            </option>
          </select>
        </div>

        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Vehículo / Unidad</label>
          <select
            v-model="selectedVehicleId"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="">Sin asignar por ahora</option>
            <option v-for="v in vehicles" :key="v.id" :value="v.id">
              {{ v.licensePlate }} ({{ v.make }} {{ v.model }})
            </option>
          </select>
        </div>
      </div>

      <!-- Pricing -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model.number="form.agreedAmount"
          type="number"
          step="0.01"
          label="Monto Acordado"
          placeholder="180.00"
        />

        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Moneda</label>
          <select
            v-model="form.currency"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="PEN">Soles (PEN S/.)</option>
            <option value="USD">Dólares (USD $)</option>
          </select>
        </div>
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Planificar Viaje
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
