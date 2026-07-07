<script lang="ts">
  import { onMount, tick } from 'svelte';
  import gsap from 'gsap';
  import { spring } from 'svelte/motion';
  import { ApiService } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';

  // State
  let email = $state('');
  let password = $state('');
  let errorMsg = $state('');
  let isSubmitting = $state(false);

  // 2FA State
  let is2faPending = $state(false);
  let tempToken = $state('');
  let otpValues = $state(['', '', '', '', '', '']);
  let otpInputs: HTMLInputElement[] = [];
  
  let timeRemaining = $state(300); // 5 dakika
  let timerInterval: ReturnType<typeof setInterval>;
  let formattedTime = $derived(`${Math.floor(timeRemaining / 60)}:${(timeRemaining % 60).toString().padStart(2, '0')}`);

  let mouseX = $state(0);
  let mouseY = $state(0);
  let eyeRect = $state({ left: 0, top: 0, width: 0, height: 0 });
  let eyeElement: HTMLElement;

  onMount(() => {
    gsap.fromTo('.auth-container', 
      { opacity: 0, y: 30 }, 
      { opacity: 1, y: 0, duration: 0.8, ease: 'power3.out' }
    );
    
    if (eyeElement)
    {
      eyeRect = eyeElement.getBoundingClientRect();
    }
    
    const handleResize = () => {
      if (eyeElement) eyeRect = eyeElement.getBoundingClientRect();
    };
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
      if (timerInterval) clearInterval(timerInterval);
    };
  });

  function handleMouseMove(e: MouseEvent)
  {
    mouseX = e.clientX;
    mouseY = e.clientY;
  }

  let targetPos = $derived.by(() => {
    if (!eyeRect.width) return { x: 0, y: 0 };
    
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

  function triggerError(msg: string)
  {
    errorMsg = msg;
    gsap.fromTo('.auth-container', 
      { x: -8 }, 
      { x: 8, duration: 0.1, yoyo: true, repeat: 3, onComplete: () => gsap.to('.auth-container', {x: 0, duration: 0.1}) }
    );
  }

  async function handleLogin(e: Event)
  {
    e.preventDefault();
    if (!email || !password)
    {
      triggerError("Lütfen tüm alanları doldurun.");
      return;
    }
    errorMsg = "";
    isSubmitting = true;

    try
    {
      const res = await ApiService.login(email, password);
      
      if (res.requiresTwoFactor)
      {
        // Switch to 2FA view
        tempToken = res.tempToken || '';
        is2faPending = true;
        timeRemaining = 300; // Reset timer
        
        // Start countdown
        if (timerInterval) clearInterval(timerInterval);
        timerInterval = setInterval(() => {
          timeRemaining--;
          if (timeRemaining <= 0) {
            clearInterval(timerInterval);
            // Session expired
            is2faPending = false;
            tempToken = '';
            otpValues = ['', '', '', '', '', ''];
            triggerError("Güvenlik oturumu süresi doldu. Lütfen tekrar giriş yapın.");
          }
        }, 1000);

        // Animate the transition
        gsap.fromTo('.otp-container', { opacity: 0, y: 10 }, { opacity: 1, y: 0, duration: 0.4, delay: 0.1 });
        await tick();
        otpInputs[0]?.focus();
      }
      else
      {
        authStore.login(res.token || '');
        // goto('/') is handled by authStore if it redirects, but authStore doesn't redirect on login.
        // Wait, init() just sets state. We should redirect explicitly.
        window.location.href = '/'; 
      }
    }
    catch (err)
    {
      triggerError("Giriş başarısız oldu.");
    }
    finally
    {
      isSubmitting = false;
    }
  }

  // OTP Logic
  async function handleOtpKeydown(e: KeyboardEvent, index: number)
  {
    if (e.key === 'Backspace')
    {
      if (!otpValues[index] && index > 0)
      {
        // Move to previous if empty
        otpInputs[index - 1].focus();
        // Prevent default to avoid jumping cursor issues
        e.preventDefault(); 
      }
    }
  }

  async function handleOtpInput(e: Event, index: number)
  {
    const target = e.target as HTMLInputElement;
    const val = target.value;
    
    if (val.length > 1)
    {
      // Handle paste directly into the input (some browsers)
      const pasted = val.slice(0, 6).split('');
      for (let i = 0; i < pasted.length && index + i < 6; i++)
      {
        otpValues[index + i] = pasted[i];
      }
      const nextIndex = Math.min(index + pasted.length, 5);
      otpInputs[nextIndex]?.focus();
      return;
    }

    if (val && index < 5)
    {
      otpInputs[index + 1]?.focus();
    }
    
    checkCompleteOtp();
  }

  function handleOtpPaste(e: ClipboardEvent)
  {
    e.preventDefault();
    const pastedData = e.clipboardData?.getData('text/plain').trim() || '';
    if (!/^\d+$/.test(pastedData)) return; // Only numbers
    
    const chars = pastedData.slice(0, 6).split('');
    chars.forEach((char, i) => {
      if (i < 6) otpValues[i] = char;
    });
    
    const nextIndex = Math.min(chars.length, 5);
    otpInputs[nextIndex]?.focus();
    checkCompleteOtp();
  }

  async function checkCompleteOtp()
  {
    const code = otpValues.join('');
    if (code.length === 6)
    {
      isSubmitting = true;
      errorMsg = '';
      try
      {
        const res = await ApiService.login2fa(email, code, tempToken);
        if (timerInterval) clearInterval(timerInterval);
        authStore.login(res.token || '');
        window.location.href = '/'; // Success
      }
      catch (err)
      {
        triggerError("Geçersiz kod.");
        otpValues = ['', '', '', '', '', ''];
        await tick();
        otpInputs[0]?.focus();
      }
      finally
      {
        isSubmitting = false;
      }
    }
  }
</script>

<!-- Global Mouse Tracker -->
<svelte:window onmousemove={handleMouseMove} />

<div class="min-h-screen bg-social-bg flex flex-col items-center justify-center p-4 selection:bg-social-accent selection:text-white">
  
  <div class="auth-container w-full max-w-[350px] bg-social-card border border-social-border rounded-lg p-8 flex flex-col items-center shadow-sm relative z-10">
    
    <!-- The Interactive Face -->
    <div 
      bind:this={eyeElement}
      class="mb-6 relative w-28 h-32 rounded-[50px] bg-gray-50 border-[3px] border-social-border flex flex-col items-center justify-center shadow-inner overflow-hidden transition-colors hover:border-social-secondary"
    >
      <div class="absolute inset-0 border-t-4 border-social-accent rounded-[50px] opacity-10"></div>
      
      <!-- The Eye -->
      <div class="relative w-16 h-16 rounded-full bg-white border-[2px] border-social-border flex items-center justify-center shadow-inner mb-2">
        <!-- Pupil -->
        <div 
          class="w-7 h-7 bg-social-primary rounded-full relative"
          style="transform: translate({$pupilSpring.x}px, {$pupilSpring.y}px);"
        >
          <!-- Cute Light Reflection -->
          <div class="absolute top-1 right-1 w-2 h-2 bg-white rounded-full opacity-90"></div>
        </div>
      </div>

      <!-- The Mouth (Static) -->
      <div class="w-8 h-3 border-b-[4px] border-social-primary rounded-b-full"></div>
    </div>

    <h1 class="font-bold text-2xl tracking-tight mb-2" style="font-family: 'Instagram Sans', sans-serif;">Transcendence</h1>
    <p class="text-sm text-social-secondary mb-8 text-center leading-relaxed">
      {#if is2faPending}
        İki aşamalı doğrulama kodunu girin
      {:else}
        Giriş yap la
      {/if}
    </p>

    {#if errorMsg}
      <div class="text-social-danger text-sm mb-4 font-medium text-center w-full">{errorMsg}</div>
    {/if}

    {#if !is2faPending}
      <form class="w-full flex flex-col gap-3" onsubmit={handleLogin}>
        <input 
          type="email" 
          bind:value={email}
          placeholder="E-posta adresi" 
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
        <button 
          type="submit" 
          disabled={isSubmitting}
          class="w-full bg-social-accent hover:bg-social-accent-hover text-white font-semibold text-sm rounded py-2.5 mt-2 transition-colors flex items-center justify-center gap-2"
        >
          {#if isSubmitting}
            <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          {/if}
          Giriş Yap
        </button>
      </form>

      <div class="w-full flex items-center gap-4 my-6">
        <div class="flex-1 h-px bg-social-border"></div>
        <span class="text-xs font-semibold text-social-secondary uppercase">YA DA</span>
        <div class="flex-1 h-px bg-social-border"></div>
      </div>

      <a href="#" class="text-sm font-semibold text-[#385185] hover:text-social-primary transition-colors flex items-center gap-2 mb-4">
        42 Intra ile Giriş Yap
      </a>

      <a href="#" class="text-xs text-social-secondary hover:text-social-primary transition-colors">Şifreni mi unuttun?</a>
    
    {:else}
      <!-- 2FA OTP UI -->
      <div class="otp-container w-full flex flex-col items-center">
        <p class="text-xs text-social-secondary mb-4 text-center">
          <span class="font-bold text-social-primary">{email}</span> adresine (veya Authenticator uygulamana) gönderilen 6 haneli kodu gir.
        </p>
        
        <div class="flex justify-between w-full gap-2 mb-4" onpaste={handleOtpPaste}>
          {#each otpValues as val, i}
            <input 
              type="text" 
              inputmode="numeric" 
              maxlength="1"
              bind:this={otpInputs[i]}
              bind:value={otpValues[i]}
              onkeydown={(e) => handleOtpKeydown(e, i)}
              oninput={(e) => handleOtpInput(e, i)}
              disabled={isSubmitting}
              class="w-10 h-12 text-center text-xl font-bold bg-social-bg border border-social-border rounded outline-none focus:border-social-accent focus:ring-1 focus:ring-social-accent transition-all"
            >
          {/each}
        </div>
        
        <div class="text-xs font-semibold px-3 py-1 rounded-full {timeRemaining < 60 ? 'bg-red-50 text-red-600' : 'bg-gray-100 text-gray-600'}">
          Kalan Süre: {formattedTime}
        </div>
        
        {#if isSubmitting}
          <span class="w-6 h-6 border-2 border-social-accent border-t-transparent rounded-full animate-spin mt-2"></span>
        {/if}
      </div>
    {/if}
  </div>

  <div class="auth-container w-full max-w-[350px] bg-social-card border border-social-border rounded-lg p-6 mt-4 flex items-center justify-center shadow-sm relative z-10">
    <p class="text-sm text-social-primary">Hesabın yok mu? <a href="/register" class="text-social-accent font-semibold hover:text-social-accent-hover">Kaydol</a></p>
  </div>

</div>
