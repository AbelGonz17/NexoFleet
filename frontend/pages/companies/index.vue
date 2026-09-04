<script setup lang="ts">
import {
  Building2,
  Plus,
  Search,
  Filter,
  MoreVertical,
  Edit,
  Eye,
  ShieldAlert,
  ShieldCheck,
  Building,
  CheckCircle2,
  XCircle,
  RefreshCw,
  MapPin,
  Mail,
  Phone,
  UserPlus
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import CompanyModal from '~/components/companies/CompanyModal.vue'
import CompanyDetailsModal from '~/components/companies/CompanyDetailsModal.vue'
import CreateAdminModal from '~/components/companies/CreateAdminModal.vue'
import ConfirmDialog from '~/components/common/ConfirmDialog.vue'
import type { CompanyResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Mantenimiento de Empresas - NexoFleet'
})

const api = useApi()
const toasts = useToasts()

// State
const companies = ref<CompanyResponse[]>([])
const loading = ref(true)
const searchQuery = ref('')
const statusFilter = ref<'ALL' | 'Active' | 'Suspended'>('ALL')

// Modals State
const showFormModal = ref(false)
const selectedCompany = ref<CompanyResponse | null>(null)

const showDetailsModal = ref(false)
const detailsCompany = ref<CompanyResponse | null>(null)

const showCreateAdminModal = ref(false)
const adminCompany = ref<CompanyResponse | null>(null)

const showConfirmModal = ref(false)
const targetCompany = ref<CompanyResponse | null>(null)
const confirmLoading = ref(false)

// Fetch Companies
async function fetchCompanies() {
  loading.value = true
  try {
    const data = await api.get<CompanyResponse[]>('/v1/companies')
    companies.value = data || []
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

// Filtered list
const filteredCompanies = computed(() => {
  return companies.value.filter((comp) => {
    const query = searchQuery.value.toLowerCase().trim()
    const matchesSearch =
      !query ||
      comp.name.toLowerCase().includes(query) ||
      comp.taxIdentification.toLowerCase().includes(query) ||
      comp.email.toLowerCase().includes(query) ||
      comp.city.toLowerCase().includes(query)

    const matchesStatus =
      statusFilter.value === 'ALL' || comp.status === statusFilter.value

    return matchesSearch && matchesStatus
  })
})

// Metrics / KPIs
const totalCompanies = computed(() => companies.value.length)
const activeCompanies = computed(() => companies.value.filter((c) => c.status === 'Active').length)
const suspendedCompanies = computed(() => companies.value.filter((c) => c.status === 'Suspended').length)

// Actions
function openCreateModal() {
  selectedCompany.value = null
  showFormModal.value = true
}

function openEditModal(company: CompanyResponse) {
  selectedCompany.value = company
  showDetailsModal.value = false
  showFormModal.value = true
}

function openDetailsModal(company: CompanyResponse) {
  detailsCompany.value = company
  showDetailsModal.value = true
}

function openCreateAdminModal(company: CompanyResponse) {
  adminCompany.value = company
  showDetailsModal.value = false
  showCreateAdminModal.value = true
}

function promptToggleStatus(company: CompanyResponse) {
  targetCompany.value = company
  showDetailsModal.value = false
  showConfirmModal.value = true
}

async function handleConfirmStatusChange() {
  if (!targetCompany.value) return

  const comp = targetCompany.value
  const isActivating = comp.status === 'Suspended'
  const endpoint = isActivating
    ? `/v1/companies/${comp.id}/enable`
    : `/v1/companies/${comp.id}/disable`

  confirmLoading.value = true
  try {
    await api.post(endpoint)
    toasts.success(
      isActivating ? 'Empresa habilitada' : 'Empresa deshabilitada',
      `La empresa "${comp.name}" ahora está ${isActivating ? 'activa para operaciones' : 'deshabilitada / suspendida'}.`
    )
    showConfirmModal.value = false
    targetCompany.value = null
    await fetchCompanies()
  } catch {
    // Handled by useApi toast
  } finally {
    confirmLoading.value = false
  }
}

function handleSaved(savedCompany: CompanyResponse) {
  const index = companies.value.findIndex((c) => c.id === savedCompany.id)
  if (index !== -1) {
    companies.value[index] = savedCompany
  } else {
    companies.value.unshift(savedCompany)
  }
}

onMounted(() => {
  fetchCompanies()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Top Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <div class="flex items-center gap-2.5">
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 text-brand-400 flex items-center justify-center">
            <Building2 class="w-5 h-5" />
          </div>
          <div>
            <h2 class="text-xl sm:text-2xl font-black text-white tracking-tight">Mantenimiento de Empresas</h2>
            <p class="text-xs text-slate-400">Administra entidades corporativas, administradores, datos fiscales y estados operativos.</p>
          </div>
        </div>
      </div>

      <div class="flex items-center gap-2.5">
        <BaseButton
          variant="secondary"
          size="sm"
          :disabled="loading"
          @click="fetchCompanies"
        >
          <RefreshCw class="w-4 h-4 mr-1.5" :class="{ 'animate-spin': loading }" />
          Actualizar
        </BaseButton>

        <BaseButton
          variant="primary"
          size="sm"
          @click="openCreateModal"
        >
          <Plus class="w-4 h-4 mr-1.5" />
          Nueva Empresa
        </BaseButton>
      </div>
    </div>

    <!-- Stats / KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <!-- Total -->
      <BaseCard class="relative overflow-hidden">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-[11px] uppercase tracking-wider font-semibold text-slate-400">Total Empresas</p>
            <p class="text-2xl font-black text-white mt-1">{{ totalCompanies }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-slate-800/80 border border-slate-700/60 text-slate-300 flex items-center justify-center">
            <Building class="w-5 h-5" />
          </div>
        </div>
        <div class="mt-3 text-[11px] text-slate-400">
          Entidades registradas en la plataforma
        </div>
      </BaseCard>

      <!-- Activas -->
      <BaseCard class="relative overflow-hidden">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-[11px] uppercase tracking-wider font-semibold text-emerald-400">Empresas Activas</p>
            <p class="text-2xl font-black text-white mt-1">{{ activeCompanies }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 flex items-center justify-center">
            <CheckCircle2 class="w-5 h-5" />
          </div>
        </div>
        <div class="mt-3 text-[11px] text-emerald-400/80">
          Operando con acceso a rutas y despachos
        </div>
      </BaseCard>

      <!-- Deshabilitadas / Suspendidas -->
      <BaseCard class="relative overflow-hidden">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-[11px] uppercase tracking-wider font-semibold text-rose-400">Deshabilitadas / Suspendidas</p>
            <p class="text-2xl font-black text-white mt-1">{{ suspendedCompanies }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 flex items-center justify-center">
            <XCircle class="w-5 h-5" />
          </div>
        </div>
        <div class="mt-3 text-[11px] text-rose-400/80">
          Operaciones pausadas temporalmente
        </div>
      </BaseCard>
    </div>

    <!-- Filters and Table Container -->
    <BaseCard padding="none">
      <!-- Toolbar -->
      <div class="p-4 border-b border-slate-800/80 bg-slate-900/40 flex flex-col sm:flex-row items-center justify-between gap-3">
        <!-- Search bar -->
        <div class="relative w-full sm:w-80">
          <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Buscar por nombre, RIF, correo..."
            class="w-full pl-9 pr-3.5 py-2 rounded-xl bg-slate-950/60 border border-slate-700/80 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500/20 transition-all"
          />
        </div>

        <!-- Status Filter Tabs -->
        <div class="flex items-center gap-1 bg-slate-950/80 p-1 rounded-xl border border-slate-800 self-stretch sm:self-auto">
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
            :class="statusFilter === 'ALL' ? 'bg-slate-800 text-white shadow' : 'text-slate-400 hover:text-slate-200'"
            @click="statusFilter = 'ALL'"
          >
            Todas ({{ totalCompanies }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
            :class="statusFilter === 'Active' ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/30' : 'text-slate-400 hover:text-slate-200'"
            @click="statusFilter = 'Active'"
          >
            Activas ({{ activeCompanies }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
            :class="statusFilter === 'Suspended' ? 'bg-rose-500/20 text-rose-300 border border-rose-500/30' : 'text-slate-400 hover:text-slate-200'"
            @click="statusFilter = 'Suspended'"
          >
            Deshabilitadas ({{ suspendedCompanies }})
          </button>
        </div>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/60 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Empresa / Razón Social</th>
              <th class="px-6 py-3.5">RIF / Identificación</th>
              <th class="px-6 py-3.5">Contacto</th>
              <th class="px-6 py-3.5">Ubicación</th>
              <th class="px-6 py-3.5">Estado</th>
              <th class="px-6 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <!-- Loading state -->
            <tr v-if="loading">
              <td colspan="6" class="py-12 text-center">
                <div class="inline-flex items-center gap-2.5 text-slate-400 font-medium text-xs">
                  <RefreshCw class="w-4 h-4 animate-spin text-brand-400" />
                  <span>Cargando empresas registradas...</span>
                </div>
              </td>
            </tr>

            <!-- Empty state -->
            <tr v-else-if="filteredCompanies.length === 0">
              <td colspan="6" class="py-12 text-center">
                <div class="flex flex-col items-center justify-center max-w-sm mx-auto">
                  <div class="w-12 h-12 rounded-2xl bg-slate-800/60 border border-slate-700/60 text-slate-400 flex items-center justify-center mb-3">
                    <Building2 class="w-6 h-6" />
                  </div>
                  <h4 class="text-sm font-bold text-white mb-1">
                    {{ searchQuery || statusFilter !== 'ALL' ? 'No se encontraron empresas con estos filtros' : 'No hay empresas registradas' }}
                  </h4>
                  <p class="text-xs text-slate-400 text-center mb-4">
                    {{ searchQuery || statusFilter !== 'ALL' ? 'Prueba ajustando el texto de búsqueda o el filtro de estado.' : 'Comienza registrando la primera empresa en el sistema.' }}
                  </p>
                  <BaseButton
                    v-if="!searchQuery && statusFilter === 'ALL'"
                    variant="primary"
                    size="sm"
                    @click="openCreateModal"
                  >
                    <Plus class="w-4 h-4 mr-1.5" />
                    Registrar Primera Empresa
                  </BaseButton>
                </div>
              </td>
            </tr>

            <!-- Company rows -->
            <tr
              v-for="comp in filteredCompanies"
              :key="comp.id"
              class="hover:bg-slate-800/30 transition-colors group"
            >
              <!-- Name & Initial -->
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-xl bg-brand-500/10 border border-brand-500/20 text-brand-400 flex items-center justify-center font-bold text-sm shrink-0">
                    {{ comp.name.charAt(0).toUpperCase() }}
                  </div>
                  <div>
                    <span class="font-bold text-white block text-sm group-hover:text-brand-400 transition-colors">
                      {{ comp.name }}
                    </span>
                    <span class="text-[10px] text-slate-500 font-mono">
                      ID: {{ comp.id.slice(0, 8) }}...
                    </span>
                  </div>
                </div>
              </td>

              <!-- Tax ID -->
              <td class="px-6 py-4">
                <span class="px-2.5 py-1 rounded-md bg-slate-900 border border-slate-800 text-slate-300 font-mono font-medium text-xs">
                  {{ comp.taxIdentification }}
                </span>
              </td>

              <!-- Contact -->
              <td class="px-6 py-4">
                <div class="space-y-0.5">
                  <div class="flex items-center gap-1.5 text-slate-300">
                    <Mail class="w-3.5 h-3.5 text-slate-500 shrink-0" />
                    <span class="truncate max-w-[180px]">{{ comp.email }}</span>
                  </div>
                  <div class="flex items-center gap-1.5 text-slate-400 text-[11px]">
                    <Phone class="w-3.5 h-3.5 text-slate-500 shrink-0" />
                    <span>{{ comp.phone }}</span>
                  </div>
                </div>
              </td>

              <!-- Location -->
              <td class="px-6 py-4">
                <div class="flex items-center gap-1.5 text-slate-300">
                  <MapPin class="w-3.5 h-3.5 text-slate-500 shrink-0" />
                  <span>{{ comp.city }}, {{ comp.country }}</span>
                </div>
              </td>

              <!-- Status Badge -->
              <td class="px-6 py-4">
                <BaseBadge
                  :variant="comp.status === 'Active' ? 'success' : 'danger'"
                  size="sm"
                >
                  <span class="inline-block w-1.5 h-1.5 rounded-full mr-1" :class="comp.status === 'Active' ? 'bg-emerald-400' : 'bg-rose-400'"></span>
                  {{ comp.status === 'Active' ? 'Activa' : 'Deshabilitada' }}
                </BaseBadge>
              </td>

              <!-- Actions -->
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <!-- Crear Administrador -->
                  <button
                    type="button"
                    class="p-2 rounded-lg text-slate-400 hover:text-brand-400 hover:bg-brand-500/10 transition-colors"
                    title="Crear Administrador"
                    @click="openCreateAdminModal(comp)"
                  >
                    <UserPlus class="w-4 h-4" />
                  </button>

                  <!-- Ver Detalle -->
                  <button
                    type="button"
                    class="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
                    title="Ver Detalles"
                    @click="openDetailsModal(comp)"
                  >
                    <Eye class="w-4 h-4" />
                  </button>

                  <!-- Editar -->
                  <button
                    type="button"
                    class="p-2 rounded-lg text-slate-400 hover:text-brand-400 hover:bg-slate-800 transition-colors"
                    title="Editar Empresa"
                    @click="openEditModal(comp)"
                  >
                    <Edit class="w-4 h-4" />
                  </button>

                  <!-- Deshabilitar / Habilitar -->
                  <button
                    type="button"
                    class="p-2 rounded-lg transition-colors"
                    :class="comp.status === 'Active'
                      ? 'text-rose-400/80 hover:text-rose-300 hover:bg-rose-500/10'
                      : 'text-emerald-400/80 hover:text-emerald-300 hover:bg-emerald-500/10'"
                    :title="comp.status === 'Active' ? 'Deshabilitar Empresa' : 'Habilitar Empresa'"
                    @click="promptToggleStatus(comp)"
                  >
                    <ShieldAlert v-if="comp.status === 'Active'" class="w-4 h-4" />
                    <ShieldCheck v-else class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Modals -->
    <CompanyModal
      v-model="showFormModal"
      :company="selectedCompany"
      @saved="handleSaved"
    />

    <CompanyDetailsModal
      v-model="showDetailsModal"
      :company="detailsCompany"
      @edit="openEditModal"
      @toggle-status="promptToggleStatus"
      @create-admin="openCreateAdminModal"
    />

    <CreateAdminModal
      v-model="showCreateAdminModal"
      :company="adminCompany"
      @admin-created="fetchCompanies"
    />

    <ConfirmDialog
      v-model="showConfirmModal"
      :title="targetCompany?.status === 'Active' ? '¿Deshabilitar Empresa?' : '¿Habilitar Empresa?'"
      :message="targetCompany?.status === 'Active'
        ? `Al deshabilitar &quot;${targetCompany?.name}&quot;, se suspenderán temporalmente sus operaciones y asignaciones de viajes.`
        : `Al habilitar &quot;${targetCompany?.name}&quot;, la empresa reactivará sus operaciones y asignaciones con normalidad.`"
      :confirm-text="targetCompany?.status === 'Active' ? 'Sí, Deshabilitar' : 'Sí, Habilitar'"
      :variant="targetCompany?.status === 'Active' ? 'danger' : 'success'"
      :loading="confirmLoading"
      @confirm="handleConfirmStatusChange"
    />
  </div>
</template>
