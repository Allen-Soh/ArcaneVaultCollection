import { useEffect } from 'react';
import type { Showoff, User } from '../types';

// ─── Delete Confirmation Modal ─────────────────────────────────────────────

interface DeleteModalProps {
  showoff: Showoff;
  onConfirm: () => void;
  onCancel: () => void;
}

export function DeleteModal({ showoff, onConfirm, onCancel }: DeleteModalProps) {
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  return (
    <div style={overlayStyle} onClick={(e) => { if (e.target === e.currentTarget) onCancel(); }}>
      <div style={{ ...cardStyle, maxWidth: 400 }}>
        <div style={{ padding: '20px 24px', borderBottom: '1px solid #21262d', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <h2 style={{ margin: 0, fontSize: 17, fontWeight: 700, color: '#e6edf3' }}>Delete Showoff</h2>
          <button onClick={onCancel} style={iconBtnStyle}>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M18 6L6 18M6 6l12 12" /></svg>
          </button>
        </div>
        <div style={{ padding: '20px 24px' }}>
          <p style={{ margin: '0 0 8px', fontSize: 14, color: '#c9d1d9' }}>
            Are you sure you want to delete <strong style={{ color: '#e6edf3' }}>{showoff.name}</strong>?
          </p>
          <p style={{ margin: '0 0 20px', fontSize: 13, color: '#f85149' }}>This cannot be undone.</p>
          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
            <button onClick={onCancel} style={{ background: 'none', border: '1px solid #30363d', borderRadius: 7, padding: '8px 20px', fontSize: 13, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>
              Cancel
            </button>
            <button onClick={onConfirm} style={{ background: '#f85149', border: 'none', borderRadius: 7, padding: '8px 20px', fontSize: 13, fontWeight: 600, color: '#fff', cursor: 'pointer', fontFamily: 'inherit' }}>
              Yes, delete
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Manage Showoffs Modal ─────────────────────────────────────────────────

interface ManageModalProps {
  showoffs: Showoff[];
  user: User;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onClose: () => void;
}

export function ManageModal({ showoffs, user, onEdit, onDelete, onClose }: ManageModalProps) {
  const myShowoffs = showoffs.filter((s) => s.ownerId === user.id);

  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  return (
    <div style={overlayStyle} onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>
      <div style={{ ...cardStyle, maxWidth: 440 }}>
        <div style={{ padding: '20px 24px', borderBottom: '1px solid #21262d', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <h2 style={{ margin: 0, fontSize: 17, fontWeight: 700, color: '#e6edf3' }}>Manage Showoffs</h2>
          <button onClick={onClose} style={iconBtnStyle}>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M18 6L6 18M6 6l12 12" /></svg>
          </button>
        </div>
        <div style={{ padding: '12px 24px 20px', display: 'flex', flexDirection: 'column', gap: 10, maxHeight: 400, overflowY: 'auto' }}>
          {myShowoffs.length === 0 && (
            <p style={{ fontSize: 13, color: '#484f58', margin: '10px 0' }}>You haven't created any showoffs yet.</p>
          )}
          {myShowoffs.map((s) => (
            <div key={s.id} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderBottom: '1px solid #21262d' }}>
              <div style={{ flex: 1, minWidth: 0 }}>
                <p style={{ margin: '0 0 2px', fontSize: 14, fontWeight: 600, color: '#e6edf3', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{s.name}</p>
                <p style={{ margin: 0, fontSize: 12, color: '#7d8590' }}>
                  {s.isPublic ? 'Public' : 'Private'} · {s.items.length} item{s.items.length !== 1 ? 's' : ''}
                </p>
              </div>
              <div style={{ display: 'flex', gap: 6, flexShrink: 0 }}>
                <button
                  onClick={() => { onEdit(s.id); onClose(); }}
                  style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 12px', fontSize: 12, color: '#e6edf3', cursor: 'pointer', fontFamily: 'inherit' }}
                >
                  Edit
                </button>
                <button
                  onClick={() => { onDelete(s.id); onClose(); }}
                  style={{ background: '#2a1f1d', border: '1px solid #5a2030', borderRadius: 6, padding: '5px 12px', fontSize: 12, color: '#f85149', cursor: 'pointer', fontFamily: 'inherit' }}
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
        <div style={{ padding: '0 24px 20px' }}>
          <button onClick={onClose} style={{ width: '100%', background: 'none', border: '1px solid #30363d', borderRadius: 7, padding: '9px 0', fontSize: 14, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>
            Close
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Login Required Modal ──────────────────────────────────────────────────

interface LoginModalProps {
  message?: string;
  onLogin: () => void;
  onClose: () => void;
}

export function LoginModal({ message, onLogin, onClose }: LoginModalProps) {
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  return (
    <div style={overlayStyle} onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>
      <div style={{ ...cardStyle, maxWidth: 360 }}>
        <div style={{ padding: '20px 24px', borderBottom: '1px solid #21262d' }}>
          <h2 style={{ margin: 0, fontSize: 17, fontWeight: 700, color: '#e6edf3' }}>Login Required</h2>
        </div>
        <div style={{ padding: '20px 24px' }}>
          <p style={{ margin: '0 0 20px', fontSize: 14, color: '#c9d1d9' }}>
            {message || 'You must be logged in, please log in.'}
          </p>
          <div style={{ display: 'flex', gap: 10 }}>
            <button onClick={onClose} style={{ flex: 1, background: 'none', border: '1px solid #30363d', borderRadius: 7, padding: '9px 0', fontSize: 13, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>
              Cancel
            </button>
            <button onClick={onLogin} style={{ flex: 2, background: '#2dd4bf', border: 'none', borderRadius: 7, padding: '9px 0', fontSize: 13, fontWeight: 700, color: '#0d1117', cursor: 'pointer', fontFamily: 'inherit' }}>
              Log in
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Staff Disable Confirmation ────────────────────────────────────────────

interface DisableModalProps {
  showoff: Showoff;
  onConfirm: () => void;
  onCancel: () => void;
}

export function DisableModal({ showoff, onConfirm, onCancel }: DisableModalProps) {
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  return (
    <div style={overlayStyle} onClick={(e) => { if (e.target === e.currentTarget) onCancel(); }}>
      <div style={{ ...cardStyle, maxWidth: 400 }}>
        <div style={{ padding: '20px 24px', borderBottom: '1px solid #21262d' }}>
          <h2 style={{ margin: 0, fontSize: 17, fontWeight: 700, color: '#e6edf3' }}>
            {showoff.isDisabled ? 'Re-enable Showoff' : 'Disable Showoff'}
          </h2>
        </div>
        <div style={{ padding: '20px 24px' }}>
          <p style={{ margin: '0 0 8px', fontSize: 14, color: '#c9d1d9' }}>
            {showoff.isDisabled
              ? `Re-enable "${showoff.name}"? It will become public again.`
              : `Disable "${showoff.name}"? This will make it permanently private until re-enabled by staff.`}
          </p>
          {!showoff.isDisabled && (
            <p style={{ margin: '0 0 16px', fontSize: 13, color: '#f85149' }}>The owner will be notified and given an appeal option.</p>
          )}
          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 16 }}>
            <button onClick={onCancel} style={{ background: 'none', border: '1px solid #30363d', borderRadius: 7, padding: '8px 20px', fontSize: 13, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>
              Cancel
            </button>
            <button onClick={onConfirm} style={{ background: showoff.isDisabled ? '#3fb950' : '#f85149', border: 'none', borderRadius: 7, padding: '8px 20px', fontSize: 13, fontWeight: 600, color: '#fff', cursor: 'pointer', fontFamily: 'inherit' }}>
              {showoff.isDisabled ? 'Re-enable' : 'Disable'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

const overlayStyle: React.CSSProperties = {
  position: 'fixed', inset: 0, zIndex: 200,
  background: 'rgba(1,4,9,0.85)',
  display: 'flex', alignItems: 'center', justifyContent: 'center',
  padding: 20,
};

const cardStyle: React.CSSProperties = {
  background: '#161b22',
  border: '1px solid #30363d',
  borderRadius: 14,
  width: '100%',
};

const iconBtnStyle: React.CSSProperties = {
  background: 'none', border: 'none', cursor: 'pointer',
  color: '#7d8590', padding: 6, borderRadius: 6,
};
