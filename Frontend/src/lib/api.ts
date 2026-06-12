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
  }
};
