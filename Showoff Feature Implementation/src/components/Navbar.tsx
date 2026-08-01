import type { Page, User } from '../types';

interface NavbarProps {
  user: User;
  currentPage: Page;
  onNavigate: (page: Page) => void;
  onToggleAuth: () => void;
}

export default function Navbar({ user, currentPage, onNavigate, onToggleAuth }: NavbarProps) {
  const pageName = currentPage.name;

  return (
    <nav
      style={{
        background: '#0d1117',
        borderBottom: '1px solid #21262d',
        position: 'sticky',
        top: 0,
        zIndex: 100,
      }}
    >
      <div
        style={{
          maxWidth: 1200,
          margin: '0 auto',
          padding: '0 24px',
          height: 56,
          display: 'flex',
          alignItems: 'center',
          gap: 32,
        }}
      >
        {/* Logo */}
        <button
          onClick={() => onNavigate({ name: 'feed' })}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            background: 'none',
            border: 'none',
            cursor: 'pointer',
            padding: 0,
            flexShrink: 0,
          }}
        >
          <svg width="22" height="22" viewBox="0 0 22 22" fill="none">
            <polygon points="11,2 20,7 20,15 11,20 2,15 2,7" stroke="#2dd4bf" strokeWidth="1.5" fill="none" />
            <polygon points="11,5 17,8.5 17,13.5 11,17 5,13.5 5,8.5" fill="#2dd4bf" opacity="0.25" />
            <circle cx="11" cy="11" r="2.5" fill="#2dd4bf" />
          </svg>
          <span style={{ color: '#e6edf3', fontWeight: 700, fontSize: 15, letterSpacing: '-0.01em' }}>
            Arcane Vault
          </span>
        </button>

        {/* Nav links */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 4, flex: 1 }}>
          {(['Home', 'Insights', 'Categories', 'Showoffs', 'Collection Items'] as const).map((label) => {
            const isShowoffs = label === 'Showoffs';
            const isActive = isShowoffs && (pageName === 'feed' || pageName === 'profile' || pageName === 'reports' || pageName === 'tag');
            return (
              <button
                key={label}
                onClick={() => isShowoffs && onNavigate({ name: 'feed' })}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  padding: '6px 12px',
                  borderRadius: 6,
                  fontSize: 14,
                  fontWeight: isActive ? 600 : 400,
                  color: isActive ? '#e6edf3' : '#7d8590',
                  transition: 'color 0.15s, background 0.15s',
                }}
                onMouseEnter={(e) => { if (!isActive) (e.target as HTMLElement).style.color = '#c9d1d9'; }}
                onMouseLeave={(e) => { if (!isActive) (e.target as HTMLElement).style.color = '#7d8590'; }}
              >
                {label}
              </button>
            );
          })}

          {user.isLoggedIn && user.isStaff && (
            <button
              onClick={() => onNavigate({ name: 'reports' })}
              style={{
                background: pageName === 'reports' ? '#2a1f1d' : 'none',
                border: pageName === 'reports' ? '1px solid #f85149' : 'none',
                cursor: 'pointer',
                padding: '5px 12px',
                borderRadius: 6,
                fontSize: 13,
                fontWeight: 500,
                color: '#f85149',
              }}
            >
              Reports
            </button>
          )}
        </div>

        {/* Auth area */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          {user.isLoggedIn ? (
            <>
              <button
                onClick={onToggleAuth}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  padding: '6px 12px',
                  borderRadius: 6,
                  fontSize: 14,
                  color: '#7d8590',
                }}
              >
                Logout
              </button>
              <button
                onClick={() => onNavigate({ name: 'profile' })}
                style={{
                  width: 32,
                  height: 32,
                  borderRadius: '50%',
                  background: pageName === 'profile' ? '#2dd4bf' : '#2a3f4a',
                  border: pageName === 'profile' ? '2px solid #2dd4bf' : '2px solid #30363d',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: 11,
                  fontWeight: 700,
                  color: pageName === 'profile' ? '#0d1117' : '#c9d1d9',
                  flexShrink: 0,
                }}
              >
                {user.avatar}
              </button>
            </>
          ) : (
            <button
              onClick={onToggleAuth}
              style={{
                background: '#2dd4bf',
                border: 'none',
                cursor: 'pointer',
                padding: '7px 16px',
                borderRadius: 6,
                fontSize: 14,
                fontWeight: 600,
                color: '#0d1117',
              }}
            >
              Login
            </button>
          )}
        </div>
      </div>
    </nav>
  );
}
