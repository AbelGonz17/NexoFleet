<script setup lang="ts">
import {
  User,
  Mail,
  Phone,
  CreditCard,
  Calendar,
  ShieldCheck,
  Building2,
  Clock,
  UserCheck,
  CheckCircle2,
  AlertCircle
} from 'lucide-vue-next'
import BaseModal from '~/components/common/BaseModal.vue'
import BaseBadge from '~/components/common/BaseBadge.vue'
import BaseButton from '~/components/common/BaseButton.vue'
import type { EmployeeResponse } from '~/types/api.types'

const props = defineProps<{
  isOpen: boolean
  employee: EmployeeResponse | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'edit'): void
  (e: 'toggle-status'): void
}>()

function getFullName(emp: EmployeeResponse | null): string {
  if (!emp) return ''
  if (typeof emp.fullName === 'string' && emp.fullName) return emp.fullName
  if (emp.firstName && emp.lastName) return `${emp.firstName} ${emp.lastName}`
  if (typeof emp.fullName === 'object' && emp.fullName) {
    return `${(emp.fullName as any).firstName || ''} ${(emp.fullName as any).lastName || ''}`.trim()
  }
  return emp.employeeCode || 'Empleado'
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Detalles del Empleado / Conductor" @close="$emit('close')">
    <div v-if="employee" class="space-y-6">
      <!-- Profile Header -->
      <div class="flex items-center gap-4 bg-slate-900/60 p-4 rounded-2xl border border-slate-800">
        <div class="w-14 h-14 rounded-2xl bg-gradient-to-tr from-brand-600 to-indigo-500 flex items-center justify-center text-white text-xl font-black shadow-lg shadow-brand-500/20 shrink-0">
          {{ employee.firstName?.charAt(0) || employee.employeeCode?.charAt(0) || 'E' }}
        </div>
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2">
            <h3 class="text-base font-bold text-white truncate">{{ getFullName(employee) }}</h3>
            <BaseBadge v-if="employee.status === 'Active'" variant="success">Activo</BaseBadge>
            <BaseBadge v-else-if="employee.status === 'Suspended'" variant="warning">Suspendido</BaseBadge>
            <BaseBadge v-else variant="danger">{{ employee.status }}</BaseBadge>
          </div>
          <p class="text-xs text-brand-400 font-mono mt-0.5">{{ employee.employeeCode }}</p>
        </div>
      </div>

      <!-- Information Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <!-- Identity Document -->
        <div class="bg-slate-900/40 border border-slate-800/80 rounded-xl p-3.5 space-y-1">
          <div class="flex items-center gap-1.5 text-slate-400 text-xs font-semibold">
            <CreditCard class="w-3.5 h-3.5 text-brand-400" />
            <span>Documento de Identidad</span>
          </div>
          <p class="text-sm font-bold text-white font-mono">{{ employee.identityDocument }}</p>
        </div>

        <!-- Hire Date -->
        <div class="bg-slate-900/40 border border-slate-800/80 rounded-xl p-3.5 space-y-1">
          <div class="flex items-center gap-1.5 text-slate-400 text-xs font-semibold">
            <Calendar class="w-3.5 h-3.5 text-indigo-400" />
            <span>Fecha de Contratación</span>
          </div>
          <p class="text-sm font-bold text-white font-mono">{{ employee.hireDate || employee.hiredOn || 'No registrada' }}</p>
        </div>

        <!-- Email -->
        <div class="bg-slate-900/40 border border-slate-800/80 rounded-xl p-3.5 space-y-1">
          <div class="flex items-center gap-1.5 text-slate-400 text-xs font-semibold">
            <Mail class="w-3.5 h-3.5 text-emerald-400" />
            <span>Correo Electrónico</span>
          </div>
          <p class="text-xs font-medium text-white truncate">{{ employee.email }}</p>
        </div>

        <!-- Phone -->
        <div class="bg-slate-900/40 border border-slate-800/80 rounded-xl p-3.5 space-y-1">
          <div class="flex items-center gap-1.5 text-slate-400 text-xs font-semibold">
            <Phone class="w-3.5 h-3.5 text-cyan-400" />
            <span>Teléfono Móvil</span>
          </div>
          <p class="text-xs font-medium text-white">{{ employee.phone }}</p>
        </div>
      </div>

      <!-- App Account Integration Banner -->
      <div class="p-3.5 rounded-xl border flex items-center gap-3"
        :class="employee.userId
          ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-300'
          : 'bg-slate-900/40 border-slate-800 text-slate-400'"
      >
        <UserCheck v-if="employee.userId" class="w-5 h-5 text-emerald-400 shrink-0" />
        <AlertCircle v-else class="w-5 h-5 text-slate-500 shrink-0" />
        <div class="text-xs flex-1">
          <p class="font-bold text-white">
            {{ employee.userId ? 'Cuenta de Acceso Móvil Vinculada' : 'Sin Cuenta de Acceso Móvil' }}
          </p>
          <p class="text-[11px] text-slate-400 mt-0.5">
            {{ employee.userId ? 'El conductor puede iniciar sesión en la app de conductores para registrar despachos.' : 'Este empleado no tiene credenciales de acceso a la app móvil.' }}
          </p>
        </div>
      </div>

      <!-- Actions Footer -->
      <div class="flex items-center justify-between pt-4 border-t border-slate-800">
        <BaseButton
          type="button"
          :variant="employee.status === 'Active' ? 'danger' : 'success'"
          size="sm"
          @click="$emit('toggle-status')"
        >
          {{ employee.status === 'Active' ? 'Suspender Empleado' : 'Reactivar Empleado' }}
        </BaseButton>

        <div class="flex items-center gap-2">
          <BaseButton type="button" variant="secondary" size="sm" @click="$emit('close')">
            Cerrar
          </BaseButton>
          <BaseButton type="button" variant="primary" size="sm" @click="$emit('edit')">
            Editar Perfil
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
