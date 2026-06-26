<script lang="ts">
  import { onMount } from 'svelte';
  import gsap from 'gsap';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';

  let user = $state({
    username: 'Celten',
    fullName: 'Enes',
    bio: 'Frontend Developer \n @Transcendence',
    posts: 0,
    followers: 100,
    following: 105,
    avatarColor: 'bg-slate-900 border border-slate-800 text-white shadow-sm shadow-slate-200',
    avatarLetter: 'C'
  });

  let posts = $state([
    { id: 1, type: 'image', color: 'bg-slate-100 border border-slate-200/60', likes: 120, comments: 14, size: 'col-span-2 row-span-2 h-[340px]' }, // Öne çıkan büyük kart
    { id: 2, type: 'image', color: 'bg-slate-50 border border-slate-200/60', likes: 89, comments: 2, size: 'col-span-1 row-span-1 h-[160px]' },
    { id: 3, type: 'image', color: 'bg-slate-900', likes: 450, comments: 42, size: 'col-span-1 row-span-2 h-[340px]' }, // Koyu kontrast dikey kart
    { id: 4, type: 'image', color: 'bg-slate-100/70 border border-slate-200/40', likes: 32, comments: 1, size: 'col-span-1 row-span-1 h-[160px]' },
    { id: 5, type: 'image', color: 'bg-slate-50 border border-slate-200/60', likes: 210, comments: 8, size: 'col-span-2 row-span-1 h-[160px]' }, // Geniş yatay kart
    { id: 6, type: 'image', color: 'bg-slate-200/50', likes: 15, comments: 0, size: 'col-span-1 row-span-1 h-[160px]' }
  ]);

  let fileInput: HTMLInputElement;

  onMount(() => {
    const tl = gsap.timeline({ defaults: { ease: 'power3.out' } });

    tl.fromTo('.editorial-sidebar', 
      { opacity: 0, x: -30 }, 
      { opacity: 1, x: 0, duration: 0.8 }
    )
    .fromTo('.portfolio-item', 
      { opacity: 0, y: 20, scale: 0.98 }, 
      { opacity: 1, y: 0, scale: 1, duration: 0.5, stagger: 0.06 },
      "-=0.5"
    );
  });

  function handleAvatarClick() {
    fileInput.click();
  }

  function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      const file = target.files[0];
      if (!file.type.startsWith('image/')) {
        alert("Lütfen sadece resim dosyası yükleyin.");
        return;
      }
      if (file.size > 2 * 1024 * 1024) {
        alert("Dosya boyutu 2MB'ı geçemez.");
        return;
      }
      
      alert(`"${file.name}" adlı fotoğraf seçildi. (MOCK Yükleme)`);
      user.avatarColor = 'bg-slate-950 text-emerald-400 font-mono';
      user.avatarLetter = '✓';
    }
  }
</script>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">

  <Sidebar />

  <main class="flex-1 overflow-y-auto custom-scrollbar flex flex-col lg:flex-row">
    
    <section class="editorial-sidebar w-full lg:w-[380px] lg:h-screen lg:border-r border-slate-100 bg-white p-8 md:p-12 flex flex-col justify-between shrink-0">
      
      <div class="flex flex-col gap-8">
        <div class="relative w-20 h-20 rounded-2xl cursor-pointer group overflow-hidden {user.avatarColor} flex items-center justify-center font-bold text-xl transition-all duration-300 hover:scale-105" 
             onclick={handleAvatarClick} onkeydown={e => e.key === 'Enter' && handleAvatarClick()} role="button" tabindex="0">
          {user.avatarLetter}
          <div class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center text-xs text-white">
            Change
          </div>
          <input bind:this={fileInput} type="file" accept="image/*" class="hidden" onchange={handleFileChange}>
        </div>

        <div>
          <h1 class="text-2xl font-semibold tracking-tight text-slate-900">{user.fullName}</h1>
          <p class="text-sm font-mono text-slate-400 mt-1">@{user.username}</p>
        </div>

        <div class="flex flex-col gap-3 py-4 border-y border-slate-100 w-full font-mono text-xs">
          <div class="flex justify-between text-slate-500">
            <span>Index Slices:</span>
            <span class="font-bold text-slate-900">{user.posts}</span>
          </div>
          <div class="flex justify-between text-slate-500">
            <span>Network Observers:</span>
            <span class="font-bold text-slate-900">{user.followers}</span>
          </div>
          <div class="flex justify-between text-slate-500">
            <span>Following Core:</span>
            <span class="font-bold text-slate-900">{user.following}</span>
          </div>
        </div>

        <div class="text-[13px] text-slate-600 leading-relaxed whitespace-pre-line font-light tracking-wide">
          {user.bio}
        </div>
      </div>

      <div class="flex flex-col gap-2 mt-8 lg:mt-0 w-full">
        <button class="w-full bg-slate-900 text-white text-xs font-semibold tracking-wide uppercase py-3 rounded-xl hover:bg-black transition-colors shadow-sm">
          Edit Settings
        </button>
        <button class="w-full bg-slate-50 text-slate-500 text-xs font-medium py-2.5 rounded-xl hover:bg-slate-100 hover:text-slate-800 transition-colors border border-slate-200/40">
          View Archive
        </button>
      </div>

    </section>

    <section class="flex-1 p-8 md:p-12 lg:p-16 overflow-y-auto custom-scrollbar bg-[#fcfcfc]">
      
      <div class="flex gap-8 text-[11px] font-bold tracking-wider uppercase text-slate-400 mb-10 border-b border-slate-100 pb-4">
        <button class="text-slate-950 flex items-center gap-1.5 relative">
          <span class="w-1 h-1 rounded-full bg-slate-950 absolute -bottom-4 left-1/2 -translate-x-1/2"></span>
          Collection
        </button>
        <button class="hover:text-slate-900 transition-colors">Saved</button>
        <button class="hover:text-slate-900 transition-colors">Tagged</button>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 auto-rows-max max-w-4xl">
        {#each posts as post}
          <div class="portfolio-item {post.size} rounded-2xl cursor-pointer relative group overflow-hidden transition-all duration-300 hover:-translate-y-1">
            
            <div class="w-full h-full {post.color} transition-transform duration-500 group-hover:scale-[1.02]"></div>

            <div class="absolute inset-0 bg-gradient-to-t from-slate-950/80 via-slate-950/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex items-end p-6 text-white">
              <div class="flex items-center gap-5 text-xs font-mono tracking-wide">
                <div class="flex items-center gap-1.5">
                  <span class="text-slate-300">▲</span> {post.likes}
                </div>
                <div class="flex items-center gap-1.5">
                  <span class="text-slate-300">⎔</span> {post.comments}
                </div>
              </div>
              <div class="absolute top-4 right-4 bg-white/10 backdrop-blur-sm px-2 py-0.5 rounded text-[9px] font-mono tracking-widest text-white/80">
                #{post.id}
              </div>
            </div>

          </div>
        {/each}
      </div>

    </section>

  </main>

  <MobileNav />

</div>

<style>
  .custom-scrollbar::-webkit-scrollbar
  {
    width: 3px;
  }
  .custom-scrollbar::-webkit-scrollbar-track
  {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb
  {
    background: #e2e8f0;
  }
</style>