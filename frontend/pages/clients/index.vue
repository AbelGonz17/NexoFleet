<script setup lang="ts">
import { Briefcase, Plus } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { ClientResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Clientes Corporativos'
})

const api = useApi()
const clients = ref<ClientResponse[]>([])
const loading = ref(true)

async function fetchClients() {
  loading.value = true
  try {
    clients.value = await api.get<ClientResponse[]>('/v1/clients')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchClients()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Clientes Corporativos</h2>
        <p class="text-xs text-slate-400 mt-1">Gestión de cuentas corporativas, contratos y contactos de facturación.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Nuevo Cliente</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Código</th>
              <th class="px-6 py-3.5">Empresa / Razón Social</th>
              <th class="px-6 py-3.5">RIF / Tax ID</th>
              <th class="px-6 py-3.5">Contacto</th>
              <th class="px-6 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="5" class="py-8 text-slate-500 font-medium">Cargando clientes...</td>
            </tr>
            <tr v-else-if="clients.length === 0" class="text-center py-12">
              <td colspan="5" class="py-8 text-slate-500 font-medium">No hay clientes registrados.</td>
            </tr>
            <tr v-for="c in clients" :key="c.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white">{{ c.clientCode }}</td>
              <td class="px-6 py-4 font-semibold text-slate-200">{{ c.name }}</td>
              <td class="px-6 py-4 text-slate-300 font-mono">{{ c.taxId }}</td>
              <td class="px-6 py-4">
                <p class="text-slate-200 font-medium">{{ c.contactPerson }}</p>
                <p class="text-[10px] text-slate-500">{{ c.email }} · {{ c.phone }}</p>
              </td>
              <td class="px-6 py-4">
                <BaseBadge :variant="c.status === 'Active' ? 'success' : 'neutral'" size="sm">{{ c.status }}</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
