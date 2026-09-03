<script setup lang="ts">
import { DollarSign, Plus } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Liquidaciones y Pagos'
})

const api = useApi()
const periods = ref<any[]>([])
const loading = ref(true)

async function fetchPeriods() {
  loading.value = true
  try {
    periods.value = await api.get<any[]>('/v1/payment-periods')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchPeriods()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Periodos y Liquidaciones</h2>
        <p class="text-xs text-slate-400 mt-1">Cierre de quincenas, cálculo de honorarios de choferes, adiciones y deducciones.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Abrir Periodo</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Código de Periodo</th>
              <th class="px-6 py-3.5">Rango de Fechas</th>
              <th class="px-6 py-3.5">Estado</th>
              <th class="px-6 py-3.5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="4" class="py-8 text-slate-500 font-medium">Cargando periodos de liquidación...</td>
            </tr>
            <tr v-else-if="periods.length === 0" class="text-center py-12">
              <td colspan="4" class="py-8 text-slate-500 font-medium">No hay periodos de pago configurados.</td>
            </tr>
            <tr v-for="p in periods" :key="p.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white tracking-wider">{{ p.code }}</td>
              <td class="px-6 py-4 text-slate-300">{{ p.startsOn }} al {{ p.endsOn }}</td>
              <td class="px-6 py-4">
                <BaseBadge :variant="p.status === 'Open' ? 'success' : 'neutral'" size="sm">{{ p.status }}</BaseBadge>
              </td>
              <td class="px-6 py-4 text-right">
                <button class="text-brand-400 hover:text-brand-300 font-semibold text-xs">Ver Relaciones</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
