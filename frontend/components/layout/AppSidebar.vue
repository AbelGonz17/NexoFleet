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
  ChevronRight
} from 'lucide-vue-next'

const route = useRoute()
const auth = useAuth()

const navigation = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Viajes Operativos', href: '/trips', icon: Navigation },
  { name: 'Flota y Vehículos', href: '/vehicles', icon: Truck },
  { name: 'Rutas Maestras', href: '/routes', icon: MapPin },
  { name: 'Programaciones', href: '/schedules', icon: Calendar },
  { name: 'Personal y Choferes', href: '/employees', icon: Users },
  { name: 'Clientes', href: '/clients', icon: Briefcase },
  { name: 'Liquidaciones y Pagos', href: '/payments', icon: DollarSign },
  { name: 'Notificaciones', href: '/notifications', icon: Bell },
  { name: 'Empresa', href: '/companies', icon: Building2 },
  { name: 'Auditoría', href: '/audit-logs', icon: FileCheck2 }
]

function isActive(href: string) {
  if (href === '/') return route.path === '/'
  return route.path.startsWith(href)
}
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
        v-for="item in navigation"
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
      <div class="flex items-center gap-3 p-2 rounded-xl bg-slate-900/80 border border-slate-800/80 mb-2">
        <div class="w-8 h-8 rounded-lg bg-slate-800 flex items-center justify-center text-xs font-bold text-slate-300 border border-slate-700">
          {{ auth.user.value?.fullName?.charAt(0) || 'U' }}
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-xs font-semibold text-white truncate">{{ auth.user.value?.fullName || 'Usuario' }}</p>
          <p class="text-[10px] text-slate-400 truncate">{{ auth.user.value?.role || 'Operador' }}</p>
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
