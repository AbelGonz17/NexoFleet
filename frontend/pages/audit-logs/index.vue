<script setup lang="ts">
import { FileCheck2 } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Registro de Auditoría'
})

const api = useApi()
const logs = ref<any[]>([])
const loading = ref(true)

async function fetchLogs() {
  loading.value = true
  try {
    logs.value = await api.get<any[]>('/v1/audit-logs')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchLogs()
})
</script>

<template>
  <div class="space-y-6">
    <div>
      <h2 class="text-2xl font-black text-white tracking-tight">Registro de Auditoría</h2>
      <p class="text-xs text-slate-400 mt-1">Trazabilidad de operaciones críticas, modificaciones de registros y accesos al sistema.</p>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Fecha y Hora UTC</th>
              <th class="px-6 py-3.5">Acción</th>
              <th class="px-6 py-3.5">Entidad</th>
              <th class="px-6 py-3.5">ID Actor</th>
              <th class="px-6 py-3.5">Dirección IP</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="5" class="py-8 text-slate-500 font-medium">Cargando registros de auditoría...</td>
            </tr>
            <tr v-else-if="logs.length === 0" class="text-center py-12">
              <td colspan="5" class="py-8 text-slate-500 font-medium">No hay eventos de auditoría registrados.</td>
            </tr>
            <tr v-for="l in logs" :key="l.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 text-slate-300 font-mono">{{ l.occurredAtUtc }}</td>
              <td class="px-6 py-4">
                <span class="font-bold text-white">{{ l.action }}</span>
              </td>
              <td class="px-6 py-4 text-slate-300 font-medium">{{ l.entityType }}</td>
              <td class="px-6 py-4 text-slate-400 font-mono">{{ l.actorUserId?.slice(0, 8) }}</td>
              <td class="px-6 py-4 text-slate-400 font-mono">{{ l.ipAddress || '—' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
