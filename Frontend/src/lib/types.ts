export interface DatabaseUser {
  id: string;
  username: string;
  fullName?: string;
  email: string;
  followersCount: number;
  following: string[];
  followRequestTime?: string;
  blockedUsers: string[];
  likedPosts: string[];
  savedPosts: string[];
  commentedPosts: string[];
  activeDMs: string[];
  posts: string[];
  termsAccepted: boolean;
  isDeleted: boolean;
  registrationDate: string;
}

export interface DatabasePost {
  id: string;
  authorId: string;
  createdAt: string;
  likes: string[]; // beğenen kullanıcıların ID'leri
  comments: string[]; // yorum ID'leri
  saves: string[]; // kaydeden kullanıcıların ID'leri
  viewsCount: number;
  content: string;
  isVideo?: boolean;
}

// Frontend arayüzünde (UI) göstermek için Backend'den aggregate edilmesini beklediğimiz DTO tipleri
export interface UserDTO {
  id: string;
  username: string;
  handle: string;
  avatar: string;
  fullName?: string;
  bio?: string;
  followersCount: number;
  followingCount: number;
  postsCount: number;
  isTwoFactorEnabled: boolean;
}

export interface PostDTO {
  id: number;
  author: UserDTO;
  imageUrl: string;
  content?: string;
  viewsCount: number;
  isVideo: boolean;
  createdAt: string;
  likesCount: number;
  commentsCount: number;
}
