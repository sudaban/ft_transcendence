<script lang="ts">
  import Sidebar from '$lib/components/Sidebar.svelte';
  import MobileNav from '$lib/components/MobileNav.svelte';

  // --- MOCK DATA ---
  let suggestions = $state([
    { id: 1, username: 'enes_celten', handle: '@idkahram', avatar: 'FE' },
    { id: 1, username: 'adalomer51', handle: '@omadali', avatar: 'OP' },
    { id: 2, username: 'sametncs', handle: '@saincesu', avatar: 'BE' },
    { id: 2, username: 'umutdbn77', handle: '@sdaban', avatar: 'BE' },
    { id: 3, username: 'Ahmetks', handle: '@asezgin', avatar: 'DB' }
  ]);

  let feedPosts = $state([
    { 
      id: 101, 
      author: 'Celten', 
      handle: '@admin',
      content: 'Welcome to the Transcendence Network.', 
      time: '1m', 
      likes: 42, 
      retweets: 42,
      replies: 42
    },
    { 
      id: 102, 
      author: 'Test', 
      handle: '@tester',
      content: 'Just checking out the new feed.', 
      time: '2h', 
      likes: 1, 
      retweets: 2,
      replies: 3
    },
    { 
      id: 102, 
      author: 'Test', 
      handle: '@tester',
      content: 'I hang around here.', 
      time: '4h', 
      likes: 1, 
      retweets: 2,
      replies: 3
    }
  ]);
</script>

<div class="min-h-screen bg-social-bg text-social-primary flex justify-center">

  <!-- 1. LEFT SIDEBAR -->
  <Sidebar />

  <!-- 2. CENTER FEED -->
  <main class="w-full max-w-[600px] border-r border-social-border min-h-screen pb-20 md:pb-0">
    
    <!-- Header -->
    <div class="sticky top-0 bg-[rgba(255,255,255,0.85)] backdrop-blur-md z-10 border-b border-social-border">
      <h2 class="font-bold text-xl p-4 cursor-pointer">Home</h2>
    </div>
    
    <!-- Create Post Area -->
    <div class="p-4 border-b border-social-border flex gap-3">
      <div class="w-10 h-10 rounded-full bg-gray-300 flex-shrink-0 flex items-center justify-center font-bold text-gray-600">
        42
      </div>
      <div class="flex-1 flex flex-col pt-1">
        <textarea 
          placeholder="What's going on bro ? Share with me! Cmon" 
          class="w-full bg-transparent outline-none text-xl resize-none placeholder-gray-500 overflow-hidden min-h-[50px]"
          rows="1"
        ></textarea>
        
        <div class="border-t border-social-border mt-3 pt-3 flex justify-between items-center">
          <div class="flex gap-2 text-[#1d9bf0]">
            <button class="w-9 h-9 rounded-full hover:bg-[#1d9bf0]/10 flex items-center justify-center transition-colors">🖼️</button>
            <button class="w-9 h-9 rounded-full hover:bg-[#1d9bf0]/10 flex items-center justify-center transition-colors">😊</button>
          </div>
          <button class="bg-[#1d9bf0] hover:bg-[#1a8cd8] text-white font-bold px-4 py-1.5 rounded-full disabled:opacity-50 transition-colors">
            Post
          </button>
        </div>
      </div>
    </div>
    
    <!-- Feed Posts -->
    <div class="w-full flex flex-col">
      {#each feedPosts as post}
        <article class="p-4 border-b border-social-border flex gap-3 hover:bg-gray-50 transition-colors cursor-pointer">
          <!-- Left Avatar -->
          <div class="shrink-0">
            <div class="w-10 h-10 rounded-full bg-gray-800 flex items-center justify-center text-white font-bold text-sm">
              {post.author.charAt(0)}
            </div>
          </div>
          
          <!-- Right Content -->
          <div class="flex-1 flex flex-col">
            <!-- Header -->
            <div class="flex items-center gap-1 mb-0.5">
              <span class="font-bold text-[15px] hover:underline truncate">{post.author}</span>
              <span class="text-social-secondary text-[15px] truncate">{post.handle}</span>
              <span class="text-social-secondary text-[15px]">·</span>
              <span class="text-social-secondary text-[15px] hover:underline">{post.time}</span>
            </div>
            
            <!-- Body -->
            <div class="text-[15px] leading-normal text-social-primary mb-3 whitespace-pre-wrap">
              {post.content}
            </div>
            
            <!-- Actions -->
            <div class="flex items-center justify-between text-social-secondary max-w-[425px]">
              <button class="flex items-center gap-2 hover:text-[#1d9bf0] transition-colors group">
                <span class="w-8 h-8 rounded-full group-hover:bg-[#1d9bf0]/10 flex items-center justify-center">💬</span>
                <span class="text-xs -ml-1">{post.replies}</span>
              </button>
              <button class="flex items-center gap-2 hover:text-[#00ba7c] transition-colors group">
                <span class="w-8 h-8 rounded-full group-hover:bg-[#00ba7c]/10 flex items-center justify-center">🔁</span>
                <span class="text-xs -ml-1">{post.retweets}</span>
              </button>
              <button class="flex items-center gap-2 hover:text-[#f91880] transition-colors group">
                <span class="w-8 h-8 rounded-full group-hover:bg-[#f91880]/10 flex items-center justify-center">❤️</span>
                <span class="text-xs -ml-1">{post.likes}</span>
              </button>
            </div>
          </div>
        </article>
      {/each}
    </div>

  </main>

  <!-- 3. RIGHT SIDEBAR -->
  <aside class="hidden lg:flex flex-col w-[350px] pl-8 pt-1 h-screen sticky top-0">
    
    <!-- Search -->
    <div class="bg-gray-100 rounded-full flex items-center mt-1 mb-4 group focus-within:bg-white focus-within:ring-1 focus-within:ring-[#1d9bf0] focus-within:border-[#1d9bf0] border border-transparent transition-all">
      <span class="pl-4 pr-3 text-gray-500 group-focus-within:text-[#1d9bf0]">🔍</span>
      <input type="text" placeholder="Search" class="bg-transparent border-none outline-none py-3 w-full rounded-r-full text-[15px]">
    </div>

    <!-- Suggested -->
    <div class="bg-gray-50 rounded-2xl flex flex-col pt-3">
      <h2 class="font-bold text-[20px] px-4 mb-4">Developers</h2>
      
      {#each suggestions as user}
        <div class="flex items-center justify-between hover:bg-gray-100 px-4 py-3 cursor-pointer transition-colors">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-full bg-gray-800 flex items-center justify-center text-white font-bold text-sm">
              {user.avatar}
            </div>
            <div class="flex flex-col">
              <span class="font-bold text-[15px] hover:underline">{user.username}</span>
              <span class="text-social-secondary text-[15px]">{user.handle}</span>
            </div>
          </div>
          <button class="bg-black text-white font-bold text-sm px-4 py-1.5 rounded-full hover:bg-gray-800 transition-colors">
            Follow
          </button>
        </div>
      {/each}
      
      <button class="p-4 text-[#1d9bf0] hover:bg-gray-100 rounded-b-2xl text-left text-[15px] transition-colors">
        Show more
      </button>
    </div>
  </aside>

  <MobileNav />

</div>
