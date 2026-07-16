<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import gsap from 'gsap';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';
  import { authStore } from '$lib/stores/auth.svelte';
  import { ApiService, API_BASE_URL } from '$lib/api';
  import type { PostDTO, UserDTO } from '$lib/types';
  import PostModal from '$lib/components/PostModal.svelte';
  import UserListModal from '$lib/components/UserListModal.svelte';
  
  let targetUsername = $derived($page.params.username);

  let isLoading = $state(true);
  let isFollowing = $state(false);
  let isActionLoading = $state(false);

  let user = $state({
    id: '',
    username: '',
    fullName: '',
    bio: '',
    posts: 0,
    followers: 0,
    following: 0,
    avatarColor: 'bg-slate-900 border border-slate-800 text-white shadow-sm shadow-slate-200',
    avatarLetter: '',
    avatar: ''
  });

  let fileInput: HTMLInputElement;
  let isUploadingAvatar = $state(false);

  // User List Modal State
  let isUserListOpen = $state(false);
  let userListTitle = $state('');
  let userListUsers = $state<UserDTO[]>([]);

  function triggerAvatarUpload() {
    if (authStore.user?.username === targetUsername) {
      fileInput.click();
    }
  }

  async function handleFileSelect(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      const file = target.files[0];
      if (!authStore.token) return;
      isUploadingAvatar = true;
      try {
        const updatedUser = await ApiService.uploadAvatar(file, authStore.token);
        user.avatar = updatedUser.avatar;
        if (authStore.user) {
          authStore.user.avatar = updatedUser.avatar;
          localStorage.setItem('avatar', updatedUser.avatar);
        }
      } catch (err) {
        console.error("Avatar yüklenemedi", err);
        alert("Avatar yüklenirken bir hata oluştu.");
      } finally {
        isUploadingAvatar = false;
        if (fileInput) fileInput.value = '';
      }
    }
  }

  let posts: any[] = $state([]);
  let selectedPost = $state<PostDTO | null>(null);

  $effect(() => {
    if (authStore.isAuthenticated && authStore.user && authStore.token) {
      if (targetUsername === authStore.user.username) {
        goto('/profile');
        return;
      }
      loadUserData();
    }
  });

  async function loadUserData() {
    if (!authStore.token || !targetUsername) return;
    isLoading = true;
    try {
      // 1. Get user data by username
      const data = await ApiService.getUserByUsername(targetUsername, authStore.token);
      user.id = data.id || '';
      user.username = data.username || '';
      user.fullName = data.fullName || '';
      user.bio = data.bio || "";
      user.followers = data.followersCount || 0;
      user.following = data.followingCount || 0;
      user.posts = data.postsCount || 0;
      user.avatarLetter = user.username ? user.username.charAt(0).toUpperCase() : '?';
      user.avatar = data.avatar || '';

      // 2. Fetch User Posts
      const userPostsData = await ApiService.getUserPosts(user.id, authStore.token);
      posts = userPostsData.map((p, index) => ({
        ...p,
        size: index === 0 ? 'col-span-2 row-span-2 h-[340px]' : 
              index === 2 ? 'col-span-1 row-span-2 h-[340px]' : 
              index === 4 ? 'col-span-2 row-span-1 h-[160px]' : 
              'col-span-1 row-span-1 h-[160px]'
      }));

      // 3. Check if we are following this user
      if (authStore.user) {
        const myFollowingList = await ApiService.getFollowing(authStore.user.id, authStore.token);
        isFollowing = myFollowingList.some(u => u.id.toString() === user.id.toString());
      }

      setTimeout(() => {
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
      }, 50);

    } catch (err) {
      console.error("Kullanıcı yüklenemedi", err);
    } finally {
      isLoading = false;
    }
  }

  async function toggleFollow() {
    if (!authStore.token || !user.id || isActionLoading) return;
    
    isActionLoading = true;
    try {
      if (isFollowing) {
        await ApiService.unfollowUser(user.id, authStore.token);
        isFollowing = false;
        user.followers = Math.max(0, user.followers - 1);
      } else {
        await ApiService.followUser(user.id, authStore.token);
        isFollowing = true;
        user.followers += 1;
      }
    } catch (err) {
      console.error("Takip işlemi başarısız", err);
      alert("İşlem sırasında bir hata oluştu.");
    } finally {
      isActionLoading = false;
    }
  }

  async function startMessage() {
    if (!user.id || !authStore.token) return;
    try {
      const existingRooms = await ApiService.getChatRooms(authStore.token);
      const existingRoom = existingRooms.find(r => 
        !r.isGroup && r.members && r.members.some(m => m.id.toString() === user.id.toString())
      );
      
      let roomId;
      if (existingRoom) {
        roomId = existingRoom.id;
      } else {
        const newRoom = await ApiService.createChatRoom(parseInt(user.id), authStore.token);
        roomId = newRoom.id;
      }
      
      goto(`/chat?roomId=${roomId}`);
    } catch (err) {
      console.error("Mesaj başlatılamadı", err);
      goto('/chat');
    }
  }

  async function openFollowersModal() {
    if (!authStore.token || !user.id) return;
    try {
      userListUsers = await ApiService.getFollowers(user.id, authStore.token);
      userListTitle = 'Takipçiler';
      isUserListOpen = true;
    } catch (err) {
      console.error("Followers fetch error", err);
    }
  }

  async function openFollowingModal() {
    if (!authStore.token || !user.id) return;
    try {
      userListUsers = await ApiService.getFollowing(user.id, authStore.token);
      userListTitle = 'Takip Edilenler';
      isUserListOpen = true;
    } catch (err) {
      console.error("Following fetch error", err);
    }
  }

</script>

<svelte:head>
  <title>@{targetUsername} / Transcendence</title>
</svelte:head>

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">

  <Sidebar />

  <main class="flex-1 overflow-y-auto custom-scrollbar flex flex-col lg:flex-row pb-20 lg:pb-0">
    
    {#if isLoading}
      <div class="flex-1 flex items-center justify-center">
        <span class="w-8 h-8 border-4 border-slate-900 border-t-transparent rounded-full animate-spin"></span>
      </div>
    {:else if !user.id}
      <div class="flex-1 flex flex-col items-center justify-center p-12 text-center animate-fade-in-up">
        <span class="text-6xl mb-4">👨‍🦯</span>
        <h2 class="text-2xl font-bold text-slate-900">Kullanıcı Bulunamadı</h2>
        <p class="text-slate-500 mt-2 max-w-sm">Böyle birisi sistemde yok veya adını yanlış yazdın. Belki de bu diyarlardan çoktan göçüp gitmiştir...</p>
        <a href="/" class="mt-6 px-6 py-2.5 bg-slate-900 text-white rounded-xl font-bold text-sm shadow-sm hover:bg-black transition-colors">Ana Sayfaya Dön</a>
      </div>
    {:else}
      <section class="editorial-sidebar w-full lg:w-[380px] lg:h-screen lg:border-r border-slate-100 bg-white p-8 md:p-12 flex flex-col justify-between shrink-0">
        
        <div class="flex flex-col gap-8">
          <!-- svelte-ignore a11y_click_events_have_key_events -->
          <!-- svelte-ignore a11y_no_static_element_interactions -->
          <div 
            class="relative w-24 h-24 rounded-[2rem] overflow-hidden flex items-center justify-center font-bold text-4xl shadow-sm border-[3px] border-slate-50 transition-all 
            {authStore.user?.username === targetUsername ? 'cursor-pointer hover:opacity-80' : ''} 
            {!user.avatar ? user.avatarColor : 'bg-slate-100 text-slate-800'}"
            onclick={triggerAvatarUpload}
          >
            {#if isUploadingAvatar}
              <span class="w-6 h-6 border-2 border-slate-900 border-t-transparent rounded-full animate-spin"></span>
            {:else if user.avatar}
              <img src={user.avatar.startsWith('http') ? user.avatar : `${API_BASE_URL}${user.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
            {:else}
              {user.avatarLetter}
            {/if}
            
            {#if authStore.user?.username === targetUsername && !isUploadingAvatar}
              <div class="absolute inset-0 bg-black/40 opacity-0 hover:opacity-100 flex items-center justify-center transition-opacity">
                <span class="text-white text-xs font-medium">Değiştir</span>
              </div>
            {/if}
          </div>
          <input type="file" accept="image/*" class="hidden" bind:this={fileInput} onchange={handleFileSelect} />

          <div>
            <h1 class="text-2xl font-semibold tracking-tight text-slate-900">{user.fullName || user.username}</h1>
            <p class="text-[15px] font-medium text-slate-400 mt-1">@{user.username}</p>
          </div>

          <div class="flex flex-col gap-3 py-4 border-y border-slate-100 w-full font-mono text-xs">
            <div class="flex justify-between text-slate-500 items-center">
              <span>Gönderiler</span>
              <span class="font-bold text-slate-900 bg-slate-50 px-2 py-1 rounded">{user.posts}</span>
            </div>
            <!-- svelte-ignore a11y_click_events_have_key_events -->
            <!-- svelte-ignore a11y_no_static_element_interactions -->
            <div class="flex justify-between text-slate-500 items-center cursor-pointer hover:text-slate-900 transition-colors" onclick={openFollowersModal}>
              <span>Takipçi</span>
              <span class="font-bold text-slate-900 bg-slate-50 px-2 py-1 rounded">{user.followers}</span>
            </div>
            <!-- svelte-ignore a11y_click_events_have_key_events -->
            <!-- svelte-ignore a11y_no_static_element_interactions -->
            <div class="flex justify-between text-slate-500 items-center cursor-pointer hover:text-slate-900 transition-colors" onclick={openFollowingModal}>
              <span>Takip Edilen</span>
              <span class="font-bold text-slate-900 bg-slate-50 px-2 py-1 rounded">{user.following}</span>
            </div>
          </div>

          {#if user.bio}
            <div class="text-[14px] text-slate-600 leading-relaxed whitespace-pre-line font-medium bg-slate-50/50 p-4 rounded-2xl border border-slate-100">
              {user.bio}
            </div>
          {/if}
        </div>

        <div class="flex flex-col gap-2 mt-8 lg:mt-0 w-full">
          <div class="flex gap-2 w-full">
            <button onclick={startMessage} title="Mesaj Gönder" class="w-12 h-12 shrink-0 rounded-xl bg-slate-100 border border-slate-200 text-slate-900 flex items-center justify-center hover:bg-slate-200 transition-colors shadow-sm">
              <svg viewBox="0 0 24 24" aria-hidden="true" class="w-6 h-6 fill-current"><g><path d="M1.998 5.5c0-1.381 1.119-2.5 2.5-2.5h15c1.381 0 2.5 1.119 2.5 2.5v13c0 1.381-1.119 2.5-2.5 2.5h-15c-1.381 0-2.5-1.119-2.5-2.5v-13zm2.5-.5c-.276 0-.5.224-.5.5v2.764l8 3.638 8-3.636V5.5c0-.276-.224-.5-.5-.5h-15zm15.5 5.463l-8 3.636-8-3.638V18.5c0 .276.224.5.5.5h15c.276 0 .5-.224.5-.5v-8.037z"></path></g></svg>
            </button>
            <div class="flex-1">
              {#if isFollowing}
                <button onclick={toggleFollow} disabled={isActionLoading} class="w-full bg-slate-50 text-slate-900 text-[14px] font-bold py-3.5 rounded-xl hover:bg-red-50 hover:text-red-600 transition-all shadow-sm disabled:opacity-50 border border-slate-200 hover:border-red-200 flex justify-center items-center h-12">
                  {#if isActionLoading}
                    <span class="inline-block w-4 h-4 border-2 border-slate-900 border-t-transparent rounded-full animate-spin"></span>
                  {:else}
                    Takipten Çık
                  {/if}
                </button>
              {:else}
                <button onclick={toggleFollow} disabled={isActionLoading} class="w-full bg-slate-900 text-white text-[14px] font-bold py-3.5 rounded-xl hover:bg-black transition-all shadow-sm shadow-slate-900/20 hover:shadow-md hover:shadow-slate-900/30 hover:-translate-y-0.5 disabled:opacity-50 flex justify-center items-center h-12">
                  {#if isActionLoading}
                    <span class="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                  {:else}
                    Takip Et
                  {/if}
                </button>
              {/if}
            </div>
          </div>
        </div>

      </section>

      <section class="flex-1 p-8 md:p-12 lg:p-16 overflow-y-auto custom-scrollbar bg-[#fcfcfc]">
        
        <div class="flex gap-8 text-[11px] font-bold tracking-wider uppercase text-slate-400 mb-10 border-b border-slate-100 pb-4">
          <button class="text-slate-950 flex items-center gap-1.5 relative">
            <span class="w-1 h-1 rounded-full bg-slate-950 absolute -bottom-4 left-1/2 -translate-x-1/2"></span>
            Gönderiler
          </button>
        </div>

        {#if posts.length === 0}
          <div class="w-full h-40 border-2 border-dashed border-slate-200 rounded-3xl flex items-center justify-center text-slate-400 font-medium text-sm">
            Kullanıcı henüz hiçbir şey paylaşmamış.
          </div>
        {:else}
          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 auto-rows-max max-w-4xl">
            {#each posts as post}
              <button onclick={() => selectedPost = post} class="portfolio-item {post.size || 'col-span-1 row-span-1 h-[160px]'} rounded-2xl cursor-pointer relative group overflow-hidden transition-all duration-300 hover:-translate-y-1 shadow-sm hover:shadow-md border border-slate-100 p-0 text-left w-full block">
                
                {#if post.imageUrl}
                  <img src={post.imageUrl.startsWith('http') ? post.imageUrl : API_BASE_URL + post.imageUrl} class="w-full h-full object-cover transition-transform duration-500 group-hover:scale-[1.02]" alt="Post" />
                {:else}
                  <div class="w-full h-full bg-gradient-to-br from-indigo-50 via-white to-cyan-50 border-2 border-transparent group-hover:border-indigo-100 p-6 flex flex-col items-center justify-center text-center transition-all duration-500 group-hover:scale-[1.02]">
                    <div class="text-3xl mb-3 opacity-20">❝</div>
                    <p class="text-slate-600 font-medium text-base md:text-lg line-clamp-4 leading-relaxed px-4">{post.content}</p>
                  </div>
                {/if}

                <div class="absolute inset-0 bg-gradient-to-t from-slate-950/90 via-slate-950/20 to-transparent opacity-0 group-hover:opacity-100 transition-all duration-300 flex items-end p-6 text-white">
                  <div class="flex items-center gap-5 text-sm font-semibold tracking-wide">
                    <div class="flex items-center gap-2">
                      <span class="text-red-400 text-lg">♥</span> {post.likesCount || 0}
                    </div>
                    <div class="flex items-center gap-2">
                      <span class="text-slate-300 text-lg">💭</span> {post.commentsCount || 0}
                    </div>
                  </div>
                </div>

              </button>
            {/each}
          </div>
        {/if}

      </section>
    {/if}
  </main>

  <MobileNav />

  <PostModal bind:post={selectedPost} onClose={() => selectedPost = null} />

  <UserListModal 
    bind:isOpen={isUserListOpen} 
    title={userListTitle} 
    users={userListUsers} 
    onclose={() => isUserListOpen = false} 
  />
</div>

<style>
  .custom-scrollbar::-webkit-scrollbar { width: 3px; }
  .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
  .custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 4px; }
</style>
