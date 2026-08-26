import type { RouteRecordRaw } from 'vue-router'
import CompaniesPage from './pages/CompaniesPage.vue'

export const companyRoutes: RouteRecordRaw[] = [
  { path: '/superadmin/companies', name: 'companies', component: CompaniesPage },
]

