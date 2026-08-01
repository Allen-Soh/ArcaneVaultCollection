import type { Showoff, User } from '../types';
import ShowoffMiniCanvas from './ShowoffMiniCanvas';

interface Props {
  userId: string;
  profile: { username: string; avatar: string; bio: string; joinedAt: string };
  showoffs: Showoff[];
  currentUser: User;
  onView: (id: string) => void;
  onLike: (id: string) => void;
  onTagClick: (tag: string) => void;
  onBack: () => void;
}

export default function UserProfilePage({ userId, profile, showoffs, currentUser, onView, onLike, onTagClick, onBack }: Props) {
  const userShowoffs = showoffs.filter((s) => s.ownerId === userId && s.isPublic && !s.isDisabled);

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: '32px 24px' }}>
      <button
        onClick={onBack}
        style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#7d8590', fontSize: 13, display: 'flex', alignItems: 'center', gap: 6, padding: 0, fontFamily: 'inherit', marginBottom: 24 }}
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M19 12H5M12 5l-7 7 7 7"/></svg>
        Back
      </button>

      {/* Profile header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 20, marginBottom: 32, padding: 24, background: '#161b22', border: '1px solid #21262d', borderRadius: 12 }}>
        <div style={{
          width: 64, height: 64, borderRadius: '50%',
          background: 'linear-gradient(135deg, #2a3f4a, #1a3a5c)',
          border: '2px solid #30363d',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 20, fontWeight: 700, color: '#c9d1d9', flexShrink: 0,
        }}>
          {profile.avatar}
        </div>
        <div style={{ flex: 1 }}>
          <h1 style={{ margin: '0 0 6px', fontSize: 22, fontWeight: 700, color: '#e6edf3' }}>{profile.username}</h1>
          <p style={{ margin: '0 0 10px', fontSize: 13, color: '#7d8590' }}>Member since {profile.joinedAt}</p>
          <p style={{ margin: 0, fontSize: 14, color: '#c9d1d9', lineHeight: 1.6 }}>{profile.bio}</p>
        </div>
        <div style={{ textAlign: 'right', flexShrink: 0 }}>
          <p style={{ margin: '0 0 2px', fontSize: 22, fontWeight: 700, color: '#e6edf3' }}>{userShowoffs.length}</p>
          <p style={{ margin: 0, fontSize: 12, color: '#7d8590' }}>public showoffs</p>
        </div>
      </div>

      <h2 style={{ margin: '0 0 16px', fontSize: 16, fontWeight: 600, color: '#e6edf3' }}>Showoffs</h2>

      {userShowoffs.length === 0 ? (
        <p style={{ fontSize: 14, color: '#484f58' }}>No public showoffs yet.</p>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: 14 }}>
          {userShowoffs.map((showoff) => {
            const hasLiked = currentUser.isLoggedIn && showoff.likedBy.includes(currentUser.id);
            return (
              <div
                key={showoff.id}
                style={{ background: '#161b22', border: '1px solid #21262d', borderRadius: 10, overflow: 'hidden', cursor: 'pointer', transition: 'border-color 0.15s' }}
                onMouseEnter={(e) => (e.currentTarget.style.borderColor = '#30363d')}
                onMouseLeave={(e) => (e.currentTarget.style.borderColor = '#21262d')}
              >
                <div onClick={() => onView(showoff.id)} style={{ padding: 12, display: 'flex', justifyContent: 'center', background: '#0d1117' }}>
                  <ShowoffMiniCanvas showoff={showoff} width={160} height={90} />
                </div>
                <div style={{ padding: '10px 12px' }}>
                  <p onClick={() => onView(showoff.id)} style={{ margin: '0 0 6px', fontSize: 13, fontWeight: 600, color: '#e6edf3', cursor: 'pointer' }}>{showoff.name}</p>
                  {showoff.tags.length > 0 && (
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4, marginBottom: 8 }}>
                      {showoff.tags.slice(0, 3).map((tag) => (
                        <button key={tag} onClick={() => onTagClick(tag)} style={{ fontSize: 10, color: '#58a6ff', background: '#0d1f35', border: '1px solid #1c3a5e', borderRadius: 4, padding: '2px 6px', cursor: 'pointer', fontFamily: 'inherit' }}>
                          {tag}
                        </button>
                      ))}
                    </div>
                  )}
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <button
                      onClick={() => onLike(showoff.id)}
                      style={{ display: 'flex', alignItems: 'center', gap: 4, background: 'none', border: 'none', padding: 0, cursor: 'pointer', fontSize: 12, color: hasLiked ? '#f85149' : '#7d8590', fontFamily: 'inherit' }}
                    >
                      <svg width="12" height="12" viewBox="0 0 24 24" fill={hasLiked ? '#f85149' : 'none'} stroke={hasLiked ? '#f85149' : '#7d8590'} strokeWidth="2">
                        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
                      </svg>
                      {showoff.likes}
                    </button>
                    <span style={{ fontSize: 11, color: '#484f58' }}>👁 {showoff.views}</span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
