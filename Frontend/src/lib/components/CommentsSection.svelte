<script lang="ts">
  import { onMount } from 'svelte';
  import { ApiService } from '$lib/api';
  import { authStore } from '$lib/stores/auth.svelte';
  import type { CommentDTO } from '$lib/types';
  import gsap from 'gsap';

  let { postId, onCommentAdded, onCommentDeleted } = $props<{ postId: number, onCommentAdded?: () => void, onCommentDeleted?: () => void }>();

  let comments = $state<CommentDTO[]>([]);
  let isLoading = $state(true);
  let newCommentContent = $state('');
  let isSubmitting = $state(false);

  onMount(async () => {
    if (!authStore.token) return;
    try {
      comments = await ApiService.getComments(postId, authStore.token);
    } catch (err) {
      console.error("Yorumlar yüklenemedi", err);
    } finally {
      isLoading = false;
    }
  });

  async function submitComment() {
    if (!authStore.token || !newCommentContent.trim()) return;
    isSubmitting = true;
    try {
      const addedComment = await ApiService.addComment(postId, newCommentContent.trim(), authStore.token);
      comments = [addedComment, ...comments];
      newCommentContent = '';
      if (onCommentAdded) onCommentAdded();
    } catch (err) {
      console.error("Yorum gönderilemedi", err);
    } finally {
      isSubmitting = false;
    }
  }

  function getAvatarInitial(username: string) {
    return username ? username.substring(0, 2).toUpperCase() : 'U';
  }

  async function handleDeleteComment(commentId: number) {
    if (!authStore.token) return;
    if (!confirm("Bu yorumu silmek istediğinize emin misiniz?")) return;
    
    try {
      await ApiService.deleteComment(commentId, authStore.token);
      comments = comments.filter(c => c.id !== commentId);
      if (onCommentDeleted) onCommentDeleted();
    } catch (err) {
      console.error("Yorum silinemedi", err);
      alert("Yorum silinirken hata oluştu.");
    }
  }
</script>

<div class="mt-4 pt-4 border-t border-social-border w-full animate-fade-in-up">
  
  <!-- Add Comment Input -->
  <div class="flex gap-3 mb-6">
    <div class="w-8 h-8 rounded-full bg-slate-900 flex-shrink-0 flex items-center justify-center text-white text-xs font-bold">
      {authStore.user ? getAvatarInitial(authStore.user.username) : 'ME'}
    </div>
    <div class="flex-1 flex items-center bg-gray-50 rounded-full border border-gray-200 px-4 focus-within:ring-2 focus-within:ring-[#1d9bf0]/20 focus-within:border-[#1d9bf0] transition-all">
      <input 
        type="text" 
        bind:value={newCommentContent}
        onkeydown={(e) => e.key === 'Enter' && submitComment()}
        placeholder="Bir yanıt yaz..." 
        disabled={isSubmitting}
        class="bg-transparent border-none outline-none py-2 w-full text-sm"
      />
      <button 
        onclick={submitComment} 
        disabled={!newCommentContent.trim() || isSubmitting}
        class="text-[#1d9bf0] font-bold text-sm ml-2 disabled:opacity-50 disabled:cursor-not-allowed hover:text-[#1a8cd8] transition-colors"
      >
        Yanıtla
      </button>
    </div>
  </div>

  <!-- Comments List -->
  {#if isLoading}
    <div class="flex justify-center py-4">
      <span class="w-5 h-5 border-2 border-[#1d9bf0] border-t-transparent rounded-full animate-spin"></span>
    </div>
  {:else if comments.length === 0}
    <div class="text-center text-gray-400 text-sm py-2">
      Henüz yorum yok. İlk yorumu sen yap!
    </div>
  {:else}
    <div class="flex flex-col gap-4">
      {#each comments as comment (comment.id)}
        <div class="flex gap-3">
          <a href="/profile/{comment.user?.username}" class="w-8 h-8 rounded-full bg-gray-200 flex-shrink-0 flex items-center justify-center text-slate-700 text-xs font-bold hover:opacity-80 transition-opacity">
            {getAvatarInitial(comment.user?.username)}
          </a>
          <div class="flex flex-col flex-1 bg-gray-50 rounded-2xl rounded-tl-none p-3 border border-gray-100">
            <div class="flex items-center gap-1 mb-1 relative">
              <a href="/profile/{comment.user?.username}" class="font-bold text-[13px] hover:underline text-slate-900">{comment.user?.username}</a>
              <!-- <span class="text-gray-400 text-[12px]">· {new Date(comment.createdAt).toLocaleDateString()}</span> -->
              {#if authStore.user?.id?.toString() === comment.user?.id?.toString()}
                <button onclick={() => handleDeleteComment(comment.id)} class="ml-auto text-gray-400 hover:text-red-500 transition-colors text-xs" title="Yorumu Sil">
                  🗑️
                </button>
              {/if}
            </div>
            <p class="text-[14px] text-slate-800 leading-snug">{comment.content}</p>
          </div>
        </div>
      {/each}
    </div>
  {/if}

</div>
