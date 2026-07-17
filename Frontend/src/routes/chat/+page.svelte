<script lang="ts">
  import { onMount, onDestroy, tick } from 'svelte';
  import { page } from '$app/stores';
  import gsap from 'gsap';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';
  
  let isMobileSidebarOpen = $state(false);

  import { ApiService, API_BASE_URL } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';
  import type { ChatRoomDTO, MessageDTO, UserDTO } from '$lib/types';
  import * as signalR from "@microsoft/signalr";
  import { toastStore } from '$lib/stores/toast.svelte';

  let chatRooms = $state<ChatRoomDTO[]>([]);
  let selectedRoomId = $state<number | null>(null);
  let isLoadingRooms = $state(true);
  let isLoadingMessages = $state(false);
  let isSending = $state(false);
  let hubConnection: signalR.HubConnection | null = null;
  let connectionPromise: Promise<void> | null = null;

  let newMessage = $state('');
  let aiTyping = $state(false);
  let aiStreamingText = $state('');
  let asciiHistory = $state<string[]>([]);
  let archivedSlices = $state<Array<{ id: number, label: string, char: string, leader: string | null, data: any[] }>>([]);
  let selectedSliceId = $state<number | null>(null);
  let isHistoryOpen = $state(false);
  let currentLoopLeader = $state<string | null>(null);
  let liveMessages = $state<any[]>([]);
  let chatContainer: HTMLElement;

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

  let isInitialized = false;

  async function initializeChat()
  {
    try
    {
      chatRooms = await ApiService.getChatRooms(authStore.token);
      
      hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/chathub`, { 
          accessTokenFactory: () => authStore.token || '',
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect()
        .build();

      hubConnection.on("ReceiveMessage", (message: MessageDTO) => {
        if (selectedRoomId === message.chatRoomId)
        {
          aiStreamingText = '';
          aiTyping = false;
          const room = chatRooms.find(r => r.id === selectedRoomId);
          if (room)
          {
            handleIncomingMessage(formatMessage(message, room));
          }
        }
      });

      hubConnection.on("AiTyping", (roomId: number, isTyping: boolean) => {
        if (selectedRoomId === roomId)
        {
          aiTyping = isTyping;
          if (!isTyping) aiStreamingText = '';
          scrollToBottom();
        }
      });

      hubConnection.on("AiMessageChunk", (roomId: number, delta: string) => {
        if (selectedRoomId === roomId)
        {
          aiStreamingText += delta;
          scrollToBottom();
        }
      });

      connectionPromise = hubConnection.start();
      await connectionPromise;
      
    }
    catch(err: any)
    {
      toastStore.error(err.message || "Odalar yüklenemedi.");
    }
    finally
    {
      isLoadingRooms = false;
    }
    
    if (chatRooms.length > 0)
    {
      await tick();
      if (document.querySelector('.horizontal-inbox-item'))
      {
        gsap.fromTo('.horizontal-inbox-item', 
          { opacity: 0, scale: 0.9 },
          { opacity: 1, scale: 1, duration: 0.4, stagger: 0.05, ease: 'power2.out' }
        );
      }
      
      const queryRoomId = $page.url.searchParams.get('roomId');
      if (queryRoomId)
      {
        const roomId = parseInt(queryRoomId);
        if (!isNaN(roomId))
        {
          selectRoom(roomId);
        }
      }
    }
    
    scrollToBottom();
  }

  $effect(() => {
    if (authStore.token && !isInitialized)
    {
      isInitialized = true;
      initializeChat();
    }
  });

  onDestroy(async () => {
    if (hubConnection)
    {
      if (connectionPromise)
      {
        try
        {
          await connectionPromise;
        }
        catch (e)
        {
        }
      }
      try
      {
        await hubConnection.stop();
      }
      catch (err)
      {
      }
    }
  });

  async function selectRoom(roomId: number)
  {
    if (!authStore.token)
      return;
    
    if (selectedRoomId && hubConnection?.state === signalR.HubConnectionState.Connected)
    {
      try
      {
        await hubConnection.invoke("LeaveRoom", selectedRoomId.toString());
      }
      catch (err: any)
      {
        toastStore.warn(err.message || "Oda değiştirilirken uyarı.");
      }
    }

    selectedRoomId = roomId;
    isLoadingMessages = true;
    selectedSliceId = null;
    aiTyping = false;
    aiStreamingText = '';

    try
    {
      const room = chatRooms.find(r => r.id === roomId);
      if (!room)
      {
        selectedRoomId = null;
        throw new Error("Bu sohbet odasına erişim izniniz yok veya oda mevcut değil.");
      }

      const rawMessages = await ApiService.getChatMessages(roomId, authStore.token);

      
      archivedSlices = [];
      asciiHistory = [];
      liveMessages = [];
      currentLoopLeader = null;

      let buffer: any[] = [];
      let tempLeader: string | null = null;
      
      for (const rawMsg of rawMessages)
      {
        const formatted = formatMessage(rawMsg, room);
        if (buffer.length === 0 && !tempLeader)
        {
          tempLeader = formatted.sender;
        }
        buffer.push(formatted);

        if (buffer.length === 8)
        {
          const bitString = buffer.map(m => m.sender === tempLeader ? '0' : '1').join('');
          const decimalValue = parseInt(bitString, 2);
          const asciiChar = (decimalValue >= 32 && decimalValue <= 126) ? String.fromCharCode(decimalValue) : '?';
          
          asciiHistory.push(asciiChar);
          archivedSlices.unshift({
            id: Date.now() + Math.random(),
            label: `#0${archivedSlices.length + 1}`,
            char: asciiChar,
            leader: tempLeader,
            data: [...buffer]
          });
          
          buffer = [];
          tempLeader = null;
        }
      }

      liveMessages = buffer;
      currentLoopLeader = tempLeader;

      if (hubConnection?.state === signalR.HubConnectionState.Connected)
      {
        try
        {
          await hubConnection.invoke("JoinRoom", roomId.toString());
        }
        catch (err: any)
        {
          toastStore.warn(err.message || "Odaya katılırken uyarı.");
        }
      }
      scrollToBottom();
    }
    catch (err: any)
    {
      toastStore.error(err.message || "Mesajlar yüklenemedi.");
    }
    finally
    {
      isLoadingMessages = false;
    }
  }

  function formatMessage(msg: MessageDTO, room: ChatRoomDTO): any
  {
    const isMe = msg.senderId.toString() === authStore.user?.id?.toString();
    let senderName = "Bilinmeyen";
    if (isMe)
      senderName = authStore.user?.username || "Ben";
    else
    {
      const otherUser = room.members?.find(m => m.id.toString() === msg.senderId.toString());
      if (otherUser)
        senderName = otherUser.username;
    }

    return {
      id: msg.id,
      sender: senderName,
      text: msg.content,
      time: new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      color: isMe ? 'border-slate-900' : 'border-slate-300'
    };
  }

  function handleIncomingMessage(formattedMsg: any)
  {
    const existsInLive = liveMessages.some(m => m.id === formattedMsg.id);
    const existsInArchive = archivedSlices.some(slice => slice.data.some(m => m.id === formattedMsg.id));
    
    if (existsInLive || existsInArchive)
    {
      return; 
    }

    if (liveMessages.length >= 8)
    {
      flushBufferToHistory();
    }
    
    if (liveMessages.length === 0 && !currentLoopLeader)
    {
      currentLoopLeader = formattedMsg.sender;
    }
    
    liveMessages.push(formattedMsg);
    scrollToBottom();
  }
  
  function flushBufferToHistory()
  {
    const bitString = liveMessages.map(msg => msg.sender === currentLoopLeader ? '0' : '1').join('');
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

  async function scrollToBottom()
  {
    await tick();
    if (chatContainer)
    {
      chatContainer.scrollTop = chatContainer.scrollHeight;
    }
  }

  function popIn(node: HTMLElement)
  {
    gsap.fromTo(node, 
      { opacity: 0, y: 12, filter: 'blur(4px)' }, 
      { opacity: 1, y: 0, filter: 'blur(0px)', duration: 0.4, ease: 'power2.out' }
    );
    scrollToBottom();
  }

  async function sendMessage(e?: Event)
  {
    if (e) e.preventDefault();
    if (!newMessage.trim() || !selectedRoomId || !authStore.token)
      return;

    if (selectedSliceId !== null)
    {
      alert("Geçmiş modundasınız. Mesaj göndermek için sol üstten canlı yayına dönün.");
      return;
    }

    isSending = true;
    try
    {
      const sentMsg = await ApiService.sendMessage(selectedRoomId, newMessage.trim(), authStore.token);
      const room = chatRooms.find(r => r.id === selectedRoomId);
      if (room)
      {
        handleIncomingMessage(formatMessage(sentMsg, room));
      }
      newMessage = '';
      scrollToBottom();
    }
    catch (err: any)
    {
      toastStore.error(err.message || "Mesaj gönderilemedi.");
    }
    finally
    {
      isSending = false;
    }
  }

  async function deleteCurrentRoom()
  {
    if (!selectedRoomId || !authStore.token)
      return;
    
    const confirmDelete = confirm("Bu sohbeti silmek istediğine emin misin?");
    if (!confirmDelete)
      return;

    try
    {
      await fetch(`${API_BASE_URL}/api/ChatRooms/${selectedRoomId}/hide`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${authStore.token}` }
      });
      
      chatRooms = chatRooms.filter(r => r.id !== selectedRoomId);
      
      if (hubConnection?.state === signalR.HubConnectionState.Connected)
      {
        try
        {
          await hubConnection.invoke("LeaveRoom", selectedRoomId.toString());
        }
        catch(e)
        {
        }
      }
      
      selectedRoomId = null;
      liveMessages = [];
      archivedSlices = [];
      asciiHistory = [];
      
      if (chatRooms.length > 0)
      {
        selectRoom(chatRooms[0].id);
      }
    }
    catch (err: any)
    {
      toastStore.error(err.message || "Sohbet silinirken hata.");
      alert("Sohbet silinemedi.");
    }
  }

  function getOtherUser(room: ChatRoomDTO): UserDTO | null
  {
    if (!room.members || room.members.length === 0)
      return null;
    return room.members.find(m => m.id.toString() !== authStore.user?.id?.toString()) || room.members[0];
  }

  function getAvatarInitial(username?: string)
  {
    return username ? username.substring(0, 2).toUpperCase() : 'U';
  }
</script>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">

  <Sidebar />

  {#if isMobileSidebarOpen}
    <div 
      class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-40 lg:hidden" 
      onclick={() => isMobileSidebarOpen = false}
    ></div>
  {/if}

  <aside class="
    fixed lg:relative inset-y-0 left-0 z-50 w-[280px] border-r border-slate-100 bg-white h-[100dvh] lg:h-screen flex flex-col shrink-0 px-8 pt-8 pb-24 lg:pb-8 justify-between overflow-y-auto custom-scrollbar transform transition-transform duration-300 ease-in-out lg:translate-x-0
    {isMobileSidebarOpen ? 'translate-x-0 shadow-2xl' : '-translate-x-full'}
  ">
    <button 
      onclick={() => isMobileSidebarOpen = false}
      class="lg:hidden absolute top-4 right-4 text-slate-400 hover:text-slate-900 p-2"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>

    <div class="flex flex-col items-start gap-6 w-full mt-4 lg:mt-0">
      
      <button 
        onclick={() => selectedSliceId = null}
        class="w-full flex items-center justify-between p-3 rounded-xl border transition-all duration-200
          {selectedSliceId === null ? 'bg-slate-900 text-white border-slate-900 shadow-sm' : 'bg-slate-50 text-slate-700 border-slate-200/60 hover:bg-slate-100'}"
      >
        <div class="flex items-center gap-3">
          <div class="w-2 h-2 rounded-full {selectedSliceId === null ? 'bg-emerald-400 animate-pulse' : 'bg-slate-400'}"></div>
          <span class="text-xs font-semibold tracking-tight">Canlı Sohbet</span>
        </div>
        <span class="text-[10px] font-mono">{liveMessages.length}/8 B</span>
      </button>

      <div>
        <h2 class="font-semibold text-base text-slate-900 tracking-tight">{authStore.user?.username || 'Kullanıcı'}</h2>
      </div>

      <div class="w-full mt-2 pt-4 border-t border-slate-100 flex flex-col gap-5">
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

    <div class="text-[11px] text-slate-400 font-mono pt-4 mt-4 border-t border-slate-50 flex items-center justify-between">
      {#if selectedRoomId}
        <button onclick={deleteCurrentRoom} class="text-red-400 hover:text-red-500 hover:underline transition-colors">Sohbeti Sil</button>
      {/if}
    </div>
  </aside>

  <main class="flex-1 flex flex-col h-[100dvh] pb-14 md:pb-0 min-w-0">
    
    <section class="h-[70px] md:h-[80px] border-b border-slate-100 bg-white/60 backdrop-blur-md flex items-center px-4 md:px-8 gap-3 shrink-0 overflow-x-auto no-scrollbar">
      <div class="flex items-center gap-2 pr-4 mr-2 border-r border-slate-200 shrink-0">
        <button onclick={() => isMobileSidebarOpen = true} class="lg:hidden text-slate-500 hover:text-slate-900 transition-colors p-1">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <div class="text-xs font-bold text-slate-400 tracking-wider uppercase">Chats</div>
      </div>
      {#if isLoadingRooms}
        <div class="text-xs font-mono text-slate-400">Loading...</div>
      {:else if chatRooms.length === 0}
        <div class="text-xs font-mono text-slate-400">No active sessions.</div>
      {:else}
        {#each chatRooms as room}
          {@const otherUser = getOtherUser(room)}
          {@const isActive = room.id === selectedRoomId}
          <button onclick={() => selectRoom(room.id)} class="horizontal-inbox-item flex items-center gap-2.5 px-3 py-1.5 rounded-full transition-all shrink-0
            {isActive ? 'bg-slate-900 text-white shadow-sm' : 'hover:bg-slate-100 text-slate-600'}"
          >
            <div class="w-6 h-6 rounded-full text-[10px] font-bold flex items-center justify-center overflow-hidden
              {isActive ? 'bg-white/20 text-white' : 'bg-slate-200 text-slate-700'}"
            >
              {#if otherUser?.avatar}
                <img src={otherUser.avatar.startsWith('http') ? otherUser.avatar : `${API_BASE_URL}${otherUser.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
              {:else}
                {getAvatarInitial(otherUser?.username)}
              {/if}
            </div>
            <span class="text-xs font-medium pr-1">{otherUser?.username || "Unknown"}</span>
          </button>
        {/each}
      {/if}
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
              {msg.sender === currentDisplayLeader ? '0' : '1'}
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

        {#if selectedSliceId === null && (aiTyping || aiStreamingText)}
          <div class="timeline-bubble relative flex flex-col">
            <div class="absolute -left-[33px] top-0.5 w-5 h-5 rounded-full bg-[#fcfcfc] flex items-center justify-center text-xs">
              🤖
            </div>
            <div class="flex items-center gap-2 mb-1.5">
              <span class="text-xs font-semibold tracking-tight text-slate-500">ai_assistant</span>
              <span class="text-[10px] text-slate-400 animate-pulse">typing...</span>
            </div>
            <div class="text-[14px] text-slate-800 leading-relaxed max-w-xl font-light">
              {#if aiStreamingText}
                {aiStreamingText}<span class="animate-pulse">▌</span>
              {:else}
                <span class="inline-flex gap-1">
                  <span class="w-1.5 h-1.5 rounded-full bg-slate-400 animate-bounce"></span>
                  <span class="w-1.5 h-1.5 rounded-full bg-slate-400 animate-bounce" style="animation-delay: 0.15s"></span>
                  <span class="w-1.5 h-1.5 rounded-full bg-slate-400 animate-bounce" style="animation-delay: 0.3s"></span>
                </span>
              {/if}
            </div>
          </div>
        {/if}

      </div>
    </div>

    <div class="p-4 md:p-6 md:px-16 bg-white border-t border-slate-100 shrink-0">
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
  .no-scrollbar::-webkit-scrollbar
  {
    display: none;
  }
  .no-scrollbar
  {
    -ms-overflow-style: none;
    scrollbar-width: none;
  }
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