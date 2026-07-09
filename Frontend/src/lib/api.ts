import type { PostDTO, UserDTO } from './types';

// Backend hazır olana kadar frontend arayüzünün çalışması için tasarladım.
// Backend hazır olduğunda buradaki sahte verileri silip gerçek fetch atmalıyız beyler

const MOCK_USERS: UserDTO[] = [
  { id: 'u1', username: 'enes_celten', handle: '@idkahram', avatar: 'FE' },
  { id: 'u2', username: 'adalomer51', handle: '@omadali', avatar: 'OP' },
  { id: 'u3', username: 'sametncs', handle: '@saincesu', avatar: 'BE' },
  { id: 'u4', username: 'umutdbn77', handle: '@sdaban', avatar: 'BE' },
  { id: 'u5', username: 'Ahmetks', handle: '@asezgin', avatar: 'DB' }
];

const MOCK_POSTS: PostDTO[] = [
  {
    id: 'p101',
    author: { id: 'admin', username: 'Celten', handle: '@admin', avatar: 'C' },
    content: 'Welcome to the Transcendence Network.',
    createdAt: '1m',
    likesCount: 42,
    repostsCount: 42,
    repliesCount: 42,
    viewsCount: 1000
  },
  {
    id: 'p102',
    author: { id: 'test1', username: 'Test', handle: '@tester', avatar: 'T' },
    content: 'Just checking out the new feed. The API contract is now active.',
    createdAt: '2h',
    likesCount: 1,
    repostsCount: 2,
    repliesCount: 3,
    viewsCount: 15
  },
  {
    id: 'p103',
    author: { id: 'test1', username: 'Test', handle: '@tester', avatar: 'T' },
    content: 'I hang around here.',
    createdAt: '4h',
    likesCount: 1,
    repostsCount: 2,
    repliesCount: 3,
    viewsCount: 10
  }
];

export const ApiService = {
  // Ana sayfadaki akışın gelme yeri
  getFeedPosts: async (): Promise<PostDTO[]> => {
    // Sanki backende istek atıyormuş gibi ağ gecikmesi yapıyorum
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(MOCK_POSTS);
      }, 500); // 500ms türünde gecikme
    });
  },

  // Sağ menüdeki kullanıcı önerilerini getirme yeri burası
  getSuggestedUsers: async (): Promise<UserDTO[]> => {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(MOCK_USERS);
      }, 300);
    });
  },

  // Yeni bir gönderi paylaşma yeri burası
  createPost: async (content: string): Promise<PostDTO> => {
    return new Promise((resolve) => {
      setTimeout(() => {
        const newPost: PostDTO = {
          id: `p${Date.now()}`,
          author: { id: 'me', username: 'MyAccount', handle: '@my_account', avatar: 'ME' },
          content: content,
          createdAt: 'Just now',
          likesCount: 0,
          repostsCount: 0,
          repliesCount: 0,
          viewsCount: 0
        };
        // Sahte verileri listeye ekleme yeri (Statik olduğu için sayfa yenilenene kadar görünecek)
        MOCK_POSTS.unshift(newPost);
        resolve(newPost);
      }, 800);
    });
  },

  // AUTHENTICATION ve 2FA endpointleri

  register: async (data: any) => {
    const res = await fetch('http://localhost:5000/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
      headers: { 'Content-Type': 'application/json' }
    });
    if (!res.ok) throw new Error("Registration failed");
    return res.json();
  },

  login: async (email: string, password: string): Promise<{ requiresTwoFactor: boolean, token: string | null, tempToken: string | null }> => {
    const res = await fetch('http://localhost:5000/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
      headers: { 'Content-Type': 'application/json' }
    });
    if (!res.ok) throw new Error("Login failed");
    return res.json();
  },

  login2fa: async (email: string, code: string, tempToken: string) => {
    const res = await fetch('http://localhost:5000/api/auth/2fa/login', {
      method: 'POST',
      body: JSON.stringify({ email, code, tempToken }),
      headers: { 'Content-Type': 'application/json' }
    });
    if (!res.ok) throw new Error("Invalid 2FA Code");
    return res.json();
  },

  setup2fa: async (token: string) => {
    const res = await fetch('http://localhost:5000/api/auth/2fa/setup', {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Setup failed");
    return res.json();
  },

  enable2fa: async (code: string, token: string) => {
    const res = await fetch('http://localhost:5000/api/auth/2fa/enable', {
      method: 'POST',
      body: JSON.stringify({ code }),
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Invalid code");
    return res.json();
  },

  disable2fa: async (token: string) => {
    const res = await fetch('http://localhost:5000/api/auth/2fa/disable', {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Disable failed");
    return res.json();
  },

  // USER endpoints
  getUserById: async (id: string, token: string) => {
    const res = await fetch(`http://localhost:5000/api/users/${id}`, {
      method: 'GET',
      headers: {'Authorization': `Bearer ${token}`}
    });
    if (!res.ok) throw new Error("Failed to fetch user");
    return res.json();
  },

  updateProfile: async (data: { FullName?: string; Bio?: string; ProfilePictureUrl?: string; }, token: string) => {
    const res = await fetch('http://localhost:5000/api/users/profile', {
      method: 'PUT',
      body: JSON.stringify(data),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    });
    if (!res.ok) throw new Error("Failed to update profile");
    return res.json();
  }
};
