export interface ShowoffItem {
  id: string;
  name: string;
  imageUrl: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface Comment {
  id: string;
  userId: string;
  username: string;
  avatar: string;
  text: string;
  createdAt: string;
  likes: number;
}

export interface Report {
  id: string;
  reporterId: string;
  reporterName: string;
  category: string;
  reason: string;
  createdAt: string;
}

export interface PublicProfile {
  userId: string;
  username: string;
  avatar: string;
  bio: string;
  joinedAt: string;
}

export interface Showoff {
  id: string;
  name: string;
  description: string;
  bgColor: string;
  bgImage?: string;
  orientation: 'landscape' | 'portrait';
  items: ShowoffItem[];
  isPublic: boolean;
  isDisabled: boolean;
  disabledAt?: string;
  tags: string[];
  ownerId: string;
  ownerName: string;
  ownerAvatar: string;
  likes: number;
  likedBy: string[];
  views: number;
  createdAt: string;
  updatedAt: string;
  comments: Comment[];
  reports: Report[];
}

export interface User {
  id: string;
  username: string;
  avatar: string;
  isStaff: boolean;
  isLoggedIn: boolean;
}

export type Page =
  | { name: 'feed' }
  | { name: 'profile' }
  | { name: 'create' }
  | { name: 'edit'; showoffId: string }
  | { name: 'reports' }
  | { name: 'tag'; tag: string }
  | { name: 'user-profile'; userId: string };

export type ModalState =
  | { type: 'view'; showoffId: string; staffView?: boolean }
  | { type: 'delete'; showoffId: string }
  | { type: 'manage' }
  | { type: 'login'; message?: string }
  | null;
