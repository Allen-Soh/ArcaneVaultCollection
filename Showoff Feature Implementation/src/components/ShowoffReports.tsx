import { useState } from 'react';
import type { Showoff, User } from '../types';
import ShowoffMiniCanvas from './ShowoffMiniCanvas';
import { DisableModal } from './Modals';
import { disableShowoff, enableShowoff } from '../data';

const REPORT_CATEGORIES = [
  'All',
  'Counterfeit / Fake item',
  'Inappropriate content',
  'Spam or self-promotion',
  'Copyright infringement',
  'Misleading information',
  'Harassment or hate speech',
  'Other reasons',
];

const CATEGORY_COLORS: Record<string, { bg: string; border: string; color: string }> = {
  'Counterfeit / Fake item':    { bg: '#2a1f0d', border: '#7a4a10', color: '#d29922' },
  'Inappropriate content':      { bg: '#2a1f1d', border: '#5a2030', color: '#f85149' },
  'Spam or self-promotion':     { bg: '#1a2035', border: '#2a3a60', color: '#58a6ff' },
  'Copyright infringement':     { bg: '#2a1a3a', border: '#5a2a7a', color: '#a371f7' },
  'Misleading information':     { bg: '#1a2a1a', border: '#2a5020', color: '#3fb950' },
  'Harassment or hate speech':  { bg: '#2a1f1d', border: '#7a3020', color: '#f0703a' },
  'Other reasons':              { bg: '#21262d', border: '#30363d', color: '#7d8590' },
};

interface Props {
  showoffs: Showoff[];
  user: User;
  onRefresh: () => void;
  onView: (id: string, staffView?: boolean) => void;
}

export default function ShowoffReports({ showoffs, user, onRefresh, onView }: Props) {
  const [disablingShowoff, setDisablingShowoff] = useState<Showoff | null>(null);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [filterCategory, setFilterCategory] = useState('All');

  if (!user.isStaff) {
    return (
      <div style={{ maxWidth: 900, margin: '0 auto', padding: '60px 24px', textAlign: 'center', color: '#484f58' }}>
        Access restricted to staff only.
      </div>
    );
  }

  const reported = showoffs.filter((s) => s.reports.length > 0);

  const filtered = filterCategory === 'All'
    ? reported
    : reported.filter((s) => s.reports.some((r) => r.category === filterCategory));

  function handleDisableToggle(showoff: Showoff) {
    if (showoff.isDisabled) {
      enableShowoff(showoff.id);
      setActionMessage(`"${showoff.name}" has been re-enabled.`);
    } else {
      disableShowoff(showoff.id);
      setActionMessage(`"${showoff.name}" has been disabled.`);
    }
    setDisablingShowoff(null);
    onRefresh();
    setTimeout(() => setActionMessage(null), 4000);
  }

  return (
    <div style={{ maxWidth: 1000, margin: '0 auto', padding: '32px 24px' }}>
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 20, flexWrap: 'wrap', gap: 12 }}>
        <div>
          <h1 style={{ margin: '0 0 4px', fontSize: 22, fontWeight: 700, color: '#e6edf3' }}>Showoff Reports</h1>
          <p style={{ margin: 0, fontSize: 14, color: '#7d8590' }}>
            {reported.length} showoff{reported.length !== 1 ? 's' : ''} with pending reports
          </p>
        </div>

        {/* Category filter */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <label style={{ fontSize: 12, color: '#7d8590', whiteSpace: 'nowrap' }}>Filter by reason:</label>
          <select
            value={filterCategory}
            onChange={(e) => setFilterCategory(e.target.value)}
            style={{ background: '#161b22', border: '1px solid #30363d', borderRadius: 6, padding: '7px 10px', fontSize: 13, color: '#e6edf3', cursor: 'pointer', fontFamily: 'inherit', outline: 'none' }}
          >
            {REPORT_CATEGORIES.map((c) => <option key={c}>{c}</option>)}
          </select>
        </div>
      </div>

      {actionMessage && (
        <div style={{ background: '#122016', border: '1px solid #2a5020', borderRadius: 8, padding: '12px 16px', marginBottom: 20, fontSize: 13, color: '#3fb950' }}>
          {actionMessage}
        </div>
      )}

      {filtered.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: '#484f58' }}>
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" style={{ marginBottom: 12 }}>
            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
          </svg>
          <p style={{ margin: 0, fontSize: 14 }}>
            {filterCategory === 'All' ? 'No reported showoffs — all clear!' : `No reports matching "${filterCategory}"`}
          </p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          {filtered.map((showoff) => {
            return (
              <div key={showoff.id} style={{ background: '#161b22', border: `1px solid ${showoff.isDisabled ? '#5a2030' : '#21262d'}`, borderRadius: 10, padding: 16 }}>
                <div style={{ display: 'flex', gap: 16, alignItems: 'flex-start' }}>
                  <div onClick={() => onView(showoff.id, true)} style={{ cursor: 'pointer', flexShrink: 0 }}>
                    <ShowoffMiniCanvas showoff={showoff} width={140} height={78} />
                  </div>

                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6 }}>
                      <button onClick={() => onView(showoff.id, true)} style={{ background: 'none', border: 'none', padding: 0, cursor: 'pointer', fontSize: 15, fontWeight: 700, color: '#e6edf3', fontFamily: 'inherit' }}>
                        {showoff.name}
                      </button>
                      {showoff.isDisabled && (
                        <span style={{ fontSize: 10, color: '#f85149', background: '#2a1f1d', padding: '2px 7px', borderRadius: 4, fontWeight: 600 }}>DISABLED</span>
                      )}
                      <span style={{ fontSize: 10, color: '#7d8590', background: '#21262d', padding: '2px 7px', borderRadius: 4 }}>
                        {showoff.reports.length} report{showoff.reports.length !== 1 ? 's' : ''}
                      </span>
                    </div>

                    {/* Owner */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 10 }}>
                      <span style={{ fontSize: 12, color: '#58a6ff' }}>{showoff.ownerName}</span>
                      <span style={{ fontSize: 11, color: '#484f58' }}>· ❤️ {showoff.likes} · 👁 {showoff.views}</span>
                    </div>

                    {/* Tags */}
                    {showoff.tags.length > 0 && (
                      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4, marginBottom: 10 }}>
                        {showoff.tags.slice(0, 5).map((tag) => (
                          <span key={tag} style={{ fontSize: 11, color: '#58a6ff', background: '#0d1f35', border: '1px solid #1c3a5e', borderRadius: 4, padding: '2px 7px' }}>
                            {tag}
                          </span>
                        ))}
                      </div>
                    )}

                    {/* Individual reports */}
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
                      {showoff.reports
                        .filter((r) => filterCategory === 'All' || r.category === filterCategory)
                        .map((r) => {
                          const rs = CATEGORY_COLORS[r.category] ?? CATEGORY_COLORS['Other reasons'];
                          return (
                            <div key={r.id} style={{ background: '#21262d', borderRadius: 6, padding: '8px 12px', fontSize: 12 }}>
                              <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: r.reason ? 4 : 0, flexWrap: 'wrap' }}>
                                <span style={{ color: '#7d8590' }}>Reported by </span>
                                <span style={{ color: '#e6edf3', fontWeight: 600 }}>{r.reporterName}</span>
                                <span style={{ color: '#484f58' }}>{r.createdAt}</span>
                                <span style={{ background: rs.bg, border: `1px solid ${rs.border}`, color: rs.color, borderRadius: 4, padding: '1px 7px', fontSize: 10, fontWeight: 600 }}>
                                  {r.category}
                                </span>
                              </div>
                              {r.reason && <p style={{ margin: 0, color: '#c9d1d9' }}>{r.reason}</p>}
                            </div>
                          );
                        })}
                    </div>
                  </div>
                </div>

                {/* Staff actions — bottom right */}
                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 14 }}>
                  <button onClick={() => onView(showoff.id, true)} style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '7px 16px', fontSize: 12, color: '#e6edf3', cursor: 'pointer', fontFamily: 'inherit', whiteSpace: 'nowrap' }}>
                    View
                  </button>
                  <button
                    onClick={() => setDisablingShowoff(showoff)}
                    style={{
                      background: showoff.isDisabled ? '#122016' : '#2a1f1d',
                      border: `1px solid ${showoff.isDisabled ? '#2a5020' : '#5a2030'}`,
                      borderRadius: 7, padding: '7px 16px', fontSize: 12, fontWeight: 600,
                      color: showoff.isDisabled ? '#3fb950' : '#f85149',
                      cursor: 'pointer', fontFamily: 'inherit', whiteSpace: 'nowrap',
                    }}
                  >
                    {showoff.isDisabled ? 'Re-enable' : 'Disable'}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {disablingShowoff && (
        <DisableModal
          showoff={disablingShowoff}
          onConfirm={() => handleDisableToggle(disablingShowoff)}
          onCancel={() => setDisablingShowoff(null)}
        />
      )}
    </div>
  );
}
