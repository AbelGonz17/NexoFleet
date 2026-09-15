<script setup lang="ts">
import {
  Truck,
  Navigation,
  Building2,
  Users,
  ShieldCheck,
  Activity,
  ArrowUpRight,
  TrendingUp,
  Clock,
  DollarSign,
  Plus,
  Globe,
  AlertTriangle,
  FileCheck2,
  CheckCircle2,
  Server,
  Database,
  Cpu,
  Layers
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseButton from '~/components/common/BaseButton.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Dashboard Ejecutivo'
})

const auth = useAuth()
const permissions = usePermissions()

const iconsMap: Record<string, any> = {
  Building2, Truck, Users, Activity, Navigation, Clock, DollarSign
}

function getStatColor(iconName: string) {
  const colors: Record<string, string> = {
    Building2: 'text-purple-400 bg-purple-500/10 border-purple-500/20',
    Truck: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
    Users: 'text-brand-400 bg-brand-500/10 border-brand-500/20',
    Activity: 'text-amber-400 bg-amber-500/10 border-amber-500/20',
    Navigation: 'text-brand-400 bg-brand-500/10 border-brand-500/20',
    Clock: 'text-sky-400 bg-sky-500/10 border-sky-500/20',
    DollarSign: 'text-amber-400 bg-amber-500/10 border-amber-500/20'
  }
  return colors[iconName] || 'text-slate-400 bg-slate-500/10 border-slate-500/20'
}

// Dashboard Data Refs
const superAdminStats = ref<any[]>([])
const topCompanies = ref<any[]>([])
const liveAuditFeed = ref<any[]>([])

const companyStats = ref<any[]>([])
const fleetStatus = ref<any>(null)
const recentTrips = ref<any[]>([])

const pending = ref(true)
const api = useApi()

onMounted(async () => {
  try {
    pending.value = true
    if (permissions.isSuperAdmin.value) {
      const [statsData, companiesData, auditData] = await Promise.all([
        api.get('/v1/dashboard/superadmin/stats'),
        api.get('/v1/dashboard/superadmin/top-companies'),
        api.get('/v1/dashboard/superadmin/audit-feed')
      ])
      
      superAdminStats.value = statsData as any[] || []
      topCompanies.value = companiesData as any[] || []
      liveAuditFeed.value = auditData as any[] || []
    } else {
      const [statsData, tripsData] = await Promise.all([
        api.get('/v1/dashboard/company/stats'),
        api.get('/v1/dashboard/company/recent-trips')
      ])
      
      const stats = statsData as any
      companyStats.value = stats?.kpis || []
      fleetStatus.value = stats?.fleetStatus || null
      recentTrips.value = tripsData as any[] || []
    }
  } catch (error) {
    console.error('Error cargando datos del dashboard', error)
  } finally {
    pending.value = false
  }
})
</script>

<template>
  <div class="space-y-8">
    <!-- ========================================== -->
    <!-- SUPERADMIN VIEW                            -->
    <!-- ========================================== -->
    <template v-if="permissions.isSuperAdmin.value">
      <!-- Welcome Header SuperAdmin -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-gradient-to-r from-purple-950/40 via-slate-900/60 to-slate-900/40 border border-purple-500/20 rounded-2xl p-6 shadow-xl backdrop-blur-xl">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-purple-500/20 text-purple-300 px-2.5 py-0.5 rounded-lg border border-purple-500/30">
              <Globe class="w-3.5 h-3.5" />
              Consola Multi-Tenant Global
            </span>
            <span class="inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-400 bg-emerald-950/60 px-2 py-0.5 rounded-lg border border-emerald-500/20">
              <div class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-ping" />
              Sistema 100% Operativo
            </span>
          </div>
          <h2 class="text-2xl font-black text-white tracking-tight">
            Panel de Control Global SuperAdmin
          </h2>
          <p class="text-xs text-slate-400 mt-1">
            Bienvenido, <span class="text-white font-semibold">{{ auth.user.value?.fullName }}</span>. Monitoreo centralizado de empresas, flota global y trazabilidad del sistema.
          </p>
        </div>

        <div class="flex items-center gap-3 shrink-0">
          <NuxtLink to="/companies">
            <BaseButton variant="primary" size="md">
              <Building2 class="w-4 h-4" />
              <span>Gestionar Empresas</span>
            </BaseButton>
          </NuxtLink>
          <NuxtLink to="/audit-logs">
            <BaseButton variant="secondary" size="md">
              <FileCheck2 class="w-4 h-4" />
              <span>Auditoría</span>
            </BaseButton>
          </NuxtLink>
        </div>
      </div>

      <!-- SuperAdmin Stats Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <div
          v-for="stat in superAdminStats"
          :key="stat.name"
          class="rounded-2xl border border-slate-800 bg-slate-900/60 backdrop-blur-xl p-5 shadow-xl relative overflow-hidden group hover:border-purple-500/40 transition-all"
        >
          <div class="flex items-center justify-between">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center border" :class="getStatColor(stat.iconName)">
              <component :is="iconsMap[stat.iconName]" class="w-5 h-5" />
            </div>
            <div class="flex items-center gap-1 text-[11px] font-bold text-purple-400 bg-purple-950/60 px-2 py-0.5 rounded-lg border border-purple-500/20">
              <TrendingUp class="w-3 h-3" />
              <span>{{ stat.change }}</span>
            </div>
          </div>

          <div class="mt-4">
            <p class="text-xs font-semibold text-slate-400">{{ stat.name }}</p>
            <p class="text-2xl font-black text-white tracking-tight mt-1">{{ stat.value }}</p>
            <p class="text-[11px] text-slate-500 mt-1">{{ stat.subtitle }}</p>
          </div>
        </div>
      </div>

      <!-- SuperAdmin Content Split -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Top Companies (2 cols) -->
        <div class="lg:col-span-2 space-y-6">
          <BaseCard title="Empresas con Mayor Actividad" subtitle="Ranking de empresas por flota y volumen operacional en la red">
            <template #header>
              <NuxtLink to="/companies" class="text-xs font-semibold text-purple-400 hover:text-purple-300 flex items-center gap-1">
                <span>Ver todas</span>
                <ArrowUpRight class="w-3.5 h-3.5" />
              </NuxtLink>
            </template>

            <div class="overflow-x-auto">
              <table class="w-full text-left text-xs">
                <thead class="text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
                  <tr>
                    <th class="pb-3">Empresa</th>
                    <th class="pb-3">RUC</th>
                    <th class="pb-3 text-center">Flota Conectada</th>
                    <th class="pb-3 text-center">Viajes (Mes)</th>
                    <th class="pb-3 text-right">Estado</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-800/60">
                  <tr v-for="c in topCompanies" :key="c.id" class="hover:bg-slate-800/30 transition-colors">
                    <td class="py-3.5">
                      <div class="flex items-center gap-3">
                        <div class="w-8 h-8 rounded-xl bg-purple-500/10 border border-purple-500/20 flex items-center justify-center font-bold text-purple-400 text-xs shrink-0">
                          {{ c.name.charAt(0) }}
                        </div>
                        <div>
                          <p class="font-bold text-white">{{ c.name }}</p>
                          <p class="text-[10px] text-slate-500">{{ c.adminEmail }}</p>
                        </div>
                      </div>
                    </td>
                    <td class="py-3.5 font-mono text-slate-300">{{ c.ruc }}</td>
                    <td class="py-3.5 text-center font-bold text-slate-200">{{ c.fleetCount }} veh.</td>
                    <td class="py-3.5 text-center font-bold text-purple-300">{{ c.tripsThisMonth }}</td>
                    <td class="py-3.5 text-right">
                      <BaseBadge v-if="c.status === 'Active'" variant="success" size="sm" dot>Activa</BaseBadge>
                      <BaseBadge v-else variant="warning" size="sm" dot>Suspendida</BaseBadge>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </BaseCard>

          <!-- Infrastructure Health -->
          <BaseCard title="Salud de Infraestructura y Servicios" subtitle="Monitoreo en tiempo real de componentes del núcleo">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <div class="p-3.5 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center justify-between">
                  <Server class="w-4 h-4 text-purple-400" />
                  <span class="text-[10px] font-bold text-emerald-400 bg-emerald-950/60 px-1.5 py-0.2 rounded border border-emerald-500/20">Óptimo</span>
                </div>
                <p class="text-xs font-bold text-white mt-2">API Core .NET 10</p>
                <p class="text-[10px] text-slate-500">Latencia: 12ms</p>
              </div>

              <div class="p-3.5 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center justify-between">
                  <Database class="w-4 h-4 text-emerald-400" />
                  <span class="text-[10px] font-bold text-emerald-400 bg-emerald-950/60 px-1.5 py-0.2 rounded border border-emerald-500/20">Conectado</span>
                </div>
                <p class="text-xs font-bold text-white mt-2">PostgreSQL 17</p>
                <p class="text-[10px] text-slate-500">18 conexiones activas</p>
              </div>

              <div class="p-3.5 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center justify-between">
                  <Cpu class="w-4 h-4 text-sky-400" />
                  <span class="text-[10px] font-bold text-emerald-400 bg-emerald-950/60 px-1.5 py-0.2 rounded border border-emerald-500/20">En Ejecución</span>
                </div>
                <p class="text-xs font-bold text-white mt-2">Background Worker</p>
                <p class="text-[10px] text-slate-500">0 errores en cola</p>
              </div>

              <div class="p-3.5 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center justify-between">
                  <ShieldCheck class="w-4 h-4 text-amber-400" />
                  <span class="text-[10px] font-bold text-emerald-400 bg-emerald-950/60 px-1.5 py-0.2 rounded border border-emerald-500/20">Protegido</span>
                </div>
                <p class="text-xs font-bold text-white mt-2">Antiforgery & RBAC</p>
                <p class="text-[10px] text-slate-500">Aislamiento Multi-tenant</p>
              </div>
            </div>
          </BaseCard>
        </div>

        <!-- SuperAdmin Live Feed (1 col) -->
        <div class="space-y-6">
          <BaseCard title="Auditoría en Tiempo Real" subtitle="Últimas acciones registradas a nivel global">
            <template #header>
              <NuxtLink to="/audit-logs" class="text-xs font-semibold text-purple-400 hover:text-purple-300 flex items-center gap-1">
                <span>Ver historial</span>
                <ArrowUpRight class="w-3.5 h-3.5" />
              </NuxtLink>
            </template>

            <div class="space-y-4">
              <div
                v-for="feed in liveAuditFeed"
                :key="feed.id"
                class="p-3 rounded-xl bg-slate-950/40 border border-slate-800/80 hover:border-slate-700/80 transition-all text-xs"
              >
                <div class="flex items-center justify-between gap-2">
                  <span class="font-mono text-[10px] font-bold text-purple-400">{{ feed.action }}</span>
                  <span class="text-[10px] text-slate-500">{{ feed.timestamp }}</span>
                </div>
                <p class="text-slate-200 mt-1 font-medium leading-relaxed">{{ feed.description }}</p>
                <div class="flex items-center gap-1.5 mt-2 text-[10px] text-slate-400">
                  <span class="text-slate-500">Actor:</span>
                  <span class="font-mono text-slate-300">{{ feed.userId }}</span>
                </div>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>
    </template>

    <!-- ========================================== -->
    <!-- COMPANY ADMIN / OPERATOR VIEW              -->
    <!-- ========================================== -->
    <template v-else>
      <!-- Top Company Welcome Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-gradient-to-r from-brand-950/40 via-slate-900/60 to-slate-900/40 border border-brand-500/20 rounded-2xl p-6 shadow-xl backdrop-blur-xl">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
              <Building2 class="w-3.5 h-3.5" />
              {{ permissions.companyName.value || 'Empresa Activa' }}
            </span>
            <span class="inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-400 bg-emerald-950/60 px-2 py-0.5 rounded-lg border border-emerald-500/20">
              <CheckCircle2 class="w-3 h-3" />
              Cuenta Habilitada
            </span>
          </div>
          <h2 class="text-2xl font-black text-white tracking-tight">
            Panel de Control Operativo
          </h2>
          <p class="text-xs text-slate-400 mt-1">
            Bienvenido de nuevo, <span class="text-slate-200 font-semibold">{{ auth.user.value?.fullName }}</span>. Monitoreo en tiempo real de flota y despachos.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <NuxtLink to="/trips">
            <BaseButton variant="primary" size="md">
              <Plus class="w-4 h-4" />
              <span>Nuevo Viaje</span>
            </BaseButton>
          </NuxtLink>
        </div>
      </div>

      <!-- Stats Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <div
          v-for="stat in companyStats"
          :key="stat.name"
          class="rounded-2xl border border-slate-800 bg-slate-900/60 backdrop-blur-xl p-5 shadow-xl relative overflow-hidden group hover:border-slate-700/80 transition-all"
        >
          <div class="flex items-center justify-between">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center border" :class="getStatColor(stat.iconName)">
              <component :is="iconsMap[stat.iconName]" class="w-5 h-5" />
            </div>
            <div class="flex items-center gap-1 text-[11px] font-bold text-emerald-400 bg-emerald-950/60 px-2 py-0.5 rounded-lg border border-emerald-500/20">
              <TrendingUp class="w-3 h-3" />
              <span>{{ stat.change }}</span>
            </div>
          </div>

          <div class="mt-4">
            <p class="text-xs font-semibold text-slate-400">{{ stat.name }}</p>
            <p class="text-2xl font-black text-white tracking-tight mt-1">{{ stat.value }}</p>
          </div>
        </div>
      </div>

      <!-- Main Content Split -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Recent Trips (2 cols) -->
        <div class="lg:col-span-2">
          <BaseCard title="Despachos Recientes" subtitle="Últimas operaciones registradas en el turno">
            <template #header>
              <NuxtLink to="/trips" class="text-xs font-semibold text-brand-400 hover:text-brand-300 flex items-center gap-1">
                <span>Ver todos</span>
                <ArrowUpRight class="w-3.5 h-3.5" />
              </NuxtLink>
            </template>

            <div class="overflow-x-auto">
              <table class="w-full text-left text-xs">
                <thead class="text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
                  <tr>
                    <th class="pb-3">Viaje</th>
                    <th class="pb-3">Ruta</th>
                    <th class="pb-3">Conductor & Unidad</th>
                    <th class="pb-3">Estado</th>
                    <th class="pb-3 text-right">Tiempo</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-800/60">
                  <tr v-for="trip in recentTrips" :key="trip.tripNumber" class="hover:bg-slate-800/30 transition-colors">
                    <td class="py-3.5 font-bold text-white">{{ trip.tripNumber }}</td>
                    <td class="py-3.5 text-slate-300">{{ trip.routeName }}</td>
                    <td class="py-3.5">
                      <p class="font-medium text-slate-200">{{ trip.driverName }}</p>
                      <p class="text-[10px] text-slate-500">{{ trip.licensePlate }}</p>
                    </td>
                    <td class="py-3.5">
                      <BaseBadge
                        v-if="trip.status === 'InProgress'"
                        variant="primary"
                        dot
                      >
                        En Curso
                      </BaseBadge>
                      <BaseBadge
                        v-else-if="trip.status === 'Completed'"
                        variant="success"
                      >
                        Completado
                      </BaseBadge>
                      <BaseBadge
                        v-else-if="trip.status === 'PendingApproval'"
                        variant="warning"
                      >
                        Por Aprobar
                      </BaseBadge>
                      <BaseBadge
                        v-else
                        variant="neutral"
                      >
                        Planificado
                      </BaseBadge>
                    </td>
                    <td class="py-3.5 text-right text-slate-400">{{ trip.createdAt }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </BaseCard>
        </div>

        <!-- Quick Operations / Status (1 col) -->
        <div class="space-y-6">
          <BaseCard title="Estado de la Flota" subtitle="Disponibilidad y turnos en tiempo real">
            <div class="space-y-3.5 text-xs">
              <div class="flex items-center justify-between p-3 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center gap-2.5">
                  <div class="w-2 h-2 rounded-full bg-emerald-400 animate-ping" />
                  <span class="font-medium text-slate-200">Unidades en Ruta</span>
                </div>
                <span class="text-emerald-400 font-semibold text-[11px]">24 Unidades</span>
              </div>

              <div class="flex items-center justify-between p-3 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center gap-2.5">
                  <div class="w-2 h-2 rounded-full bg-brand-400" />
                  <span class="font-medium text-slate-200">En Base / Disponibles</span>
                </div>
                <span class="text-brand-400 font-semibold text-[11px]">24 Unidades</span>
              </div>

              <div class="flex items-center justify-between p-3 rounded-xl bg-slate-950/40 border border-slate-800/80">
                <div class="flex items-center gap-2.5">
                  <div class="w-2 h-2 rounded-full bg-amber-400" />
                  <span class="font-medium text-slate-200">En Mantenimiento</span>
                </div>
                <span class="text-amber-400 font-semibold text-[11px]">4 Unidades</span>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>
    </template>
  </div>
</template>

