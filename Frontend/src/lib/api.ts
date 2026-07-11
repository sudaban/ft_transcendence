import type { PostDTO, UserDTO } from './types';

// Backend hazır olana kadar frontend arayüzünün çalışması için tasarladım.
// Backend hazır olduğunda buradaki sahte verileri silip gerçek fetch atmalıyız beyler

const MOCK_USERS: UserDTO[] = [
  { id: 'u1', username: 'enes_celten', handle: '@idkahram', avatar: 'FE', followersCount: 0, followingCount: 0, postsCount: 0, isTwoFactorEnabled: false },
  { id: 'u2', username: 'adalomer51', handle: '@omadali', avatar: 'OP', followersCount: 0, followingCount: 0, postsCount: 0, isTwoFactorEnabled: false },
  { id: 'u3', username: 'sametncs', handle: '@saincesu', avatar: 'BE', followersCount: 0, followingCount: 0, postsCount: 0, isTwoFactorEnabled: false },
  { id: 'u4', username: 'umutdbn77', handle: '@sdaban', avatar: 'BE', followersCount: 0, followingCount: 0, postsCount: 0, isTwoFactorEnabled: false },
  { id: 'u5', username: 'Ahmetks', handle: '@asezgin', avatar: 'DB', followersCount: 0, followingCount: 0, postsCount: 0, isTwoFactorEnabled: false }
];

export const API_BASE_URL = 'http://localhost:5000';
export const API_URL = `${API_BASE_URL}/api`;

export const ApiService = {
  // Sağ menüdeki kullanıcı önerilerini getirme yeri burası
  getSuggestedUsers: async (): Promise<UserDTO[]> => {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(MOCK_USERS);
      }, 300);
    });
  },

  // Gönderi (Post) endpointleri
  getFeedPosts: async (token: string): Promise<PostDTO[]> => {
    const res = await fetch(`${API_URL}/posts/feed`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch feed");
    return res.json();
  },

  getUserPosts: async (userId: string, token: string): Promise<PostDTO[]> => {
    const res = await fetch(`${API_URL}/posts/user/${userId}`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch user posts");
    return res.json();
  },

  createPost: async (formData: FormData, token: string): Promise<PostDTO> => {
    const res = await fetch(`${API_URL}/posts`, {
      method: 'POST',
      body: formData,
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to create post");
    return res.json();
  },

  deletePost: async (postId: number, token: string) => {
    const res = await fetch(`${API_URL}/posts/${postId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to delete post");
    return res.json();
  },

  // AUTHENTICATION ve 2FA endpointleri

  register: async (data: any) => {
    const res = await fetch(`${API_URL}/auth/register`, {
      method: 'POST',
      body: JSON.stringify(data),
      headers: { 'Content-Type': 'application/json' }
    });
    if (!res.ok) throw new Error("Registration failed");
    return res.json();
  },

  login: async (email: string, password: string): Promise<{ requiresTwoFactor: boolean, token: string | null, tempToken: string | null }> => {
    const res = await fetch(`${API_URL}/auth/login`, {
      method: 'POST',
      body: JSON.stringify({ email, password }),
      headers: { 'Content-Type': 'application/json' }
    });
    if (!res.ok) throw new Error("Login failed");
    return res.json();
  },

  login2fa: async (email: string, code: string, tempToken: string) => {
    const res = await fetch(`${API_URL}/auth/2fa/login`, {
      method: 'POST',
      body: JSON.stringify({ email, code, tempToken }),
      headers: { 'Content-Type': 'application/json' }
    });
    if (!res.ok) throw new Error("Invalid 2FA Code");
    return res.json();
  },

  setup2fa: async (token: string) => {
    const res = await fetch(`${API_URL}/auth/2fa/setup`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Setup failed");
    return res.json();
  },

  enable2fa: async (code: string, token: string) => {
    const res = await fetch(`${API_URL}/auth/2fa/enable`, {
      method: 'POST',
      body: JSON.stringify({ code }),
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Invalid code");
    return res.json();
  },

  disable2fa: async (token: string) => {
    const res = await fetch(`${API_URL}/auth/2fa/disable`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Disable failed");
    return res.json();
  },

  // USER endpoints
  getUserById: async (id: string, token: string) => {
    const res = await fetch(`${API_URL}/users/${id}`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch user");
    return res.json();
  },

  updateProfile: async (data: { FullName?: string; Bio?: string; ProfilePictureUrl?: string; }, token: string) => {
    const res = await fetch(`${API_URL}/users/profile`, {
      method: 'PUT',
      body: JSON.stringify(data),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    });
    if (!res.ok) throw new Error("Failed to update profile");
    return res.json();
  },

  getUserByUsername: async (username: string, token: string) => {
    const res = await fetch(`${API_URL}/users/username/${username}`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch user by username");
    return res.json();
  },

  // FOLLOW endpoints
  followUser: async (targetUserId: string, token: string) => {
    const res = await fetch(`${API_URL}/follows/${targetUserId}`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to follow user");
    return res.json();
  },

  unfollowUser: async (targetUserId: string, token: string) => {
    const res = await fetch(`${API_URL}/follows/${targetUserId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to unfollow user");
    return res.json();
  },

  getFollowing: async (userId: string, token: string): Promise<UserDTO[]> => {
    const res = await fetch(`${API_URL}/follows/${userId}/following`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch following");
    return res.json();
  },

  // LIKES & COMMENTS endpoints
  likePost: async (postId: number, token: string) => {
    const res = await fetch(`${API_URL}/posts/${postId}/likes`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to like post");
    return res.json();
  },

  unlikePost: async (postId: number, token: string) => {
    const res = await fetch(`${API_URL}/posts/${postId}/likes`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to unlike post");
    return res.json();
  },

  getComments: async (postId: number, token: string) => {
    const res = await fetch(`${API_URL}/posts/${postId}/comments`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch comments");
    return res.json();
  },

  addComment: async (postId: number, content: string, token: string) => {
    const res = await fetch(`${API_URL}/posts/${postId}/comments`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ content })
    });
    if (!res.ok) throw new Error("Failed to add comment");
    return res.json();
  },

  deleteComment: async (commentId: number, token: string) => {
    const res = await fetch(`${API_URL}/comments/${commentId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to delete comment");
    return res.json();
  },

  // CHAT endpoints
  getChatRooms: async (token: string) => {
    const res = await fetch(`${API_URL}/ChatRooms/my`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch chat rooms");
    return res.json();
  },

  getChatMessages: async (roomId: number, token: string) => {
    const res = await fetch(`${API_URL}/chat-rooms/${roomId}/messages`, {
      method: 'GET',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Failed to fetch messages");
    return res.json();
  },

  sendMessage: async (roomId: number, content: string, token: string) => {
    const res = await fetch(`${API_URL}/chat-rooms/${roomId}/messages`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ content })
    });
    if (!res.ok) throw new Error("Failed to send message");
    return res.json();
  },

  createChatRoom: async (targetUserId: number, token: string) => {
    const res = await fetch(`${API_URL}/ChatRooms`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ isGroup: false, targetUserId: targetUserId })
    });
    if (!res.ok) throw new Error("Failed to create chat room");
    return res.json();
  }
};
