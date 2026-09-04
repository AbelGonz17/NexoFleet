<script setup lang="ts">
import { Building2, Hash, Globe, MapPin, Phone, Mail, Save } from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseInput from '~/components/common/BaseInput.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { CompanyResponse, CreateCompanyRequest, UpdateCompanyProfileRequest } from '~/types/api.types'

const props = defineProps<{
  modelValue: boolean
  company?: CompanyResponse | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'saved', company: CompanyResponse): void
}>()

const api = useApi()
const toasts = useToasts()

const isEdit = computed(() => !!props.company)
const modalTitle = computed(() => isEdit.value ? 'Editar Empresa' : 'Nueva Empresa')
const modalDescription = computed(() =>
  isEdit.value
    ? 'Actualiza los datos corporativos, fiscales y de contacto de la empresa.'
    : 'Registra una nueva entidad empresarial en la plataforma NexoFleet.'
)

const form = reactive({
  name: '',
  taxIdentification: '',
  country: 'Venezuela',
  city: '',
  phone: '',
  email: ''
})

const errors = reactive({
  name: '',
  taxIdentification: '',
  country: '',
  city: '',
  phone: '',
  email: ''
})

const loading = ref(false)

function resetForm() {
  if (props.company) {
    form.name = props.company.name
    form.taxIdentification = props.company.taxIdentification
    form.country = props.company.country
    form.city = props.company.city
    form.phone = props.company.phone
    form.email = props.company.email
  } else {
    form.name = ''
    form.taxIdentification = ''
    form.country = 'Venezuela'
    form.city = ''
    form.phone = ''
    form.email = ''
  }

  errors.name = ''
  errors.taxIdentification = ''
  errors.country = ''
  errors.city = ''
  errors.phone = ''
  errors.email = ''
}

watch(
  () => props.modelValue,
  (open) => {
    if (open) {
      resetForm()
    }
  }
)

watch(
  () => props.company,
  () => {
    if (props.modelValue) {
      resetForm()
    }
  }
)

function validate(): boolean {
  let valid = true

  errors.name = ''
  errors.taxIdentification = ''
  errors.country = ''
  errors.city = ''
  errors.phone = ''
  errors.email = ''

  if (!form.name.trim()) {
    errors.name = 'La razón social es requerida.'
    valid = false
  }

  if (!form.taxIdentification.trim()) {
    errors.taxIdentification = 'El RIF o identificación fiscal es requerido.'
    valid = false
  }

  if (!form.country.trim()) {
    errors.country = 'El país es requerido.'
    valid = false
  }

  if (!form.city.trim()) {
    errors.city = 'La ciudad es requerida.'
    valid = false
  }

  if (!form.phone.trim()) {
    errors.phone = 'El número telefónico es requerido.'
    valid = false
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!form.email.trim()) {
    errors.email = 'El correo electrónico es requerido.'
    valid = false
  } else if (!emailRegex.test(form.email.trim())) {
    errors.email = 'Introduce un correo electrónico válido.'
    valid = false
  }

  return valid
}

async function handleSubmit() {
  if (!validate()) return

  loading.value = true
  try {
    if (isEdit.value && props.company) {
      const payload: UpdateCompanyProfileRequest = {
        name: form.name.trim(),
        taxIdentification: form.taxIdentification.trim(),
        country: form.country.trim(),
        city: form.city.trim(),
        phone: form.phone.trim(),
        email: form.email.trim()
      }

      const updated = await api.put<CompanyResponse>(`/v1/companies/${props.company.id}`, payload)
      toasts.success('Empresa actualizada', `Los datos de "${updated.name}" se han actualizado con éxito.`)
      emit('saved', updated)
      emit('update:modelValue', false)
    } else {
      const payload: CreateCompanyRequest = {
        name: form.name.trim(),
        taxIdentification: form.taxIdentification.trim(),
        country: form.country.trim(),
        city: form.city.trim(),
        phone: form.phone.trim(),
        email: form.email.trim()
      }

      const created = await api.post<CompanyResponse>('/v1/companies', payload)
      toasts.success('Empresa registrada', `La empresa "${created.name}" fue creada exitosamente.`)
      emit('saved', created)
      emit('update:modelValue', false)
    }
  } catch {
    // Errors handled with toast in useApi
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    :title="modalTitle"
    :description="modalDescription"
    max-width="lg"
    :persistent="loading"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <!-- Razón Social -->
        <div class="sm:col-span-2">
          <BaseInput
            v-model="form.name"
            label="Razón Social / Nombre de Empresa"
            placeholder="Ej. Transporte Ejecutivo Express, C.A."
            required
            :error="errors.name"
            :disabled="loading"
          />
        </div>

        <!-- Identificación Fiscal / RIF -->
        <div>
          <BaseInput
            v-model="form.taxIdentification"
            label="RIF / Identificación Fiscal"
            placeholder="Ej. J-12345678-9"
            required
            :error="errors.taxIdentification"
            :disabled="loading"
          />
        </div>

        <!-- Correo Electrónico -->
        <div>
          <BaseInput
            v-model="form.email"
            label="Correo Electrónico Corporativo"
            type="email"
            placeholder="contacto@empresa.com"
            required
            :error="errors.email"
            :disabled="loading"
          />
        </div>

        <!-- Teléfono -->
        <div>
          <BaseInput
            v-model="form.phone"
            label="Teléfono de Contacto"
            placeholder="+58 412 1234567"
            required
            :error="errors.phone"
            :disabled="loading"
          />
        </div>

        <!-- País -->
        <div>
          <BaseInput
            v-model="form.country"
            label="País"
            placeholder="Venezuela"
            required
            :error="errors.country"
            :disabled="loading"
          />
        </div>

        <!-- Ciudad -->
        <div class="sm:col-span-2">
          <BaseInput
            v-model="form.city"
            label="Ciudad / Estado"
            placeholder="Ej. Caracas, Distrito Capital"
            required
            :error="errors.city"
            :disabled="loading"
          />
        </div>
      </div>
    </form>

    <template #footer>
      <div class="flex items-center justify-end gap-3 w-full">
        <BaseButton
          variant="secondary"
          size="sm"
          :disabled="loading"
          @click="emit('update:modelValue', false)"
        >
          Cancelar
        </BaseButton>

        <BaseButton
          variant="primary"
          size="sm"
          :loading="loading"
          @click="handleSubmit"
        >
          <Save class="w-4 h-4 mr-1.5" />
          {{ isEdit ? 'Guardar Cambios' : 'Registrar Empresa' }}
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>
