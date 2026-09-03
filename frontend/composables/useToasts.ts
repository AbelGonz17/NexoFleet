import { useToastsStore } from '~/stores/toasts.store'

export function useToasts() {
  const store = useToastsStore()
  return {
    toasts: computed(() => store.toasts),
    show: store.show,
    success: store.success,
    error: store.error,
    info: store.info,
    warning: store.warning,
    remove: store.remove
  }
}
