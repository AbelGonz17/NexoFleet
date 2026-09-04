<script setup lang="ts">
import {
  Users,
  Plus,
  Search,
  RefreshCw,
  UserCheck,
  CreditCard,
  Phone,
  Mail,
  CheckCircle2,
  AlertTriangle,
  Inbox,
  Eye,
  Edit2,
  Power,
  ShieldAlert
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import CreateEmployeeModal from '~/components/employees/CreateEmployeeModal.vue'
import EditEmployeeModal from '~/components/employees/EditEmployeeModal.vue'
import EmployeeDetailsModal from '~/components/employees/EmployeeDetailsModal.vue'
import type { EmployeeResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Personal y Conductores'
})

const api = useApi()
const toasts = useToasts()
const employees = ref<EmployeeResponse[]>([])
const loading = ref(true)
const actionLoading = ref(false)
const searchQuery = ref('')
const selectedStatus = ref('ALL')

// Modal states
const isCreateModalOpen = ref(false)
const isEditModalOpen = ref(false)
const isDetailsModalOpen = ref(false)
const selectedEmployee = ref<EmployeeResponse | null>(null)

async function fetchEmployees() {
  loading.value = true
  try {
    const res = await api.get<EmployeeResponse[]>('/v1/employees')
    employees.value = res || []
  } catch {
    employees.value = []
  } finally {
    loading.value = false
  }
}

function getFullName(emp: EmployeeResponse): string {
  if (typeof emp.fullName === 'string' && emp.fullName) return emp.fullName
  if (emp.firstName && emp.lastName) return `${emp.firstName} ${emp.lastName}`
  if (typeof emp.fullName === 'object' && emp.fullName) {
    return `${(emp.fullName as any).firstName || ''} ${(emp.fullName as any).lastName || ''}`.trim()
  }
  return emp.employeeCode || 'Empleado'
}

const filteredEmployees = computed(() => {
  return employees.value.filter(e => {
    const name = getFullName(e)
    const matchSearch =
      searchQuery.value.trim() === '' ||
      e.employeeCode.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      e.identityDocument.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      e.email.toLowerCase().includes(searchQuery.value.toLowerCase())

    const matchStatus =
      selectedStatus.value === 'ALL' || e.status === selectedStatus.value

    return matchSearch && matchStatus
  })
})

const kpiTotal = computed(() => employees.value.length)
const kpiActive = computed(() => employees.value.filter(e => e.status === 'Active').length)
const kpiLinkedUsers = computed(() => employees.value.filter(e => !!e.userId).length)

// Modal open helpers
function openDetails(emp: EmployeeResponse) {
  selectedEmployee.value = emp
  isDetailsModalOpen.value = true
}

function openEdit(emp: EmployeeResponse) {
  selectedEmployee.value = emp
  isDetailsModalOpen.value = false
  isEditModalOpen.value = true
}

async function toggleEmployeeStatus(emp: EmployeeResponse) {
  actionLoading.value = true
  try {
    if (emp.status === 'Active') {
      await api.post(`/v1/employees/${emp.id}/suspend`, {})
      toasts.info('Empleado suspendido', `Se ha suspendido temporalmente a ${getFullName(emp)}.`)
    } else {
      await api.post(`/v1/employees/${emp.id}/activate`, {})
      toasts.success('Empleado activado', `Se ha reactivado a ${getFullName(emp)}.`)
    }
    isDetailsModalOpen.value = false
    await fetchEmployees()
  } catch {
    // Handled by useApi
  } finally {
    actionLoading.value = false
  }
}

onMounted(() => {
  fetchEmployees()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Users class="w-3.5 h-3.5" />
            Recursos Humanos
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Personal y Conductores</h2>
        <p class="text-xs text-slate-400 mt-1">Directorio de choferes, personal de operaciones y vinculación de cuentas.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchEmployees">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>

        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Registrar Conductor / Personal</span>
        </BaseButton>
      </div>
    </div>

    <!-- KPIs -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Total Personal Registrado</p>
            <p class="text-2xl font-black text-white mt-1">{{ kpiTotal }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400">
            <Users class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Personal Activo / Operativo</p>
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
            <p class="text-xs text-slate-400 font-medium">Con Cuenta de Usuario Móvil</p>
            <p class="text-2xl font-black text-cyan-400 mt-1">{{ kpiLinkedUsers }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400">
            <UserCheck class="w-5 h-5" />
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
          placeholder="Buscar por nombre, código de empleado, DNI o correo..."
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
          <option value="Suspended">Suspendido</option>
          <option value="Retired">Retirado</option>
        </select>
      </div>
    </div>

    <!-- Employees Table -->
    <BaseCard class="overflow-hidden border-slate-800">
      <div v-if="loading" class="p-12 text-center">
        <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
        <p class="text-sm font-semibold text-white">Consultando nómina de personal...</p>
        <p class="text-xs text-slate-400 mt-1">Conectando con la base de datos de la empresa</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredEmployees.length === 0" class="p-16 text-center">
        <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
          <Inbox class="w-8 h-8" />
        </div>
        <h3 class="text-base font-bold text-white mb-1">No hay personal registrado</h3>
        <p class="text-xs text-slate-400 max-w-sm mx-auto mb-5">
          Registra a tus conductores y personal de operaciones para asignarles viajes y programaciones.
        </p>
        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Registrar Primer Conductor</span>
        </BaseButton>
      </div>

      <!-- Table Content -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-300">
          <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
            <tr>
              <th class="px-4 py-3.5">Código</th>
              <th class="px-4 py-3.5">Nombre y Apellidos</th>
              <th class="px-4 py-3.5">Documento Identidad</th>
              <th class="px-4 py-3.5">Contacto</th>
              <th class="px-4 py-3.5">Fecha Contratación</th>
              <th class="px-4 py-3.5">Estado</th>
              <th class="px-4 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-for="e in filteredEmployees" :key="e.id" class="hover:bg-slate-900/40 transition-colors">
              <td class="px-4 py-3.5 font-bold font-mono text-brand-400">
                {{ e.employeeCode }}
              </td>
              <td class="px-4 py-3.5 font-bold text-white">
                <div>{{ getFullName(e) }}</div>
                <div v-if="e.userId" class="inline-flex items-center gap-1 text-[10px] text-emerald-400 font-normal mt-0.5">
                  <UserCheck class="w-3 h-3" />
                  <span>Usuario App Vinculado</span>
                </div>
              </td>
              <td class="px-4 py-3.5 font-mono text-slate-300">
                {{ e.identityDocument }}
              </td>
              <td class="px-4 py-3.5 space-y-0.5">
                <div class="flex items-center gap-1.5 text-slate-200">
                  <Mail class="w-3 h-3 text-slate-400" />
                  <span>{{ e.email }}</span>
                </div>
                <div class="flex items-center gap-1.5 text-slate-400 text-[11px]">
                  <Phone class="w-3 h-3 text-slate-500" />
                  <span>{{ e.phone }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5 font-mono text-slate-300 text-[11px]">
                {{ e.hireDate || e.hiredOn }}
              </td>
              <td class="px-4 py-3.5">
                <BaseBadge v-if="e.status === 'Active'" variant="success">Activo</BaseBadge>
                <BaseBadge v-else-if="e.status === 'Suspended'" variant="warning">Suspendido</BaseBadge>
                <BaseBadge v-else variant="danger">{{ e.status }}</BaseBadge>
              </td>
              <td class="px-4 py-3.5 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <!-- View Details Button -->
                  <button
                    class="p-1.5 bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white rounded-lg transition-colors"
                    title="Ver Detalles"
                    @click="openDetails(e)"
                  >
                    <Eye class="w-3.5 h-3.5" />
                  </button>

                  <!-- Edit Button -->
                  <button
                    class="p-1.5 bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white rounded-lg transition-colors"
                    title="Editar Empleado"
                    @click="openEdit(e)"
                  >
                    <Edit2 class="w-3.5 h-3.5" />
                  </button>

                  <!-- Suspend / Activate Toggle -->
                  <button
                    class="p-1.5 rounded-lg transition-colors"
                    :class="e.status === 'Active'
                      ? 'bg-rose-500/10 hover:bg-rose-500/20 text-rose-400'
                      : 'bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400'"
                    :title="e.status === 'Active' ? 'Suspender Empleado' : 'Reactivar Empleado'"
                    :disabled="actionLoading"
                    @click="toggleEmployeeStatus(e)"
                  >
                    <Power class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Modals -->
    <CreateEmployeeModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="fetchEmployees"
    />

    <EditEmployeeModal
      :is-open="isEditModalOpen"
      :employee="selectedEmployee"
      @close="isEditModalOpen = false"
      @updated="fetchEmployees"
    />

    <EmployeeDetailsModal
      :is-open="isDetailsModalOpen"
      :employee="selectedEmployee"
      @close="isDetailsModalOpen = false"
      @edit="openEdit(selectedEmployee!)"
      @toggle-status="toggleEmployeeStatus(selectedEmployee!)"
    />
  </div>
</template>
