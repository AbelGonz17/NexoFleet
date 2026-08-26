import { createRouter, createWebHistory } from 'vue-router'
import { authRoutes } from '../../modules/auth/routes'
import { companyRoutes } from '../../modules/companies/routes'
import { dashboardRoutes } from '../../modules/dashboard/routes'
import { useAuthStore } from '../../modules/auth/authStore'
import { userRoles, type UserRole } from '../../modules/auth/types'

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

function homeForRoles(roles: UserRole[] = []) {
  if (roles.includes(userRoles.superAdmin)) return '/superadmin'
  if (roles.includes(userRoles.administrator)) return '/admin'
  return '/employee'
}

router.beforeEach(async (to) => {
  const authStore = useAuthStore()
  await authStore.initialize()

  if (to.meta.guestOnly && authStore.isAuthenticated) {
    return homeForRoles(authStore.user?.roles)
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  const roles = to.meta.roles as UserRole[] | undefined
  if (roles?.length && !authStore.hasAnyRole(roles)) {
    return homeForRoles(authStore.user?.roles)
  }
})

export default router
