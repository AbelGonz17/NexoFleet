<script setup lang="ts">
import { Bell, Check } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { NotificationResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Notificaciones'
})

const api = useApi()
const notifications = ref<NotificationResponse[]>([])
const loading = ref(true)

async function fetchNotifications() {
  loading.value = true
  try {
    notifications.value = await api.get<NotificationResponse[]>('/v1/notifications/my')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

async function markAsRead(id: string) {
  try {
    await api.post(`/v1/notifications/${id}/read`)
    const target = notifications.value.find(n => n.id === id)
    if (target) target.status = 'Read'
  } catch {
    // Handled by useApi
  }
}

onMounted(() => {
  fetchNotifications()
})
</script>

<template>
  <div class="space-y-6 max-w-4xl mx-auto">
    <div>
      <h2 class="text-2xl font-black text-white tracking-tight">Centro de Notificaciones</h2>
      <p class="text-xs text-slate-400 mt-1">Alertas operativas, asignación de viajes y avisos administrativos.</p>
    </div>

    <BaseCard padding="none">
      <div v-if="loading" class="text-center py-12 text-slate-500 font-medium">
        Cargando notificaciones...
      </div>
      <div v-else-if="notifications.length === 0" class="text-center py-12 text-slate-500 font-medium">
        No tienes notificaciones pendientes.
      </div>
      <div v-else class="divide-y divide-slate-800/60">
        <div
          v-for="notif in notifications"
          :key="notif.id"
          class="p-5 flex items-start justify-between gap-4 hover:bg-slate-800/30 transition-colors"
          :class="{ 'bg-brand-950/10': notif.status === 'Unread' }"
        >
          <div class="flex items-start gap-3.5">
            <div class="w-8 h-8 rounded-xl bg-slate-800 border border-slate-700 flex items-center justify-center shrink-0 mt-0.5">
              <Bell class="w-4 h-4 text-brand-400" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <p class="text-sm font-bold text-white">{{ notif.title }}</p>
                <BaseBadge v-if="notif.status === 'Unread'" variant="primary" size="sm" dot>Nueva</BaseBadge>
              </div>
              <p class="text-xs text-slate-300 mt-1 leading-relaxed">{{ notif.message }}</p>
              <p class="text-[10px] text-slate-500 mt-2">{{ notif.createdAtUtc }}</p>
            </div>
          </div>

          <button
            v-if="notif.status === 'Unread'"
            type="button"
            class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
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
