<script setup lang="ts">
import {
  Truck,
  Navigation,
  CheckCircle2,
  AlertTriangle,
  ArrowUpRight,
  TrendingUp,
  Clock,
  DollarSign,
  Plus
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

const stats = [
  {
    name: 'Viajes Activos Hoy',
    value: '24',
    change: '+12%',
    trend: 'up',
    icon: Navigation,
    color: 'text-brand-400 bg-brand-500/10 border-brand-500/20'
  },
  {
    name: 'Flota Operativa',
    value: '48 / 52',
    change: '92.3%',
    trend: 'up',
    icon: Truck,
    color: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20'
  },
  {
    name: 'Puntualidad en Rutas',
    value: '98.5%',
    change: '+0.8%',
    trend: 'up',
    icon: Clock,
    color: 'text-sky-400 bg-sky-500/10 border-sky-500/20'
  },
  {
    name: 'Facturación Mensual',
    value: '$45,280',
    change: '+18.4%',
    trend: 'up',
    icon: DollarSign,
    color: 'text-amber-400 bg-amber-500/10 border-amber-500/20'
  }
]

const recentTrips = [
  {
    number: 'TRIP-2026-089',
    route: 'Zona Industrial -> Planta Central',
    driver: 'Carlos Mendoza',
    vehicle: 'Toyota Coaster (A89BC2)',
    status: 'InProgress',
    time: 'Hace 25 min'
  },
  {
    number: 'TRIP-2026-088',
    route: 'Ruta Norte 02 -> Terminal Este',
    driver: 'Luis Navarro',
    vehicle: 'Iveco Daily (X12YZ3)',
    status: 'Completed',
    time: 'Hace 1 hora'
  },
  {
    number: 'TRIP-2026-087',
    route: 'Personal Turno Noche -> Sede B',
    driver: 'José Ramírez',
    vehicle: 'Mercedes Sprinter (M44KK1)',
    status: 'Planned',
    time: 'Programado 18:30'
  },
  {
    number: 'TRIP-2026-086',
    route: 'Despacho Especial Clínico',
    driver: 'Manuel Díaz',
    vehicle: 'Ford Transit (F90LL8)',
    status: 'PendingApproval',
    time: 'Por aprobar'
  }
]
</script>

<template>
  <div class="space-y-8">
    <!-- Top Welcome & Quick Action Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
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
        v-for="stat in stats"
        :key="stat.name"
        class="rounded-2xl border border-slate-800 bg-slate-900/60 backdrop-blur-xl p-5 shadow-xl relative overflow-hidden group hover:border-slate-700/80 transition-all"
      >
        <div class="flex items-center justify-between">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center border" :class="stat.color">
            <component :is="stat.icon" class="w-5 h-5" />
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
                <tr v-for="trip in recentTrips" :key="trip.number" class="hover:bg-slate-800/30 transition-colors">
                  <td class="py-3.5 font-bold text-white">{{ trip.number }}</td>
                  <td class="py-3.5 text-slate-300">{{ trip.route }}</td>
                  <td class="py-3.5">
                    <p class="font-medium text-slate-200">{{ trip.driver }}</p>
                    <p class="text-[10px] text-slate-500">{{ trip.vehicle }}</p>
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
                  <td class="py-3.5 text-right text-slate-400">{{ trip.time }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </BaseCard>
      </div>

      <!-- Quick Operations / Status (1 col) -->
      <div class="space-y-6">
        <BaseCard title="Estado del Sistema" subtitle="Salud de servicios e integraciones">
          <div class="space-y-3.5 text-xs">
            <div class="flex items-center justify-between p-3 rounded-xl bg-slate-950/40 border border-slate-800/80">
              <div class="flex items-center gap-2.5">
                <div class="w-2 h-2 rounded-full bg-emerald-400 animate-ping" />
                <span class="font-medium text-slate-200">API Gateway .NET 10</span>
              </div>
              <span class="text-emerald-400 font-semibold text-[11px]">En Línea</span>
            </div>

            <div class="flex items-center justify-between p-3 rounded-xl bg-slate-950/40 border border-slate-800/80">
              <div class="flex items-center gap-2.5">
                <div class="w-2 h-2 rounded-full bg-emerald-400" />
                <span class="font-medium text-slate-200">PostgreSQL Database</span>
              </div>
              <span class="text-emerald-400 font-semibold text-[11px]">Conectado</span>
            </div>

            <div class="flex items-center justify-between p-3 rounded-xl bg-slate-950/40 border border-slate-800/80">
              <div class="flex items-center gap-2.5">
                <div class="w-2 h-2 rounded-full bg-emerald-400" />
                <span class="font-medium text-slate-200">Almacenamiento IFileStorage</span>
              </div>
              <span class="text-emerald-400 font-semibold text-[11px]">Listo</span>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </div>
</template>
