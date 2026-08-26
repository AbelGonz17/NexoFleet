import type { RouteRecordRaw } from 'vue-router'
import DashboardPage from './pages/DashboardPage.vue'
import RoleDashboardPage from './pages/RoleDashboardPage.vue'
import { userRoles } from '../auth/types'

export const dashboardRoutes: RouteRecordRaw[] = [
  {
    path: '/superadmin',
    name: 'superadmin-dashboard',
    component: DashboardPage,
    meta: { requiresAuth: true, roles: [userRoles.superAdmin] },
  },
  {
    path: '/admin',
    name: 'admin-dashboard',
    component: RoleDashboardPage,
    meta: { requiresAuth: true, roles: [userRoles.administrator] },
  },
  {
    path: '/employee',
    name: 'employee-dashboard',
    component: RoleDashboardPage,
    meta: { requiresAuth: true, roles: [userRoles.employee] },
  },
]
