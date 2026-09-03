<script setup lang="ts">
import { Users, Plus } from 'lucide-vue-next'
import BaseCard from '~/components/common/BaseCard.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import type { EmployeeResponse } from '~/types/api.types'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Personal y Conductores'
})

const api = useApi()
const employees = ref<EmployeeResponse[]>([])
const loading = ref(true)

async function fetchEmployees() {
  loading.value = true
  try {
    employees.value = await api.get<EmployeeResponse[]>('/v1/employees')
  } catch {
    // Handled by useApi
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchEmployees()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-black text-white tracking-tight">Personal y Conductores</h2>
        <p class="text-xs text-slate-400 mt-1">Directorio de colaboradores, asignaciones y estado de contratación.</p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="primary" size="md">
          <Plus class="w-4 h-4" />
          <span>Registrar Empleado</span>
        </BaseButton>
      </div>
    </div>

    <BaseCard padding="none">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-950/40 text-slate-400 border-b border-slate-800 text-[11px] uppercase tracking-wider font-semibold">
            <tr>
              <th class="px-6 py-3.5">Código</th>
              <th class="px-6 py-3.5">Nombre Completo</th>
              <th class="px-6 py-3.5">Documento Identidad</th>
              <th class="px-6 py-3.5">Contacto</th>
              <th class="px-6 py-3.5">Fecha Contratación</th>
              <th class="px-6 py-3.5">Estado</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            <tr v-if="loading" class="text-center py-12 text-slate-400">
              <td colspan="6" class="py-8 text-slate-500 font-medium">Cargando personal...</td>
            </tr>
            <tr v-else-if="employees.length === 0" class="text-center py-12">
              <td colspan="6" class="py-8 text-slate-500 font-medium">No hay empleados registrados.</td>
            </tr>
            <tr v-for="emp in employees" :key="emp.id" class="hover:bg-slate-800/30 transition-colors">
              <td class="px-6 py-4 font-bold text-white">{{ emp.employeeCode }}</td>
              <td class="px-6 py-4 font-semibold text-slate-200">{{ emp.fullName.firstName }} {{ emp.fullName.lastName }}</td>
              <td class="px-6 py-4 text-slate-300 font-mono">{{ emp.identityDocument }}</td>
              <td class="px-6 py-4">
                <p class="text-slate-300">{{ emp.phone }}</p>
                <p class="text-[10px] text-slate-500">{{ emp.email }}</p>
              </td>
              <td class="px-6 py-4 text-slate-400">{{ emp.hiredOn }}</td>
              <td class="px-6 py-4">
                <BaseBadge :variant="emp.status === 'Active' ? 'success' : emp.status === 'Suspended' ? 'warning' : 'danger'" size="sm">
                  {{ emp.status }}
                </BaseBadge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>
