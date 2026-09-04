<script setup lang="ts">
import {
  LayoutDashboard,
  Truck,
  MapPin,
  Calendar,
  Navigation,
  Users,
  Building2,
  Briefcase,
  DollarSign,
  Bell,
  FileCheck2,
  LogOut,
  ChevronRight,
  ShieldCheck,
  Building,
  User
} from 'lucide-vue-next'
import BaseBadge from '~/components/common/BaseBadge.vue'

const route = useRoute()
const auth = useAuth()
const permissions = usePermissions()

interface NavItem {
  name: string
  href: string
  icon: any
  roles: string[]
}

const allNavigation: NavItem[] = [
  // Dashboard
  { name: 'Dashboard', href: '/', icon: LayoutDashboard, roles: ['SuperAdmin', 'Administrator', 'Employee'] },

  // SuperAdmin Only
  { name: 'Empresas del Sistema', href: '/companies', icon: Building2, roles: ['SuperAdmin'] },
  { name: 'Auditoría Global', href: '/audit-logs', icon: FileCheck2, roles: ['SuperAdmin'] },

  // Company Administrator
  { name: 'Viajes Operativos', href: '/trips', icon: Navigation, roles: ['Administrator'] },
  { name: 'Flota y Vehículos', href: '/vehicles', icon: Truck, roles: ['Administrator'] },
  { name: 'Rutas Maestras', href: '/routes', icon: MapPin, roles: ['Administrator'] },
  { name: 'Programaciones', href: '/schedules', icon: Calendar, roles: ['Administrator'] },
  { name: 'Personal y Choferes', href: '/employees', icon: Users, roles: ['Administrator'] },
  { name: 'Clientes', href: '/clients', icon: Briefcase, roles: ['Administrator'] },
  { name: 'Liquidaciones y Pagos', href: '/payments', icon: DollarSign, roles: ['Administrator'] },

  // Employee (Conductor / Operador)
  { name: 'Mis Viajes', href: '/trips', icon: Navigation, roles: ['Employee'] },
  { name: 'Mi Vehículo', href: '/vehicles', icon: Truck, roles: ['Employee'] },
  { name: 'Mis Pagos', href: '/payments', icon: DollarSign, roles: ['Employee'] },

  // Common
  { name: 'Notificaciones', href: '/notifications', icon: Bell, roles: ['SuperAdmin', 'Administrator', 'Employee'] }
]

const visibleNavigation = computed(() => {
  return allNavigation.filter(item => {
    return item.roles.some(r => permissions.roles.value.includes(r))
  })
})

function isActive(href: string) {
  if (href === '/') return route.path === '/'
  return route.path.startsWith(href)
}

const roleBadgeVariant = computed(() => {
  if (permissions.isSuperAdmin.value) return 'primary'
  if (permissions.isCompanyAdmin.value) return 'success'
  return 'default'
})
</script>

<template>
  <aside class="w-64 flex flex-col bg-slate-900/90 border-r border-slate-800 backdrop-blur-xl h-screen sticky top-0 shrink-0 select-none z-30">
    <!-- Brand Header -->
    <div class="h-16 flex items-center px-6 border-b border-slate-800 gap-3">
      <div class="w-9 h-9 rounded-xl bg-gradient-to-tr from-brand-600 to-indigo-400 flex items-center justify-center shadow-lg shadow-brand-500/20">
        <Truck class="w-5 h-5 text-white" />
      </div>
      <div>
        <h1 class="text-base font-extrabold tracking-tight text-white leading-tight">NexoFleet</h1>
        <p class="text-[10px] font-semibold text-brand-400 uppercase tracking-wider">Transport Platform</p>
      </div>
    </div>

    <!-- Navigation Links -->
    <div class="flex-1 overflow-y-auto px-3 py-4 space-y-1">
      <NuxtLink
        v-for="item in visibleNavigation"
        :key="item.name"
        :to="item.href"
        class="flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-semibold transition-all group relative"
        :class="isActive(item.href)
          ? 'bg-brand-600/15 text-brand-300 border border-brand-500/20 shadow-sm'
          : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'"
      >
        <component
          :is="item.icon"
          class="w-4 h-4 shrink-0 transition-colors"
          :class="isActive(item.href) ? 'text-brand-400' : 'text-slate-500 group-hover:text-slate-300'"
        />
        <span class="flex-1 truncate">{{ item.name }}</span>
        <ChevronRight
          v-if="isActive(item.href)"
          class="w-3.5 h-3.5 text-brand-400 shrink-0"
        />
      </NuxtLink>
    </div>

    <!-- User / Session Footer -->
    <div class="p-3 border-t border-slate-800 bg-slate-950/40">
      <div class="p-2.5 rounded-xl bg-slate-900/80 border border-slate-800/80 mb-2 space-y-2">
        <div class="flex items-center gap-2.5">
          <div class="w-8 h-8 rounded-lg bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-xs font-black text-brand-400">
            {{ auth.user.value?.fullName?.charAt(0) || auth.user.value?.email?.charAt(0)?.toUpperCase() || 'U' }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-xs font-bold text-white truncate">{{ auth.user.value?.fullName || 'Usuario' }}</p>
            <p class="text-[10px] text-slate-400 truncate">{{ auth.user.value?.email }}</p>
          </div>
        </div>

        <div class="flex items-center justify-between pt-1 border-t border-slate-800/60 text-[10px]">
          <span class="text-slate-500 font-medium">Rol activo:</span>
          <span
            class="px-2 py-0.5 rounded-md font-semibold"
            :class="permissions.isSuperAdmin.value
              ? 'bg-purple-500/20 text-purple-300 border border-purple-500/30'
              : permissions.isCompanyAdmin.value
              ? 'bg-brand-500/20 text-brand-300 border border-brand-500/30'
              : 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/30'"
          >
            {{ permissions.roleLabel.value }}
          </span>
        </div>
      </div>

      <button
        type="button"
        class="w-full flex items-center justify-center gap-2 px-3 py-2 rounded-xl text-xs font-semibold text-rose-400 hover:bg-rose-950/30 hover:text-rose-300 transition-colors"
        @click="auth.logout()"
      >
        <LogOut class="w-4 h-4" />
        <span>Cerrar Sesión</span>
      </button>
    </div>
  </aside>
</template>
