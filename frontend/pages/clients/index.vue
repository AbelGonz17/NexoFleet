<script setup lang="ts">
import {
  Briefcase,
  Plus,
  Search,
  RefreshCw,
  Building2,
  Phone,
  Mail,
  CheckCircle2,
  FileSpreadsheet,
  Inbox,
  User
} from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import CreateClientModal from '~/components/clients/CreateClientModal.vue'
import type { ClientResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Clientes Corporativos'
})

const api = useApi()
const toasts = useToasts()
const clients = ref<ClientResponse[]>([])
const loading = ref(true)
const searchQuery = ref('')
const selectedStatus = ref('ALL')
const isCreateModalOpen = ref(false)

async function fetchClients() {
  loading.value = true
  try {
    const res = await api.get<ClientResponse[]>('/v1/clients')
    clients.value = res || []
  } catch {
    clients.value = []
  } finally {
    loading.value = false
  }
}

const filteredClients = computed(() => {
  return clients.value.filter(c => {
    const tax = c.taxIdentification || c.taxId || ''
    const contact = c.contactName || c.contactPerson || ''
    const matchSearch =
      searchQuery.value.trim() === '' ||
      c.clientCode.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      c.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      tax.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      contact.toLowerCase().includes(searchQuery.value.toLowerCase())

    const matchStatus =
      selectedStatus.value === 'ALL' || c.status === selectedStatus.value

    return matchSearch && matchStatus
  })
})

const kpiTotal = computed(() => clients.value.length)
const kpiActive = computed(() => clients.value.filter(c => c.status === 'Active').length)

onMounted(() => {
  fetchClients()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2 mb-1">
          <span class="inline-flex items-center gap-1.5 text-xs font-bold bg-brand-500/20 text-brand-300 px-2.5 py-0.5 rounded-lg border border-brand-500/30">
            <Briefcase class="w-3.5 h-3.5" />
            Cuentas Comerciales
          </span>
        </div>
        <h2 class="text-2xl font-black text-white tracking-tight">Clientes Corporativos</h2>
        <p class="text-xs text-slate-400 mt-1">Directorio de empresas clientes, contratos y contactos de logística.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" size="md" :loading="loading" @click="fetchClients">
          <RefreshCw class="w-4 h-4" />
        </BaseButton>

        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Registrar Cliente</span>
        </BaseButton>
      </div>
    </div>

    <!-- KPIs -->
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Total Clientes Registrados</p>
            <p class="text-2xl font-black text-white mt-1">{{ kpiTotal }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400">
            <Briefcase class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>

      <BaseCard class="p-4 border-slate-800 bg-slate-900/60">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs text-slate-400 font-medium">Cuentas Activas</p>
            <p class="text-2xl font-black text-emerald-400 mt-1">{{ kpiActive }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400">
            <CheckCircle2 class="w-5 h-5" />
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filter & Search -->
    <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-slate-900/60 border border-slate-800/80 rounded-2xl p-3">
      <div class="relative flex-1">
        <Search class="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por razón social, código, RUC o persona de contacto..."
          class="w-full bg-slate-950 border border-slate-800 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-brand-500"
        />
      </div>

      <div class="flex items-center gap-2">
        <select
          v-model="selectedStatus"
          class="bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-300 focus:outline-none focus:border-brand-500"
        >
          <option value="ALL">Todos los Estados</option>
          <option value="Active">Activo</option>
          <option value="Inactive">Inactivo</option>
        </select>
      </div>
    </div>

    <!-- Clients Table -->
    <BaseCard class="overflow-hidden border-slate-800">
      <div v-if="loading" class="p-12 text-center">
        <RefreshCw class="w-8 h-8 text-brand-400 animate-spin mx-auto mb-3" />
        <p class="text-sm font-semibold text-white">Consultando cartera de clientes...</p>
        <p class="text-xs text-slate-400 mt-1">Conectando con la base de datos de la empresa</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredClients.length === 0" class="p-16 text-center">
        <div class="w-16 h-16 rounded-2xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mx-auto mb-4">
          <Inbox class="w-8 h-8" />
        </div>
        <h3 class="text-base font-bold text-white mb-1">No hay clientes corporativos registrados</h3>
        <p class="text-xs text-slate-400 max-w-sm mx-auto mb-5">
          Registra tus cuentas comerciales y empresas clientes para asociarlas a rutas y liquidaciones.
        </p>
        <BaseButton variant="primary" size="md" @click="isCreateModalOpen = true">
          <Plus class="w-4 h-4" />
          <span>Registrar Primer Cliente</span>
        </BaseButton>
      </div>

      <!-- Table Content -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-300">
          <thead class="bg-slate-950 text-slate-400 font-bold border-b border-slate-800">
            <tr>
              <th class="px-4 py-3.5">Código</th>
              <th class="px-4 py-3.5">Razón Social / Empresa</th>
              <th class="px-4 py-3.5">RUC / ID Fiscal</th>
              <th class="px-4 py-3.5">Contacto / Logística</th>
              <th class="px-4 py-3.5">Canal de Contacto</th>
              <th class="px-4 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-for="c in filteredClients" :key="c.id" class="hover:bg-slate-900/40 transition-colors">
              <td class="px-4 py-3.5 font-bold font-mono text-brand-400">
                {{ c.clientCode }}
              </td>
              <td class="px-4 py-3.5 font-bold text-white">
                <div class="flex items-center gap-1.5">
                  <Building2 class="w-3.5 h-3.5 text-brand-400" />
                  <span>{{ c.name }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5 font-mono text-slate-300">
                {{ c.taxIdentification || c.taxId || 'No registrado' }}
              </td>
              <td class="px-4 py-3.5 text-slate-200">
                <div class="flex items-center gap-1.5">
                  <User class="w-3.5 h-3.5 text-slate-400" />
                  <span>{{ c.contactName || c.contactPerson || 'Sin contacto directo' }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5 space-y-0.5">
                <div v-if="c.email" class="flex items-center gap-1.5 text-slate-300">
                  <Mail class="w-3 h-3 text-slate-400" />
                  <span>{{ c.email }}</span>
                </div>
                <div v-if="c.phone" class="flex items-center gap-1.5 text-slate-400 text-[11px]">
                  <Phone class="w-3 h-3 text-slate-500" />
                  <span>{{ c.phone }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5">
                <BaseBadge v-if="c.status === 'Active'" variant="success">Activo</BaseBadge>
                <BaseBadge v-else variant="danger">Inactivo</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>

    <!-- Create Client Modal -->
    <CreateClientModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="fetchClients"
    />
  </div>
</template>
