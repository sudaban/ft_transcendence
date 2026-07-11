<script lang="ts">
  import { ApiService, API_BASE_URL } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';
  import type { PostDTO } from '$lib/types';
  import CommentsSection from './CommentsSection.svelte';

  let { post = $bindable(), onClose } = $props<{ post: PostDTO | null, onClose: () => void }>();

  async function toggleLike() {
    if (!authStore.token || !post) return;
    
    const currentlyLiked = post.isLiked ?? false;
    post.isLiked = !currentlyLiked;
    post.likesCount += currentlyLiked ? -1 : 1;

    try {
      if (!currentlyLiked) {
        await ApiService.likePost(post.id, authStore.token);
      } else {
        await ApiService.unlikePost(post.id, authStore.token);
      }
    } catch (err) {
      console.error("Beğeni işlemi başarısız", err);
      post.isLiked = currentlyLiked;
      post.likesCount += currentlyLiked ? 1 : -1;
    }
  }
</script>

{#if post}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/80 backdrop-blur-sm animate-fade-in" onclick={onClose}>
    <!-- Modal Container -->
    <div 
      class="bg-white rounded-3xl overflow-hidden shadow-2xl w-full max-w-5xl h-[85vh] flex flex-col md:flex-row relative"
      onclick={(e) => e.stopPropagation()}
    >
      <!-- Close Button -->
      <button onclick={onClose} class="absolute top-4 right-4 z-10 w-8 h-8 bg-black/50 hover:bg-black text-white rounded-full flex items-center justify-center transition-colors">
        ✕
      </button>

      <!-- Left: Image -->
      <div class="w-full md:w-[60%] bg-black flex items-center justify-center">
        {#if post.imageUrl}
          <img src={post.imageUrl.startsWith('http') ? post.imageUrl : API_BASE_URL + post.imageUrl} class="w-full h-full object-contain" alt="Post" />
        {:else}
          <div class="text-white/50 text-sm">Resim yok</div>
        {/if}
      </div>

      <!-- Right: Content & Comments -->
      <div class="w-full md:w-[40%] flex flex-col bg-white h-full">
        <!-- Header -->
        <div class="p-4 border-b border-slate-100 flex items-center gap-3">
          <a href="/profile/{post.author.username}" class="w-10 h-10 rounded-full bg-slate-900 flex items-center justify-center text-white text-xs font-bold">
            {post.author.avatar || post.author.username.substring(0, 2).toUpperCase()}
          </a>
          <div class="flex flex-col">
            <a href="/profile/{post.author.username}" class="font-bold text-sm hover:underline">{post.author.username}</a>
            <span class="text-xs text-slate-500">{new Date(post.createdAt).toLocaleDateString()}</span>
          </div>
        </div>

        <!-- Post Content -->
        {#if post.content}
          <div class="p-4 text-sm text-slate-800 border-b border-slate-50 whitespace-pre-wrap">
            {post.content}
          </div>
        {/if}

        <!-- Comments Area (Scrollable) -->
        <div class="flex-1 overflow-y-auto p-4 custom-scrollbar">
          <CommentsSection postId={post.id} onCommentAdded={() => post.commentsCount++} onCommentDeleted={() => Math.max(0, post.commentsCount--)} />
        </div>

        <!-- Actions Footer -->
        <div class="p-4 border-t border-slate-100 bg-white">
          <div class="flex items-center gap-4 mb-2">
            <button onclick={toggleLike} class="flex items-center gap-1.5 text-2xl {post.isLiked ? 'text-[#f91880]' : 'text-slate-800 hover:text-slate-500'} transition-colors">
              {post.isLiked ? '❤️' : '🤍'}
            </button>
            <button class="flex items-center gap-1.5 text-2xl text-slate-800 hover:text-slate-500 transition-colors">
              💬
            </button>
          </div>
          <div class="font-bold text-sm text-slate-900">{post.likesCount} beğenme</div>
        </div>
      </div>
    </div>
  </div>
{/if}

<style>
  .custom-scrollbar::-webkit-scrollbar { width: 4px; }
  .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
  .custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
  .animate-fade-in { animation: fadeIn 0.2s ease-out forwards; }
  @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
</style>
