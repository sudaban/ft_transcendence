<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { authStore } from '$lib/stores/auth.svelte';
  import { ApiService, API_BASE_URL } from '$lib/api';
  import type { UserDTO } from '$lib/types';
  import Sidebar from '$lib/components/Sidebar.svelte';

  let users: (UserDTO & { isBanned?: boolean })[] = $state([]);
  let isLoading = $state(true);
  let errorMsg = $state('');

  onMount(async () => {
    // Wait a brief moment to ensure authStore initializes from localStorage if necessary
    setTimeout(async () => {
      if (!authStore.isAuthenticated || authStore.user?.role !== 'Admin') {
        goto('/');
        return;
      }

      await fetchUsers();
    }, 100);
  });

  async function fetchUsers() {
    if (!authStore.token) return;
    try {
      isLoading = true;
      const data = await ApiService.getAllUsers(authStore.token);
      // Let's assume some users might be banned. Our API doesn't expose `isBanned` directly yet, 
      // but we will keep track of it if we toggle it here, or we can just keep state locally.
      users = data;
    } catch (err) {
      console.error(err);
      errorMsg = "Kullanıcılar yüklenirken bir hata oluştu.";
    } finally {
      isLoading = false;
    }
  }

  async function toggleBan(targetUser: UserDTO & { isBanned?: boolean }) {
    if (!authStore.token) return;
    const isCurrentlyBanned = !!targetUser.isBanned;
    const confirmMsg = isCurrentlyBanned 
      ? `${targetUser.username} adlı kullanıcının banı kaldırılsın mı?`
      : `${targetUser.username} adlı kullanıcıyı banlamak istediğine emin misin?`;

    if (!confirm(confirmMsg)) return;

    try {
      await ApiService.adminBanUser(targetUser.id, !isCurrentlyBanned, authStore.token);
      targetUser.isBanned = !isCurrentlyBanned;
      // Trigger reactivity
      users = [...users];
    } catch (err) {
      console.error(err);
      alert("Ban işlemi başarısız oldu.");
    }
  }

  async function deleteUser(targetUser: UserDTO) {
    if (!authStore.token) return;
    const confirmMsg = `DİKKAT! ${targetUser.username} adlı hesabı ve tüm verilerini kalıcı olarak silmek istediğinize emin misiniz? Bu işlem geri alınamaz!`;

    if (!confirm(confirmMsg)) return;

    try {
      await ApiService.adminDeleteUser(targetUser.id, authStore.token);
      users = users.filter(u => u.id !== targetUser.id);
    } catch (err) {
      console.error(err);
      alert("Kullanıcı silinemedi.");
    }
  }

</script>

<svelte:head>
  <title>Admin Paneli / Transcendence</title>
</svelte:head>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden">
  <Sidebar />

  <main class="flex-1 overflow-y-auto custom-scrollbar flex flex-col pb-20 md:pb-0">
    <div class="p-6 md:p-12 lg:p-16 max-w-6xl w-full mx-auto">
      
      <header class="mb-12 flex flex-col gap-2">
        <div class="flex items-center gap-4">
          <span class="text-4xl">🛡️</span>
          <h1 class="text-3xl md:text-4xl font-bold tracking-tight text-slate-900">Admin Paneli</h1>
        </div>
        <p class="text-slate-500 font-medium ml-14">Sistemdeki tüm kullanıcıları yönetin ve denetleyin.</p>
      </header>

      {#if isLoading}
        <div class="flex items-center justify-center py-20">
          <span class="w-10 h-10 border-4 border-slate-900 border-t-transparent rounded-full animate-spin"></span>
        </div>
      {:else if errorMsg}
        <div class="bg-red-50 text-red-600 p-6 rounded-2xl font-medium shadow-sm border border-red-100 flex items-center gap-3">
          <span class="text-2xl">⚠️</span> {errorMsg}
        </div>
      {:else}
        
        <div class="bg-white rounded-3xl shadow-sm border border-slate-100 overflow-hidden">
          <!-- Desktop Table Header -->
          <div class="hidden md:grid grid-cols-12 gap-4 bg-slate-50 border-b border-slate-100 p-6 text-xs font-bold text-slate-400 uppercase tracking-wider">
            <div class="col-span-4 lg:col-span-3">Kullanıcı</div>
            <div class="col-span-3 lg:col-span-3">İstatistikler</div>
            <div class="col-span-2 lg:col-span-3 text-center">Durum</div>
            <div class="col-span-3 lg:col-span-3 text-right">Aksiyonlar</div>
          </div>

          <div class="divide-y divide-slate-100">
            {#each users as targetUser}
              <!-- Row -->
              <div class="grid grid-cols-1 md:grid-cols-12 gap-4 items-center p-6 hover:bg-slate-50/50 transition-colors">
                
                <!-- User Info -->
                <div class="col-span-1 md:col-span-4 lg:col-span-3 flex items-center gap-4">
                  <div class="w-12 h-12 rounded-full bg-slate-100 text-slate-600 flex items-center justify-center font-bold text-lg shrink-0 overflow-hidden shadow-sm">
                    {#if targetUser.avatar}
                      <img src={targetUser.avatar.startsWith('http') ? targetUser.avatar : `${API_BASE_URL}${targetUser.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
                    {:else}
                      {targetUser.username.charAt(0).toUpperCase()}
                    {/if}
                  </div>
                  <div class="flex flex-col min-w-0">
                    <span class="font-bold text-slate-900 truncate">{targetUser.fullName || targetUser.username}</span>
                    <a href={`/profile/${targetUser.username}`} class="text-[13px] text-slate-400 hover:text-slate-600 truncate transition-colors">@{targetUser.username}</a>
                  </div>
                </div>

                <!-- Stats -->
                <div class="col-span-1 md:col-span-3 lg:col-span-3 flex md:flex-col gap-4 md:gap-1 text-[13px] text-slate-500 font-mono mt-2 md:mt-0">
                  <div class="flex gap-2 items-center"><span class="text-slate-300">📝</span> {targetUser.postsCount} Post</div>
                  <div class="flex gap-2 items-center"><span class="text-slate-300">👥</span> {targetUser.followersCount} Takipçi</div>
                </div>

                <!-- Status -->
                <div class="col-span-1 md:col-span-2 lg:col-span-3 flex items-center md:justify-center mt-2 md:mt-0">
                  {#if targetUser.isBanned}
                    <span class="px-3 py-1 bg-orange-100 text-orange-700 text-xs font-bold rounded-full">Yasaklı</span>
                  {:else}
                    <span class="px-3 py-1 bg-emerald-100 text-emerald-700 text-xs font-bold rounded-full">Aktif</span>
                  {/if}
                </div>

                <!-- Actions -->
                <div class="col-span-1 md:col-span-3 lg:col-span-3 flex items-center md:justify-end gap-2 mt-4 md:mt-0">
                  {#if targetUser.id !== authStore.user?.id}
                    <button 
                      onclick={() => toggleBan(targetUser)} 
                      class="px-4 py-2 rounded-xl text-[13px] font-bold transition-all shadow-sm flex-1 md:flex-none
                        {targetUser.isBanned 
                          ? 'bg-slate-900 text-white hover:bg-black' 
                          : 'bg-orange-50 text-orange-600 hover:bg-orange-100'}"
                    >
                      {targetUser.isBanned ? 'Banı Kaldır' : 'Banla'}
                    </button>
                    <button 
                      onclick={() => deleteUser(targetUser)} 
                      class="px-4 py-2 bg-red-50 text-red-600 hover:bg-red-100 rounded-xl text-[13px] font-bold transition-all shadow-sm flex-1 md:flex-none"
                    >
                      Sil
                    </button>
                  {:else}
                    <span class="text-xs font-bold text-slate-400 uppercase tracking-widest px-4">Sen</span>
                  {/if}
                </div>

              </div>
            {/each}
            
            {#if users.length === 0 && !isLoading && !errorMsg}
              <div class="p-12 text-center text-slate-400 font-medium">Sistemde henüz kullanıcı bulunmuyor.</div>
            {/if}
          </div>
        </div>

      {/if}
    </div>
  </main>
</div>

<style>
  .custom-scrollbar::-webkit-scrollbar {
    width: 3px;
  }
  .custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: #e2e8f0;
  }
</style>
