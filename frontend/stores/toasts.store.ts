import { defineStore } from 'pinia'

export interface ToastMessage {
  id: string
  type: 'success' | 'error' | 'info' | 'warning'
  title: string
  message?: string
  timeoutMs?: number
}

export const useToastsStore = defineStore('toasts', () => {
  const toasts = ref<ToastMessage[]>([])

  function show(toast: Omit<ToastMessage, 'id'>) {
    const id = Math.random().toString(36).slice(2, 9)
    const newToast: ToastMessage = { id, timeoutMs: 5000, ...toast }
    toasts.value.push(newToast)

    if (newToast.timeoutMs && newToast.timeoutMs > 0) {
      setTimeout(() => {
        remove(id)
      }, newToast.timeoutMs)
    }
  }

  function success(title: string, message?: string) {
    show({ type: 'success', title, message })
  }

  function error(title: string, message?: string) {
    show({ type: 'error', title, message, timeoutMs: 7000 })
  }

  function info(title: string, message?: string) {
    show({ type: 'info', title, message })
  }

  function warning(title: string, message?: string) {
    show({ type: 'warning', title, message })
  }

  function remove(id: string) {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }

  return {
    toasts,
    show,
    success,
    error,
    info,
    warning,
    remove
  }
})
