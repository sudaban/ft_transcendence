<script lang="ts">
  import { onMount } from 'svelte';
  import QRCode from 'qrcode';
  import { ApiService } from '$lib/api';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';

  let is2faEnabled = $state(false);
  let isSettingUp = $state(false);
  let qrCodeDataUrl = $state('');
  let secretKey = $state('');
  
  let verificationCode = $state('');
  let isSubmitting = $state(false);
  let errorMsg = $state('');
  let successMsg = $state('');

  async function startSetup()
  {
    isSettingUp = true;
    errorMsg = '';
    
    try
    {
      // Normalde localStorage'dan token alınır: const token = localStorage.getItem('token') || '';
      const token = "mock_session_jwt_token";
      
      const res = await ApiService.setup2fa(token);
      secretKey = res.secretKey;
      
      // Generate QR Code from URI
      qrCodeDataUrl = await QRCode.toDataURL(res.qrCodeUri, {
        width: 250,
        margin: 2,
        color: {
          dark: '#0f1419',
          light: '#ffffff'
        }
      });
      
    }
    catch (err)
    {
      errorMsg = "2FA kurulumu başlatılamadı.";
      isSettingUp = false;
    }
  }

  async function verifyAndEnable()
  {
    if (verificationCode.length !== 6)
    {
      errorMsg = "Lütfen 6 haneli kodu girin.";
      return;
    }
    
    isSubmitting = true;
    errorMsg = '';
    
    try
    {
      const token = "mock_session_jwt_token";
      await ApiService.enable2fa(verificationCode, token);
      
      is2faEnabled = true;
      isSettingUp = false;
      successMsg = "İki aşamalı doğrulama başarıyla aktifleştirildi!";
    }
    catch (err)
    {
      errorMsg = "Geçersiz kod. Lütfen tekrar deneyin.";
    }
    finally
    {
      isSubmitting = false;
    }
  }
</script>

<div class="min-h-screen bg-social-bg text-social-primary flex justify-center">

  <Sidebar />

  <main class="w-full max-w-[600px] border-r border-social-border min-h-screen pb-20 md:pb-0">
    
    <div class="sticky top-0 bg-[rgba(255,255,255,0.85)] backdrop-blur-md z-10 border-b border-social-border flex items-center px-4 h-14">
      <a href="/profile" class="mr-4 hover:bg-gray-100 p-2 rounded-full transition-colors">←</a>
      <h2 class="font-bold text-xl">Güvenlik Ayarları (2FA)</h2>
    </div>
    
    <div class="p-6">
      
      {#if successMsg}
        <div class="bg-green-50 text-green-700 p-4 rounded-lg mb-6 border border-green-200">
          {successMsg}
        </div>
      {/if}

      <div class="border border-social-border rounded-xl p-6">
        <h3 class="font-bold text-xl mb-2">İki Aşamalı Doğrulama (2FA)</h3>
        
        {#if !is2faEnabled && !isSettingUp}
          <p class="text-social-secondary mb-6 leading-relaxed">
            Hesabınızı yetkisiz erişimlere karşı koruyun. Giriş yaparken şifrenize ek olarak bir güvenlik kodu girmeniz gerekecektir.
          </p>
          <button 
            onclick={startSetup}
            class="bg-social-primary text-white font-bold py-2.5 px-6 rounded-full hover:bg-gray-800 transition-colors"
          >
            Kurulumu Başlat
          </button>
        
        {:else if isSettingUp}
          <p class="text-social-secondary mb-6 leading-relaxed">
            1. Google Authenticator veya benzeri bir uygulamayı indirin.<br>
            2. Aşağıdaki QR kodu okutun veya gizli anahtarı manuel girin.<br>
            3. Uygulamada beliren 6 haneli kodu aşağıya yazın.
          </p>

          <div class="flex flex-col items-center mb-6 bg-gray-50 p-4 rounded-lg border border-gray-200">
            {#if qrCodeDataUrl}
              <img src={qrCodeDataUrl} alt="2FA QR Code" class="rounded-lg shadow-sm mb-4">
            {:else}
              <div class="w-[250px] h-[250px] bg-gray-200 animate-pulse rounded-lg mb-4"></div>
            {/if}
            
            <div class="w-full">
              <p class="text-xs text-social-secondary font-bold uppercase mb-1">Gizli Anahtar (Manuel Kurulum):</p>
              <code class="block bg-white p-2 rounded border border-social-border text-center font-mono tracking-widest text-lg">
                {secretKey || 'Yükleniyor...'}
              </code>
            </div>
          </div>

          <div class="w-full">
            {#if errorMsg}
              <p class="text-red-500 text-sm mb-2">{errorMsg}</p>
            {/if}
            
            <div class="flex gap-2">
              <input 
                type="text" 
                bind:value={verificationCode}
                placeholder="6 haneli kod" 
                maxlength="6"
                class="flex-1 bg-white border border-social-border rounded px-4 py-2.5 outline-none focus:border-social-accent transition-colors text-center text-xl tracking-widest font-mono"
              >
              <button 
                onclick={verifyAndEnable}
                disabled={isSubmitting || verificationCode.length !== 6}
                class="bg-social-accent hover:bg-social-accent-hover text-white font-bold px-6 py-2.5 rounded disabled:opacity-50 transition-colors flex items-center"
              >
                {#if isSubmitting}
                  <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></span>
                {/if}
                Doğrula
              </button>
            </div>
          </div>
          
        {:else if is2faEnabled}
          <div class="flex items-center gap-3 text-green-600 font-bold mb-6">
            <span class="text-2xl">✓</span> İki aşamalı doğrulama aktif.
          </div>
          <button class="border border-red-200 text-red-600 font-bold py-2 px-6 rounded-full hover:bg-red-50 transition-colors">
            Devre Dışı Bırak
          </button>
        {/if}

      </div>
    </div>
  </main>

  <aside class="hidden lg:flex flex-col w-[350px] pl-8 pt-4 h-screen sticky top-0"></aside>

  <MobileNav />

</div>
