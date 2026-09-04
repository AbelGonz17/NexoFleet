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

interface AuditEvent {
  id: string
  occurredAtUtc: string
  action: string
  actionLabel: string
  entityType: string
  entityId: string
  actorEmail: string
  actorRole: string
  ipAddress: string
  severity: 'info' | 'warning' | 'danger' | 'success'
  details: string
  metadata?: Record<string, any>
}

// Rich Mock Audit Dataset
const mockAuditLogs: AuditEvent[] = [
  {
    id: 'aud-001',
    occurredAtUtc: '2026-09-04 14:45:22',
    action: 'COMPANY_REGISTERED',
    actionLabel: 'Registro de Empresa',
    entityType: 'Company',
    entityId: 'c8f42019-3f0e-4340-9a2c-90b58e7ad100',
    actorEmail: 'admin@nexofleet.test',
    actorRole: 'SuperAdmin',
    ipAddress: '192.168.1.100 (Lima, PE)',
    severity: 'success',
    details: 'Se registró la empresa "Transportes Los Andes S.A.C." con RUC 20601234567',
    metadata: {
      companyName: 'Transportes Los Andes S.A.C.',
      taxId: '20601234567',
      email: 'contacto@losandes.pe',
      status: 'Active'
    }
  },
  {
    id: 'aud-002',
    occurredAtUtc: '2026-09-04 14:15:10',
    action: 'ADMIN_PROVISIONED',
    actionLabel: 'Creación de Administrador',
    entityType: 'User',
    entityId: 'u7b81920-55aa-43d9-9f12-00234a5ef890',
    actorEmail: 'admin@nexofleet.test',
    actorRole: 'SuperAdmin',
    ipAddress: '192.168.1.100 (Lima, PE)',
    severity: 'info',
    details: 'Se aprovisionó cuenta de Administrador para edramirez@gmail.com vinculada a Transportes Los Andes',
    metadata: {
      email: 'edramirez@gmail.com',
      assignedRole: 'Administrator',
      companyId: 'c8f42019-3f0e-4340-9a2c-90b58e7ad100'
    }
  },
  {
    id: 'aud-003',
    occurredAtUtc: '2026-09-04 12:30:45',
    action: 'SECURITY_LOGIN_FAILED',
    actionLabel: 'Fallo de Autenticación',
    entityType: 'Security',
    entityId: 'sec-9921',
    actorEmail: 'unknown_attempt@ext.com',
    actorRole: 'Anonymous',
    ipAddress: '190.234.12.88 (Arequipa, PE)',
    severity: 'danger',
    details: '3 intentos fallidos de inicio de sesión con contraseña inválida',
    metadata: {
      attemptedEmail: 'admin@nexofleet.test',
      failureReason: 'InvalidCredentials',
      blockedTemporarily: false
    }
  },
  {
    id: 'aud-004',
    occurredAtUtc: '2026-09-04 10:18:00',
    action: 'COMPANY_SUSPENDED',
    actionLabel: 'Suspensión de Empresa',
    entityType: 'Company',
    entityId: 'c1a23456-9900-47b2-8451-aa9988112233',
    actorEmail: 'admin@nexofleet.test',
    actorRole: 'SuperAdmin',
    ipAddress: '192.168.1.100 (Lima, PE)',
    severity: 'warning',
    details: 'Suspensión administrativa a "Logística & Distribución Rápida" por vencimiento de documentación',
    metadata: {
      companyName: 'Logística & Distribución Rápida',
      previousStatus: 'Active',
      newStatus: 'Suspended',
      reason: 'Póliza de seguro SOAT vencida'
    }
  },
  {
    id: 'aud-005',
    occurredAtUtc: '2026-09-04 08:00:00',
    action: 'SYSTEM_BACKUP_COMPLETED',
    actionLabel: 'Copia de Seguridad del Sistema',
    entityType: 'System',
    entityId: 'sys-bk-491',
    actorEmail: 'system-worker@nexofleet.internal',
    actorRole: 'System',
    ipAddress: '127.0.0.1 (Localhost)',
    severity: 'info',
    details: 'Snapshot automatizado de base de datos PostgreSQL 17 completado (Tamaño: 24.5 MB)',
    metadata: {
      backupType: 'FullDatabaseSnapshot',
      database: 'nexofleet',
      durationMs: 1420
    }
  },
  {
    id: 'aud-006',
    occurredAtUtc: '2026-09-03 19:40:12',
    action: 'COMPANY_UPDATED',
    actionLabel: 'Actualización de Perfil',
    entityType: 'Company',
    entityId: 'c8f42019-3f0e-4340-9a2c-90b58e7ad100',
    actorEmail: 'admin@nexofleet.test',
    actorRole: 'SuperAdmin',
    ipAddress: '192.168.1.100 (Lima, PE)',
    severity: 'info',
    details: 'Actualización de teléfono corporativo y dirección legal de la empresa',
    metadata: {
      oldPhone: '+51 987000111',
      newPhone: '+51 987654321',
      updatedField: 'Phone, Address'
    }
  }
]

const logs = ref<AuditEvent[]>([])
const loading = ref(true)
const searchQuery = ref('')
const selectedEntityType = ref('ALL')
const selectedSeverity = ref('ALL')

// Selected log for detail modal
const selectedLog = ref<AuditEvent | null>(null)
const isDetailOpen = ref(false)

async function fetchLogs() {
  loading.value = true
  try {
    const res = await api.get<any[]>('/v1/audit-logs')
    if (res && res.length > 0) {
      logs.value = res
    } else {
      logs.value = mockAuditLogs
    }
  } catch {
    logs.value = mockAuditLogs
  } finally {
    loading.value = false
  }
}

const filteredLogs = computed(() => {
  return logs.value.filter(log => {
    const matchesSearch =
      searchQuery.value.trim() === '' ||
      log.action.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      log.actionLabel.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      log.details.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      log.actorEmail.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      log.ipAddress.toLowerCase().includes(searchQuery.value.toLowerCase())

    const matchesEntity =
      selectedEntityType.value === 'ALL' || log.entityType === selectedEntityType.value

    const matchesSeverity =
      selectedSeverity.value === 'ALL' || log.severity === selectedSeverity.value

    return matchesSearch && matchesEntity && matchesSeverity
  })
})

function viewLogDetails(log: AuditEvent) {
  selectedLog.value = log
  isDetailOpen.value = true
}

onMounted(() => {
  fetchLogs()
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
        <p class="text-2xl font-black text-white mt-1">{{ logs.length }}</p>
        <p class="text-[11px] text-purple-300/80 mt-0.5">Historial íntegro</p>
      </div>

      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Operaciones de Empresa</span>
          <Building2 class="w-4 h-4 text-brand-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          {{ logs.filter(l => l.entityType === 'Company').length }}
        </p>
        <p class="text-[11px] text-slate-400 mt-0.5">Altas, bajas y perfiles</p>
      </div>

      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Eventos de Seguridad</span>
          <ShieldCheck class="w-4 h-4 text-emerald-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          {{ logs.filter(l => l.entityType === 'Security' || l.action.includes('LOGIN')).length }}
        </p>
        <p class="text-[11px] text-emerald-400/80 mt-0.5">Accesos y validaciones</p>
      </div>

      <div class="p-4 rounded-xl bg-slate-900/60 border border-slate-800 shadow-lg">
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-slate-400">Alertas y Advertencias</span>
          <AlertTriangle class="w-4 h-4 text-amber-400" />
        </div>
        <p class="text-2xl font-black text-white mt-1">
          {{ logs.filter(l => l.severity === 'warning' || l.severity === 'danger').length }}
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
            <tr v-else-if="filteredLogs.length === 0" class="text-center py-12">
              <td colspan="7" class="py-12 text-slate-500 font-medium">No se encontraron eventos coincidentes.</td>
            </tr>
            <tr
              v-for="l in filteredLogs"
              :key="l.id"
              class="hover:bg-slate-800/30 transition-colors group cursor-pointer"
              @click="viewLogDetails(l)"
            >
              <td class="px-6 py-4 text-slate-300 font-mono text-[11px] whitespace-nowrap">
                <div class="flex items-center gap-1.5">
                  <Clock class="w-3.5 h-3.5 text-slate-500" />
                  <span>{{ l.occurredAtUtc }}</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <BaseBadge
                  v-if="l.severity === 'success'"
                  variant="success"
                  size="sm"
                  dot
                >
                  Éxito
                </BaseBadge>
                <BaseBadge
                  v-else-if="l.severity === 'warning'"
                  variant="warning"
                  size="sm"
                  dot
                >
                  Aviso
                </BaseBadge>
                <BaseBadge
                  v-else-if="l.severity === 'danger'"
                  variant="danger"
                  size="sm"
                  dot
                >
                  Crítico
                </BaseBadge>
                <BaseBadge
                  v-else
                  variant="neutral"
                  size="sm"
                >
                  Info
                </BaseBadge>
              </td>
              <td class="px-6 py-4">
                <div class="font-bold text-white">{{ l.actionLabel }}</div>
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
      :title="'Detalle de Evento: ' + (selectedLog?.actionLabel || '')"
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

        <div>
          <label class="block text-xs font-semibold text-slate-400 mb-1">Descripción del Evento</label>
          <div class="p-3 rounded-xl bg-slate-950/50 border border-slate-800 text-slate-200 leading-relaxed">
            {{ selectedLog.details }}
          </div>
        </div>

        <div v-if="selectedLog.metadata">
          <label class="block text-xs font-semibold text-slate-400 mb-1">Payload / Metadatos del Cambio (JSON)</label>
          <pre class="p-3 rounded-xl bg-slate-950 border border-slate-800 font-mono text-[11px] text-purple-300 overflow-x-auto">{{ JSON.stringify(selectedLog.metadata, null, 2) }}</pre>
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

