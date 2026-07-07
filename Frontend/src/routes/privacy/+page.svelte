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
      <h1 class="text-3xl font-bold tracking-tight mb-2" style="font-family: 'Instagram Sans', sans-serif;">Gizlilik Politikası (Privacy Policy)</h1>
      <p class="text-social-secondary text-sm">Son Güncelleme: 9 Haziran 2026</p>
    </div>

    <div class="prose prose-sm md:prose-base text-social-primary leading-relaxed space-y-6">
      <p>
        Bu proje, 42 Okulu "ft_transcendence" müfredatının bir parçası olarak geliştirilmiştir. Gizliliğiniz bizim için önemlidir. Bu Gizlilik Politikası, platformumuzu kullanırken kişisel bilgilerinizin nasıl toplandığını, kullanıldığını ve korunduğunu açıklamaktadır.
      </p>

      <h2 class="text-xl font-bold mt-8 mb-4">1. Toplanan Bilgiler</h2>
      <p>
        Sisteme kayıt olduğunuzda veya giriş yaptığınızda (42 Intra veya standart kayıt yoluyla) kullanıcı adınız, e-posta adresiniz ve profil fotoğrafınız (avatar) gibi temel hesap bilgilerinizi güvenli bir şekilde saklıyoruz.
      </p>

      <h2 class="text-xl font-bold mt-8 mb-4">2. Bilgilerin Kullanımı</h2>
      <p>
        Toplanan veriler yalnızca aşağıdaki amaçlar için kullanılmaktadır:
      </p>
      <ul class="list-disc pl-5 space-y-2 text-social-secondary">
        <li>Hesabınızın güvenliğini sağlamak ve doğrulama işlemlerini gerçekleştirmek.</li>
        <li>Kullanıcılar arası etkileşimi (arkadaşlık sistemi, canlı sohbet, post paylaşımları) mümkün kılmak.</li>
        <li>Yapay Zeka (AI) modülümüz aracılığıyla daha iyi yanıtlar üretebilmek için istemleri (prompt) işlemek.</li>
      </ul>

      <h2 class="text-xl font-bold mt-8 mb-4">3. Veri Güvenliği (HashiCorp Vault & WAF)</h2>
      <p>
        Verilerinizin güvenliğini en üst düzeyde tutmak amacıyla sistemimizde **ModSecurity (WAF)** kullanılmakta ve tüm hassas anahtarlarımız **HashiCorp Vault** ile şifrelenerek korunmaktadır. Kullanıcı şifreleri asla düz metin (plain text) olarak saklanmaz, endüstri standardı algoritmalarla (hash) saklanır.
      </p>

      <h2 class="text-xl font-bold mt-8 mb-4">4. İletişim</h2>
      <p>
        Bu gizlilik politikası hakkında sorularınız veya veri silme talepleriniz varsa (GDPR kuralları gereği), sistem yöneticileri ile iletişime geçebilirsiniz.
      </p>
    </div>
  </article>

</div>
