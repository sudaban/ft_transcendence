<script lang="ts">
  import { page } from '$app/stores';

  let navItems = [
    { icon: '🌏', label: 'Home', href: '/' },
    { icon: '🗨️', label: 'Messages', href: '/chat' },
    { icon: '🧠', label: 'AI', href: '/ai' }
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

</aside>
