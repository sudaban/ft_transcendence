<script lang="ts">
  import { onMount } from 'svelte';
  import gsap from 'gsap';

  onMount(() => {
    gsap.fromTo('.legal-content', 
      { opacity: 0, y: 20 }, 
      { opacity: 1, y: 0, duration: 0.8, ease: 'power3.out' }
    );
  });
</script>

<div class="min-h-screen bg-social-bg flex flex-col items-center py-12 px-4 selection:bg-social-accent selection:text-white">
  
  <div class="w-full max-w-[600px] mb-8">
    <button onclick={() => history.back()} class="flex items-center gap-2 text-social-secondary hover:text-social-primary transition-colors font-semibold">
      <span class="text-xl">←</span> Geri Dön
    </button>
  </div>

  <article class="legal-content w-full max-w-[600px] bg-social-card border border-social-border rounded-lg p-8 md:p-12 shadow-sm">
    <div class="mb-8 border-b border-social-border pb-6">
      <h1 class="text-3xl font-bold tracking-tight mb-2" style="font-family: 'Instagram Sans', sans-serif;">Hizmet Koşulları (Terms of Service)</h1>
      <p class="text-social-secondary text-sm">Son Güncelleme: 11 Temmuz 2026</p>
    </div>

    <div class="prose prose-sm md:prose-base text-social-primary leading-relaxed space-y-6">
      <p>
        Transcendence platformuna ("Hizmet") hoş geldiniz. Bu hizmeti kullanarak aşağıdaki koşulları kabul etmiş sayılırsınız. Lütfen dikkatlice okuyunuz.
      </p>

      <h2 class="text-xl font-bold mt-8 mb-4">1. Kabul Edilebilir Kullanım</h2>
      <p>
        Platformumuz bir sosyal medya ve Yapay Zeka etkileşim alanıdır. Kullanıcılar şu kurallara uymak zorundadır:
      </p>
      <ul class="list-disc pl-5 space-y-2 text-social-secondary">
        <li>Diğer kullanıcılara saygılı olmak; nefret söylemi, taciz veya zorbalık içeren içerikler paylaşmamak.</li>
        <li>Sistemin işleyişini bozacak (Spam, DDoS vb.) zararlı aktivitelerde bulunmamak.</li>
        <li>Platform üzerindeki Yapay Zeka (AI) sohbet sistemini yasa dışı içerik üretmek amacıyla suistimal etmemek.</li>
      </ul>

      <h2 class="text-xl font-bold mt-8 mb-4">2. Hesap Güvenliği</h2>
      <p>
        Hesabınızın güvenliği tamamen sizin sorumluluğunuzdadır. Şifrenizi veya 42 Intra oturum bilgilerinizi kimseyle paylaşmayınız. Yetkisiz bir giriş tespit ederseniz derhal yöneticilere bildiriniz.
      </p>

      <h2 class="text-xl font-bold mt-8 mb-4">3. İçerik ve Mülkiyet</h2>
      <p>
        Platformda paylaştığınız her türlü gönderi, mesaj veya dosya sizin sorumluluğunuzdadır. Ancak, yasa dışı veya kurallarımızı ihlal eden içerikleri bildirim yapmaksızın silme (Content Moderation AI veya manuel) hakkını saklı tutarız.
      </p>

      <h2 class="text-xl font-bold mt-8 mb-4">4. Hizmet Değişiklikleri</h2>
      <p>
        Transcendence bir eğitim projesidir. Sistemde yaşanabilecek veri kayıpları, anlık kesintiler veya hizmetin tamamen durdurulması durumlarından dolayı hiçbir yasal sorumluluk kabul edilmemektedir. Platformu "olduğu gibi" kullanmayı kabul edersiniz.
      </p>
    </div>
  </article>

</div>
