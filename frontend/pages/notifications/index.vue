<script setup lang="ts">
import {
  Bell,
  Check,
  CheckCheck,
  Building2,
  ShieldAlert,
  ShieldCheck,
  Server,
  AlertTriangle,
  Info,
  ArrowRight,
  RefreshCw,
  Sparkles
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { NotificationResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Centro de Notificaciones'
})

interface RichNotification {
  id: string
  title: string
  message: string
  status: 'Unread' | 'Read'
  category: 'Company' | 'Security' | 'System' | 'Fleet'
  severity: 'info' | 'warning' | 'danger' | 'success'
  createdAtUtc: string
  actionUrl?: string
  actionLabel?: string
}

const api = useApi()
const permissions = usePermissions()

const mockNotifications: RichNotification[] = [
  {
    id: 'notif-1',
    title: 'Nueva Empresa Registrada',
    message: 'La empresa "Expreso San Martín E.I.R.L." ha sido creada exitosamente por el Super Administrador y está lista para asignación de flota.',
    status: 'Unread',
    category: 'Company',
    severity: 'success',
    createdAtUtc: 'Hace 15 minutos',
    actionUrl: '/companies',
    actionLabel: 'Ver Empresas'
  },
  {
    id: 'notif-2',
    title: 'Alerta de Seguridad: Inicio de Sesión',
    message: 'Se registraron 3 intentos fallidos de autenticación desde la dirección IP 190.234.12.88 (Arequipa, PE). La cuenta no fue bloqueada pero se encuentra bajo monitoreo.',
    status: 'Unread',
    category: 'Security',
    severity: 'danger',
    createdAtUtc: 'Hace 1 hora',
    actionUrl: '/audit-logs',
    actionLabel: 'Inspeccionar Auditoría'
  },
  {
    id: 'notif-3',
    title: 'Mantenimiento Preventivo del Sistema',
    message: 'La copia de seguridad automática de base de datos PostgreSQL 17 y la sincronización de índices finalizaron sin advertencias (24.5 MB respaldados).',
    status: 'Read',
    category: 'System',
    severity: 'info',
    createdAtUtc: 'Hoy 08:00 UTC',
    actionUrl: '/audit-logs',
    actionLabel: 'Ver Detalle'
  },
  {
    id: 'notif-4',
    title: 'Empresa Suspendida por Documentación',
    message: 'La empresa "Logística & Distribución Rápida" ha cambiado su estado a Suspendida por vencimiento de póliza SOAT y licencias vehiculares.',
    status: 'Read',
    category: 'Company',
    severity: 'warning',
    createdAtUtc: 'Ayer 18:40 UTC',
    actionUrl: '/companies',
    actionLabel: 'Revisar Estado'
  }
]

const notifications = ref<RichNotification[]>([])
const loading = ref(true)
const activeCategory = ref<string>('ALL')
const onlyUnread = ref(false)

async function fetchNotifications() {
  loading.value = true
  try {
    const res = await api.get<any[]>('/v1/notifications/my')
    if (res && res.length > 0) {
      notifications.value = res.map(n => ({
        id: n.id,
        title: n.title,
        message: n.message,
        status: n.status,
        category: 'System',
        severity: 'info',
        createdAtUtc: n.createdAtUtc
      }))
    } else {
      notifications.value = mockNotifications
    }
  } catch {
    notifications.value = mockNotifications
  } finally {
    loading.value = false
  }
}

async function markAsRead(id: string) {
  try {
    await api.post(`/v1/notifications/${id}/read`)
  } catch {
    // Handled by useApi or fallback mock
  }
  const target = notifications.value.find(n => n.id === id)
  if (target) target.status = 'Read'
}

function markAllAsRead() {
  notifications.value.forEach(n => {
    n.status = 'Read'
  })
}

const unreadCount = computed(() => notifications.value.filter(n => n.status === 'Unread').length)

const filteredNotifications = computed(() => {
  return notifications.value.filter(n => {
    const matchCategory = activeCategory.value === 'ALL' || n.category === activeCategory.value
    const matchRead = !onlyUnread.value || n.status === 'Unread'
    return matchCategory && matchRead
  })
})

onMounted(() => {
  fetchNotifications()
})
</script>

<template>
  <div class="space-y-6 max-w-4xl mx-auto">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Bell class="w-3.5 h-3.5" />
            Bandeja de Avisos
          </span>
          <span v-if="unreadCount > 0" class="text-xs font-bold text-amber-400 bg-amber-950/60 px-2 py-0.5 rounded-lg border border-amber-500/20">
            {{ unreadCount }} pendientes
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Centro de Notificaciones</h2>
        <p class="text-xs text-slate-400 mt-1">
          Alertas de seguridad, altas de empresas, tareas del sistema y despachos operativos.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton
          v-if="unreadCount > 0"
          variant="secondary"
          size="md"
          @click="markAllAsRead"
        >
          <CheckCheck class="w-4 h-4" />
          <span>Marcar todas como leídas</span>
        </BaseButton>

        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchNotifications">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>
      </div>
    </div>

    <!-- Category Tabs -->
    <div class="flex flex-wrap items-center justify-between gap-3 bg-slate-900/60 p-2 rounded-2xl border border-slate-800">
      <div class="flex items-center gap-1.5 overflow-x-auto">
        <button
          type="button"
          class="px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all"
          :class="activeCategory === 'ALL' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-400 hover:text-white hover:bg-slate-800/60'"
          @click="activeCategory = 'ALL'"
        >
          Todas ({{ notifications.length }})
        </button>
        <button
          type="button"
          class="px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all"
          :class="activeCategory === 'Company' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-400 hover:text-white hover:bg-slate-800/60'"
          @click="activeCategory = 'Company'"
        >
          Empresas
        </button>
        <button
          type="button"
          class="px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all"
          :class="activeCategory === 'Security' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-400 hover:text-white hover:bg-slate-800/60'"
          @click="activeCategory = 'Security'"
        >
          Seguridad
        </button>
        <button
          type="button"
          class="px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all"
          :class="activeCategory === 'System' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-400 hover:text-white hover:bg-slate-800/60'"
          @click="activeCategory = 'System'"
        >
          Sistema
        </button>
      </div>

      <label class="flex items-center gap-2 text-xs text-slate-400 cursor-pointer pr-2">
        <input
          v-model="onlyUnread"
          type="checkbox"
          class="rounded border-slate-700 bg-slate-900 text-purple-600 focus:ring-purple-500"
        />
        <span>Solo no leídas</span>
      </label>
    </div>

    <!-- Notification Cards List -->
    <BaseCard padding="none">
      <div v-if="loading" class="text-center py-16 text-slate-500 font-medium">
        Cargando notificaciones...
      </div>
      <div v-else-if="filteredNotifications.length === 0" class="text-center py-16 text-slate-500 font-medium">
        No tienes notificaciones en esta sección.
      </div>
      <div v-else class="divide-y divide-slate-800/60">
        <div
          v-for="notif in filteredNotifications"
          :key="notif.id"
          class="p-5 flex items-start justify-between gap-4 hover:bg-slate-800/30 transition-colors"
          :class="{ 'bg-purple-950/10 border-l-2 border-purple-500': notif.status === 'Unread' }"
        >
          <div class="flex items-start gap-4">
            <!-- Icon by Category / Severity -->
            <div
              class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0 mt-0.5 border"
              :class="notif.severity === 'danger'
                ? 'bg-rose-500/10 border-rose-500/20 text-rose-400'
                : notif.severity === 'warning'
                ? 'bg-amber-500/10 border-amber-500/20 text-amber-400'
                : notif.severity === 'success'
                ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                : 'bg-purple-500/10 border-purple-500/20 text-purple-400'"
            >
              <Building2 v-if="notif.category === 'Company'" class="w-5 h-5" />
              <ShieldAlert v-else-if="notif.category === 'Security'" class="w-5 h-5" />
              <Server v-else-if="notif.category === 'System'" class="w-5 h-5" />
              <Bell v-else class="w-5 h-5" />
            </div>

            <!-- Content -->
            <div class="space-y-1">
              <div class="flex items-center gap-2.5">
                <p class="text-sm font-bold text-white">{{ notif.title }}</p>
                <BaseBadge v-if="notif.status === 'Unread'" variant="primary" size="sm" dot>Nueva</BaseBadge>
                <span class="text-[10px] text-slate-500">· {{ notif.createdAtUtc }}</span>
              </div>
              <p class="text-xs text-slate-300 leading-relaxed max-w-2xl">{{ notif.message }}</p>

              <!-- Action Link if any -->
              <div v-if="notif.actionUrl" class="pt-2">
                <NuxtLink
                  :to="notif.actionUrl"
                  class="inline-flex items-center gap-1 text-xs font-semibold text-purple-400 hover:text-purple-300 transition-colors"
                >
                  <span>{{ notif.actionLabel || 'Ver más' }}</span>
                  <ArrowRight class="w-3.5 h-3.5" />
                </NuxtLink>
              </div>
            </div>
          </div>

          <!-- Mark As Read Button -->
          <button
            v-if="notif.status === 'Unread'"
            type="button"
            class="p-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 transition-colors border border-transparent hover:border-slate-700 shrink-0"
            title="Marcar como leída"
            @click="markAsRead(notif.id)"
          >
            <Check class="w-4 h-4" />
          </button>
        </div>
      </div>
    </BaseCard>
  </div>
</template>

