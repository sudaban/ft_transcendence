<script lang="ts">
  import { page } from '$app/stores';

  let navItems = [
    { icon: '🌏', label: 'Home', href: '/' },
    { icon: '🗨️', label: 'Messages', href: '/chat' },
    { icon: '🪪', label: 'Profile', href: '/profile' },
    { icon: '⚙️', label: 'Settings', href: '/settings' }
  ];

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
