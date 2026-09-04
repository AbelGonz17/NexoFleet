<script setup lang="ts">
import {
  Building2,
  Hash,
  MapPin,
  Phone,
  Mail,
  Calendar,
  Clock,
  Check,
  Copy,
  Edit2,
  ShieldAlert,
  ShieldCheck,
  UserPlus,
  Users,
  User
} from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { CompanyResponse, CompanyAdminUser } from '~/types/api.types'

const props = defineProps<{
  modelValue: boolean
  company: CompanyResponse | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'edit', company: CompanyResponse): void
  (e: 'toggle-status', company: CompanyResponse): void
  (e: 'create-admin', company: CompanyResponse): void
}>()

const api = useApi()
const copied = ref(false)
const admins = ref<CompanyAdminUser[]>([])
const loadingAdmins = ref(false)

async function fetchAdmins() {
  if (!props.company) return
  loadingAdmins.value = true
  try {
    const list = await api.get<CompanyAdminUser[]>(`/v1/companies/${props.company.id}/admins`)
    admins.value = list || []
  } catch {
    admins.value = []
  } finally {
    loadingAdmins.value = false
  }
}

watch(
  () => props.modelValue,
  (val) => {
    if (val && props.company) {
      fetchAdmins()
    }
  }
)

watch(
  () => props.company,
  (val) => {
    if (val && props.modelValue) {
      fetchAdmins()
    }
  }
)

function copyId() {
  if (!props.company) return
  if (navigator?.clipboard) {
    navigator.clipboard.writeText(props.company.id)
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 2000)
  }
}

function formatDate(isoDate?: string | null) {
  if (!isoDate) return 'Sin registro'
  try {
    const d = new Date(isoDate)
    return new Intl.DateTimeFormat('es-VE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(d)
  } catch {
    return isoDate
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    title="Detalles de la Empresa"
    description="Información corporativa, administradores y metadatos del sistema."
    max-width="lg"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div v-if="company" class="space-y-4">
      <!-- Header summary card -->
      <div class="p-4 rounded-xl bg-slate-950/60 border border-slate-800 flex items-center justify-between">
        <div class="flex items-center gap-3.5">
          <div class="w-12 h-12 rounded-xl bg-brand-500/10 border border-brand-500/20 text-brand-400 flex items-center justify-center font-black text-lg">
            {{ company.name.charAt(0).toUpperCase() }}
          </div>
          <div>
            <h4 class="text-base font-bold text-white">{{ company.name }}</h4>
            <div class="flex items-center gap-2 mt-1">
              <span class="text-xs font-mono text-slate-400">{{ company.taxIdentification }}</span>
              <span class="text-slate-600">•</span>
              <BaseBadge
                :variant="company.status === 'Active' ? 'success' : 'danger'"
                size="sm"
              >
                {{ company.status === 'Active' ? 'Activa' : 'Deshabilitada' }}
              </BaseBadge>
            </div>
          </div>
        </div>
      </div>

      <!-- Grid of details -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
        <!-- UUID -->
        <div class="sm:col-span-2 p-3 rounded-xl bg-slate-800/40 border border-slate-800/80">
          <p class="text-[11px] text-slate-400 font-medium mb-1">Identificador Único (UUID)</p>
          <div class="flex items-center justify-between font-mono text-slate-200">
            <span class="truncate text-[11px]">{{ company.id }}</span>
            <button
              type="button"
              class="p-1 rounded hover:bg-slate-700 text-slate-400 hover:text-white transition-colors ml-2 shrink-0"
              title="Copiar ID"
              @click="copyId"
            >
              <Check v-if="copied" class="w-3.5 h-3.5 text-emerald-400" />
              <Copy v-else class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <!-- Contact Email -->
        <div class="p-3 rounded-xl bg-slate-800/40 border border-slate-800/80">
          <div class="flex items-center gap-1.5 text-slate-400 font-medium mb-1">
            <Mail class="w-3.5 h-3.5 text-brand-400" />
            <span>Correo Corporativo</span>
          </div>
          <p class="text-white font-medium truncate">{{ company.email }}</p>
        </div>

        <!-- Contact Phone -->
        <div class="p-3 rounded-xl bg-slate-800/40 border border-slate-800/80">
          <div class="flex items-center gap-1.5 text-slate-400 font-medium mb-1">
            <Phone class="w-3.5 h-3.5 text-emerald-400" />
            <span>Teléfono de Contacto</span>
          </div>
          <p class="text-white font-medium">{{ company.phone }}</p>
        </div>

        <!-- Location -->
        <div class="p-3 rounded-xl bg-slate-800/40 border border-slate-800/80">
          <div class="flex items-center gap-1.5 text-slate-400 font-medium mb-1">
            <MapPin class="w-3.5 h-3.5 text-amber-400" />
            <span>Ubicación</span>
          </div>
          <p class="text-white font-medium">{{ company.city }}, {{ company.country }}</p>
        </div>

        <!-- Created At -->
        <div class="p-3 rounded-xl bg-slate-800/40 border border-slate-800/80">
          <div class="flex items-center gap-1.5 text-slate-400 font-medium mb-1">
            <Calendar class="w-3.5 h-3.5 text-indigo-400" />
            <span>Fecha de Registro</span>
          </div>
          <p class="text-slate-200">{{ formatDate(company.createdAtUtc) }}</p>
        </div>
      </div>

      <!-- Administrators Section -->
      <div class="p-3.5 rounded-xl bg-slate-950/60 border border-slate-800 space-y-2.5">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <Users class="w-4 h-4 text-brand-400" />
            <h5 class="text-xs font-bold text-white">Administradores de la Empresa</h5>
          </div>

          <BaseButton
            variant="outline"
            size="sm"
            @click="emit('create-admin', company)"
          >
            <UserPlus class="w-3.5 h-3.5 mr-1" />
            + Crear Admin
          </BaseButton>
        </div>

        <div v-if="loadingAdmins" class="text-center py-3 text-slate-500 text-xs">
          Cargando administradores...
        </div>

        <div v-else-if="admins.length === 0" class="text-center py-3 text-slate-500 text-xs">
          Esta empresa aún no tiene un usuario administrador registrado.
        </div>

        <div v-else class="space-y-1.5">
          <div
            v-for="admin in admins"
            :key="admin.id"
            class="p-2.5 rounded-lg bg-slate-900 border border-slate-800 flex items-center justify-between text-xs"
          >
            <div class="flex items-center gap-2.5">
              <div class="w-7 h-7 rounded-full bg-brand-500/10 text-brand-400 flex items-center justify-center font-bold text-[11px]">
                {{ admin.firstName ? admin.firstName.charAt(0).toUpperCase() : 'A' }}
              </div>
              <div>
                <p class="font-bold text-white">{{ admin.firstName }} {{ admin.lastName }}</p>
                <p class="text-[11px] text-slate-400">{{ admin.email }}</p>
              </div>
            </div>

            <BaseBadge variant="success" size="sm">
              Administrator
            </BaseBadge>
          </div>
        </div>
      </div>
    </div>

    <template #footer>
      <div v-if="company" class="flex items-center justify-between w-full">
        <BaseButton
          :variant="company.status === 'Active' ? 'danger' : 'outline'"
          size="sm"
          @click="emit('toggle-status', company)"
        >
          <ShieldAlert v-if="company.status === 'Active'" class="w-4 h-4 mr-1.5" />
          <ShieldCheck v-else class="w-4 h-4 mr-1.5" />
          {{ company.status === 'Active' ? 'Deshabilitar' : 'Habilitar' }}
        </BaseButton>

        <div class="flex items-center gap-2">
          <BaseButton
            variant="secondary"
            size="sm"
            @click="emit('update:modelValue', false)"
          >
            Cerrar
          </BaseButton>

          <BaseButton
            variant="primary"
            size="sm"
            @click="emit('edit', company)"
          >
            <Edit2 class="w-3.5 h-3.5 mr-1.5" />
            Editar
          </BaseButton>
        </div>
      </div>
    </template>
  </BaseModal>
</template>
