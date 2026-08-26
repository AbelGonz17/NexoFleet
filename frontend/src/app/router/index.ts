import { createRouter, createWebHistory } from 'vue-router'
import { authRoutes } from '../../modules/auth/routes'
import { companyRoutes } from '../../modules/companies/routes'
import { dashboardRoutes } from '../../modules/dashboard/routes'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/login' },
    ...authRoutes,
    ...dashboardRoutes,
    ...companyRoutes,
    { path: '/:pathMatch(.*)*', redirect: '/login' },
  ],
})

export default router

