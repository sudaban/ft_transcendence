<script lang="ts">
  import { page } from '$app/stores';

  import { authStore } from '$lib/stores/auth.svelte';

  let navItems = $derived([
    { icon: '🌏', label: 'Ana Sayfa', href: '/' },
    { icon: '🗨️', label: 'Sohbet', href: '/chat' },
    { icon: '🪪', label: 'Profil', href: '/profile' },
    { icon: '⚙️', label: 'Ayarlar', href: '/settings' },
    ...(authStore.user?.role === 'Admin' ? [{ icon: '🛡️', label: 'Admin', href: '/admin' }] : [])
  ]);

  let pathname = $derived($page.url.pathname);
</script>

<div class="md:hidden fixed bottom-0 left-0 right-0 h-14 bg-social-bg border-t border-social-border flex items-center justify-around z-50">
  {#each navItems as item}
    {@const isActive = pathname === item.href}
    <a href={item.href} class="flex items-center justify-center w-full h-full">
      <span class="text-2xl text-social-primary transition-transform {isActive ? 'scale-110' : ''}">
        {item.icon}
      </span>
    </a>
  {/each}
</div>
