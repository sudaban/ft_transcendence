<script lang="ts">
  import { onMount } from 'svelte';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';
  import { authStore } from '$lib/stores/auth.svelte';
  import { ApiService } from '$lib/api';
  import QRCode from 'qrcode';
  import { toastStore } from '$lib/stores/toast.svelte';

  let activeTab = $state('security');
  
  // 2FA States
  let is2FaLoading = $state(false);
  let qrCodeDataUrl = $state('');
  let secretKey = $state('');
  let verifyCode = $state('');
  let isVerifying = $state(false);
  let setupMode = $state(false);
  let is2faEnabled = $state(false);
  let isFetchingStatus = $state(true);
  
  // Theme State
  // Removed dark mode logic

  onMount(async () => {
    if (authStore.isAuthenticated && authStore.user && authStore.token)
    {
      try
      {
        const user = await ApiService.getUserById(authStore.user.id, authStore.token);
        is2faEnabled = user.isTwoFactorEnabled;
      }
      catch (err)
      {
        toastStore.error("Güvenlik durumu alınamadı.");
      }
      finally
      {
        isFetchingStatus = false;
      }
    }
    else
    {
      isFetchingStatus = false;
    }
  });

  async function start2FaSetup()
  {
    if (!authStore.token) return;
    is2FaLoading = true;
    try
    {
      const data = await ApiService.setup2fa(authStore.token);
      secretKey = data.secretKey;
      qrCodeDataUrl = await QRCode.toDataURL(data.qrCodeUri, { margin: 1, width: 200, color: { dark: '#0f172a', light: '#ffffff' } });
      setupMode = true;
    }
    catch (err: any)
    {
      toastStore.error(err.message || "2FA Kurulumu başlatılamadı.");
      alert("2FA Kurulumu başlatılamadı.");
    }
    finally
    {
      is2FaLoading = false;
    }
  }

  async function enable2Fa()
  {
    if (!authStore.token || !verifyCode)
      return;
    isVerifying = true;
    try
    {
      await ApiService.enable2fa(verifyCode, authStore.token);
      alert("Harika! İki Aşamalı Doğrulama başarıyla aktifleştirildi!");
      setupMode = false;
      is2faEnabled = true;
      secretKey = '';
      qrCodeDataUrl = '';
      verifyCode = '';
    }
    catch (err: any)
    {
      toastStore.error(err.message || "2FA aktifleştirilemedi.");
      alert("Girdiğiniz kod hatalı veya süresi dolmuş olabilir. Tekrar deneyin.");
    }
    finally
    {
      isVerifying = false;
    }
  }

  async function disable2Fa()
  {
    if (!authStore.token)
      return;
    if (!confirm("İki Aşamalı Doğrulamayı (2FA) devre dışı bırakmak istediğinize emin misiniz? Güvenliğiniz azalacaktır."))
      return;
    is2FaLoading = true;
    try
    {
      await ApiService.disable2fa(authStore.token);
      alert("2FA başarıyla devre dışı bırakıldı.");
      setupMode = false;
      is2faEnabled = false;
    }
    catch (err: any)
    {
      toastStore.error(err.message || "2FA devre dışı bırakılamadı.");
      alert("2FA devre dışı bırakılamadı.");
    }
    finally
    {
      is2FaLoading = false;
    }
  }
</script>

<svelte:head>
  <title>Settings / Transcendence</title>
</svelte:head>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">
  
  <Sidebar />

  <main class="flex-1 overflow-y-auto flex flex-col md:flex-row pb-20 md:pb-0">
    
    <!-- Settings Nav (Left Sidebar) -->
    <section class="w-full md:w-[280px] lg:w-[320px] md:h-screen md:border-r border-slate-100 bg-white p-6 md:p-8 flex flex-col shrink-0 md:sticky top-0">
      <h1 class="text-2xl font-bold tracking-tight text-slate-900 mb-8">Settings</h1>
      
      <div class="flex flex-col gap-2">
        <button 
          class="text-left px-4 py-3 rounded-xl text-sm font-semibold transition-colors {activeTab === 'general' ? 'bg-slate-900 text-white shadow-sm' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'}"
          onclick={() => activeTab = 'general'}
        >
          Genel Ayarlar
        </button>
        <button 
          class="text-left px-4 py-3 rounded-xl text-sm font-semibold transition-colors {activeTab === 'security' ? 'bg-slate-900 text-white shadow-sm' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'}"
          onclick={() => activeTab = 'security'}
        >
          Güvenlik (2FA)
        </button>
      </div>
    </section>

    <!-- Settings Content -->
    <section class="flex-1 p-6 md:p-10 lg:p-16 max-w-4xl mx-auto md:mx-0 w-full animate-fade-in-up">
      
      {#if activeTab === 'security'}
        <div class="bg-white border border-slate-100 rounded-3xl p-8 lg:p-10 shadow-sm transition-all">
          <div class="flex items-center gap-3 mb-3">
            <span class="w-10 h-10 rounded-full bg-slate-900 flex items-center justify-center text-white text-lg">🛡️</span>
            <h2 class="text-2xl font-bold text-slate-900 tracking-tight">İki Aşamalı Doğrulama</h2>
          </div>
          <p class="text-slate-500 mb-10 leading-relaxed max-w-2xl text-[15px]">Hesabınızı güvende tutmak için ekstra bir güvenlik katmanı ekleyin. Giriş yaparken şifrenize ek olarak telefonunuzdaki uygulamadan (Örn: Google Authenticator) alacağınız dinamik kodu girmeniz gerekecektir.</p>

          {#if isFetchingStatus}
            <div class="flex items-center justify-center p-8">
              <span class="w-8 h-8 border-4 border-slate-900 border-t-transparent rounded-full animate-spin"></span>
            </div>
          {:else if setupMode}
            <div class="flex flex-col md:flex-row gap-10 bg-slate-50/50 p-8 rounded-2xl border border-slate-100 items-start shadow-inner">
              
              {#if qrCodeDataUrl}
                <div class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200 shrink-0 mx-auto md:mx-0 flex flex-col items-center gap-3">
                  <img src={qrCodeDataUrl} alt="2FA QR Code" class="w-48 h-48 rounded-lg" />
                  <span class="text-[10px] font-mono text-slate-400 font-bold tracking-widest uppercase">Taratın</span>
                </div>
              {/if}

              <div class="flex flex-col flex-1 gap-6 w-full">
                <div>
                  <h3 class="font-bold text-slate-900 mb-1 text-[15px]">1. Authenticator Uygulamasını Açın</h3>
                  <p class="text-[13px] text-slate-500 leading-relaxed">Telefonunuza indirdiğiniz Google Authenticator, Authy veya 1Password gibi bir uygulamayı kullanarak yandaki barkodu taratın.</p>
                </div>
                <div>
                  <h3 class="font-bold text-slate-900 mb-1 text-[15px]">2. Doğrulama Kodunu Girin</h3>
                  <p class="text-[13px] text-slate-500 mb-4 leading-relaxed">Uygulamada görünen 6 haneli kodu aşağıya girin. Bu kod sürekli değişeceği için hızlı olmalısınız.</p>
                  <input 
                    type="text" 
                    bind:value={verifyCode} 
                    placeholder="000 000" 
                    maxlength="6" 
                    class="w-full max-w-[240px] bg-white border-2 border-slate-200 rounded-xl px-5 py-3.5 text-xl tracking-[0.25em] font-mono outline-none focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10 transition-all text-center placeholder:tracking-normal placeholder:text-slate-300 shadow-sm"
                  >
                </div>
                <div class="flex flex-wrap gap-3 mt-4 pt-6 border-t border-slate-200/60">
                  <button onclick={() => setupMode = false} class="px-6 py-3 text-[14px] font-semibold text-slate-600 bg-white border border-slate-200 hover:bg-slate-50 hover:text-slate-900 rounded-xl transition-all shadow-sm">
                    İptal Et
                  </button>
                  <button onclick={enable2Fa} disabled={isVerifying || verifyCode.length < 6} class="px-8 py-3 text-[14px] font-bold text-white bg-emerald-500 hover:bg-emerald-600 rounded-xl transition-all disabled:opacity-50 disabled:hover:bg-emerald-500 flex items-center gap-2 shadow-sm shadow-emerald-500/20 hover:shadow-md hover:shadow-emerald-500/30">
                    {#if isVerifying}
                      <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                    {/if}
                    Doğrula ve Aktifleştir
                  </button>
                </div>
              </div>

            </div>
          {:else}
            {#if is2faEnabled}
              <div class="p-6 rounded-2xl bg-emerald-50 border border-emerald-100 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-sm">
                <div class="flex items-center gap-4">
                  <span class="w-12 h-12 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center text-xl">✅</span>
                  <div>
                    <h3 class="font-bold text-slate-900 text-[15px]">İki Aşamalı Doğrulama Aktif</h3>
                    <p class="text-[13px] text-slate-600 mt-1">Hesabınız şu anda ekstra güvenlik katmanıyla korunuyor.</p>
                  </div>
                </div>
                <button onclick={disable2Fa} disabled={is2FaLoading} class="px-6 py-3 text-[14px] font-bold text-red-600 bg-white hover:bg-red-50 border border-red-100 rounded-xl transition-colors disabled:opacity-50 shrink-0">
                  {#if is2FaLoading}
                    <span class="w-4 h-4 border-2 border-red-600 border-t-transparent rounded-full animate-spin inline-block align-middle"></span>
                  {:else}
                    Devre Dışı Bırak
                  {/if}
                </button>
              </div>
            {:else}
              <div class="p-6 rounded-2xl bg-amber-50 border border-amber-100 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-sm">
                <div class="flex items-center gap-4">
                  <span class="w-12 h-12 rounded-full bg-amber-100 text-amber-600 flex items-center justify-center text-xl">⚠️</span>
                  <div>
                    <h3 class="font-bold text-slate-900 text-[15px]">İki Aşamalı Doğrulama Kapalı</h3>
                    <p class="text-[13px] text-slate-600 mt-1">Hesabınızın güvenliğini artırmak için 2FA kurulumunu tamamlayın.</p>
                  </div>
                </div>
                <button onclick={start2FaSetup} disabled={is2FaLoading} class="px-6 py-3 text-[14px] font-bold text-white bg-slate-900 hover:bg-black rounded-xl transition-colors disabled:opacity-50 shrink-0 shadow-md">
                  {#if is2FaLoading}
                    <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin inline-block align-middle"></span>
                  {:else}
                    Kurulumu Başlat
                  {/if}
                </button>
              </div>
            {/if}
          {/if}
        </div>
      {:else if activeTab === 'general'}
        <div class="bg-white border border-slate-100 rounded-3xl p-12 shadow-sm flex flex-col items-center justify-center min-h-[400px] text-center gap-6">
          <div class="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center text-2xl border border-slate-100">
            🪪
          </div>
          <div>
            <h3 class="text-lg font-bold text-slate-900 mb-1">Genel Ayarlar</h3>
            <p class="text-slate-500 text-[14px] max-w-sm">Ad, soyad veya biyografi gibi profil bilgilerinizi düzenlemek için lütfen doğrudan Profil sayfanızı ziyaret edin.</p>
          </div>
          <a href="/profile" class="mt-2 px-6 py-2.5 bg-slate-900 text-white text-[13px] font-bold rounded-full hover:bg-black transition-colors shadow-sm">
            Profile Git
          </a>

        </div>
      {/if}

      <!-- Mobile Logout Button -->
      <div class="md:hidden mt-8 w-full flex justify-center">
        <button onclick={() => authStore.logout()} class="w-full max-w-sm flex items-center justify-center gap-2 px-6 py-3.5 bg-red-50 text-red-600 font-bold text-[14px] rounded-2xl hover:bg-red-100 transition-colors">
          <span class="text-xl">🚪</span> Çıkış Yap
        </button>
      </div>

      <!-- Mobile Legal Footer -->
      <div class="md:hidden mt-12 mb-8 flex flex-col items-center gap-3 text-[13px] text-slate-500 font-medium">
        <div class="flex gap-6">
          <a href="/terms" class="hover:text-slate-900 transition-colors">Hizmet Koşulları</a>
          <a href="/privacy" class="hover:text-slate-900 transition-colors">Gizlilik Politikası</a>
        </div>
        <span class="text-[11px] text-slate-400">© 2026 Transcendence</span>
      </div>
    </section>
  </main>
  
  <MobileNav />

</div>
