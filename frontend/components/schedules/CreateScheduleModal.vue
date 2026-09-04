<script setup lang="ts">
import { Calendar, Clock, Compass, DollarSign, Layers, User, Truck } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { RouteResponse, VehicleResponse, EmployeeResponse, RouteScheduleResponse } from '~/types/api.types'

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

const routes = ref<RouteResponse[]>([])
const vehicles = ref<VehicleResponse[]>([])
const employees = ref<EmployeeResponse[]>([])

const selectedRouteId = ref<string>('')
const selectedEmployeeId = ref<string>('')
const selectedVehicleId = ref<string>('')

const form = ref({
  shift: 0, // 0: Morning, 1: Afternoon, 2: Night
  startTime: '07:00',
  endTime: '08:30',
  days: [1, 2, 3, 4, 5], // Mon-Fri
  effectiveFrom: new Date().toISOString().split('T')[0],
  defaultAmount: 180,
  defaultCurrency: 'PEN'
})

const daysOfWeek = [
  { id: 1, label: 'Lun' },
  { id: 2, label: 'Mar' },
  { id: 3, label: 'Mié' },
  { id: 4, label: 'Jue' },
  { id: 5, label: 'Vie' },
  { id: 6, label: 'Sáb' },
  { id: 0, label: 'Dom' }
]

function toggleDay(dayId: number) {
  const index = form.value.days.indexOf(dayId)
  if (index > -1) {
    form.value.days.splice(index, 1)
  } else {
    form.value.days.push(dayId)
  }
}

async function loadCatalogs() {
  try {
    const [r, v, e] = await Promise.allSettled([
      api.get<RouteResponse[]>('/v1/routes'),
      api.get<VehicleResponse[]>('/v1/vehicles'),
      api.get<EmployeeResponse[]>('/v1/employees')
    ])
    if (r.status === 'fulfilled' && r.value) {
      routes.value = r.value
      if (!selectedRouteId.value && r.value.length > 0) {
        selectedRouteId.value = r.value[0].id
      }
    }
    if (v.status === 'fulfilled' && v.value) vehicles.value = v.value
    if (e.status === 'fulfilled' && e.value) employees.value = e.value
  } catch {
    // Handled
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    form.value = {
      shift: 0,
      startTime: '07:00',
      endTime: '08:30',
      days: [1, 2, 3, 4, 5],
      effectiveFrom: new Date().toISOString().split('T')[0],
      defaultAmount: 180,
      defaultCurrency: 'PEN'
    }
    selectedEmployeeId.value = ''
    selectedVehicleId.value = ''
    loadCatalogs()
  }
})

async function handleSubmit() {
  if (!selectedRouteId.value) {
    toasts.warning('Debes seleccionar una ruta maestra.')
    return
  }
  if (form.value.days.length === 0) {
    toasts.warning('Selecciona al menos un día de la semana para la programación.')
    return
  }

  isSubmitting.value = true
  try {
    const formatTime = (t: string) => t.length === 5 ? `${t}:00` : t

    const payload = {
      routeId: selectedRouteId.value,
      shift: Number(form.value.shift),
      startTime: formatTime(form.value.startTime),
      endTime: form.value.endTime ? formatTime(form.value.endTime) : null,
      days: form.value.days,
      effectiveFrom: form.value.effectiveFrom,
      defaultAmount: form.value.defaultAmount ? Number(form.value.defaultAmount) : null,
      defaultCurrency: form.value.defaultCurrency || 'PEN'
    }

    const created = await api.post<RouteScheduleResponse>('/v1/route-schedules', payload)

    // Assign driver/vehicle if selected
    if (created && created.id && selectedEmployeeId.value) {
      try {
        await api.post(`/v1/route-schedules/${created.id}/assignments`, {
          employeeId: selectedEmployeeId.value,
          vehicleId: selectedVehicleId.value || null,
          validFrom: form.value.effectiveFrom
        })
      } catch {
        toasts.warning('Programación creada; detalle al asignar recursos.')
      }
    }

    toasts.success('Programación de ruta registrada exitosamente.')
    emit('created')
    emit('close')
  } catch {
    // Handled by useApi
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Programar Despacho Recurrente" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div class="space-y-1">
        <label class="block text-xs font-semibold text-slate-300">Ruta Maestra</label>
        <select
          v-model="selectedRouteId"
          class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          required
        >
          <option value="" disabled>Seleccione una ruta...</option>
          <option v-for="r in routes" :key="r.id" :value="r.id">
            {{ r.routeCode }} - {{ r.name }}
          </option>
        </select>
        <p v-if="routes.length === 0" class="text-[11px] text-amber-400 mt-1">
          Aún no tienes rutas registradas. Por favor crea primero una ruta en "Rutas Maestras".
        </p>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Turno</label>
          <select
            v-model.number="form.shift"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option :value="0">Mañana (Matutino)</option>
            <option :value="1">Tarde (Vespertino)</option>
            <option :value="2">Noche (Nocturno)</option>
          </select>
        </div>

        <BaseInput
          v-model="form.startTime"
          type="time"
          label="Hora de Salida"
          required
        />

        <BaseInput
          v-model="form.endTime"
          type="time"
          label="Hora Estimada Llegada"
        />
      </div>

      <!-- Days of Week Selector -->
      <div class="space-y-1.5">
        <label class="block text-xs font-semibold text-slate-300">Días de Frecuencia Semanal</label>
        <div class="flex items-center gap-1.5">
          <button
            v-for="d in daysOfWeek"
            :key="d.id"
            type="button"
            class="flex-1 py-1.5 rounded-lg text-xs font-bold transition-all"
            :class="form.days.includes(d.id)
              ? 'bg-brand-500 text-white shadow-md shadow-brand-500/20'
              : 'bg-slate-900 text-slate-400 border border-slate-800 hover:text-white'"
            @click="toggleDay(d.id)"
          >
            {{ d.label }}
          </button>
        </div>
      </div>

      <!-- Resource Assignments (Driver & Vehicle) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Chofer Asignado al Turno</label>
          <select
            v-model="selectedEmployeeId"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="">Sin chofer asignado</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">
              {{ e.fullName || `${e.firstName} ${e.lastName}` }} ({{ e.employeeCode }})
            </option>
          </select>
        </div>

        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Vehículo Asignado</label>
          <select
            v-model="selectedVehicleId"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="">Sin vehículo asignado</option>
            <option v-for="v in vehicles" :key="v.id" :value="v.id">
              {{ v.licensePlate }} ({{ v.make }} {{ v.model }})
            </option>
          </select>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.effectiveFrom"
          type="date"
          label="Vigente Desde"
          required
        />

        <BaseInput
          v-model.number="form.defaultAmount"
          type="number"
          step="0.01"
          label="Tarifa por Servicio (PEN)"
          placeholder="180.00"
        />
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Guardar Programación
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
