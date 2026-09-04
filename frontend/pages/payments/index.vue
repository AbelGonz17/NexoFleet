<script setup lang="ts">
import {
  DollarSign,
  Plus,
  Search,
  Filter,
  RefreshCw,
  Calendar,
  FileSpreadsheet,
  CheckCircle2,
  Clock,
  TrendingUp,
  CreditCard,
  Inbox,
  User
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { PaymentPeriodResponse, PaymentReportResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Liquidaciones y Pagos'
})

const api = useApi()
const activeTab = ref<'periods' | 'reports'>('periods')
const loading = ref(true)
const periods = ref<PaymentPeriodResponse[]>([])
const reports = ref<PaymentReportResponse[]>([])

async function fetchData() {
  loading.value = true
  try {
    const [pRes, rRes] = await Promise.allSettled([
      api.get<PaymentPeriodResponse[]>('/v1/payment-periods'),
      api.get<PaymentReportResponse[]>('/v1/payment-reports')
    ])
    if (pRes.status === 'fulfilled' && pRes.value) {
      periods.value = pRes.value
    } else {
      periods.value = []
    }
    if (rRes.status === 'fulfilled' && rRes.value) {
      reports.value = rRes.value
    } else {
      reports.value = []
    }
  } catch {
    periods.value = []
    reports.value = []
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <DollarSign class="w-3.5 h-3.5" />
            Finanzas y Nómina Operativa
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Liquidaciones y Pagos</h2>
        <p class="text-xs text-slate-400 mt-1">Cálculo automatizado de liquidación por viajes devengados a conductores y transportistas.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchData">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>
      </div>
    </div>

    <!-- Navigation Tabs -->
    <div class="flex items-center gap-2 border-b border-slate-800 pb-2">
      <button
        type="button"
        class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all"
        :class="activeTab === 'periods'
          ? 'bg-brand-500 text-white shadow-lg shadow-brand-500/20'
          : 'text-slate-400 hover:text-white hover:bg-slate-900'"
        @click="activeTab = 'periods'"
      >
        <Calendar class="w-4 h-4" />
        <span>Periodos de Liquidación</span>
        <span class="ml-1 text-[10px] px-1.5 py-0.2 rounded-full" :class="activeTab === 'periods' ? 'bg-white/20' : 'bg-slate-800'">
          {{ periods.length }}
        </span>
      </button>

      <button
        type="button"
        class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all"
        :class="activeTab === 'reports'
          ? 'bg-brand-500 text-white shadow-lg shadow-brand-500/20'
          : 'text-slate-400 hover:text-white hover:bg-slate-900'"
        @click="activeTab = 'reports'"
      >
        <FileSpreadsheet class="w-4 h-4" />
        <span>Reportes de Nómina a Choferes</span>
        <span class="ml-1 text-[10px] px-1.5 py-0.2 rounded-full" :class="activeTab === 'reports' ? 'bg-white/20' : 'bg-slate-800'">
          {{ reports.length }}
        </span>
      </button>
    </div>

    <!-- TAB 1: PERIODS -->
    <div v-if="activeTab === 'periods'" class="space-y-4">
      <BaseCard class="overflow-hidden border-slate-800">
        <div v-if="loading" class="p-12 text-center">
          <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
          <p class="text-sm font-semibold text-white">Consultando periodos de pago...</p>
        </div>

        <div v-else-if="periods.length === 0" class="p-16 text-center">
          <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
            <Inbox class="w-8 h-8" />
          </div>
          <h3 class="text-base font-bold text-white mb-1">No hay periodos de liquidación registrados</h3>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Los periodos quincenales y mensuales de liquidación se generan y calculan a partir de los viajes completados.
          </p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs text-slate-300">
            <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
              <tr>
                <th class="px-4 py-3.5">Código Periodo</th>
                <th class="px-4 py-3.5">Fecha Inicio</th>
                <th class="px-4 py-3.5">Fecha Cierre</th>
                <th class="px-4 py-3.5">Total Liquidado</th>
                <th class="px-4 py-3.5">Estado</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-800/60">
              <tr v-for="p in periods" :key="p.id" class="hover:bg-slate-900/40 transition-colors">
                <td class="px-4 py-3.5 font-mono font-bold text-brand-400">
                  {{ p.periodNumber }}
                </td>
                <td class="px-4 py-3.5 font-mono text-slate-300">
                  {{ p.startDate }}
                </td>
                <td class="px-4 py-3.5 font-mono text-slate-300">
                  {{ p.endDate }}
                </td>
                <td class="px-4 py-3.5 font-bold text-white">
                  {{ p.totalAmount.toLocaleString('es-PE', { minimumFractionDigits: 2 }) }} {{ p.currency }}
                </td>
                <td class="px-4 py-3.5">
                  <BaseBadge v-if="p.status === 'Settled'" variant="success">Liquidado</BaseBadge>
                  <BaseBadge v-else-if="p.status === 'Open'" variant="brand">Abierto</BaseBadge>
                  <BaseBadge v-else variant="default">{{ p.status }}</BaseBadge>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </BaseCard>
    </div>

    <!-- TAB 2: REPORTS -->
    <div v-if="activeTab === 'reports'" class="space-y-4">
      <BaseCard class="overflow-hidden border-slate-800">
        <div v-if="loading" class="p-12 text-center">
          <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
          <p class="text-sm font-semibold text-white">Consultando reportes de nómina...</p>
        </div>

        <div v-else-if="reports.length === 0" class="p-16 text-center">
          <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
            <Inbox class="w-8 h-8" />
          </div>
          <h3 class="text-base font-bold text-white mb-1">No hay liquidaciones a choferes generadas</h3>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Cuando se completen viajes y se procesen los cierres de periodo, aquí se mostrarán los resúmenes de pago individuales.
          </p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs text-slate-300">
            <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
              <tr>
                <th class="px-4 py-3.5">Conductor / Transportista</th>
                <th class="px-4 py-3.5">Viajes</th>
                <th class="px-4 py-3.5">Monto Bruto</th>
                <th class="px-4 py-3.5">Deducciones</th>
                <th class="px-4 py-3.5">Monto Neto a Pagar</th>
                <th class="px-4 py-3.5">Estado</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-800/60">
              <tr v-for="r in reports" :key="r.id" class="hover:bg-slate-900/40 transition-colors">
                <td class="px-4 py-3.5 font-bold text-white">
                  <div class="flex items-center gap-1.5">
                    <User class="w-3.5 h-3.5 text-brand-400" />
                    <span>{{ r.employeeName }}</span>
                  </div>
                </td>
                <td class="px-4 py-3.5 font-medium text-slate-200">
                  {{ r.tripsCount }} viajes
                </td>
                <td class="px-4 py-3.5 font-mono text-slate-300">
                  {{ r.grossAmount.toFixed(2) }} {{ r.currency }}
                </td>
                <td class="px-4 py-3.5 font-mono text-rose-400">
                  -{{ r.deductions.toFixed(2) }} {{ r.currency }}
                </td>
                <td class="px-4 py-3.5 font-bold text-emerald-400">
                  {{ r.netAmount.toFixed(2) }} {{ r.currency }}
                </td>
                <td class="px-4 py-3.5">
                  <BaseBadge v-if="r.status === 'Paid'" variant="success">Pagado</BaseBadge>
                  <BaseBadge v-else-if="r.status === 'Pending'" variant="warning">Pendiente</BaseBadge>
                  <BaseBadge v-else variant="default">{{ r.status }}</BaseBadge>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </BaseCard>
    </div>
  </div>
</template>
