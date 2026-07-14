<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { ApiService } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';

  let isProcessing = $state(true);
  let errorMsg = $state('');

  onMount(async () => {
    // Read the provider from the state parameter instead of the URL path
    const provider = $page.url.searchParams.get('state');
    const code = $page.url.searchParams.get('code');
    const redirectUri = `${window.location.origin}/auth/callback`;

    if (!code || !provider)
    {
      errorMsg = 'OAuth bilgileri eksik veya hatalı.';
      isProcessing = false;
      return;
    }

    try
    {
      const res = await ApiService.oauthLogin(provider, code, redirectUri);
      
      if (res.requiresTwoFactor)
      {
        window.location.href = `/login?tempToken=${res.tempToken}&email=oauth_user`;
      }
      else
      {
        authStore.login(res.token || '');
        window.location.href = '/';
      }
    }
    catch (err)
    {
      console.error('OAuth Login Error:', err);
      errorMsg = 'Giriş başarısız oldu.';
      isProcessing = false;
    }
  });
</script>

<div class="min-h-screen bg-social-bg flex flex-col items-center justify-center p-4">
  <div class="auth-container w-full max-w-[350px] bg-social-card border border-social-border rounded-lg p-8 flex flex-col items-center shadow-sm relative z-10 text-center">
    {#if isProcessing}
      <span class="w-8 h-8 border-4 border-social-accent border-t-transparent rounded-full animate-spin mb-4"></span>
      <h2 class="font-bold text-xl text-social-primary mb-2">Onaylanıyor...</h2>
      <p class="text-sm text-social-secondary">Lütfen bekleyin, giriş işleminiz tamamlanıyor.</p>
    {:else if errorMsg}
      <div class="text-6xl mb-4">❌</div>
      <h2 class="font-bold text-xl text-social-primary mb-2">Giriş Başarısız</h2>
      <p class="text-sm text-social-danger mb-6">{errorMsg}</p>
      <a href="/login" class="w-full bg-social-accent hover:bg-social-accent-hover text-white font-semibold text-sm rounded py-2.5 transition-colors flex items-center justify-center">
        Giriş Sayfasına Dön
      </a>
    {/if}
  </div>
</div>
