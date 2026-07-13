<script lang="ts">
  import { onMount, tick } from 'svelte';
  import gsap from 'gsap';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';
  import { authStore } from '$lib/stores/auth.svelte';
  import { ApiService, API_BASE_URL } from '$lib/api';
  import type { PostDTO } from '$lib/types';
  import PostModal from '$lib/components/PostModal.svelte';
  import UserListModal from '$lib/components/UserListModal.svelte';
  import type { UserDTO } from '$lib/types';

  let isLoading = $state(true);
  let isEditing = $state(false);
  let isSaving = $state(false);

  let user = $state({
    id: '',
    username: '',
    fullName: '',
    bio: '',
    posts: 0,
    followers: 0,
    following: 0,
    avatarColor: 'bg-slate-900 border border-slate-800 text-white shadow-sm shadow-slate-200',
    avatarLetter: ''
  });

  let editForm = $state({
    fullName: '',
    bio: ''
  });

  let posts: any[] = $state([]);
  let selectedPost = $state<PostDTO | null>(null);
  let isPostModalOpen = $state(false);

  // User List Modal State
  let isUserListOpen = $state(false);
  let userListTitle = $state('');
  let userListUsers = $state<UserDTO[]>([]);

  let fileInput: HTMLInputElement;

  onMount(async () => {
    if (authStore.isAuthenticated && authStore.user && authStore.token) {
      try {
        const data = await ApiService.getUserById(authStore.user.id, authStore.token);
        user.id = data.id || '';
        user.username = data.username || '';
        user.fullName = data.fullName || '';
        user.bio = data.bio || "Bir Dev Bir Dev'e ne demiş Gel beraber Dev olalım demiş";
        user.followers = data.followersCount || 0;
        user.following = data.followingCount || 0;
        user.posts = data.postsCount || 0;
        user.avatarLetter = user.username ? user.username.charAt(0).toUpperCase() : '?';
        
        if (data.avatar) {
          if (authStore.user) authStore.user.avatar = data.avatar;
          localStorage.setItem('avatar', data.avatar);
        }

        // Fetch User Posts
        const userPostsData = await ApiService.getUserPosts(authStore.user.id, authStore.token);
        posts = userPostsData.map((p, index) => ({
          ...p,
          size: index === 0 ? 'col-span-2 row-span-2 h-[340px]' : 
                index === 2 ? 'col-span-1 row-span-2 h-[340px]' : 
                index === 4 ? 'col-span-2 row-span-1 h-[160px]' : 
                'col-span-1 row-span-1 h-[160px]'
        }));
        
        await tick();
        
        const tl = gsap.timeline({ defaults: { ease: 'power3.out' } });
        tl.fromTo('.editorial-sidebar', 
          { opacity: 0, x: -30 }, 
          { opacity: 1, x: 0, duration: 0.8 }
        );
        if (posts.length > 0) {
          tl.fromTo('.portfolio-item', 
            { opacity: 0, y: 20, scale: 0.98 }, 
            { opacity: 1, y: 0, scale: 1, duration: 0.5, stagger: 0.06 },
            "-=0.5"
          );
        }
      } catch (err) {
        console.error("Profil yüklenemedi", err);
      } finally {
        isLoading = false;
      }
    } else {
      isLoading = false;
    }
  });

  function openEditModal() {
    editForm.fullName = user.fullName;
    editForm.bio = user.bio;
    isEditing = true;
  }

  function closeEditModal() {
    isEditing = false;
  }

  async function saveProfile() {
    if (!authStore.token) return;
    isSaving = true;
    try {
      await ApiService.updateProfile({
        FullName: editForm.fullName,
        Bio: editForm.bio
      }, authStore.token);
      
      user.fullName = editForm.fullName;
      user.bio = editForm.bio;
      isEditing = false;
    } catch (err) {
      console.error("Güncelleme hatası:", err);
      alert("Profil güncellenirken bir hata oluştu.");
    } finally {
      isSaving = false;
    }
  }

  function handleAvatarClick() {
    fileInput.click();
  }

  let isUploadingAvatar = $state(false);

  async function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      const file = target.files[0];
      if (!authStore.token) return;
      isUploadingAvatar = true;
      try {
        const updatedUser = await ApiService.uploadAvatar(file, authStore.token);
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

<div class="min-h-screen bg-[#fcfcfc] text-slate-800 font-sans flex overflow-hidden selection:bg-slate-900 selection:text-white">

  <Sidebar />

  <main class="flex-1 overflow-y-auto custom-scrollbar flex flex-col lg:flex-row">
    
    <section class="editorial-sidebar w-full lg:w-[380px] lg:h-screen lg:border-r border-slate-100 bg-white p-8 md:p-12 flex flex-col justify-between shrink-0">
      
      <div class="flex flex-col gap-8">
        <div class="relative w-20 h-20 rounded-2xl cursor-pointer group overflow-hidden flex items-center justify-center font-bold text-xl transition-all duration-300 hover:scale-105 {!authStore.user?.avatar ? user.avatarColor : 'bg-slate-100 text-slate-800 shadow-sm border-[3px] border-slate-50'}" 
             onclick={handleAvatarClick} onkeydown={e => e.key === 'Enter' && handleAvatarClick()} role="button" tabindex="0">
          {#if isUploadingAvatar}
            <span class="w-6 h-6 border-2 border-slate-900 border-t-transparent rounded-full animate-spin"></span>
          {:else if authStore.user?.avatar}
            <img src={authStore.user.avatar.startsWith('http') ? authStore.user.avatar : `${API_BASE_URL}${authStore.user.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
          {:else}
            {user.avatarLetter}
          {/if}
          <div class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center text-xs text-white">
            Değiştir
          </div>
          <input bind:this={fileInput} type="file" accept="image/*" class="hidden" onchange={handleFileChange}>
        </div>

        <div>
          <h1 class="text-2xl font-semibold tracking-tight text-slate-900">{user.fullName}</h1>
          <p class="text-sm font-mono text-slate-400 mt-1">@{user.username}</p>
        </div>

        <div class="flex flex-col gap-3 py-4 border-y border-slate-100 w-full font-mono text-xs">
          <div class="flex justify-between text-slate-500">
            <span>Gönderiler</span>
            <span class="font-bold text-slate-900">{user.posts}</span>
          </div>
          <!-- svelte-ignore a11y_click_events_have_key_events -->
          <!-- svelte-ignore a11y_no_static_element_interactions -->
          <div class="flex justify-between text-slate-500 cursor-pointer hover:text-slate-900 transition-colors" onclick={openFollowersModal}>
            <span>Takipçi</span>
            <span class="font-bold text-slate-900">{user.followers}</span>
          </div>
          <!-- svelte-ignore a11y_click_events_have_key_events -->
          <!-- svelte-ignore a11y_no_static_element_interactions -->
          <div class="flex justify-between text-slate-500 cursor-pointer hover:text-slate-900 transition-colors" onclick={openFollowingModal}>
            <span>Takip Edilen</span>
            <span class="font-bold text-slate-900">{user.following}</span>
          </div>
        </div>

        <div class="text-[13px] text-slate-600 leading-relaxed whitespace-pre-line font-light tracking-wide">
          {user.bio}
        </div>
      </div>

      <div class="flex flex-col gap-2 mt-8 lg:mt-0 w-full">
        <button onclick={openEditModal} class="w-full bg-slate-900 text-white text-xs font-semibold tracking-wide uppercase py-3 rounded-xl hover:bg-black transition-colors shadow-sm">
          Edit Settings
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
          <button onclick={() => selectedPost = post} class="portfolio-item {post.size || 'col-span-1 row-span-1 h-[160px]'} rounded-2xl cursor-pointer relative group overflow-hidden transition-all duration-300 hover:-translate-y-1 p-0 text-left w-full block">
            
            {#if post.imageUrl}
              <img src={post.imageUrl.startsWith('http') ? post.imageUrl : API_BASE_URL + post.imageUrl} class="w-full h-full object-cover transition-transform duration-500 group-hover:scale-[1.02]" alt="Post" />
            {:else}
              <div class="w-full h-full bg-slate-100 transition-transform duration-500 group-hover:scale-[1.02]"></div>
            {/if}

            <div class="absolute inset-0 bg-gradient-to-t from-slate-950/80 via-slate-950/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex items-end p-6 text-white">
              <div class="flex items-center gap-5 text-xs font-mono tracking-wide">
                <div class="flex items-center gap-1.5">
                  <span class="text-slate-300">❤️</span> {post.likesCount || 0}
                </div>
                <div class="flex items-center gap-1.5">
                  <span class="text-slate-300">💬</span> {post.commentsCount || 0}
                </div>
              </div>
              <div class="absolute top-4 right-4 bg-white/10 backdrop-blur-sm px-2 py-0.5 rounded text-[9px] font-mono tracking-widest text-white/80">
                #{post.id}
              </div>
            </div>

          </button>
        {/each}
      </div>

    </section>

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

<!-- Edit Profile Modal -->
{#if isEditing}
<div class="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
  <div class="bg-white rounded-2xl w-full max-w-[400px] shadow-2xl p-6 flex flex-col gap-4 animate-fade-in-up">
    
    <div class="flex justify-between items-center border-b border-slate-100 pb-3">
      <h3 class="text-lg font-bold text-slate-900">Edit Profile</h3>
      <button onclick={closeEditModal} class="text-slate-400 hover:text-slate-700 transition-colors">✕</button>
    </div>

    <div class="flex flex-col gap-3">
      <label class="flex flex-col gap-1">
        <span class="text-xs font-bold text-slate-500 uppercase tracking-wide">Full Name</span>
        <input type="text" bind:value={editForm.fullName} class="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm outline-none focus:border-slate-900 transition-colors" placeholder="Adınız Soyadınız">
      </label>

      <label class="flex flex-col gap-1">
        <span class="text-xs font-bold text-slate-500 uppercase tracking-wide">Bio</span>
        <textarea bind:value={editForm.bio} rows="4" class="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm outline-none focus:border-slate-900 transition-colors resize-none" placeholder="Kendinizden bahsedin..."></textarea>
      </label>
    </div>

    <div class="flex justify-end gap-2 mt-2 pt-4 border-t border-slate-100">
      <button onclick={closeEditModal} class="px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-lg transition-colors">İptal</button>
      <button onclick={saveProfile} disabled={isSaving} class="px-5 py-2 text-sm font-semibold text-white bg-slate-900 hover:bg-black rounded-lg transition-colors flex items-center gap-2">
        {#if isSaving}
          <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
        {/if}
        Kaydet
      </button>
    </div>

  </div>
</div>
{/if}

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