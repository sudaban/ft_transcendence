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
    // const res = await fetch('http://localhost:5000/api/auth/register', { method: 'POST', body: JSON.stringify(data), headers: {'Content-Type': 'application/json'} });
    // return res.json();
    return new Promise((resolve) => setTimeout(() => resolve({ message: "Mock Register Success", token: "mock_jwt_token" }), 500));
  },

  login: async (email: string, password: string): Promise<{ requiresTwoFactor: boolean, token: string | null, tempToken: string | null }> => {
    // const res = await fetch('http://localhost:5000/api/auth/login', { method: 'POST', body: JSON.stringify({email, password}), headers: {'Content-Type': 'application/json'} });
    // return res.json();

    return new Promise((resolve) => {
      setTimeout(() => {
        if (email === '2fa@test.com') {
          resolve({ requiresTwoFactor: true, token: null, tempToken: "mock_temp_token_for_2fa" });
        } else {
          resolve({ requiresTwoFactor: false, token: "mock_final_jwt_token", tempToken: null });
        }
      }, 600);
    });
  },

  login2fa: async (email: string, code: string, tempToken: string) => {
    // const res = await fetch('http://localhost:5000/api/auth/2fa/login', { method: 'POST', body: JSON.stringify({email, code, tempToken}), headers: {'Content-Type': 'application/json'} });
    // return res.json();

    return new Promise((resolve, reject) => {
      setTimeout(() => {
        if (code === '123456') { // Mock valid code
          resolve({ requiresTwoFactor: false, token: "mock_final_jwt_token", tempToken: null });
        } else {
          reject(new Error("Invalid 2FA Code"));
        }
      }, 500);
    });
  },

  setup2fa: async (token: string) => {
    // const res = await fetch('http://localhost:5000/api/auth/2fa/setup', { method: 'POST', headers: {'Authorization': `Bearer ${token}`} });
    // return res.json();
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          secretKey: "ORSXG5BRGIZTINJWG44DS",
          qrCodeUri: "otpauth://totp/ft_transcendence:dev%40example.com?secret=ORSXG5BRGIZTINJWG44DS&issuer=ft_transcendence&algorithm=SHA1&digits=6&period=30"
        });
      }, 400);
    });
  },

  enable2fa: async (code: string, token: string) => {
    // const res = await fetch('http://localhost:5000/api/auth/2fa/enable', { method: 'POST', body: JSON.stringify({code}), headers: {'Content-Type': 'application/json', 'Authorization': `Bearer ${token}`} });
    // return res.json();
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        if (code === '123456') resolve({ message: "2FA enabled." });
        else reject(new Error("Invalid code."));
      }, 400);
    });
  }
};
