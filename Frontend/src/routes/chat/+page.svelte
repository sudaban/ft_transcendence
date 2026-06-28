<script lang="ts">
  import { onMount, tick } from 'svelte';
  import gsap from 'gsap';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';

  let newMessage = $state('');
  let asciiHistory = $state<string[]>([]);
  
  let archivedSlices = $state<Array<{ id: number, label: string, char: string, leader: string | null, data: any[] }>>([]);
  let selectedSliceId = $state<number | null>(null);
  let isHistoryOpen = $state(false);

  let currentLoopLeader = $state<string | null>('Main Character');
  
  let liveMessages = $state([
    { id: 1, sender: 'Main Character', text: 'Hey, did you check out the new chat page?', time: '10:00 AM', color: 'border-slate-300' },
    { id: 2, sender: 'Main Character', text: 'It looks super clean.', time: '10:02 AM', color: 'border-slate-300' },
    { id: 3, sender: 'Player 1', text: 'Ehehe I know bc i make it.', time: '10:05 AM', color: 'border-slate-900' }
  ]);

  let currentDisplayMessages = $derived.by(() => {
    if (selectedSliceId === null) return liveMessages;
    const found = archivedSlices.find(s => s.id === selectedSliceId);
    return found ? found.data : [];
  });

  let currentDisplayLeader = $derived.by(() => {
    if (selectedSliceId === null) return currentLoopLeader;
    const found = archivedSlices.find(s => s.id === selectedSliceId);
    return found ? found.leader : null;
  });

  let selectedSliceLabel = $derived.by(() => {
    if (selectedSliceId === null) return '';
    const found = archivedSlices.find(s => s.id === selectedSliceId);
    return found ? found.label : '';
  });

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

    if (selectedSliceId !== null) {
      alert("Geçmiş modundasınız. Mesaj göndermek için sol üstten canlı yayına dönün.");
      return;
    }

    if (liveMessages.length >= 8) {
      const bitString = liveMessages.map(msg => msg.sender === currentLoopLeader ? '1' : '0').join('');
      const decimalValue = parseInt(bitString, 2);
      
      const asciiChar = (decimalValue >= 32 && decimalValue <= 126) 
        ? String.fromCharCode(decimalValue) 
        : `?`; 

      asciiHistory.push(asciiChar);
      
      archivedSlices.unshift({
        id: Date.now(),
        label: `#0${archivedSlices.length + 1}`,
        char: asciiChar,
        leader: currentLoopLeader,
        data: [...liveMessages]
      });

      liveMessages = [];
      currentLoopLeader = null;
    }

    if (liveMessages.length === 0 && !currentLoopLeader) {
      currentLoopLeader = 'Player 1';
    }

    liveMessages.push({
      id: Date.now(),
      sender: 'Player 1',
      text: newMessage,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      color: 'border-slate-900'
    });
    
    newMessage = '';
    scrollToBottom();
  }
</script>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">

  <Sidebar />

  <aside class="hidden lg:flex w-[280px] border-r border-slate-100 bg-white h-screen flex-col shrink-0 p-8 justify-between overflow-y-auto custom-scrollbar">
    <div class="flex flex-col items-start gap-6 w-full">
      
      <button 
        onclick={() => selectedSliceId = null}
        class="w-full flex items-center justify-between p-3 rounded-xl border transition-all duration-200
          {selectedSliceId === null ? 'bg-slate-900 text-white border-slate-900 shadow-sm' : 'bg-slate-50 text-slate-700 border-slate-200/60 hover:bg-slate-100'}"
      >
        <div class="flex items-center gap-3">
          <div class="w-2 h-2 rounded-full {selectedSliceId === null ? 'bg-emerald-400 animate-pulse' : 'bg-slate-400'}"></div>
          <span class="text-xs font-semibold tracking-tight">Live Session</span>
        </div>
        <span class="text-[10px] font-mono">{liveMessages.length}/8 B</span>
      </button>

      <div>
        <h2 class="font-semibold text-base text-slate-900 tracking-tight">Main Character</h2>
        <p class="text-xs text-slate-400">Creative Director</p>
      </div>

      <div class="w-full mt-2 pt-4 border-t border-slate-100 flex flex-col gap-5">
        <div>
          <span class="text-[11px] font-bold tracking-wider text-slate-400 uppercase block mb-3">Shared Space</span>
          <div class="grid grid-cols-2 gap-2">
            <div class="h-12 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-xs text-slate-400 cursor-pointer hover:bg-slate-100 transition-colors">📂 Docs</div>
            <div class="h-12 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-xs text-slate-400 cursor-pointer hover:bg-slate-100 transition-colors">🔗 Links</div>
          </div>
        </div>

        <div class="pt-2 border-t border-slate-100 w-full">
          <button 
            onclick={() => isHistoryOpen = !isHistoryOpen}
            class="w-full flex justify-between items-center text-[11px] font-bold tracking-wider text-slate-400 uppercase font-mono group hover:text-slate-900 transition-colors"
          >
            <div class="flex items-center gap-1.5">
              <span>History Slices (ROM)</span>
            </div>
            <span class="text-xs transition-transform duration-200 {isHistoryOpen ? 'rotate-180 text-slate-900' : 'text-slate-300 group-hover:text-slate-500'}">▼</span>
          </button>
          
          {#if isHistoryOpen}
            <div class="grid grid-cols-3 gap-2 mt-3 max-h-[160px] overflow-y-auto pr-1 custom-scrollbar">
              {#if archivedSlices.length === 0}
                <span class="text-slate-400 text-xs font-light py-1 col-span-3">// RAM Empty</span>
              {:else}
                {#each archivedSlices as slice}
                  <button 
                    onclick={() => selectedSliceId = slice.id}
                    class="flex flex-col items-center justify-center aspect-[4/5] p-2 rounded-xl border font-mono transition-all duration-150 relative group
                      {selectedSliceId === slice.id 
                        ? 'bg-slate-950 text-emerald-400 border-slate-950 shadow-sm font-bold ring-2 ring-slate-950/10' 
                        : 'bg-slate-50 text-slate-700 border-slate-200/60 hover:bg-slate-100'}"
                  >
                    <span class="text-base font-bold tracking-normal">{slice.char}</span>
                    <span class="text-[9px] text-slate-400 mt-1 font-sans group-hover:text-slate-600 {selectedSliceId === slice.id ? 'text-emerald-500/70' : ''}">
                      {slice.label}
                    </span>
                  </button>
                {/each}
              {/if}
            </div>
          {/if}
        </div>

        <div class="pt-2 border-t border-slate-100 w-full">
          <span class="text-[11px] font-bold tracking-wider text-slate-400 uppercase block mb-2.5 font-mono">ASCII Pool</span>
          <div class="w-full bg-slate-950 text-emerald-400 font-mono text-sm p-3 rounded-xl border border-slate-900 shadow-inner min-h-[44px] flex items-center tracking-widest overflow-x-auto no-scrollbar">
            {#if asciiHistory.length === 0}
              <span class="text-slate-600 text-xs font-normal tracking-normal">// void...</span>
            {:else}
              <span class="text-emerald-600 mr-0.5">&gt;</span>{asciiHistory.join('')}
            {/if}
          </div>
        </div>
      </div>
    </div>

    <div class="text-[11px] text-slate-400 font-mono pt-4 mt-4 border-t border-slate-50">
      Enc: Active (Dümenden)
    </div>
  </aside>

  <main class="flex-1 flex flex-col h-screen">
    
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

    <div bind:this={chatContainer} class="flex-1 overflow-y-auto custom-scrollbar px-6 md:px-16 py-8 flex flex-col bg-[#fcfcfc]">
      
      {#if selectedSliceId !== null}
        <div class="max-w-2xl w-full mx-auto mb-6 bg-slate-100 text-slate-700 border border-slate-200 px-4 py-2 rounded-xl text-xs font-mono flex justify-between items-center">
          <span>⚠️ Reading Memory Bank {selectedSliceLabel} (Read-Only)</span>
          <button onclick={() => selectedSliceId = null} class="underline font-bold text-slate-900 hover:text-black">Return to Live</button>
        </div>
      {/if}

      <div class="w-full max-w-2xl mx-auto flex flex-col gap-8 relative pl-6 border-l border-slate-200/60">
        
        {#if currentDisplayMessages.length === 0}
          <div class="text-center text-xs text-slate-400 font-mono py-12 tracking-wide animate-pulse">
            // Buffer flushed into history repository. Standby for new bits...
          </div>
        {/if}

        {#each currentDisplayMessages as msg (msg.id)}
          <div use:popIn class="timeline-bubble relative group flex flex-col {msg.sender === 'Player 1' ? 'pl-8' : ''}">
            
            <div class="absolute -left-[33px] top-0.5 w-5 h-5 rounded-full bg-[#fcfcfc] flex items-center justify-center font-mono text-xs font-bold transition-all duration-200 group-hover:scale-125
              {msg.sender === currentDisplayLeader ? 'text-slate-900 scale-110' : 'text-slate-300'}"
            >
              {msg.sender === currentDisplayLeader ? '1' : '0'}
            </div>

            <div class="flex items-center gap-2 mb-1.5">
              <span class="text-xs font-semibold tracking-tight {msg.sender === 'Player 1' ? 'text-slate-900' : 'text-slate-500'}">
                {msg.sender}
              </span>
              <span class="text-[10px] text-slate-400 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                {msg.time}
              </span>
            </div>

            <div class="text-[14px] text-slate-800 leading-relaxed max-w-xl font-light">
              {msg.text}
            </div>

          </div>
        {/each}

      </div>
    </div>

    <div class="p-6 md:px-16 bg-white border-t border-slate-100 shrink-0">
      <div class="max-w-2xl mx-auto flex flex-col gap-3">
        <form class="flex items-center gap-4" onsubmit={sendMessage}>
          <input 
            type="text" 
            bind:value={newMessage}
            disabled={selectedSliceId !== null}
            placeholder={selectedSliceId !== null ? "Cannot type in history mode..." : "Write as you wish..."} 
            class="flex-1 bg-transparent border-none outline-none text-[14px] text-slate-900 placeholder-slate-400 disabled:opacity-50"
          >
          <div class="flex items-center gap-3">
            {#if newMessage.trim() && selectedSliceId === null}
              <button type="submit" class="text-xs font-bold tracking-wider uppercase text-slate-900 hover:text-black transition-colors">
                Attack 💥
              </button>
            {:else}
              <button type="button" class="text-slate-400 hover:text-slate-800 text-sm transition-colors disabled:opacity-30" disabled={selectedSliceId !== null}>Attach</button>
            {/if}
          </div>
        </form>
      </div>
    </div>

  </main>

  <MobileNav />

</div>

<style>
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