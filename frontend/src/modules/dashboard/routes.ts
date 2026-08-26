import type { RouteRecordRaw } from 'vue-router'
import DashboardPage from './pages/DashboardPage.vue'

export const dashboardRoutes: RouteRecordRaw[] = [
  { path: '/superadmin', name: 'superadmin-dashboard', component: DashboardPage },
]

