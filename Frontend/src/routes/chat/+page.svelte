<script lang="ts">
  import { onMount, tick } from 'svelte';
  import gsap from 'gsap';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';

  let newMessage = $state('');
  
  let messages = $state([
    { id: 1, sender: 'Main Character', text: 'Hey, did you check out the new chat page?', time: '10:00 AM', isMe: false, color: 'border-slate-300' },
    { id: 2, sender: 'Main Character', text: 'It looks super clean.', time: '10:02 AM', isMe: false, color: 'border-slate-300' },
    { id: 3, sender: 'Player 1', text: 'Ehehe I know bc i make it.', time: '10:05 AM', isMe: true, color: 'border-slate-900' }
  ]);

  let inbox = [
    { id: 1, username: 'Main Character', active: true, avatar: 'MC' },
    { id: 2, username: 'Player 2', active: false, avatar: 'P2' },
    { id: 3, username: 'Player 3', active: false, avatar: 'P3' }
  ];

  let chatContainer: HTMLElement;

  onMount(() => {
    gsap.fromTo('.horizontal-inbox-item', 
      { opacity: 0, scale: 0.9 },
      { opacity: 1, scale: 1, duration: 0.4, stagger: 0.05, ease: 'power2.out' }
    );
    gsap.fromTo('.timeline-bubble', 
      { opacity: 0, x: -10 },
      { opacity: 1, x: 0, duration: 0.5, stagger: 0.06, ease: 'power3.out' }
    );
    scrollToBottom();
  });

  async function scrollToBottom() {
    await tick();
    if (chatContainer) {
      chatContainer.scrollTop = chatContainer.scrollHeight;
    }
  }

  function popIn(node: HTMLElement) {
    gsap.fromTo(node, 
      { opacity: 0, y: 12, filter: 'blur(4px)' }, 
      { opacity: 1, y: 0, filter: 'blur(0px)', duration: 0.4, ease: 'power2.out' }
    );
    scrollToBottom();
  }

  function sendMessage(e?: Event) {
    if (e) e.preventDefault();
    if (!newMessage.trim()) return;

    messages.push({
      id: Date.now(),
      sender: 'Player 1',
      text: newMessage,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      isMe: true,
      color: 'border-slate-900'
    });
    
    newMessage = '';
  }
</script>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">

  <!-- Orijinal Sol Navigasyon Barı -->
  <Sidebar />

  <!-- YENİ DÜZEN: ODAK NOKTASI SOL PANEL (Gelen Kutusu Yerine Kişi Detayı) -->
  <aside class="hidden lg:flex w-[280px] border-r border-slate-100 bg-white h-screen flex-col shrink-0 p-8 justify-between">
    <div class="flex flex-col items-start gap-6">
      <div class="w-14 h-14 rounded-2xl bg-slate-900 text-white flex items-center justify-center font-semibold text-sm shadow-md shadow-slate-200">
        MC
      </div>
      <div>
        <h2 class="font-semibold text-lg text-slate-900 tracking-tight">Main Character</h2>
        <p class="text-xs text-slate-400 mt-1">Creative Director</p>
        <p class="text-[11px] text-emerald-500 font-medium mt-2 flex items-center gap-1">
          <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span> Online
        </p>
      </div>

      <!-- Ortak Paylaşımlar / Hızlı Bağlantılar Alanı -->
      <div class="w-full mt-6 pt-6 border-t border-slate-100">
        <span class="text-[11px] font-bold tracking-wider text-slate-400 uppercase block mb-3">Shared Space</span>
        <div class="grid grid-cols-2 gap-2">
          <div class="h-16 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-xs text-slate-400 cursor-pointer hover:bg-slate-100 transition-colors">📂 Docs</div>
          <div class="h-16 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-xs text-slate-400 cursor-pointer hover:bg-slate-100 transition-colors">🔗 Links</div>
        </div>
      </div>
    </div>

    <div class="text-xs text-slate-400 font-light">
      Encryption: Active ( Dümenden )
    </div>
  </aside>

  <!-- ANA CHAT ALANI (Dikey ve Akıcı) -->
  <main class="flex-1 flex flex-col h-screen">
    
    <!-- YATAY GELEN KUTUSU ŞERİDİ (Geleneksel sol listeyi yıkan kısım) -->
    <section class="h-[80px] border-b border-slate-100 bg-white/60 backdrop-blur-md flex items-center px-8 gap-3 shrink-0 overflow-x-auto no-scrollbar">
      <div class="text-xs font-bold text-slate-400 tracking-wider uppercase border-r border-slate-200 pr-4 mr-2 shrink-0">Chats</div>
      
      {#each inbox as chat}
        <button class="horizontal-inbox-item flex items-center gap-2.5 px-3 py-1.5 rounded-full transition-all shrink-0
          {chat.active ? 'bg-slate-900 text-white shadow-sm' : 'hover:bg-slate-100 text-slate-600'}"
        >
          <div class="w-6 h-6 rounded-full text-[10px] font-bold flex items-center justify-center
            {chat.active ? 'bg-white/20 text-white' : 'bg-slate-200 text-slate-700'}"
          >
            {chat.avatar}
          </div>
          <span class="text-xs font-medium pr-1">{chat.username}</span>
        </button>
      {/each}
    </section>

    <!-- ZAMAN ÇİZGİSİ TASARIMLI MESAJ ALANI -->
    <div bind:this={chatContainer} class="flex-1 overflow-y-auto custom-scrollbar px-6 md:px-16 py-8 flex flex-col bg-[#fcfcfc]">
      
      <!-- Merkezi Dikey Akış Çizgisi -->
      <div class="w-full max-w-2xl mx-auto flex flex-col gap-8 relative pl-6 border-l border-slate-200/60">
        
        {#each messages as msg (msg.id)}
          <div use:popIn class="timeline-bubble relative group flex flex-col {msg.isMe ? 'pl-8' : ''}">
            
            <!-- Çizgi üzerindeki düğüm noktası -->
            <div class="absolute -left-[29px] top-1.5 w-3 h-3 rounded-full bg-white border-2 {msg.isMe ? 'border-slate-900' : 'border-slate-300'} transition-transform group-hover:scale-125"></div>

            <!-- Gönderen Bilgisi -->
            <div class="flex items-center gap-2 mb-1.5">
              <span class="text-xs font-semibold tracking-tight {msg.isMe ? 'text-slate-900' : 'text-slate-500'}">
                {msg.sender}
              </span>
              <span class="text-[10px] text-slate-400 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                {msg.time}
              </span>
            </div>

            <!-- Saf ve Çizgisel Mesaj Metni -->
            <div class="text-[14px] text-slate-800 leading-relaxed max-w-xl font-light">
              {msg.text}
            </div>

          </div>
        {/each}

      </div>
    </div>

    <!-- ENTEGRE EDİTORİAL INPUT ALANI -->
    <div class="p-6 md:px-16 bg-white border-t border-slate-100 shrink-0">
      <div class="max-w-2xl mx-auto">
        <form class="flex items-center gap-4" onsubmit={sendMessage}>
          <input 
            type="text" 
            bind:value={newMessage}
            placeholder="Write openly..." 
            class="flex-1 bg-transparent border-none outline-none text-[14px] text-slate-900 placeholder-slate-400"
          >
          <div class="flex items-center gap-4">
            {#if newMessage.trim()}
              <button type="submit" class="text-xs font-bold tracking-wider uppercase text-slate-900 hover:text-black transition-colors">
                Attack 💥
              </button>
            {:else}
              <button type="button" class="text-slate-400 hover:text-slate-800 text-sm transition-colors">Attach</button>
            {/if}
          </div>
        </form>
      </div>
    </div>

  </main>

  <MobileNav />

</div>

<style>
  /* Kaydırma çubuklarını gizleme ve özelleştirme */
  .no-scrollbar::-webkit-scrollbar {
    display: none;
  }
  .no-scrollbar {
    -ms-overflow-style: none;
    scrollbar-width: none;
  }
  .custom-scrollbar::-webkit-scrollbar {
    width: 3px;
  }
  .custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: #e2e8f0;
  }
</style>