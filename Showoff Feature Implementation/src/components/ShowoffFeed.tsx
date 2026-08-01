import { useState, useEffect, useRef, useCallback } from 'react';
import type { Showoff, User, Page } from '../types';
import ShowoffCard from './ShowoffCard';

import { SITE_CATEGORIES } from '../categories';
const SORT_OPTIONS = ['Most Popular', 'Least Popular', 'Newest', 'Oldest'] as const;
const PAGE_SIZE = 8;

interface Props {
  showoffs: Showoff[];
  user: User;
  currentPage: Page;
  onView: (id: string) => void;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onLike: (id: string) => void;
  onTagClick: (tag: string) => void;
  onNavigate: (page: Page) => void;
}

export default function ShowoffFeed({ showoffs, user, currentPage, onView, onEdit, onDelete, onLike, onTagClick, onNavigate }: Props) {
  const [search, setSearch] = useState('');
  const [itemSearch, setItemSearch] = useState('');
  const [catSearch, setCatSearch] = useState('');
  const [selectedCat, setSelectedCat] = useState('All');
  const [recentCats, setRecentCats] = useState<string[]>([]);
  const [sort, setSort] = useState('Most Popular');
  const [visibleCount, setVisibleCount] = useState(PAGE_SIZE);
  const [isLoading, setIsLoading] = useState(false);
  const sentinelRef = useRef<HTMLDivElement>(null);
  const isTagPage = currentPage.name === 'tag';
  const activeTag = isTagPage ? (currentPage as { name: 'tag'; tag: string }).tag : null;

  const filtered = showoffs
    .filter((s) => {
      if (activeTag) return s.tags.includes(activeTag);
      const q = search.toLowerCase();
      const iq = itemSearch.toLowerCase();
      const matchSearch = !q || s.name.toLowerCase().includes(q) || s.description.toLowerCase().includes(q) || s.tags.some((t) => t.toLowerCase().includes(q));
      const matchItem = !iq || s.items.some((i) => i.name.toLowerCase().includes(iq));
      const cq = catSearch.toLowerCase();
      const matchCatSearch = !cq || s.tags.some((t) => t.toLowerCase().includes(cq));
      const matchSelectedCat = selectedCat === 'All' || s.tags.includes(selectedCat);
      return matchSearch && matchItem && matchCatSearch && matchSelectedCat;
    })
    .sort((a, b) => {
      if (sort === 'Most Popular') return b.likes - a.likes;
      if (sort === 'Least Popular') return a.likes - b.likes;
      if (sort === 'Newest') return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      if (sort === 'Oldest') return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      return 0;
    });

  const visible = filtered.slice(0, visibleCount);

  const loadMore = useCallback(() => {
    if (isLoading || visibleCount >= filtered.length) return;
    setIsLoading(true);
    setTimeout(() => {
      setVisibleCount((v) => Math.min(v + PAGE_SIZE, filtered.length));
      setIsLoading(false);
    }, 600);
  }, [isLoading, visibleCount, filtered.length]);

  useEffect(() => {
    const el = sentinelRef.current;
    if (!el) return;
    const obs = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting) loadMore();
    }, { threshold: 0.1 });
    obs.observe(el);
    return () => obs.disconnect();
  }, [loadMore]);

  useEffect(() => { setVisibleCount(PAGE_SIZE); }, [search, itemSearch, catSearch, selectedCat, sort, activeTag]);

  return (
    <div style={{ maxWidth: 1200, margin: '0 auto', padding: '24px 24px' }}>
      {/* Tab bar */}
      <div style={{ display: 'flex', gap: 2, marginBottom: 20, borderBottom: '1px solid #21262d' }}>
        {(['feed', 'profile'] as const).map((tab) => {
          const label = tab === 'feed' ? 'Feed' : 'My Profile';
          const active = tab === 'feed'
            ? (currentPage.name === 'feed' || currentPage.name === 'tag')
            : currentPage.name === 'profile';
          return (
            <button
              key={tab}
              onClick={() => onNavigate(tab === 'feed' ? { name: 'feed' } : { name: 'profile' })}
              style={{
                background: 'none',
                border: 'none',
                borderBottom: active ? '2px solid #2dd4bf' : '2px solid transparent',
                padding: '10px 16px',
                fontSize: 14,
                fontWeight: active ? 600 : 400,
                color: active ? '#e6edf3' : '#7d8590',
                cursor: 'pointer',
                marginBottom: -1,
                fontFamily: 'inherit',
              }}
            >
              {label}
            </button>
          );
        })}
      </div>

      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
        <h1 style={{ margin: 0, fontSize: 22, fontWeight: 700, color: '#e6edf3' }}>
          {activeTag ? (
            <span>
              Tag: <span style={{ color: '#58a6ff' }}>#{activeTag}</span>
            </span>
          ) : (
            'Community Showoffs'
          )}
        </h1>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          {activeTag && (
            <button
              onClick={() => onNavigate({ name: 'feed' })}
              style={{ background: 'none', border: '1px solid #30363d', borderRadius: 6, padding: '6px 12px', fontSize: 13, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}
            >
              ← Clear tag
            </button>
          )}
          <select
            value={sort}
            onChange={(e) => setSort(e.target.value)}
            style={{
              background: '#161b22',
              border: '1px solid #30363d',
              borderRadius: 6,
              padding: '6px 10px',
              fontSize: 13,
              color: '#e6edf3',
              cursor: 'pointer',
              fontFamily: 'inherit',
            }}
          >
            {SORT_OPTIONS.map((o) => <option key={o}>{o}</option>)}
          </select>
        </div>
      </div>

      {/* Search bars */}
      {!activeTag && (
        <div style={{ display: 'flex', gap: 10, marginBottom: 16, flexWrap: 'wrap' }}>
          <input
            type="text"
            placeholder="Search users..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={inputStyle}
          />
          <input
            type="text"
            placeholder="Search items..."
            value={itemSearch}
            onChange={(e) => setItemSearch(e.target.value)}
            style={inputStyle}
          />
          <input
            type="text"
            placeholder="Search categories..."
            value={catSearch}
            onChange={(e) => setCatSearch(e.target.value)}
            style={inputStyle}
          />
        </div>
      )}

      {/* Category filter — All + up to 4 recent slots */}
      {!activeTag && (
        <div style={{ display: 'flex', gap: 6, marginBottom: 20, flexWrap: 'wrap', alignItems: 'center' }}>
          <button
            onClick={() => setSelectedCat('All')}
            style={{ background: selectedCat === 'All' ? '#1a3a2a' : '#161b22', border: `1px solid ${selectedCat === 'All' ? '#3fb950' : '#21262d'}`, borderRadius: 20, padding: '4px 12px', fontSize: 12, color: selectedCat === 'All' ? '#3fb950' : '#7d8590', cursor: 'pointer', fontFamily: 'inherit', transition: 'all 0.15s' }}
          >
            All
          </button>
          {Array.from({ length: 4 }).map((_, i) => {
            const cat = recentCats[i];
            const isEmpty = !cat;
            const isActive = !isEmpty && selectedCat === cat;
            return (
              <button
                key={i}
                onClick={() => { if (!isEmpty) setSelectedCat(cat); }}
                style={{ background: isActive ? '#1a3a2a' : '#161b22', border: `1px solid ${isActive ? '#3fb950' : isEmpty ? '#1a1f26' : '#21262d'}`, borderRadius: 20, padding: '4px 12px', fontSize: 12, color: isActive ? '#3fb950' : isEmpty ? '#2a3040' : '#7d8590', cursor: isEmpty ? 'default' : 'pointer', fontFamily: 'inherit', transition: 'all 0.15s', minWidth: 120, textAlign: 'center' }}
              >
                {isEmpty ? `Recent Category ${i + 1}` : cat}
              </button>
            );
          })}
        </div>
      )}

      {/* Grid */}
      {visible.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: '#484f58' }}>
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" style={{ marginBottom: 12 }}>
            <circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" />
          </svg>
          <p style={{ margin: 0, fontSize: 14 }}>No showoffs found</p>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 14 }}>
          {visible.map((s) => (
            <ShowoffCard
              key={s.id}
              showoff={s}
              user={user}
              onView={onView}
              onEdit={onEdit}
              onDelete={onDelete}
              onLike={onLike}
              onTagClick={(tag) => {
                if (SITE_CATEGORIES.includes(tag)) {
                  setRecentCats((prev) => [tag, ...prev.filter((c) => c !== tag)].slice(0, 4));
                }
                onTagClick(tag);
              }}
            />
          ))}
        </div>
      )}

      {/* Sentinel + spinner */}
      <div ref={sentinelRef} style={{ height: 20, marginTop: 20 }} />
      {isLoading && (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '16px 0' }}>
          <div style={{
            width: 24,
            height: 24,
            border: '2px solid #21262d',
            borderTop: '2px solid #2dd4bf',
            borderRadius: '50%',
            animation: 'spin 0.8s linear infinite',
          }} />
        </div>
      )}
      {!isLoading && visibleCount < filtered.length && (
        <p style={{ textAlign: 'center', fontSize: 12, color: '#484f58' }}>Scroll to load more</p>
      )}
      {visibleCount >= filtered.length && filtered.length > 0 && (
        <p style={{ textAlign: 'center', fontSize: 12, color: '#484f58' }}>All showoffs loaded</p>
      )}

      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  flex: 1,
  minWidth: 140,
  background: '#161b22',
  border: '1px solid #30363d',
  borderRadius: 6,
  padding: '8px 12px',
  fontSize: 13,
  color: '#e6edf3',
  outline: 'none',
  fontFamily: 'inherit',
};
