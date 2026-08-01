import type { Showoff, User, Comment } from './types';

export const CURRENT_USER: User = {
  id: 'user1',
  username: 'admin_user',
  avatar: 'AU',
  isStaff: true,
  isLoggedIn: true,
};

function makeComments(count: number): Comment[] {
  const texts = [
    'This collection is absolutely stunning! How long did it take to curate?',
    'I have the same Charizard! One of my prized possessions.',
    'The layout is perfect, the background colour really makes the cards pop.',
    'Where did you find these in such good condition?',
    'Amazing showoff! Gives me inspiration for mine.',
    'The base set is legendary. Great choice!',
    'Love the arrangement, very aesthetic.',
    'This is my dream collection right here.',
    'Wow, mint condition too? Incredible.',
    'I need to organise mine like this.',
    'Such a clean display, well done.',
    'These bring back so many memories.',
    'Do you have the shadowless variant?',
    'The colour palette is chef\'s kiss.',
    'Followed! Can\'t wait to see more.',
  ];
  const users = [
    { userId: 'u2', username: 'collector99', avatar: 'C9' },
    { userId: 'u3', username: 'vinylking', avatar: 'VK' },
    { userId: 'u4', username: 'rare_finds', avatar: 'RF' },
    { userId: 'u5', username: 'poke_master', avatar: 'PM' },
    { userId: 'u6', username: 'comicshop', avatar: 'CS' },
  ];
  return Array.from({ length: count }, (_, i) => {
    const u = users[i % users.length];
    const d = new Date(2026, 6, i + 1);
    return {
      id: `c${i}`,
      userId: u.userId,
      username: u.username,
      avatar: u.avatar,
      text: texts[i % texts.length],
      createdAt: d.toISOString().split('T')[0],
      likes: Math.floor(Math.random() * 12),
    };
  });
}

const initialShowoffs: Showoff[] = [
  {
    id: 'show1',
    name: 'My Pokemon Collection',
    description: 'My favourite Pokemon cards from the base set era. These bring back incredible memories of trading in the schoolyard.',
    bgColor: '#1a3a5c',
    orientation: 'landscape',
    items: [
      { id: 'i1', name: 'Base set Charizard', imageUrl: 'https://images.unsplash.com/photo-1613771404784-3a5686aa2be3?w=240&h=336&fit=crop&auto=format', x: 8, y: 10, width: 28, height: 80 },
      { id: 'i2', name: 'Base set Blastoise', imageUrl: 'https://images.unsplash.com/photo-1542779632-f0e27ebfe0c7?w=240&h=336&fit=crop&auto=format', x: 40, y: 10, width: 28, height: 80 },
    ],
    isPublic: true,
    isDisabled: false,
    tags: ['Pokemon', 'Trading Cards', 'Gaming'],
    ownerId: 'user1',
    ownerName: 'admin_user',
    ownerAvatar: 'AU',
    likes: 2,
    likedBy: ['user2'],
    views: 47,
    createdAt: '2026-07-10',
    updatedAt: '2026-07-22',
    comments: makeComments(18),
    reports: [],
  },
  {
    id: 'show2',
    name: 'Vinyl Classics',
    description: 'A curated selection of my most treasured vinyl records from the golden era of rock and jazz.',
    bgColor: '#2a1a3a',
    orientation: 'portrait',
    items: [
      { id: 'i3', name: 'Dark Side of the Moon', imageUrl: 'https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?w=240&h=240&fit=crop&auto=format', x: 10, y: 8, width: 80, height: 38 },
      { id: 'i4', name: 'Kind of Blue', imageUrl: 'https://images.unsplash.com/photo-1488376739361-f85e7cb62c89?w=240&h=240&fit=crop&auto=format', x: 10, y: 52, width: 80, height: 38 },
    ],
    isPublic: true,
    isDisabled: false,
    tags: ['Vinyl'],
    ownerId: 'user2',
    ownerName: 'vinylking',
    ownerAvatar: 'VK',
    likes: 1,
    likedBy: [],
    views: 23,
    createdAt: '2026-07-05',
    updatedAt: '2026-07-18',
    comments: makeComments(7),
    reports: [],
  },
  {
    id: 'show3',
    name: 'Rare Comics',
    description: 'First edition comics that defined my childhood. Each one is bagged and boarded with love.',
    bgColor: '#3a2a10',
    orientation: 'landscape',
    items: [
      { id: 'i5', name: 'Amazing Fantasy #15', imageUrl: 'https://images.unsplash.com/photo-1531259683007-016a7b628fc3?w=200&h=300&fit=crop&auto=format', x: 10, y: 5, width: 24, height: 90 },
      { id: 'i6', name: 'X-Men #1', imageUrl: 'https://images.unsplash.com/photo-1612036782180-6f0b6cd846fe?w=200&h=300&fit=crop&auto=format', x: 40, y: 5, width: 24, height: 90 },
      { id: 'i7', name: 'Detective Comics #27', imageUrl: 'https://images.unsplash.com/photo-1635805737707-575885ab0820?w=200&h=300&fit=crop&auto=format', x: 70, y: 5, width: 24, height: 90 },
    ],
    isPublic: true,
    isDisabled: false,
    tags: ['Comics'],
    ownerId: 'user3',
    ownerName: 'comicshop',
    ownerAvatar: 'CS',
    likes: 1,
    likedBy: [],
    views: 61,
    createdAt: '2026-06-28',
    updatedAt: '2026-07-15',
    comments: makeComments(12),
    reports: [],
  },
  {
    id: 'show4',
    name: 'Chari',
    description: 'My prized Charizard — base set, first edition, PSA 9. One of the greatest cards ever printed.',
    bgColor: '#cc2200',
    orientation: 'landscape',
    items: [
      { id: 'i8', name: 'Base set Charizard', imageUrl: 'https://images.unsplash.com/photo-1613771404784-3a5686aa2be3?w=300&h=420&fit=crop&auto=format', x: 25, y: 5, width: 50, height: 90 },
    ],
    isPublic: true,
    isDisabled: false,
    tags: ['Pokemon', 'Trading Cards'],
    ownerId: 'user1',
    ownerName: 'admin_user',
    ownerAvatar: 'AU',
    likes: 0,
    likedBy: [],
    views: 14,
    createdAt: '2026-07-28',
    updatedAt: '2026-07-28',
    comments: makeComments(3),
    reports: [
      { id: 'r1', reporterId: 'user2', reporterName: 'vinylking', category: 'Counterfeit / Fake item', reason: 'Possible counterfeit item image', createdAt: '2026-07-29' },
    ],
  },
  {
    id: 'show5',
    name: 'Sneaker Wall',
    description: 'My grail sneakers displayed the way they deserve. Each pair has a story.',
    bgColor: '#0a1a2a',
    orientation: 'portrait',
    items: [
      { id: 'i9', name: 'Air Jordan 1 Chicago', imageUrl: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=280&h=200&fit=crop&auto=format', x: 5, y: 5, width: 90, height: 42 },
      { id: 'i10', name: 'Nike SB Dunk Low', imageUrl: 'https://images.unsplash.com/photo-1600185365483-26d7a4cc7519?w=280&h=200&fit=crop&auto=format', x: 5, y: 53, width: 90, height: 42 },
    ],
    isPublic: true,
    isDisabled: false,
    tags: ['Sneakers'],
    ownerId: 'user4',
    ownerName: 'rare_finds',
    ownerAvatar: 'RF',
    likes: 5,
    likedBy: ['user1', 'user2', 'user3'],
    views: 112,
    createdAt: '2026-06-15',
    updatedAt: '2026-07-10',
    comments: makeComments(9),
    reports: [],
  },
  {
    id: 'show6',
    name: 'Vintage Cameras',
    description: 'A lifetime of hunting flea markets and estate sales. Each camera still works.',
    bgColor: '#1a1a0a',
    orientation: 'landscape',
    items: [
      { id: 'i11', name: 'Leica M3', imageUrl: 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=260&h=200&fit=crop&auto=format', x: 8, y: 25, width: 35, height: 50 },
      { id: 'i12', name: 'Rolleiflex 2.8F', imageUrl: 'https://images.unsplash.com/photo-1606676539940-12768ce0e762?w=260&h=200&fit=crop&auto=format', x: 55, y: 15, width: 38, height: 70 },
    ],
    isPublic: false,
    isDisabled: false,
    tags: ['Cameras'],
    ownerId: 'user1',
    ownerName: 'admin_user',
    ownerAvatar: 'AU',
    likes: 0,
    likedBy: [],
    views: 3,
    createdAt: '2026-07-30',
    updatedAt: '2026-07-30',
    comments: makeComments(2),
    reports: [],
  },
];

// Mutable store (simulates backend state)
let _showoffs: Showoff[] = [...initialShowoffs];

export function getShowoffs(): Showoff[] {
  return _showoffs;
}

export function getShowoff(id: string): Showoff | undefined {
  return _showoffs.find((s) => s.id === id);
}

export function getUserShowoffs(userId: string): Showoff[] {
  return _showoffs.filter((s) => s.ownerId === userId);
}

export function getPublicShowoffs(): Showoff[] {
  return _showoffs.filter((s) => s.isPublic && !s.isDisabled);
}

export function getReportedShowoffs(): Showoff[] {
  return _showoffs.filter((s) => s.reports.length > 0);
}

export function saveShowoff(showoff: Showoff): void {
  const idx = _showoffs.findIndex((s) => s.id === showoff.id);
  if (idx >= 0) {
    _showoffs[idx] = { ...showoff, updatedAt: new Date().toISOString().split('T')[0] };
  } else {
    _showoffs = [..._showoffs, showoff];
  }
}

export function deleteShowoff(id: string): void {
  _showoffs = _showoffs.filter((s) => s.id !== id);
}

export function toggleLike(showoffId: string, userId: string): void {
  const s = _showoffs.find((x) => x.id === showoffId);
  if (!s) return;
  if (s.likedBy.includes(userId)) {
    s.likedBy = s.likedBy.filter((id) => id !== userId);
    s.likes = Math.max(0, s.likes - 1);
  } else {
    s.likedBy.push(userId);
    s.likes += 1;
  }
}

export function incrementViews(showoffId: string): void {
  const s = _showoffs.find((x) => x.id === showoffId);
  if (s) s.views += 1;
}

export function addComment(showoffId: string, userId: string, username: string, avatar: string, text: string): void {
  const s = _showoffs.find((x) => x.id === showoffId);
  if (!s) return;
  s.comments.push({
    id: `c${Date.now()}`,
    userId,
    username,
    avatar,
    text,
    createdAt: new Date().toISOString().split('T')[0],
    likes: 0,
  });
}

export function reportShowoff(showoffId: string, reporterId: string, reporterName: string, category: string, reason: string): void {
  const s = _showoffs.find((x) => x.id === showoffId);
  if (!s) return;
  s.reports.push({
    id: `r${Date.now()}`,
    reporterId,
    reporterName,
    category,
    reason,
    createdAt: new Date().toISOString().split('T')[0],
  });
}

export const PUBLIC_PROFILES: Record<string, { username: string; avatar: string; bio: string; joinedAt: string }> = {
  user4: {
    username: 'rare_finds',
    avatar: 'RF',
    bio: 'Hunting down grail sneakers and rare collectibles since 2015. Every pair has a story — ask me anything.',
    joinedAt: '2015-03-12',
  },
};

export function disableShowoff(showoffId: string): void {
  const s = _showoffs.find((x) => x.id === showoffId);
  if (!s) return;
  s.isDisabled = true;
  s.isPublic = false;
  s.disabledAt = new Date().toISOString().split('T')[0];
}

export function enableShowoff(showoffId: string): void {
  const s = _showoffs.find((x) => x.id === showoffId);
  if (!s) return;
  s.isDisabled = false;
  s.isPublic = true;
  s.disabledAt = undefined;
}

