import { useState, useRef, useEffect } from 'react';
import type { Showoff, User, Page } from '../types';
import ShowoffMiniCanvas from './ShowoffMiniCanvas';

interface Props {
  showoffs: Showoff[];
  user: User;
  onNavigate: (page: Page) => void;
  onView: (id: string) => void;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onLike: (id: string) => void;
  onTagClick: (tag: string) => void;
  onCreate: () => void;
}

export default function ShowoffProfile({ showoffs, user, onNavigate, onView, onEdit, onDelete, onLike, onTagClick, onCreate }: Props) {
  const myShowoffs = showoffs.filter((s) => s.ownerId === user.id);
  const [description, setDescription] = useState('Collector of rare and interesting things. Passionate about preserving history one item at a time.');
  const [editingDesc, setEditingDesc] = useState(false);
  const [draftDesc, setDraftDesc] = useState(description);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    if (editingDesc) textareaRef.current?.focus();
  }, [editingDesc]);

  function saveDescription() {
    setDescription(draftDesc);
    setEditingDesc(false);
  }

  function cancelDescription() {
    setDraftDesc(description);
    setEditingDesc(false);
  }

  return (
    <div style={{ maxWidth: 1200, margin: '0 auto', padding: '24px 24px' }}>
      {/* Tabs */}
      <div style={{ display: 'flex', gap: 2, marginBottom: 24, borderBottom: '1px solid #21262d' }}>
        {(['feed', 'profile'] as const).map((tab) => {
          const label = tab === 'feed' ? 'Feed' : 'My Profile';
          const active = tab === 'profile';
          return (
            <button
              key={tab}
              onClick={() => onNavigate(tab === 'feed' ? { name: 'feed' } : { name: 'profile' })}
              style={{
                background: 'none', border: 'none',
                borderBottom: active ? '2px solid #2dd4bf' : '2px solid transparent',
                padding: '10px 16px', fontSize: 14,
                fontWeight: active ? 600 : 400,
                color: active ? '#e6edf3' : '#7d8590',
                cursor: 'pointer', marginBottom: -1, fontFamily: 'inherit',
              }}
            >
              {label}
            </button>
          );
        })}
      </div>

      {/* Profile header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 28 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
          <div style={{
            width: 48, height: 48, borderRadius: '50%',
            background: '#2a3f4a', border: '2px solid #30363d',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 16, fontWeight: 700, color: '#c9d1d9',
          }}>
            {user.avatar}
          </div>
          <div>
            <p style={{ margin: 0, fontSize: 16, fontWeight: 700, color: '#e6edf3' }}>{user.username}</p>
            <p style={{ margin: 0, fontSize: 13, color: '#7d8590' }}>{myShowoffs.length} showoff{myShowoffs.length !== 1 ? 's' : ''}</p>
          </div>
        </div>
      </div>

      {/* Description section */}
      <div style={{ background: '#161b22', border: '1px solid #21262d', borderRadius: 10, padding: '16px 18px', marginBottom: 24 }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: editingDesc ? 10 : 0 }}>
          <span style={{ fontSize: 13, fontWeight: 600, color: '#7d8590', textTransform: 'uppercase', letterSpacing: '0.05em' }}>About</span>
          {!editingDesc && (
            <button
              onClick={() => { setDraftDesc(description); setEditingDesc(true); }}
              style={{ background: 'none', border: '1px solid #30363d', borderRadius: 6, padding: '4px 10px', fontSize: 12, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit', display: 'flex', alignItems: 'center', gap: 5 }}
            >
              <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              Edit
            </button>
          )}
        </div>

        {editingDesc ? (
          <div>
            <textarea
              ref={textareaRef}
              value={draftDesc}
              onChange={(e) => setDraftDesc(e.target.value)}
              rows={3}
              placeholder="Tell people about yourself..."
              style={{ width: '100%', background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '10px 12px', fontSize: 14, color: '#e6edf3', outline: 'none', fontFamily: 'inherit', resize: 'none', boxSizing: 'border-box', lineHeight: 1.6 }}
            />
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 8 }}>
              <button onClick={cancelDescription} style={{ background: 'none', border: '1px solid #30363d', borderRadius: 6, padding: '6px 14px', fontSize: 13, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>
                Cancel
              </button>
              <button onClick={saveDescription} style={{ background: '#2dd4bf', border: 'none', borderRadius: 6, padding: '6px 14px', fontSize: 13, fontWeight: 600, color: '#0d1117', cursor: 'pointer', fontFamily: 'inherit' }}>
                Save
              </button>
            </div>
          </div>
        ) : (
          <p style={{ margin: description ? '8px 0 0' : 0, fontSize: 14, color: description ? '#c9d1d9' : '#484f58', lineHeight: 1.6, fontStyle: description ? 'normal' : 'italic' }}>
            {description || 'No description yet. Click Edit to add one.'}
          </p>
        )}
      </div>

      {/* Showoffs — single column, 1 per row */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
        {myShowoffs.map((showoff) => (
          <ProfileShowoffRow
            key={showoff.id}
            showoff={showoff}
            user={user}
            onView={onView}
            onEdit={onEdit}
            onDelete={onDelete}
            onLike={onLike}
            onTagClick={onTagClick}
          />
        ))}

        {/* Create new showoff card */}
        <button
          onClick={onCreate}
          style={{
            background: '#161b22',
            border: '2px dashed #30363d',
            borderRadius: 10,
            padding: '28px 20px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 12,
            color: '#484f58',
            fontSize: 14,
            fontFamily: 'inherit',
            transition: 'border-color 0.15s, color 0.15s',
          }}
          onMouseEnter={(e) => {
            (e.currentTarget as HTMLElement).style.borderColor = '#2dd4bf';
            (e.currentTarget as HTMLElement).style.color = '#2dd4bf';
          }}
          onMouseLeave={(e) => {
            (e.currentTarget as HTMLElement).style.borderColor = '#30363d';
            (e.currentTarget as HTMLElement).style.color = '#484f58';
          }}
        >
          <div style={{ display: 'flex', gap: 8 }}>
            {[0, 1, 2].map((i) => (
              <div key={i} style={{ width: 36, height: 36, border: '2px dashed currentColor', borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center', opacity: 0.6 }}>
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M12 5v14M5 12h14"/></svg>
              </div>
            ))}
          </div>
          Create new showoff
        </button>
      </div>
    </div>
  );
}

function ProfileShowoffRow({ showoff, user, onView, onEdit, onDelete, onLike, onTagClick }: {
  showoff: Showoff; user: User;
  onView: (id: string) => void; onEdit: (id: string) => void;
  onDelete: (id: string) => void; onLike: (id: string) => void;
  onTagClick: (tag: string) => void;
}) {
  const hasLiked = user.isLoggedIn && showoff.likedBy.includes(user.id);

  return (
    <div style={{ background: '#161b22', border: '1px solid #21262d', borderRadius: 10, padding: 16, display: 'flex', gap: 16 }}>
      {/* Canvas thumb */}
      <div
        onClick={() => onView(showoff.id)}
        style={{ cursor: 'pointer', flexShrink: 0 }}
      >
        <ShowoffMiniCanvas showoff={showoff} width={160} height={90} />
      </div>

      {/* Details */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 8, minWidth: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <button onClick={() => onView(showoff.id)} style={{ background: 'none', border: 'none', padding: 0, cursor: 'pointer', fontSize: 15, fontWeight: 700, color: '#e6edf3', fontFamily: 'inherit', textAlign: 'left' }}>
            {showoff.name}
          </button>
          <span style={{
            fontSize: 10, fontWeight: 600,
            color: showoff.isDisabled ? '#f85149' : showoff.isPublic ? '#3fb950' : '#7d8590',
            background: showoff.isDisabled ? '#2a1f1d' : showoff.isPublic ? '#122016' : '#21262d',
            padding: '2px 7px', borderRadius: 4,
          }}>
            {showoff.isDisabled ? 'DISABLED' : showoff.isPublic ? 'Public' : 'Private'}
          </span>
        </div>

        {showoff.description && (
          <p style={{ margin: 0, fontSize: 13, color: '#7d8590', overflow: 'hidden', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' }}>
            {showoff.description}
          </p>
        )}

        {/* Tags */}
        {showoff.tags.length > 0 && (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4 }}>
            {showoff.tags.slice(0, 5).map((tag) => (
              <button key={tag} onClick={() => onTagClick(tag)} style={{ fontSize: 11, color: '#58a6ff', background: '#0d1f35', border: '1px solid #1c3a5e', borderRadius: 4, padding: '2px 7px', cursor: 'pointer', fontFamily: 'inherit' }}>
                #{tag}
              </button>
            ))}
          </div>
        )}

        {/* Meta */}
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12, fontSize: 12, color: '#7d8590', alignItems: 'center' }}>
          <span>❤️ {showoff.likes} likes</span>
          <span>👁 {showoff.views} views</span>
          <span>Created {showoff.createdAt}</span>
          <span>Updated {showoff.updatedAt}</span>
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', gap: 8, marginTop: 'auto' }}>
          <button
            onClick={() => onLike(showoff.id)}
            style={{
              display: 'flex', alignItems: 'center', gap: 5,
              background: hasLiked ? '#2d1b1e' : '#21262d',
              border: `1px solid ${hasLiked ? '#5a2030' : '#30363d'}`,
              borderRadius: 6, padding: '5px 10px', fontSize: 12,
              color: hasLiked ? '#f85149' : '#7d8590', cursor: 'pointer', fontFamily: 'inherit',
            }}
          >
            <svg width="12" height="12" viewBox="0 0 24 24" fill={hasLiked ? '#f85149' : 'none'} stroke={hasLiked ? '#f85149' : '#7d8590'} strokeWidth="2">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
            </svg>
            {hasLiked ? 'Liked' : 'Like'}
          </button>
          <button onClick={() => onEdit(showoff.id)} style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 10px', fontSize: 12, color: '#e6edf3', cursor: 'pointer', fontFamily: 'inherit' }}>
            Edit
          </button>
          <button onClick={() => onDelete(showoff.id)} style={{ background: '#2a1f1d', border: '1px solid #5a2030', borderRadius: 6, padding: '5px 10px', fontSize: 12, color: '#f85149', cursor: 'pointer', fontFamily: 'inherit' }}>
            Delete
          </button>
          {showoff.isPublic && (
            <button
              onClick={() => { navigator.clipboard.writeText(`${window.location.origin}/showoff/${showoff.id}`).catch(() => {}); }}
              style={{ background: 'none', border: '1px solid #21262d', borderRadius: 6, padding: '5px 10px', fontSize: 12, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}
            >
              Share
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
