<script setup lang="ts">
import {
  FileCheck2,
  Search,
  Filter,
  ShieldAlert,
  ShieldCheck,
  Building2,
  User,
  AlertTriangle,
  Info,
  Eye,
  RefreshCw,
  Clock,
  Laptop
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseModal from '~/components/common/BaseModal.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Registro de Auditoría Global'
})

const api = useApi()

interface AuditLogResponse {
  id: string
  action: string
  entityType: string
  entityId: string
  severity: string
  actorEmail: string
  actorRole: string
  ipAddress: string
  data: string
  occurredAtUtc: string
}

interface AuditStatsResponse {
  totalEvents: number
  companyOperations: number
  securityEvents: number
  alerts: number
}

// Variables reactivas
const logs = ref<AuditLogResponse[]>([])
const stats = ref<AuditStatsResponse>({
  totalEvents: 0,
  companyOperations: 0,
  securityEvents: 0,
  alerts: 0
})
const loading = ref(true)
const loadingStats = ref(true)
const searchQuery = ref('')
const selectedEntityType = ref('ALL')
const selectedSeverity = ref('ALL')

// Variables para el modal
const selectedLog = ref<AuditLogResponse | null>(null)
const isDetailOpen = ref(false)

// Funciones para UI format
function getSeverityBadge(severityStr: string): 'info' | 'warning' | 'danger' | 'success' {
  const s = severityStr.toLowerCase()
  if (s === 'success') return 'success'
  if (s === 'warning') return 'warning'
  if (s === 'critical' || s === 'danger') return 'danger'
  return 'info'
}

function getSeverityBadgeLabel(severityStr: string): string {
  const s = severityStr.toLowerCase()
  if (s === 'success') return 'Éxito'
  if (s === 'warning') return 'Aviso'
  if (s === 'critical' || s === 'danger') return 'Crítico'
  return 'Info'
}

function formatActionLabel(action: string): string {
  const mapping: Record<string, string> = {
    'COMPANY_REGISTERED': 'Registro de Empresa',
    'ADMIN_PROVISIONED': 'Creación de Administrador',
    'SECURITY_LOGIN_FAILED': 'Fallo de Autenticación',
    'COMPANY_SUSPENDED': 'Suspensión de Empresa',
    'SYSTEM_BACKUP_COMPLETED': 'Copia de Seguridad del Sistema',
    'COMPANY_UPDATED': 'Actualización de Perfil'
  }
  return mapping[action] || action
}

function parseData(jsonStr?: string): any {
  if (!jsonStr) return {}
  try {
    return JSON.parse(jsonStr)
  } catch {
    return { raw: jsonStr }
  }
}

function formatUtcTime(utcDate: string): string {
  try {
    const d = new Date(utcDate)
    return d.toLocaleString('es-PE', { hour12: false })
  } catch {
    return utcDate
  }
}

// Lógica de datos
async function fetchLogs() {
  loading.value = true
  try {
    const query = new URLSearchParams()
    if (searchQuery.value) query.append('search', searchQuery.value)
    if (selectedEntityType.value !== 'ALL') query.append('entityType', selectedEntityType.value)
    
    // Mapear UI severity al backend severity
    let backendSeverity = selectedSeverity.value
    if (backendSeverity === 'danger') backendSeverity = 'Critical'
    else if (backendSeverity !== 'ALL') backendSeverity = backendSeverity.charAt(0).toUpperCase() + backendSeverity.slice(1)
    
    if (backendSeverity !== 'ALL') query.append('severity', backendSeverity)

    const res = await api.get<AuditLogResponse[]>(`/v1/audit-logs?${query.toString()}`)
    logs.value = res || []
  } catch (e) {
    console.error(e)
    logs.value = []
  } finally {
    loading.value = false
  }
}

async function fetchStats() {
  loadingStats.value = true
  try {
    const res = await api.get<AuditStatsResponse>('/v1/audit-logs/stats')
    if (res) stats.value = res
  } catch (e) {
    console.error(e)
  } finally {
    loadingStats.value = false
  }
}

function viewLogDetails(log: AuditLogResponse) {
  selectedLog.value = log
  isDetailOpen.value = true
}

// Watchers y lifecycle
let searchTimeout: any
watch(searchQuery, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    fetchLogs()
  }, 500)
})

watch([selectedEntityType, selectedSeverity], () => {
  fetchLogs()
})

onMounted(() => {
  fetchLogs()
  fetchStats()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-purple-500/20 text-purple-300 px-2.5 py-0.5 rounded-lg border border-purple-500/30">
            <FileCheck2 class="w-3.5 h-3.5" />
            Trazabilidad Inmutable
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Registro de Auditoría Global</h2>
        <p class="text-xs text-slate-400 mt-1">
          Registro cronológico de operaciones críticas, mutaciones de datos, accesos y seguridad del sistema.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchLogs">
          <RefreshCw class="w-4 h-4" />
          <span>Actualizar</span>
        </BaseButton>
      </div>
    </div>

    <!-- Quick KPIs -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Total Eventos Registrados</span>
          <FileCheck2 class="w-4 h-4 text-purple-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          <span v-if="loadingStats" class="text-slate-600">--</span>
          <span v-else>{{ stats.totalEvents }}</span>
        </p>
        <p class="text-[11px] text-purple-300/80 mt-0.5">Historial íntegro</p>
      </div>

      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Operaciones de Empresa</span>
          <Building2 class="w-4 h-4 text-brand-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          <span v-if="loadingStats" class="text-slate-600">--</span>
          <span v-else>{{ stats.companyOperations }}</span>
        </p>
        <p class="text-[11px] text-slate-400 mt-0.5">Altas, bajas y perfiles</p>
      </div>

      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Eventos de Seguridad</span>
          <ShieldCheck class="w-4 h-4 text-emerald-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          <span v-if="loadingStats" class="text-slate-600">--</span>
          <span v-else>{{ stats.securityEvents }}</span>
        </p>
        <p class="text-[11px] text-emerald-400/80 mt-0.5">Accesos y validaciones</p>
      </div>

      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Alertas y Advertencias</span>
          <AlertTriangle class="w-4 h-4 text-amber-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          <span v-if="loadingStats" class="text-slate-600">--</span>
          <span v-else>{{ stats.alerts }}</span>
        </p>
        <p class="text-[11px] text-amber-300/80 mt-0.5">Eventos a monitorear</p>
      </div>
    </div>

    <!-- Filter & Search Bar -->
    <div class="flex flex-col sm:flex-row gap-4 justify-between items-stretch sm:items-center bg-slate-900/60 p-4 rounded-2xl border border-slate-800">
      <div class="relative flex-1">
        <Search class="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por acción, usuario, IP o detalle..."
          class="w-full bg-slate-950/80 border border-slate-800 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-purple-500 focus:ring-1 focus:ring-purple-500"
        />
      </div>

      <div class="flex items-center gap-3">
        <!-- Entity Filter -->
        <select
          v-model="selectedEntityType"
          class="bg-slate-950/80 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-200 focus:outline-none focus:border-purple-500"
        >
          <option value="ALL">Todas las Entidades</option>
          <option value="Company">Empresas</option>
          <option value="User">Usuarios</option>
          <option value="Security">Seguridad</option>
          <option value="System">Sistema</option>
        </select>

        <!-- Severity Filter -->
        <select
          v-model="selectedSeverity"
          class="bg-slate-950/80 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-200 focus:outline-none focus:border-purple-500"
        >
          <option value="ALL">Todas las Severidades</option>
          <option value="info">Información</option>
          <option value="success">Éxito</option>
          <option value="warning">Advertencia</option>
          <option value="danger">Crítico / Peligro</option>
        </select>
      </div>
    </div>

    <!-- Audit Log Table -->
    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/60 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Fecha y Hora</th>
              <th class="px-6 py-3.5">Severidad</th>
              <th class="px-6 py-3.5">Acción</th>
              <th class="px-6 py-3.5">Entidad</th>
              <th class="px-6 py-3.5">Actor</th>
              <th class="px-6 py-3.5">IP Origen</th>
              <th class="px-6 py-3.5 text-right">Detalles</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="7" class="py-12 text-slate-500 font-medium">Cargando registros de auditoría...</td>
            </tr>
            <tr v-else-if="logs.length === 0" class="text-center py-12">
              <td colspan="7" class="py-12 text-slate-500 font-medium">No se encontraron eventos coincidentes.</td>
            </tr>
            <tr
              v-for="l in logs"
              :key="l.id"
              class="hover:bg-slate-800/30 transition-colors group cursor-pointer"
              @click="viewLogDetails(l)"
            >
              <td class="px-6 py-4 text-slate-300 font-mono text-[11px] whitespace-nowrap">
                <div class="flex items-center gap-1.5">
                  <Clock class="w-3.5 h-3.5 text-slate-500" />
                  <span>{{ formatUtcTime(l.occurredAtUtc) }}</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <BaseBadge
                  :variant="getSeverityBadge(l.severity)"
                  size="sm"
                  :dot="l.severity === 'Success' || l.severity === 'Warning' || l.severity === 'Critical'"
                >
                  {{ getSeverityBadgeLabel(l.severity) }}
                </BaseBadge>
              </td>
              <td class="px-6 py-4">
                <div class="font-bold text-white">{{ formatActionLabel(l.action) }}</div>
                <div class="font-mono text-[10px] text-purple-400">{{ l.action }}</div>
              </td>
              <td class="px-6 py-4">
                <span class="px-2 py-0.5 rounded-md bg-slate-800 border border-slate-700 text-slate-300 font-semibold text-[11px]">
                  {{ l.entityType }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="font-medium text-slate-200">{{ l.actorEmail }}</div>
                <div class="text-[10px] text-slate-500">{{ l.actorRole }}</div>
              </td>
              <td class="px-6 py-4 text-slate-400 font-mono text-[11px]">
                {{ l.ipAddress }}
              </td>
              <td class="px-6 py-4 text-right">
                <button
                  type="button"
                  class="p-1.5 rounded-lg text-slate-400 group-hover:text-white group-hover:bg-slate-800 transition-colors"
                  title="Ver metadata"
                >
                  <Eye class="w-4 h-4" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Detail & Payload Inspector Modal -->
    <BaseModal
      :is-open="isDetailOpen"
      :title="'Detalle de Evento: ' + (selectedLog ? formatActionLabel(selectedLog.action) : '')"
      @close="isDetailOpen = false"
    >
      <div v-if="selectedLog" class="space-y-4 text-xs">
        <div class="p-3 rounded-xl bg-slate-950/80 border border-slate-800 space-y-2">
          <div class="flex items-center justify-between">
            <span class="text-slate-400">Código de Acción:</span>
            <span class="font-mono font-bold text-purple-400">{{ selectedLog.action }}</span>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-slate-400">Fecha y Hora UTC:</span>
            <span class="font-mono text-slate-200">{{ selectedLog.occurredAtUtc }}</span>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-slate-400">Usuario Responsable:</span>
            <span class="text-slate-200 font-semibold">{{ selectedLog.actorEmail }} ({{ selectedLog.actorRole }})</span>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-slate-400">IP y Ubicación:</span>
            <span class="font-mono text-slate-300">{{ selectedLog.ipAddress }}</span>
          </div>
        </div>

        <div v-if="selectedLog.data">
          <label class="block text-xs font-semibold text-slate-400 mb-1">Payload / Metadatos del Cambio (JSON)</label>
          <pre class="p-3 rounded-xl bg-slate-950 border border-slate-800 font-mono text-[11px] text-purple-300 overflow-x-auto">{{ JSON.stringify(parseData(selectedLog.data), null, 2) }}</pre>
        </div>

        <div class="pt-2 flex justify-end">
          <BaseButton variant="secondary" size="md" @click="isDetailOpen = false">
            Cerrar
          </BaseButton>
        </div>
      </div>
    </BaseModal>
  </div>
</template>
