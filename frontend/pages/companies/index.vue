<script setup lang="ts">
import { Building2 } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { CompanyResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Configuración de Empresa'
})

const api = useApi()
const companies = ref<CompanyResponse[]>([])
const loading = ref(true)

async function fetchCompanies() {
  loading.value = true
  try {
    companies.value = await api.get<CompanyResponse[]>('/v1/companies')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchCompanies()
})
</script>

<template>
  <div class="space-y-6">
    <div>
      <h2 class="text-2xl font-black text-white tracking-tight">Empresas del Sistema</h2>
      <p class="text-xs text-slate-400 mt-1">Configuración corporativa, identificación fiscal y estado operativo de la empresa.</p>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Razón Social</th>
              <th class="px-6 py-3.5">RIF / Tax ID</th>
              <th class="px-6 py-3.5">Contacto</th>
              <th class="px-6 py-3.5">Ubicación</th>
              <th class="px-6 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="5" class="py-8 text-slate-500 font-medium">Cargando datos de empresa...</td>
            </tr>
            <tr v-else-if="companies.length === 0" class="text-center py-12">
              <td colspan="5" class="py-8 text-slate-500 font-medium">No hay empresas registradas.</td>
            </tr>
            <tr v-for="comp in companies" :key="comp.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white">{{ comp.name }}</td>
              <td class="px-6 py-4 text-slate-300 font-mono">{{ comp.taxId }}</td>
              <td class="px-6 py-4">
                <p class="text-slate-200">{{ comp.email }}</p>
                <p class="text-[10px] text-slate-500">{{ comp.phone }}</p>
              </td>
              <td class="px-6 py-4 text-slate-400">{{ comp.address?.city }}, {{ comp.address?.state }}</td>
              <td class="px-6 py-4">
                <BaseBadge :variant="comp.status === 'Active' ? 'success' : 'danger'" size="sm">{{ comp.status }}</BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
