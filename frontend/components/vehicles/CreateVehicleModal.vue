<script setup lang="ts">
import { Truck, ShieldCheck, Calendar, User } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { RegisterCompanyVehicleRequest } from '~/types/api.types'

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

const form = ref<RegisterCompanyVehicleRequest>({
  licensePlate: '',
  make: '',
  model: '',
  manufactureYear: new Date().getFullYear(),
  color: 'Blanco',
  type: 1, // 1: Van, 2: Minibus, 3: Bus, 0: Car, 4: Truck
  passengerCapacity: 15
})

function resetForm() {
  form.value = {
    licensePlate: '',
    make: '',
    model: '',
    manufactureYear: new Date().getFullYear(),
    color: 'Blanco',
    type: 1,
    passengerCapacity: 15
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) resetForm()
})

async function handleSubmit() {
  if (!form.value.licensePlate.trim()) {
    toasts.warning('La placa vehicular es requerida.')
    return
  }
  if (!form.value.make.trim() || !form.value.model.trim()) {
    toasts.warning('Marca y modelo son obligatorios.')
    return
  }

  isSubmitting.value = true
  try {
    await api.post('/v1/vehicles/company', {
      licensePlate: form.value.licensePlate.trim().toUpperCase(),
      make: form.value.make.trim(),
      model: form.value.model.trim(),
      manufactureYear: Number(form.value.manufactureYear),
      color: form.value.color?.trim() || null,
      type: Number(form.value.type),
      passengerCapacity: Number(form.value.passengerCapacity) || null
    })
    toasts.success(`Vehículo ${form.value.licensePlate.toUpperCase()} registrado exitosamente en la flota.`)
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
  <BaseModal :is-open="isOpen" title="Registrar Nuevo Vehículo en Flota" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.licensePlate"
          label="Placa de Rodaje"
          placeholder="ej. ABC-123"
          required
        >
          <template #prefix>
            <Truck class="w-4 h-4 text-brand-400" />
          </template>
        </BaseInput>

        <div class="space-y-1">
          <label class="block text-xs font-semibold text-slate-300">Tipo de Vehículo</label>
          <select
            v-model.number="form.type"
            class="w-full bg-slate-900 border border-slate-700 rounded-xl px-3.5 py-2 text-xs text-white focus:outline-none focus:border-brand-500"
          >
            <option :value="1">Van / Minivan (10-15 pas.)</option>
            <option :value="2">Minibus / Cúster (16-30 pas.)</option>
            <option :value="3">Bus Interprovincial / Panorámico</option>
            <option :value="0">Sedán / Auto Ejecutivo</option>
            <option :value="4">Camión / Transporte Pesado</option>
          </select>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <BaseInput
          v-model="form.make"
          label="Marca"
          placeholder="ej. Toyota, Mercedes-Benz, Hyundai"
          required
        />
        <BaseInput
          v-model="form.model"
          label="Modelo"
          placeholder="ej. Coaster, Sprinter, H350"
          required
        />
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <BaseInput
          v-model.number="form.manufactureYear"
          type="number"
          label="Año de Fabricación"
          required
        />
        <BaseInput
          v-model="form.color"
          label="Color"
          placeholder="ej. Blanco / Plateado"
        />
        <BaseInput
          v-model.number="form.passengerCapacity"
          type="number"
          label="Capacidad Pasajeros"
          placeholder="15"
          required
        />
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
        <BaseButton type="button" variant="secondary" size="md" @click="$emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton type="submit" variant="primary" size="md" :loading="isSubmitting">
          Registrar Vehículo
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
