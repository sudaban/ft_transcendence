<script lang="ts">
  import { onMount } from 'svelte';
  import { ApiService } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';
  import type { ChatRoomDTO, MessageDTO, UserDTO } from '$lib/types';
  import * as signalR from "@microsoft/signalr";

  let chatRooms = $state<ChatRoomDTO[]>([]);
  let selectedRoomId = $state<number | null>(null);
  let messages = $state<MessageDTO[]>([]);
  let newMessageContent = $state('');
  let isLoadingRooms = $state(true);
  let isLoadingMessages = $state(false);
  let isSending = $state(false);

  let hubConnection: signalR.HubConnection | null = null;
  let messagesContainer: HTMLElement;

  onMount(async () => {
    if (!authStore.token) return;
    
    try
    {
      chatRooms = await ApiService.getChatRooms(authStore.token);
      
      hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/chathub`, {
          accessTokenFactory: () => authStore.token || ''
        })
        .withAutomaticReconnect()
        .build();

      hubConnection.on("ReceiveMessage", (message: MessageDTO) => {
        if (selectedRoomId === message.chatRoomId) {
          messages = [...messages, message];
          scrollToBottom();
        }
      });

      await hubConnection.start();
      console.log("SignalR connected.");
    }
    catch (err)
    {
      console.error("Error loading chat rooms or connecting to SignalR:", err);
    }
    finally
    {
      isLoadingRooms = false;
    }

    return () => {
      if (hubConnection) {
        hubConnection.stop();
      }
    };
  });

  async function selectRoom(roomId: number)
  {
    if (!authStore.token)
      return;
    
    if (selectedRoomId)
    {
      await hubConnection?.invoke("LeaveRoom", selectedRoomId.toString());
    }

    selectedRoomId = roomId;
    isLoadingMessages = true;
    try
    {
      messages = await ApiService.getChatMessages(roomId, authStore.token);
      
      await hubConnection?.invoke("JoinRoom", roomId.toString());
      scrollToBottom();
    }
    catch (err)
    {
      console.error("Mesajlar yüklenemedi", err);
    }
    finally
    {
      isLoadingMessages = false;
    }
  }

  async function handleSendMessage()
  {
    if (!authStore.token || !selectedRoomId || !newMessageContent.trim())
      return;

    isSending = true;
    try
    {
      const sentMsg = await ApiService.sendMessage(selectedRoomId, newMessageContent.trim(), authStore.token);
      
      if (!messages.find(m => m.id === sentMsg.id))
      {
        messages = [...messages, sentMsg];
      }
      
      newMessageContent = '';
      scrollToBottom();
    }
    catch (err)
    {
      console.error("Mesaj gönderilemedi", err);
    }
    finally
    {
      isSending = false;
    }
  }

  function scrollToBottom() {
    setTimeout(() => {
      if (messagesContainer) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
      }
    }, 100);
  }

  function getOtherUser(room: ChatRoomDTO): UserDTO | null {
    if (!room.members || room.members.length === 0) return null;
    return room.members.find(m => m.id.toString() !== authStore.user?.id?.toString()) || room.members[0];
  }

  function getAvatarInitial(username?: string) {
    return username ? username.substring(0, 2).toUpperCase() : 'U';
  }
</script>

<div class="min-h-screen bg-social-bg text-social-primary flex justify-center">

  <Sidebar />

  <main class="flex-1 max-w-[900px] w-full min-h-screen border-x border-social-border flex flex-col md:flex-row pb-[60px] md:pb-0">
    
    <!-- Chat List -->
    <div class="w-full md:w-[350px] border-r border-social-border flex flex-col h-full md:h-screen {selectedRoomId ? 'hidden md:flex' : 'flex'}">
      <div class="sticky top-0 bg-social-bg/80 backdrop-blur-md p-4 border-b border-social-border z-10 flex justify-between items-center">
        <h1 class="text-xl font-bold">Mesajlar</h1>
      </div>

      <div class="flex-1 overflow-y-auto custom-scrollbar">
        {#if isLoadingRooms}
          <div class="flex justify-center p-8">
            <span class="w-8 h-8 border-4 border-[#1d9bf0] border-t-transparent rounded-full animate-spin"></span>
          </div>
        {:else if chatRooms.length === 0}
          <div class="p-8 text-center text-social-secondary flex flex-col items-center">
            <div class="text-4xl mb-3">💬</div>
            <h2 class="text-xl font-bold mb-2 text-social-primary">Sohbet kutun boş</h2>
            <p class="text-[15px]">Burada aktif sohbetlerin görünecek.</p>
          </div>
        {:else}
          {#each chatRooms as room}
            {@const otherUser = getOtherUser(room)}
            <button 
              onclick={() => selectRoom(room.id)}
              class="w-full p-4 flex gap-3 hover:bg-gray-50 transition-colors border-b border-social-border/50 text-left {selectedRoomId === room.id ? 'bg-gray-50 border-r-2 border-r-[#1d9bf0]' : ''}"
            >
              <div class="shrink-0 w-12 h-12 rounded-full bg-slate-900 flex items-center justify-center text-white font-bold text-sm">
                {otherUser?.avatar || getAvatarInitial(otherUser?.username)}
              </div>
              <div class="flex-1 min-w-0 flex flex-col justify-center">
                <div class="flex justify-between items-center mb-1">
                  <span class="font-bold text-[15px] truncate text-slate-900">{otherUser?.username || "Bilinmeyen Kullanıcı"}</span>
                  <span class="text-xs text-slate-500 whitespace-nowrap ml-2">
                    {new Date(room.createdAt).toLocaleDateString('tr-TR')}
                  </span>
                </div>
                <span class="text-social-secondary text-[14px] truncate">
                  <!-- Last message preview goes here -->
                  Sohbete katıl...
                </span>
              </div>
            </button>
          {/each}
        {/if}
      </div>
    </div>

    <!-- Chat Messages -->
    <div class="flex-1 flex flex-col h-[calc(100vh-60px)] md:h-screen {!selectedRoomId ? 'hidden md:flex' : 'flex'}">
      {#if !selectedRoomId}
        <div class="flex-1 flex flex-col items-center justify-center p-8 text-center">
          <div class="text-6xl mb-4">📫</div>
          <h2 class="text-2xl font-bold mb-2">Bir mesaj seç</h2>
          <p class="text-social-secondary">Mevcut sohbetlerinden birini seç veya yeni bir sohbet başlat.</p>
        </div>
      {:else}
        {@const currentRoom = chatRooms.find(r => r.id === selectedRoomId)}
        {@const otherUser = currentRoom ? getOtherUser(currentRoom) : null}

        <!-- Chat Header -->
        <div class="sticky top-0 bg-social-bg/80 backdrop-blur-md p-4 border-b border-social-border z-10 flex items-center gap-4">
          <button class="md:hidden p-2 -ml-2 rounded-full hover:bg-gray-100" onclick={() => selectedRoomId = null}>
            ⬅️
          </button>
          <a href="/profile/{otherUser?.username}" class="flex items-center gap-3 group">
            <div class="w-10 h-10 rounded-full bg-slate-900 flex items-center justify-center text-white font-bold text-xs group-hover:opacity-80 transition-opacity">
              {otherUser?.avatar || getAvatarInitial(otherUser?.username)}
            </div>
            <div class="flex flex-col">
              <span class="font-bold text-[15px] hover:underline">{otherUser?.username || "Bilinmeyen Kullanıcı"}</span>
              <span class="text-social-secondary text-xs">{otherUser?.handle || "@user"}</span>
            </div>
          </a>
        </div>

        <!-- Messages Area -->
        <div class="flex-1 overflow-y-auto p-4 custom-scrollbar flex flex-col gap-4 bg-slate-50/50" bind:this={messagesContainer}>
          {#if isLoadingMessages}
            <div class="flex justify-center p-8">
              <span class="w-8 h-8 border-4 border-[#1d9bf0] border-t-transparent rounded-full animate-spin"></span>
            </div>
          {:else if messages.length === 0}
            <div class="text-center p-8 text-social-secondary">
              İlk mesajı sen gönder! 👋
            </div>
          {:else}
            {#each messages as msg}
              {@const isMe = msg.senderId.toString() === authStore.user?.id?.toString()}
              <div class="flex {isMe ? 'justify-end' : 'justify-start'} animate-fade-in-up">
                <div class="max-w-[75%] rounded-2xl p-3 {isMe ? 'bg-[#1d9bf0] text-white rounded-tr-sm' : 'bg-white border border-gray-200 text-slate-800 rounded-tl-sm'} shadow-sm">
                  <p class="text-[15px] leading-snug whitespace-pre-wrap">{msg.content}</p>
                  <div class="text-[11px] mt-1 text-right {isMe ? 'text-blue-100' : 'text-gray-400'}">
                    {new Date(msg.sentAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
              </div>
            {/each}
          {/if}
        </div>

        <!-- Input Area -->
        <div class="p-4 bg-white border-t border-social-border flex gap-3 items-end">
          <textarea 
            bind:value={newMessageContent}
            onkeydown={(e) => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSendMessage(); } }}
            placeholder="Yeni bir mesaj yaz..."
            class="flex-1 bg-gray-100 rounded-2xl py-3 px-4 resize-none outline-none focus:ring-2 focus:ring-[#1d9bf0]/20 focus:bg-white transition-all text-[15px] max-h-32"
            rows="1"
            disabled={isSending}
          ></textarea>
          <button 
            onclick={handleSendMessage}
            disabled={!newMessageContent.trim() || isSending}
            class="bg-[#1d9bf0] hover:bg-[#1a8cd8] text-white rounded-full w-11 h-11 flex items-center justify-center shrink-0 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <svg viewBox="0 0 24 24" aria-hidden="true" class="w-5 h-5 fill-current"><g><path d="M2.504 21.866l.526-2.108C3.04 19.719 4 15.823 4 12s-.96-7.719-.97-7.757l-.527-2.109L22.236 12 2.504 21.866zM5.981 13c-.072 1.962-.34 3.833-.583 5.183L17.764 12 5.398 5.818c.242 1.349.51 3.221.583 5.183H10v2H5.981z"></path></g></svg>
          </button>
        </div>
      {/if}
    </div>

  </main>

  <MobileNav />

</div>
