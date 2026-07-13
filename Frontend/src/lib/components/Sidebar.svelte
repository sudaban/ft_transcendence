<script lang="ts">
  import { page } from '$app/stores';
  import { authStore } from '$lib/stores/auth.svelte';
  import { API_BASE_URL } from '$lib/api';

  let navItems = [
    { icon: '🌏', label: 'Ana Sayfa', href: '/' },
    { icon: '🗨️', label: 'Sohbet', href: '/chat' },
    { icon: '🪪', label: 'Profil', href: '/profile' },
    { icon: '⚙️', label: 'Ayarlar', href: '/settings' }
  ];

  let pathname = $derived($page.url.pathname);
</script>

<aside class="hidden md:flex flex-col w-20 xl:w-64 h-screen pt-4 pb-6 px-3 xl:px-4 shrink-0 z-10 sticky top-0 border-r border-social-border">
  
  <!-- Logo -->
  <div class="mb-4 p-3 flex items-center justify-center xl:justify-start">
    <div class="w-10 h-10 flex items-center justify-center text-social-primary font-bold text-2xl hover:bg-gray-100 rounded-full cursor-pointer transition-colors">
      T
    </div>
  </div>
  
  <!-- Nav -->
  <nav class="flex-1 flex flex-col gap-1">
    {#each navItems as item}
      {@const isActive = pathname === item.href}
      <a 
        href={item.href}
        class="flex items-center justify-center xl:justify-start gap-4 p-3 rounded-full hover:bg-gray-100 transition-colors w-max"
      >
        <span class="text-2xl text-social-primary">{item.icon}</span>
        <span class="hidden xl:block text-xl {isActive ? 'font-bold' : 'font-normal'} text-social-primary">
          {item.label}
        </span>
      </a>
    {/each}
  </nav>

  <!-- User Profile & Auth -->
  <div class="mt-auto pt-4 flex flex-col gap-2 w-full items-center xl:items-stretch">
    {#if authStore.isAuthenticated && authStore.user}
      <a href="/profile" class="flex items-center justify-center xl:justify-start gap-3 p-3 rounded-full hover:bg-gray-100 transition-colors cursor-pointer w-max xl:w-full">
        <div class="w-10 h-10 rounded-full bg-social-accent text-white flex items-center justify-center font-bold text-lg shrink-0 overflow-hidden">
          {#if authStore.user.avatar}
            <img src={authStore.user.avatar.startsWith('http') ? authStore.user.avatar : `${API_BASE_URL}${authStore.user.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
          {:else}
            {authStore.user.username.charAt(0).toUpperCase()}
          {/if}
        </div>
        <div class="hidden xl:flex flex-col overflow-hidden">
          <span class="text-sm font-bold text-social-primary truncate">{authStore.user.username}</span>
          <span class="text-xs text-social-secondary truncate">{authStore.user.email}</span>
        </div>
      </a>
      <button onclick={() => authStore.logout()} class="text-red-500 hover:bg-red-50 p-3 rounded-full transition-colors flex items-center justify-center xl:justify-start gap-3 w-max xl:w-full">
        <span class="text-xl">🚪</span>
        <span class="hidden xl:block font-bold">Çıkış Yap</span>
      </button>
    {:else}
      <a href="/login" class="bg-social-accent text-white font-bold p-3 rounded-full transition-colors hover:bg-social-accent-hover flex items-center justify-center w-10 h-10 xl:w-full xl:h-auto gap-2">
        <span class="text-xl xl:hidden">🔑</span>
        <span class="hidden xl:block">Giriş Yap</span>
      </a>
    {/if}
  </div>

  <!-- Footer links for PP/ToS -->
  <div class="hidden xl:flex flex-wrap gap-x-3 gap-y-2 mt-4 pb-2 px-4 text-[13px] text-social-secondary/70">
    <a href="/terms" class="hover:underline">Hizmet Koşulları</a>
    <a href="/privacy" class="hover:underline">Gizlilik Politikası</a>
    <span>© 2026 Transcendence</span>
  </div>

</aside>
