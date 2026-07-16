export interface Toast {
  id: number;
  message: string;
  type: 'error' | 'warning' | 'success';
}

function createToastStore() {
  let toasts = $state<Toast[]>([]);

  function add(message: string, type: 'error' | 'warning' | 'success' = 'error', duration = 4000) {
    const id = Date.now() + Math.random();
    toasts.push({ id, message, type });
    
    setTimeout(() => {
      remove(id);
    }, duration);
  }

  function remove(id: number) {
    const index = toasts.findIndex(t => t.id === id);
    if (index !== -1) {
      toasts.splice(index, 1);
    }
  }

  return {
    get toasts() {
      return toasts;
    },
    error: (msg: string) => add(msg, 'error'),
    warn: (msg: string) => add(msg, 'warning'),
    success: (msg: string) => add(msg, 'success'),
    remove
  };
}

export const toastStore = createToastStore();
