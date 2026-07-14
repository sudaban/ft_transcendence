<script lang="ts">
  import { onMount } from 'svelte';
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';
  import { ApiService, API_BASE_URL } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';
  import type { PostDTO, UserDTO } from '$lib/types';
  import CommentsSection from '$lib/components/CommentsSection.svelte';

  let suggestions = $state<UserDTO[]>([]);
  let feedPosts = $state<PostDTO[]>([]);
  let isLoading = $state(true);
  
  let newPostContent = $state('');
  let isSubmitting = $state(false);
  
  let fileInput: HTMLInputElement;
  let selectedFile = $state<File | null>(null);

  onMount(async () => {
    if (!authStore.isAuthenticated || !authStore.token) {
      isLoading = false;
      return;
    }
    try {
      const [postsRes, usersRes, savedData] = await Promise.all([
        ApiService.getFeedPosts(authStore.token),
        ApiService.getSuggestedUsers(),
        ApiService.getSavedPosts(authStore.token).catch(() => [])
      ]);
      
      const savedIds = new Set(savedData.map((s: PostDTO) => s.id));
      
      feedPosts = postsRes.map(post => ({
        ...post,
        isSaved: savedIds.has(post.id)
      }));
      suggestions = usersRes;
      
      // eğer localstorage da avatar yoksa backend den çekiyoz
      if (authStore.user && (!authStore.user.avatar || authStore.user.avatar.length <= 3)) {
        try {
          const profile = await ApiService.getUserById(authStore.user.id, authStore.token);
          if (profile && profile.avatar) {
            authStore.user.avatar = profile.avatar;
            localStorage.setItem('avatar', profile.avatar);
          }
        } catch(e) {}
      }
    } catch (error) {
      console.error("Veriler çekilirken hata oluştu:", error);
    } finally {
      isLoading = false;
    }
  });

  function triggerFileInput() {
    fileInput.click();
  }

  function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      selectedFile = target.files[0];
    }
  }

  let openComments = $state<Record<number, boolean>>({});

  function toggleComments(postId: number) {
    openComments[postId] = !openComments[postId];
  }

  async function toggleLike(post: PostDTO) {
    if (!authStore.token) return;
    
    // Optimistic UI update
    const currentlyLiked = post.isLiked ?? false;
    post.isLiked = !currentlyLiked;
    post.likesCount += currentlyLiked ? -1 : 1;
    feedPosts = [...feedPosts];

    try {
      if (!currentlyLiked) {
        await ApiService.likePost(post.id, authStore.token);
      } else {
        await ApiService.unlikePost(post.id, authStore.token);
      }
    } catch (err) {
      console.error("Beğeni işlemi başarısız", err);
      // Revert optimistic update
      post.isLiked = currentlyLiked;
      post.likesCount += currentlyLiked ? 1 : -1;
      feedPosts = [...feedPosts];
    }
  }

  async function toggleSave(post: PostDTO) {
    if (!authStore.token) return;

    const currentlySaved = post.isSaved ?? false;
    post.isSaved = !currentlySaved;
    feedPosts = [...feedPosts];

    try {
      if (!currentlySaved) {
        await ApiService.savePost(post.id, authStore.token);
      } else {
        await ApiService.unsavePost(post.id, authStore.token);
      }
    } catch (err) {
      console.error("Kaydetme işlemi başarısız", err);
      post.isSaved = currentlySaved;
      feedPosts = [...feedPosts];
    }
  }

  async function handlePostSubmit() {
    if (!authStore.token) return;
    if (!selectedFile && !newPostContent.trim()) {
      alert("Gönderi paylaşmak için fotoğraf seçmeli veya metin yazmalısınız!");
      return;
    }
    
    isSubmitting = true;
    try {
      const formData = new FormData();
      if (selectedFile) {
        formData.append('File', selectedFile);
      }
      if (newPostContent.trim()) {
        formData.append('Content', newPostContent.trim());
      }
      
      const newPost = await ApiService.createPost(formData, authStore.token);
      
      // backend den dönen modelde Author null ise kendimiz dolduruyoruz
      // bunu yapmamın sebebi backend yeni gönderinin bilgilerini gönderirken kullanıcının bilgilerini göndermiyo
      if (!newPost.author && authStore.user)
      {
        newPost.author = {
          id: authStore.user.id,
          username: authStore.user.username,
          handle: '@' + authStore.user.username,
          avatar: authStore.user.avatar || '',
          followersCount: 0,
          followingCount: 0,
          postsCount: 0,
          isTwoFactorEnabled: false
        };
      }
      
      feedPosts = [newPost, ...feedPosts];
      newPostContent = '';
      selectedFile = null;
      if (fileInput) fileInput.value = '';
    } catch (error) {
      console.error("Post paylaşılamadı:", error);
    } finally {
      isSubmitting = false;
    }
  }

  async function handleDeletePost(postId: number) {
    if (!authStore.token) return;
    if (!confirm("Bu gönderiyi silmek istediğine emin misin?")) return;
    
    try {
      await ApiService.deletePost(postId, authStore.token);
      feedPosts = feedPosts.filter(p => p.id !== postId);
    } catch (err) {
      console.error("Post silinemedi", err);
      alert("Gönderi silinirken bir hata oluştu.");
    }
  }
</script>

<div class="min-h-screen bg-social-bg text-social-primary flex justify-center">
  <div class="w-full max-w-[1280px] flex justify-between">
    <!-- 1. LEFT SIDEBAR -->
  <Sidebar />

    <!-- 2. CENTER FEED -->
    <main class="flex-1 max-w-[700px] border-x border-social-border min-h-screen pb-20 md:pb-0 mx-auto">
    
    <!-- Header -->
    <div class="sticky top-0 bg-[rgba(255,255,255,0.85)] backdrop-blur-md z-10 border-b border-social-border">
      <h2 class="font-bold text-xl p-4 cursor-pointer">Keşfet</h2>
    </div>
    
    <!-- Create Post Area -->
    <div class="p-4 border-b border-social-border flex gap-3">
      <div class="w-10 h-10 rounded-full bg-gray-300 flex-shrink-0 flex items-center justify-center font-bold text-gray-600 overflow-hidden">
        {#if authStore.user?.avatar && authStore.user.avatar.length > 3}
          <img src={authStore.user.avatar.startsWith('http') ? authStore.user.avatar : `${API_BASE_URL}${authStore.user.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
        {:else}
          {authStore.user?.username?.substring(0, 2).toUpperCase() || '42'}
        {/if}
      </div>
      <div class="flex-1 flex flex-col pt-1">
        <textarea 
          bind:value={newPostContent}
          placeholder="What's going on bro ? Share with me! Cmon" 
          class="w-full bg-transparent outline-none text-xl resize-none placeholder-gray-500 overflow-hidden min-h-[50px]"
          rows="1"
          disabled={isSubmitting}
        ></textarea>
        
        {#if selectedFile}
          <div class="mt-2 text-sm text-[#1d9bf0] font-mono">
            📎 {selectedFile.name}
          </div>
        {/if}
        
        <div class="border-t border-social-border mt-3 pt-3 flex justify-between items-center">
          <div class="flex gap-2 text-[#1d9bf0]">
            <button onclick={triggerFileInput} class="w-9 h-9 rounded-full hover:bg-[#1d9bf0]/10 flex items-center justify-center transition-colors disabled:opacity-50" disabled={isSubmitting}>🖼️</button>
            <input bind:this={fileInput} type="file" accept="image/*,video/*" class="hidden" onchange={handleFileChange} />
            <button class="w-9 h-9 rounded-full hover:bg-[#1d9bf0]/10 flex items-center justify-center transition-colors disabled:opacity-50" disabled={isSubmitting}>😊</button>
          </div>
          <button 
            onclick={handlePostSubmit}
            disabled={(!newPostContent.trim() && !selectedFile) || isSubmitting}
            class="bg-[#1d9bf0] hover:bg-[#1a8cd8] text-white font-bold px-4 py-1.5 rounded-full disabled:opacity-50 transition-colors flex items-center gap-2"
          >
            {#if isSubmitting}
              <span class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            {/if}
            Post
          </button>
        </div>
      </div>
    </div>
    
    <!-- Feed Posts -->
    <div class="w-full flex flex-col relative">
      {#if isLoading}
        <div class="flex justify-center py-8">
          <span class="w-8 h-8 border-4 border-[#1d9bf0] border-t-transparent rounded-full animate-spin"></span>
        </div>
      {:else}
        {#each feedPosts as post}
        <article class="p-4 border-b border-social-border flex gap-3 hover:bg-gray-50 transition-colors cursor-pointer">
          <!-- Left Avatar -->
          <div class="shrink-0">
            <a href="/profile/{post.author.username}" class="w-10 h-10 rounded-full bg-gray-800 flex items-center justify-center text-white font-bold text-sm hover:opacity-80 transition-opacity overflow-hidden shrink-0">
              {#if post.author.avatar && post.author.avatar.length > 3}
                <img src={post.author.avatar.startsWith('http') ? post.author.avatar : `${API_BASE_URL}${post.author.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
              {:else}
                {post.author.avatar || post.author.username.substring(0, 2).toUpperCase()}
              {/if}
            </a>
          </div>
          
          <!-- Right Content -->
          <div class="flex-1 flex flex-col">
            <!-- Header -->
            <div class="flex items-center gap-1 mb-0.5 relative">
              <a href="/profile/{post.author.username}" class="font-bold text-[15px] hover:underline truncate">{post.author.username}</a>
              <span class="text-social-secondary text-[15px] truncate">{post.author.handle}</span>
              <span class="text-social-secondary text-[15px]">·</span>
              <span class="text-social-secondary text-[15px] hover:underline">
                {new Date(post.createdAt).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', hour: '2-digit', minute:'2-digit' })}
              </span>
              {#if authStore.user?.id?.toString() === post.author.id?.toString() || authStore.user?.role === 'Admin'}
                <button onclick={(e) => { e.stopPropagation(); handleDeletePost(post.id); }} class="ml-auto text-gray-400 hover:text-red-500 transition-colors" title="Gönderiyi Sil">
                  🗑️
                </button>
              {/if}
            </div>
            
            <!-- Body -->
            <div class="text-[15px] leading-normal text-social-primary mb-3 whitespace-pre-wrap">
              {post.content || ''}
            </div>
            
            {#if post.imageUrl}
              <div class="mb-3 rounded-2xl overflow-hidden border border-social-border">
                <img src={post.imageUrl.startsWith('http') ? post.imageUrl : API_BASE_URL + post.imageUrl} alt="Post media" class="w-full h-auto object-cover max-h-[500px]" />
              </div>
            {/if}
            
            <!-- Actions -->
            <div class="flex items-center justify-between text-social-secondary max-w-[425px]">
              <button onclick={() => toggleLike(post)} class="flex items-center gap-2 {post.isLiked ? 'text-[#f91880]' : 'hover:text-[#f91880]'} transition-colors group">
                <span class="w-8 h-8 rounded-full group-hover:bg-[#f91880]/10 flex items-center justify-center text-lg">
                  {post.isLiked ? '❤️' : '🤍'}
                </span>
                <span class="text-xs -ml-1">{post.likesCount || 0}</span>
              </button>
              <button onclick={() => toggleComments(post.id)} class="flex items-center gap-2 hover:text-[#1d9bf0] transition-colors group">
                <span class="w-8 h-8 rounded-full group-hover:bg-[#1d9bf0]/10 flex items-center justify-center">💬</span>
                <span class="text-xs -ml-1">{post.commentsCount || 0}</span>
              </button>
              <button onclick={() => toggleSave(post)} class="flex items-center gap-2 {post.isSaved ? 'text-[#1d9bf0]' : 'hover:text-[#1d9bf0]'} transition-colors group">
                <span class="w-8 h-8 rounded-full group-hover:bg-[#1d9bf0]/10 flex items-center justify-center text-lg">
                  {post.isSaved ? '📥' : '💾'}
                </span>
              </button>
            </div>
            
            {#if openComments[post.id]}
              <CommentsSection 
                postId={post.id} 
                onCommentAdded={() => post.commentsCount++} 
                onCommentDeleted={() => post.commentsCount = Math.max(0, post.commentsCount - 1)}
              />
            {/if}
          </div>
        </article>
      {/each}
      {/if}
    </div>

  </main>

  <!-- 3. RIGHT SIDEBAR -->
  <aside class="hidden lg:flex flex-col w-[350px] pl-8 pt-1 h-screen sticky top-0">
    
    <!-- Search -->
    <div class="bg-gray-100 rounded-full flex items-center mt-1 mb-4 group focus-within:bg-white focus-within:ring-1 focus-within:ring-[#1d9bf0] focus-within:border-[#1d9bf0] border border-transparent transition-all">
      <span class="pl-4 pr-3 text-gray-500 group-focus-within:text-[#1d9bf0]">🔍</span>
      <input type="text" placeholder="Ara..." class="bg-transparent border-none outline-none py-3 w-full rounded-r-full text-[15px]">
    </div>

    <!-- Suggested -->
    <div class="bg-gray-50 rounded-2xl flex flex-col pt-3">
      <h2 class="font-bold text-[20px] px-4 mb-4">Developers</h2>
      
      {#if isLoading}
        <div class="flex justify-center py-4">
          <span class="w-6 h-6 border-2 border-[#1d9bf0] border-t-transparent rounded-full animate-spin"></span>
        </div>
      {:else}
        {#each suggestions as user}
          <div class="flex items-center justify-between hover:bg-gray-100 px-4 py-3 cursor-pointer transition-colors">
            <div class="flex items-center gap-3">
              <a href="/profile/{user.username}" class="w-10 h-10 rounded-full bg-gray-800 flex items-center justify-center text-white font-bold text-sm hover:opacity-80 overflow-hidden shrink-0">
                {#if user.avatar && user.avatar.length > 3}
                  <img src={user.avatar.startsWith('http') ? user.avatar : `${API_BASE_URL}${user.avatar}`} alt="Avatar" class="w-full h-full object-cover" />
                {:else}
                  {user.avatar || user.username.substring(0, 2).toUpperCase()}
                {/if}
              </a>
              <div class="flex flex-col">
                <a href="/profile/{user.username}" class="font-bold text-[15px] hover:underline">{user.username}</a>
                <span class="text-social-secondary text-[15px]">{user.handle}</span>
              </div>
            </div>
            <a href="/profile/{user.username}" class="bg-black text-white font-bold text-sm px-4 py-1.5 rounded-full hover:bg-gray-800 transition-colors">
              Profil
            </a>
          </div>
        {/each}
      {/if}
      
      <button class="p-4 text-[#1d9bf0] hover:bg-gray-100 rounded-b-2xl text-left text-[15px] transition-colors">
        Daha fazlası...
      </button>
    </div>
  </aside>

  <MobileNav />
  </div>
</div>
