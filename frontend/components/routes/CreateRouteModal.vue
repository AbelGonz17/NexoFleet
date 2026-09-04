<script setup lang="ts">
import { MapPin, Route as RouteIcon, Clock, DollarSign, FileText, Building2 } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { CreateRouteRequest, ClientResponse } from '~/types/api.types'

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
const clients = ref<ClientResponse[]>([])

const form = ref<CreateRouteRequest>({
  routeCode: '',
  name: '',
  origin: { address: '', latitude: -12.046374, longitude: -77.042793 },
  destination: { address: '', latitude: -12.091234, longitude: -77.031543 },
  clientId: null,
  instructions: 'Ruta corporativa fija con paradas autorizadas.',
  estimatedDurationMinutes: 45,
  referenceAmount: 180,
  referenceCurrency: 'PEN'
})

function resetForm() {
  form.value = {
    routeCode: `RT-${Math.floor(100 + Math.random() * 900)}`,
    name: '',
    origin: { address: '', latitude: -12.046374, longitude: -77.042793 },
    destination: { address: '', latitude: -12.091234, longitude: -77.031543 },
    clientId: null,
    instructions: 'Ruta corporativa fija con paradas autorizadas.',
    estimatedDurationMinutes: 45,
    referenceAmount: 180,
    referenceCurrency: 'PEN'
  }
}

async function loadClients() {
  try {
    const res = await api.get<ClientResponse[]>('/v1/clients')
    if (res) clients.value = res
  } catch {
    // Handled
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    resetForm()
    loadClients()
  }
})

async function handleSubmit() {
  if (!form.value.routeCode.trim() || !form.value.name.trim()) {
    toasts.warning('El código y nombre de ruta son obligatorios.')
    return
  }
  if (!form.value.origin.address.trim() || !form.value.destination.address.trim()) {
    toasts.warning('Origen y destino son requeridos.')
    return
  }

  isSubmitting.value = true
  try {
    await api.post('/v1/routes', {
      routeCode: form.value.routeCode.trim(),
      name: form.value.name.trim(),
      origin: {
        address: form.value.origin.address.trim(),
        latitude: form.value.origin.latitude || -12.046374,
        longitude: form.value.origin.longitude || -77.042793
      },
      destination: {
        address: form.value.destination.address.trim(),
        latitude: form.value.destination.latitude || -12.091234,
        longitude: form.value.destination.longitude || -77.031543
      },
      clientId: form.value.clientId || null,
      instructions: form.value.instructions?.trim() || null,
      estimatedDurationMinutes: form.value.estimatedDurationMinutes ? Number(form.value.estimatedDurationMinutes) : null,
      referenceAmount: form.value.referenceAmount ? Number(form.value.referenceAmount) : null,
      referenceCurrency: form.value.referenceCurrency || 'PEN'
    })
    toasts.success(`Ruta ${form.value.name} creada exitosamente.`)
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
  <BaseModal :is-open="isOpen" title="Crear Nueva Ruta Maestra" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.routeCode"
          label="Código de Ruta"
          placeholder="ej. RT-101"
          required
        />
        <BaseInput
          v-model="form.name"
          label="Nombre de la Ruta"
          placeholder="ej. Turno Mañana - Planta Central"
          required
        />
      </div>

      <div class="space-y-3">
        <BaseInput
          v-model="form.origin.address"
          label="Origen / Punto de Salida"
          placeholder="ej. Terminal Norte - Av. Elmer Faucett 120"
          required
        >
          <template #prefix>
            <MapPin class="w-4 h-4 text-emerald-400" />
          </template>
        </BaseInput>

        <BaseInput
          v-model="form.destination.address"
          label="Destino / Punto de Llegada"
          placeholder="ej. Sede Sur - Av. Los Rosales 450"
          required
        >
          <template #prefix>
            <MapPin class="w-4 h-4 text-rose-400" />
          </template>
        </BaseInput>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <BaseInput
          v-model.number="form.estimatedDurationMinutes"
          type="number"
          label="Duración Estimada (min)"
          placeholder="45"
        />
        <BaseInput
          v-model.number="form.referenceAmount"
          type="number"
          step="0.01"
          label="Tarifa Referencial"
          placeholder="180.00"
        />
        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Moneda</label>
          <select
            v-model="form.referenceCurrency"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option value="PEN">Soles (PEN S/.)</option>
            <option value="USD">Dólares (USD $)</option>
          </select>
        </div>
      </div>

      <div v-if="clients.length > 0" class="space-y-1">
        <label class="block text-xs font-semibold text-slate-300">Cliente Corporativo Asignado (Opcional)</label>
        <select
          v-model="form.clientId"
          class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
        >
          <option :value="null">Sin cliente específico (Ruta interna o general)</option>
          <option v-for="c in clients" :key="c.id" :value="c.id">
            {{ c.name }} ({{ c.taxIdentification || c.clientCode }})
          </option>
        </select>
      </div>

      <BaseInput
        v-model="form.instructions"
        label="Instrucciones Operativas / Paradas"
        placeholder="ej. Vía expresa, paradas intermedias en estación 2 y 4."
      />

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Crear Ruta Maestra
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
