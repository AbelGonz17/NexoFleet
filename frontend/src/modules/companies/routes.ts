import type { RouteRecordRaw } from 'vue-router'
import CompaniesPage from './pages/CompaniesPage.vue'
import { userRoles } from '../auth/types'

export const companyRoutes: RouteRecordRaw[] = [
  {
    path: '/superadmin/companies',
    name: 'companies',
    component: CompaniesPage,
    meta: { requiresAuth: true, roles: [userRoles.superAdmin] },
  },
]
