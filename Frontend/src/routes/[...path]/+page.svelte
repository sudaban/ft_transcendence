<script lang="ts">
  import gsap from 'gsap';
  import { onMount } from 'svelte';

  onMount(() => {
    if (document.querySelector('.error-content')) {
      gsap.fromTo('.error-content', 
        { opacity: 0, y: 30 }, 
        { opacity: 1, y: 0, duration: 0.8, ease: 'power3.out' }
      );
    }
  });
</script>

<div class="min-h-screen bg-social-bg flex flex-col items-center justify-center p-4 selection:bg-social-accent selection:text-white">
  <div class="error-content text-center flex flex-col items-center">
    
    <div class="animate-pulse text-red-600 mb-2" aria-hidden="true">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="w-16 h-16">
        <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178Z" />
        <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
      </svg>
    </div>

    <div class="text-[120px] font-bold leading-none mb-4" style="font-family: 'Instagram Sans', sans-serif;">404</div>
    <h1 class="text-2xl font-semibold mb-2">Sayfa Bulunamadı</h1>
    <p class="text-social-secondary mb-8 max-w-md mx-auto">
      Aradığın sayfaya ulaşılamıyor. Sayfa kaldırılmış olabilir veya URL'yi daha düzgün yazmayı deneyebilirsin.
    </p>
    <a 
      href="/" 
      class="inline-block bg-social-accent hover:bg-social-accent-hover text-white font-semibold px-6 py-2.5 rounded-lg transition-colors"
    >
      Ana Sayfaya Dön
    </a>
  </div>
</div>
