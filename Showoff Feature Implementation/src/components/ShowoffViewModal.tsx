import { useState, useRef, useEffect, useCallback } from 'react';
import type { Showoff, User } from '../types';
import { addComment, reportShowoff } from '../data';

const COMMENT_PAGE_SIZE = 10;

interface Props {
  showoff: Showoff;
  user: User;
  staffView?: boolean;
  onClose: () => void;
  onLike: (id: string) => void;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onOwnerClick: (ownerId: string) => void;
  onTagClick: (tag: string) => void;
  onLoginRequired: () => void;
  onShareClick: (showoff: Showoff) => void;
}

export default function ShowoffViewModal({ showoff, user, staffView, onClose, onLike, onEdit, onDelete, onOwnerClick, onTagClick, onLoginRequired, onShareClick }: Props) {
  const [commentText, setCommentText] = useState('');
  const [visibleComments, setVisibleComments] = useState(COMMENT_PAGE_SIZE);
  const [showReportModal, setShowReportModal] = useState(false);
  const [reportCategory, setReportCategory] = useState('');
  const [reportReason, setReportReason] = useState('');
  const [reportSent, setReportSent] = useState(false);
  const [copied, setCopied] = useState(false);
  const commentSentinelRef = useRef<HTMLDivElement>(null);
  const isOwner = user.isLoggedIn && user.id === showoff.ownerId && !staffView;
  const hasLiked = user.isLoggedIn && showoff.likedBy.includes(user.id);

  // Virtual comment scroll
  const loadMoreComments = useCallback(() => {
    setVisibleComments((v) => Math.min(v + COMMENT_PAGE_SIZE, showoff.comments.length));
  }, [showoff.comments.length]);

  useEffect(() => {
    const el = commentSentinelRef.current;
    if (!el) return;
    const obs = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting) loadMoreComments();
    }, { threshold: 0.1 });
    obs.observe(el);
    return () => obs.disconnect();
  }, [loadMoreComments]);

  // Prevent bg scroll
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  function submitComment() {
    if (!user.isLoggedIn) { onLoginRequired(); return; }
    if (!commentText.trim()) return;
    addComment(showoff.id, user.id, user.username, user.avatar, commentText.trim());
    setCommentText('');
  }

  function handleReport() {
    if (!user.isLoggedIn) { onLoginRequired(); return; }
    setShowReportModal(true);
  }

  function submitReport() {
    if (!reportCategory) return;
    reportShowoff(showoff.id, user.id, user.username, reportCategory, reportReason.trim());
    setReportSent(true);
  }

  function handleShare() {
    if (!user.isLoggedIn) { onLoginRequired(); return; }
    if (!showoff.isPublic) return;
    const url = `${window.location.origin}/showoff/${showoff.id}`;
    navigator.clipboard.writeText(url).catch(() => {});
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  const isLandscape = showoff.orientation !== 'portrait';

  return (
    <div
      style={{
        position: 'fixed', inset: 0, zIndex: 200,
        background: 'rgba(1,4,9,0.85)',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        padding: 20,
      }}
      onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}
    >
      <div
        style={{
          background: '#161b22',
          border: '1px solid #30363d',
          borderRadius: 14,
          width: '100%',
          maxWidth: 820,
          maxHeight: '90vh',
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {/* Header */}
        <div style={{ padding: '16px 20px', borderBottom: '1px solid #21262d', display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexShrink: 0 }}>
          <div>
            <h2 style={{ margin: 0, fontSize: 18, fontWeight: 700, color: '#e6edf3' }}>{showoff.name}</h2>
            {showoff.isDisabled && (
              <span style={{ fontSize: 11, color: '#f85149', background: '#2a1f1d', padding: '2px 8px', borderRadius: 4, fontWeight: 600, marginTop: 4, display: 'inline-block' }}>
                DISABLED BY STAFF
              </span>
            )}
          </div>
          <button onClick={onClose} style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#7d8590', padding: 6, borderRadius: 6 }}>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M18 6L6 18M6 6l12 12" /></svg>
          </button>
        </div>

        {/* Scrollable body */}
        <div style={{ overflowY: 'auto', flex: 1 }}>
          {/* Canvas display */}
          <div style={{ display: 'flex', justifyContent: 'center', padding: '20px 20px 0' }}>
          <div
            style={{
              borderRadius: 10,
              overflow: 'hidden',
              background: showoff.bgColor || '#1c2128',
              aspectRatio: isLandscape ? '16/9' : '9/16',
              width: isLandscape ? '100%' : 'auto',
              height: isLandscape ? 'auto' : 360,
              maxHeight: isLandscape ? 340 : 'none',
              position: 'relative',
            }}
          >
            {showoff.bgImage && (
              <img src={showoff.bgImage} alt="" style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover' }} />
            )}
            {showoff.items.map((item) => (
              <img
                key={item.id}
                src={item.imageUrl}
                alt={item.name}
                style={{
                  position: 'absolute',
                  left: `${item.x}%`,
                  top: `${item.y}%`,
                  width: `${item.width}%`,
                  height: `${item.height}%`,
                  objectFit: 'contain',
                  pointerEvents: 'none',
                }}
              />
            ))}
          </div>
          </div>

          {/* Details */}
          <div style={{ padding: '16px 20px 16px', display: 'flex', flexDirection: 'column', gap: 12 }}>
            {showoff.description && (
              <p style={{ margin: 0, fontSize: 14, color: '#c9d1d9', lineHeight: 1.6 }}>{showoff.description}</p>
            )}

            {/* Item names */}
            {showoff.items.length > 0 && (
              <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                {showoff.items.map((item) => (
                  <span key={item.id} style={{ fontSize: 12, color: '#7d8590', background: '#21262d', padding: '3px 10px', borderRadius: 6 }}>
                    {item.name}
                  </span>
                ))}
              </div>
            )}

            {/* Tags */}
            {showoff.tags.length > 0 && (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                {showoff.tags.map((tag) => (
                  <button
                    key={tag}
                    onClick={() => { onClose(); onTagClick(tag); }}
                    style={{
                      fontSize: 12, color: '#58a6ff', background: '#0d1f35',
                      border: '1px solid #1c3a5e', borderRadius: 20,
                      padding: '3px 10px', cursor: 'pointer', fontFamily: 'inherit',
                    }}
                  >
                    #{tag}
                  </button>
                ))}
              </div>
            )}

            {/* Meta row */}
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 16, fontSize: 12, color: '#7d8590' }}>
              <span>❤️ {showoff.likes} likes</span>
              <span>👁 {showoff.views} views</span>
              <span>Created {showoff.createdAt}</span>
              <span>Updated {showoff.updatedAt}</span>
            </div>

            {/* Owner */}
            <button
              onClick={() => onOwnerClick(showoff.ownerId)}
              style={{ display: 'flex', alignItems: 'center', gap: 10, background: 'none', border: 'none', padding: 0, cursor: 'pointer', width: 'fit-content' }}
            >
              <div style={{
                width: 32, height: 32, borderRadius: '50%',
                background: 'linear-gradient(135deg, #2a3f4a, #1a3a5c)',
                border: '2px solid #30363d',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                fontSize: 11, fontWeight: 700, color: '#c9d1d9', flexShrink: 0,
              }}>
                {showoff.ownerAvatar}
              </div>
              <span style={{ fontSize: 13, color: '#58a6ff', fontWeight: 600 }}>{showoff.ownerName}</span>
            </button>

            {/* Disabled notice for owner */}
            {showoff.isDisabled && showoff.ownerId === user.id && (
              <div style={{ background: '#2a1f1d', border: '1px solid #5a2030', borderRadius: 8, padding: 14 }}>
                <p style={{ margin: '0 0 8px', fontSize: 13, color: '#f85149', fontWeight: 600 }}>
                  This showoff has been disabled by staff.
                </p>
                <p style={{ margin: '0 0 10px', fontSize: 13, color: '#c9d1d9' }}>
                  To appeal this decision, please contact our support team.
                </p>
                <button style={{ background: '#f85149', border: 'none', borderRadius: 6, padding: '7px 16px', fontSize: 13, fontWeight: 600, color: '#fff', cursor: 'pointer', fontFamily: 'inherit' }}>
                  Appeal
                </button>
              </div>
            )}

            {/* Action buttons */}
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              <button
                onClick={() => onLike(showoff.id)}
                style={{
                  display: 'flex', alignItems: 'center', gap: 6,
                  background: hasLiked ? '#2d1b1e' : '#21262d',
                  border: `1px solid ${hasLiked ? '#5a2030' : '#30363d'}`,
                  borderRadius: 7, padding: '7px 14px', fontSize: 13,
                  color: hasLiked ? '#f85149' : '#7d8590', cursor: 'pointer', fontFamily: 'inherit',
                }}
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill={hasLiked ? '#f85149' : 'none'} stroke={hasLiked ? '#f85149' : '#7d8590'} strokeWidth="2">
                  <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
                </svg>
                {hasLiked ? 'Liked' : 'Like'} · {showoff.likes}
              </button>

              {showoff.isPublic && (
                <button
                  onClick={handleShare}
                  style={{
                    display: 'flex', alignItems: 'center', gap: 6,
                    background: '#21262d', border: '1px solid #30363d',
                    borderRadius: 7, padding: '7px 14px', fontSize: 13,
                    color: copied ? '#3fb950' : '#7d8590', cursor: 'pointer', fontFamily: 'inherit',
                  }}
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <circle cx="18" cy="5" r="3"/><circle cx="6" cy="12" r="3"/><circle cx="18" cy="19" r="3"/>
                    <path d="m8.59 13.51 6.83 3.98M15.41 6.51l-6.82 3.98"/>
                  </svg>
                  {copied ? 'Copied!' : 'Share'}
                </button>
              )}

              {!reportSent && (
                <button
                  onClick={handleReport}
                  style={{
                    display: 'flex', alignItems: 'center', gap: 6,
                    background: '#21262d', border: '1px solid #30363d',
                    borderRadius: 7, padding: '7px 14px', fontSize: 13,
                    color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit',
                  }}
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"/><line x1="4" y1="22" x2="4" y2="15"/>
                  </svg>
                  Report
                </button>
              )}
              {reportSent && <span style={{ fontSize: 13, color: '#3fb950', padding: '7px 0' }}>Report submitted</span>}

              {isOwner && (
                <>
                  <button onClick={() => onEdit(showoff.id)} style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '7px 14px', fontSize: 13, color: '#e6edf3', cursor: 'pointer', fontFamily: 'inherit' }}>
                    Edit
                  </button>
                  <button onClick={() => onDelete(showoff.id)} style={{ background: '#2a1f1d', border: '1px solid #5a2030', borderRadius: 7, padding: '7px 14px', fontSize: 13, color: '#f85149', cursor: 'pointer', fontFamily: 'inherit' }}>
                    Delete
                  </button>
                </>
              )}
            </div>
          </div>

          {/* Comments section */}
          <div style={{ padding: '0 20px 20px', borderTop: '1px solid #21262d', paddingTop: 16 }}>
            <h3 style={{ margin: '0 0 14px', fontSize: 14, fontWeight: 600, color: '#e6edf3' }}>
              Comments ({showoff.comments.length})
            </h3>

            {/* Add comment */}
            {user.isLoggedIn ? (
              <div style={{ display: 'flex', gap: 10, marginBottom: 16 }}>
                <div style={{ width: 30, height: 30, borderRadius: '50%', background: '#2a3f4a', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 11, fontWeight: 700, color: '#c9d1d9', flexShrink: 0 }}>
                  {user.avatar}
                </div>
                <div style={{ flex: 1, display: 'flex', gap: 8 }}>
                  <input
                    type="text"
                    value={commentText}
                    onChange={(e) => setCommentText(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && submitComment()}
                    placeholder="Write a comment..."
                    style={{ flex: 1, background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '8px 12px', fontSize: 13, color: '#e6edf3', outline: 'none', fontFamily: 'inherit' }}
                  />
                  <button
                    onClick={submitComment}
                    style={{ background: '#2dd4bf', border: 'none', borderRadius: 7, padding: '8px 14px', fontSize: 13, fontWeight: 600, color: '#0d1117', cursor: 'pointer', fontFamily: 'inherit' }}
                  >
                    Post
                  </button>
                </div>
              </div>
            ) : (
              <p style={{ fontSize: 13, color: '#7d8590', marginBottom: 16 }}>
                <button onClick={onLoginRequired} style={{ background: 'none', border: 'none', color: '#58a6ff', cursor: 'pointer', fontSize: 13, padding: 0, fontFamily: 'inherit' }}>Log in</button> to comment.
              </p>
            )}

            {/* Comment list */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              {showoff.comments.slice(0, visibleComments).map((c) => (
                <div key={c.id} style={{ display: 'flex', gap: 10 }}>
                  <div style={{ width: 28, height: 28, borderRadius: '50%', background: '#2a3450', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 10, fontWeight: 700, color: '#c9d1d9', flexShrink: 0 }}>
                    {c.avatar}
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, marginBottom: 2 }}>
                      <span style={{ fontSize: 13, fontWeight: 600, color: '#e6edf3' }}>{c.username}</span>
                      <span style={{ fontSize: 11, color: '#484f58' }}>{c.createdAt}</span>
                    </div>
                    <p style={{ margin: 0, fontSize: 13, color: '#c9d1d9', lineHeight: 1.5 }}>{c.text}</p>
                  </div>
                </div>
              ))}
            </div>
            <div ref={commentSentinelRef} style={{ height: 10 }} />
            {visibleComments < showoff.comments.length && (
              <p style={{ textAlign: 'center', fontSize: 12, color: '#484f58', marginTop: 8 }}>Scroll to load more comments</p>
            )}
          </div>
        </div>
      </div>

      {/* Report sub-modal */}
      {showReportModal && (
        <div style={{ position: 'fixed', inset: 0, zIndex: 300, background: 'rgba(1,4,9,0.7)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 20 }}>
          <div style={{ background: '#161b22', border: '1px solid #30363d', borderRadius: 12, padding: 24, width: '100%', maxWidth: 400 }}>
            <h3 style={{ margin: '0 0 12px', fontSize: 16, fontWeight: 700, color: '#e6edf3' }}>Report Showoff</h3>
            {reportSent ? (
              <p style={{ fontSize: 14, color: '#3fb950' }}>Thank you — your report has been submitted for review.</p>
            ) : (
              <>
                <div style={{ marginBottom: 12 }}>
                  <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#c9d1d9', marginBottom: 6 }}>
                    Reason
                  </label>
                  <select
                    value={reportCategory}
                    onChange={(e) => setReportCategory(e.target.value)}
                    style={{ width: '100%', background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '9px 12px', fontSize: 13, color: reportCategory ? '#e6edf3' : '#7d8590', outline: 'none', fontFamily: 'inherit', cursor: 'pointer' }}
                  >
                    <option value="">Select a reason...</option>
                    <option>Counterfeit / Fake item</option>
                    <option>Inappropriate content</option>
                    <option>Spam or self-promotion</option>
                    <option>Copyright infringement</option>
                    <option>Misleading information</option>
                    <option>Harassment or hate speech</option>
                    <option>Other reasons</option>
                  </select>
                </div>
                <div style={{ marginBottom: 12 }}>
                  <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#c9d1d9', marginBottom: 6 }}>
                    Describe the issue
                    {reportCategory !== 'Other reasons' && reportCategory && <span style={{ fontWeight: 400, color: '#7d8590' }}> (optional)</span>}
                  </label>
                  <textarea
                    value={reportReason}
                    onChange={(e) => setReportReason(e.target.value)}
                    placeholder="Provide any additional context..."
                    rows={3}
                    style={{ width: '100%', background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '10px 12px', fontSize: 13, color: '#e6edf3', resize: 'none', outline: 'none', fontFamily: 'inherit' }}
                  />
                </div>
                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
                  <button onClick={() => setShowReportModal(false)} style={{ background: 'none', border: '1px solid #30363d', borderRadius: 7, padding: '8px 16px', fontSize: 13, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>Cancel</button>
                  <button onClick={submitReport} disabled={!reportCategory} style={{ background: reportCategory ? '#f85149' : '#3a2020', border: 'none', borderRadius: 7, padding: '8px 16px', fontSize: 13, fontWeight: 600, color: reportCategory ? '#fff' : '#7d8590', cursor: reportCategory ? 'pointer' : 'not-allowed', fontFamily: 'inherit' }}>Submit Report</button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
