<script lang="ts">
  import { fade, scale } from 'svelte/transition';
  import type { UserDTO } from '$lib/types';
  import { API_BASE_URL } from '$lib/api';

  let { 
    isOpen = $bindable(false), 
    title = '', 
    users = [] as UserDTO[],
    onclose 
  } = $props<{
    isOpen: boolean;
    title: string;
    users: UserDTO[];
    onclose: () => void;
  }>();

  function close()
  {
    isOpen = false;
    if (onclose) onclose();
  }
</script>

{#if isOpen}
  <!-- svelte-ignore a11y_click_events_have_key_events -->
  <!-- svelte-ignore a11y_no_static_element_interactions -->
  <div 
    class="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex justify-center items-center p-4"
    transition:fade={{ duration: 200 }}
    onclick={close}
  >
    <div 
      class="bg-white w-full max-w-[400px] max-h-[80vh] rounded-2xl shadow-2xl flex flex-col overflow-hidden"
      transition:scale={{ duration: 300, start: 0.95, opacity: 0 }}
      onclick={(e) => e.stopPropagation()}
    >
      <!-- Header -->
      <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-white sticky top-0 z-10">
        <h3 class="font-bold text-lg text-slate-900">{title}</h3>
        <button 
          onclick={close}
          class="w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-100 text-slate-500 transition-colors"
        >
          ✕
        </button>
      </div>

      <!-- User List -->
      <div class="overflow-y-auto custom-scrollbar flex-1 p-2">
        {#if users.length === 0}
          <div class="py-12 text-center text-slate-500 flex flex-col items-center justify-center gap-2">
            <span class="text-4xl">😵</span>
            <p class="text-sm">Burada kimsecikler yok.</p>
          </div>
        {:else}
          <div class="flex flex-col">
            {#each users as user}
              <a 
                href="/profile/{user.username}" 
                onclick={close}
                class="flex items-center gap-3 p-3 hover:bg-slate-50 rounded-xl transition-colors group"
              >
                <!-- Avatar -->
                <div class="w-12 h-12 rounded-full bg-slate-200 flex items-center justify-center font-bold text-slate-600 overflow-hidden shrink-0 border border-slate-100 group-hover:border-slate-300 transition-colors">
                  {#if user.avatar && user.avatar.length > 3}
                    <img src={user.avatar.startsWith('http') ? user.avatar : `${API_BASE_URL}${user.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
                  {:else}
                    {user.username.substring(0, 2).toUpperCase()}
                  {/if}
                </div>
                
                <!-- Info -->
                <div class="flex flex-col flex-1 min-w-0">
                  <span class="font-bold text-sm text-slate-900 truncate group-hover:text-blue-600 transition-colors">{user.username}</span>
                  <span class="text-xs text-slate-500 truncate">{user.handle || '@' + user.username}</span>
                </div>
                
                <!-- Action -->
                <div class="shrink-0 text-slate-400 group-hover:text-blue-500 transition-colors pr-2">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                  </svg>
                </div>
              </a>
            {/each}
          </div>
        {/if}
      </div>
    </div>
  </div>
{/if}

<style>
  .custom-scrollbar::-webkit-scrollbar
  {
    width: 6px;
  }
  .custom-scrollbar::-webkit-scrollbar-track
  {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb
  {
    background-color: #cbd5e1;
    border-radius: 20px;
  }
</style>
