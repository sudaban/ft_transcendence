<script lang="ts">
  import { onMount } from 'svelte';
  import gsap from 'gsap';
  import { spring } from 'svelte/motion';
  import { ApiService } from '$lib/api';

  let username = $state('');
  let email = $state('');
  let password = $state('');
  let confirmPassword = $state('');
  let errorMsg = $state('');
  let isSubmitting = $state(false);
  let agreedToTerms = $state(false);

  let mouseX = $state(0);
  let mouseY = $state(0);
  let eyeRect = $state({ left: 0, top: 0, width: 0, height: 0 });
  let eyeElement: HTMLElement;

  onMount(() => {
    if (document.querySelector('.auth-container'))
    {
      gsap.fromTo('.auth-container', 
        { opacity: 0, y: 30 }, 
        { opacity: 1, y: 0, duration: 0.8, ease: 'power3.out' }
      );
    }
    
    if (eyeElement)
    {
      eyeRect = eyeElement.getBoundingClientRect();
    }
    
    const handleResize = () => {
      if (eyeElement)
        eyeRect = eyeElement.getBoundingClientRect();
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  });

  function handleMouseMove(e: MouseEvent)
  {
    mouseX = e.clientX;
    mouseY = e.clientY;
  }

  let targetPos = $derived.by(() => {
    if (!eyeRect.width)
      return { x: 0, y: 0 };
    
    const centerX = eyeRect.left + eyeRect.width / 2;
    const centerY = eyeRect.top + eyeRect.height / 2;
    
    let dx = (mouseX - centerX) * 0.05;
    let dy = (mouseY - centerY) * 0.05;
    
    const distance = Math.sqrt(dx * dx + dy * dy);
    const maxRadius = 10;
    
    if (distance > maxRadius)
    {
      dx = (dx / distance) * maxRadius;
      dy = (dy / distance) * maxRadius;
    }
    
    return { x: dx, y: dy };
  });

  let pupilSpring = spring({ x: 0, y: 0 }, {
    stiffness: 0.1,
    damping: 0.4
  });

  $effect(() => {
    pupilSpring.set(targetPos);
  });

  async function handleRegister(e: Event)
  {
    e.preventDefault();
    if (!username || !email || !password || !confirmPassword)
    {
      errorMsg = "Lütfen tüm alanları doldurun.";
      triggerErrorAnimation();
      return;
    }
    if (password !== confirmPassword)
    {
      errorMsg = "Şifreler birbiriyle eşleşmiyor!";
      triggerErrorAnimation();
      return;
    }
    if (password.length < 6)
    {
      errorMsg = "Şifreniz en az 6 karakter olmalıdır.";
      triggerErrorAnimation();
      return;
    }
    
    errorMsg = "";
    isSubmitting = true;
    
    try
    {
      const res = await ApiService.register({ username, email, password });
      window.location.href = '/login';
    }
    catch (err)
    {
      errorMsg = "Kayıt olurken bir hata oluştu.";
      triggerErrorAnimation();
    }
    finally
    {
      isSubmitting = false;
    }
  }

  function triggerErrorAnimation()
  {
    if (document.querySelector('.auth-container'))
    {
      gsap.fromTo('.auth-container', 
        { x: -8 }, 
        { x: 8, duration: 0.1, yoyo: true, repeat: 3, onComplete: () => {
          if (document.querySelector('.auth-container'))
          {
            gsap.to('.auth-container', {x: 0, duration: 0.1});
          }
        }}
      );
    }
  }
</script>

<svelte:window onmousemove={handleMouseMove} />

<div class="min-h-screen bg-social-bg flex flex-col items-center justify-center p-4 selection:bg-social-accent selection:text-white py-12">
  
  <div class="auth-container w-full max-w-[350px] bg-social-card border border-social-border rounded-lg p-8 flex flex-col items-center shadow-sm relative z-10">
    
    <div 
      bind:this={eyeElement}
      class="mb-6 relative w-28 h-32 rounded-[50px] bg-gray-50 border-[3px] border-social-border flex flex-col items-center justify-center shadow-inner overflow-hidden transition-colors hover:border-social-secondary"
    >
      <div class="absolute inset-0 border-t-4 border-social-accent rounded-[50px] opacity-10"></div>
      
      <div class="relative w-16 h-16 rounded-full bg-white border-[2px] border-social-border flex items-center justify-center shadow-inner mb-2">
        <div 
          class="w-7 h-7 bg-social-primary rounded-full relative"
          style="transform: translate({$pupilSpring.x}px, {$pupilSpring.y}px);"
        >
          <div class="absolute top-1 right-1 w-2 h-2 bg-white rounded-full opacity-90"></div>
        </div>
      </div>

      <div class="w-8 h-3 border-b-[4px] border-social-primary rounded-b-full"></div>
    </div>

    <h1 class="font-bold text-2xl tracking-tight mb-2" style="font-family: 'Instagram Sans', sans-serif;">Transcendence</h1>
    <p class="text-sm text-social-secondary mb-6 text-center leading-relaxed">Arkadaşlarınızla ve AI ile etkileşime geçmek için kaydolun.</p>

    {#if errorMsg}
      <div class="text-social-danger text-sm mb-4 font-medium text-center w-full">{errorMsg}</div>
    {/if}

    <form class="w-full flex flex-col gap-3" onsubmit={handleRegister}>
      <input 
        type="email" 
        bind:value={email}
        placeholder="E-posta adresi" 
        disabled={isSubmitting}
        class="w-full bg-social-bg border border-social-border rounded px-3 py-2.5 text-sm outline-none focus:border-social-secondary transition-colors"
      >
      <input 
        type="text" 
        bind:value={username}
        placeholder="Kullanıcı adı" 
        disabled={isSubmitting}
        class="w-full bg-social-bg border border-social-border rounded px-3 py-2.5 text-sm outline-none focus:border-social-secondary transition-colors"
      >
      <input 
        type="password" 
        bind:value={password}
        placeholder="Şifre" 
        disabled={isSubmitting}
        class="w-full bg-social-bg border border-social-border rounded px-3 py-2.5 text-sm outline-none focus:border-social-secondary transition-colors"
      >
      <input 
        type="password" 
        bind:value={confirmPassword}
        placeholder="Şifreyi onayla" 
        disabled={isSubmitting}
        class="w-full bg-social-bg border border-social-border rounded px-3 py-2.5 text-sm outline-none focus:border-social-secondary transition-colors"
      >
      
      <div class="flex items-start gap-2 mt-2 px-1">
        <input 
          type="checkbox" 
          id="terms" 
          bind:checked={agreedToTerms}
          disabled={isSubmitting}
          class="mt-0.5 w-3.5 h-3.5 text-social-accent bg-social-bg border-social-border rounded focus:ring-social-accent focus:ring-1 transition-colors cursor-pointer"
        >
        <label for="terms" class="text-[11px] text-social-secondary leading-tight cursor-pointer">
          Kaydolarak <a href="/terms" class="text-social-[#385185] font-semibold hover:underline" onclick={(e) => e.stopPropagation()}>Koşullarımızı</a> ve <a href="/privacy" class="text-social-[#385185] font-semibold hover:underline" onclick={(e) => e.stopPropagation()}>Gizlilik İlkemizi</a> okuduğumu ve kabul ettiğimi onaylıyorum.
        </label>
      </div>

      <button 
        type="submit" 
        disabled={isSubmitting || !agreedToTerms}
        class="w-full bg-social-accent hover:bg-social-accent-hover disabled:bg-social-accent/50 disabled:cursor-not-allowed text-white font-semibold text-sm rounded py-2.5 mt-2 transition-colors flex items-center justify-center gap-2"
      >
        {#if isSubmitting}
          <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
        {/if}
        Kayıt Ol
      </button>
    </form>
  </div>

  <div class="auth-container w-full max-w-[350px] bg-social-card border border-social-border rounded-lg p-6 mt-4 flex items-center justify-center shadow-sm relative z-10">
    <p class="text-sm text-social-primary">Hesabın var mı? <a href="/login" class="text-social-accent font-semibold hover:text-social-accent-hover">Giriş Yap</a></p>
  </div>

</div>
