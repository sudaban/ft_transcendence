<script lang="ts">
  import { toastStore } from '$lib/stores/toast.svelte';
  import { fade, slide } from 'svelte/transition';
</script>

<div class="fixed top-4 right-4 z-[9999] flex flex-col gap-2 pointer-events-none">
  {#each toastStore.toasts as toast (toast.id)}
    <div 
      transition:slide={{ duration: 200 }}
      class="pointer-events-auto min-w-[300px] max-w-sm px-4 py-3 rounded-xl shadow-xl flex items-start justify-between gap-3 text-sm font-semibold border backdrop-blur-md
      {toast.type === 'error' ? 'bg-red-50/90 text-red-700 border-red-200' : 
       toast.type === 'warning' ? 'bg-yellow-50/90 text-yellow-700 border-yellow-200' : 
       'bg-emerald-50/90 text-emerald-700 border-emerald-200'}"
    >
      <div class="flex items-start gap-2 pt-0.5">
        <span class="shrink-0 text-lg">
          {#if toast.type === 'error'}
            🚨
          {:else if toast.type === 'warning'}
            ⚠️
          {:else}
            ✅
          {/if}
        </span>
        <span class="leading-relaxed">{toast.message}</span>
      </div>
      <button onclick={() => toastStore.remove(toast.id)} class="text-xl opacity-50 hover:opacity-100 transition-opacity shrink-0">
        &times;
      </button>
    </div>
  {/each}
</div>
