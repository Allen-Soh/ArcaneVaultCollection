import type { Showoff, User } from '../types';
import ShowoffMiniCanvas from './ShowoffMiniCanvas';

interface Props {
  showoff: Showoff;
  user: User;
  onView: (id: string) => void;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onLike: (id: string) => void;
  onTagClick: (tag: string) => void;
  compact?: boolean;
}

export default function ShowoffCard({ showoff, user, onView, onEdit, onDelete, onLike, onTagClick, compact }: Props) {
  const isOwner = user.isLoggedIn && user.id === showoff.ownerId;
  const hasLiked = user.isLoggedIn && showoff.likedBy.includes(user.id);

  return (
    <div
      style={{
        background: '#161b22',
        border: '1px solid #21262d',
        borderRadius: 10,
        overflow: 'hidden',
        transition: 'border-color 0.15s, transform 0.15s',
        cursor: 'pointer',
      }}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLElement).style.borderColor = '#30363d';
        (e.currentTarget as HTMLElement).style.transform = 'translateY(-1px)';
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLElement).style.borderColor = '#21262d';
        (e.currentTarget as HTMLElement).style.transform = 'translateY(0)';
      }}
    >
      {/* Canvas preview */}
      <div
        onClick={() => onView(showoff.id)}
        style={{
          padding: 12,
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          background: '#0d1117',
          minHeight: compact ? 80 : 100,
        }}
      >
        <ShowoffMiniCanvas showoff={showoff} width={compact ? 140 : 160} height={compact ? 78 : 90} />
      </div>

      {/* Info */}
      <div style={{ padding: compact ? '10px 12px' : '12px 14px' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 6 }}>
          <span
            onClick={() => onView(showoff.id)}
            style={{ fontSize: 13, fontWeight: 600, color: '#e6edf3', cursor: 'pointer', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 120 }}
          >
            {showoff.name}
          </span>
          <div style={{ display: 'flex', alignItems: 'center', gap: 4, flexShrink: 0 }}>
            {showoff.isDisabled && (
              <span style={{ fontSize: 10, color: '#f85149', background: '#2a1f1d', padding: '2px 6px', borderRadius: 4, fontWeight: 600 }}>
                DISABLED
              </span>
            )}
            {!showoff.isDisabled && (
              <span style={{
                fontSize: 10,
                color: showoff.isPublic ? '#3fb950' : '#7d8590',
                background: showoff.isPublic ? '#122016' : '#21262d',
                padding: '2px 6px',
                borderRadius: 4,
                fontWeight: 600,
              }}>
                {showoff.isPublic ? 'Public' : 'Private'}
              </span>
            )}
          </div>
        </div>

        {/* Tags */}
        {showoff.tags.length > 0 && (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4, marginBottom: 8 }}>
            {showoff.tags.slice(0, 3).map((tag) => (
              <button
                key={tag}
                onClick={(e) => { e.stopPropagation(); onTagClick(tag); }}
                style={{
                  fontSize: 10,
                  color: '#58a6ff',
                  background: '#0d1f35',
                  border: '1px solid #1c3a5e',
                  borderRadius: 4,
                  padding: '2px 6px',
                  cursor: 'pointer',
                  fontFamily: 'inherit',
                }}
              >
                #{tag}
              </button>
            ))}
            {showoff.tags.length > 3 && (
              <span style={{ fontSize: 10, color: '#7d8590' }}>+{showoff.tags.length - 3}</span>
            )}
          </div>
        )}

        {/* Actions */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
          <button
            onClick={(e) => { e.stopPropagation(); onLike(showoff.id); }}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 4,
              background: hasLiked ? '#2d1b1e' : 'none',
              border: hasLiked ? '1px solid #5a2030' : '1px solid #21262d',
              borderRadius: 6,
              padding: '4px 8px',
              cursor: 'pointer',
              fontSize: 12,
              color: hasLiked ? '#f85149' : '#7d8590',
              transition: 'all 0.15s',
              fontFamily: 'inherit',
            }}
          >
            <svg width="12" height="12" viewBox="0 0 24 24" fill={hasLiked ? '#f85149' : 'none'} stroke={hasLiked ? '#f85149' : '#7d8590'} strokeWidth="2">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
            </svg>
            {showoff.likes}
          </button>

          <span style={{ fontSize: 11, color: '#484f58', display: 'flex', alignItems: 'center', gap: 3 }}>
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
              <circle cx="12" cy="12" r="3" />
            </svg>
            {showoff.views}
          </span>

          <div style={{ flex: 1 }} />

          {isOwner && onEdit && (
            <button
              onClick={(e) => { e.stopPropagation(); onEdit(showoff.id); }}
              style={{
                background: 'none',
                border: '1px solid #21262d',
                borderRadius: 5,
                padding: '3px 8px',
                fontSize: 11,
                color: '#7d8590',
                cursor: 'pointer',
                fontFamily: 'inherit',
              }}
            >
              Edit
            </button>
          )}
          {isOwner && onDelete && (
            <button
              onClick={(e) => { e.stopPropagation(); onDelete(showoff.id); }}
              style={{
                background: 'none',
                border: '1px solid #2a1f1d',
                borderRadius: 5,
                padding: '3px 8px',
                fontSize: 11,
                color: '#f85149',
                cursor: 'pointer',
                fontFamily: 'inherit',
              }}
            >
              Delete
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
