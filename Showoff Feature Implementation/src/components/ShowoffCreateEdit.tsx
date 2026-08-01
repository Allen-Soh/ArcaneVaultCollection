import { useState, useRef, useCallback } from 'react';
import type { Showoff, ShowoffItem, User } from '../types';
import { SITE_CATEGORIES } from '../categories';
import { saveShowoff } from '../data';

interface DragState {
  itemId: string;
  mode: 'move' | 'resize';
  startMouseX: number;
  startMouseY: number;
  startItemX: number;
  startItemY: number;
  startItemW: number;
  startItemH: number;
}

interface Props {
  user: User;
  editShowoff?: Showoff;
  onSave: () => void;
  onCancel: () => void;
}

function uid() { return Math.random().toString(36).slice(2, 10); }

export default function ShowoffCreateEdit({ user, editShowoff, onSave, onCancel }: Props) {
  const isEdit = !!editShowoff;
  const [name, setName] = useState(editShowoff?.name ?? '');
  const [bgColor, setBgColor] = useState(editShowoff?.bgColor ?? '#1c2128');
  const [bgImage, setBgImage] = useState<string | undefined>(editShowoff?.bgImage);
  const [orientation, setOrientation] = useState<'landscape' | 'portrait'>(editShowoff?.orientation ?? 'landscape');
  const [isPublic, setIsPublic] = useState(editShowoff?.isPublic ?? true);
  const [description, setDescription] = useState(editShowoff?.description ?? '');
  const [tags, setTags] = useState<string[]>(editShowoff?.tags ?? []);
  const [catQuery, setCatQuery] = useState('');
  const [catOpen, setCatOpen] = useState(false);
  const [items, setItems] = useState<ShowoffItem[]>(editShowoff?.items ?? []);
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null);
  const [expanded, setExpanded] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  const canvasRef = useRef<HTMLDivElement>(null);
  const dragRef = useRef<DragState | null>(null);
  const fileInputRefs = useRef<(HTMLInputElement | null)[]>([null, null, null]);

  const ASPECT = orientation === 'landscape' ? 16 / 9 : 9 / 16;

  // Drag/resize handlers
  const startDrag = useCallback((e: React.PointerEvent, itemId: string, mode: 'move' | 'resize') => {
    e.preventDefault();
    e.stopPropagation();
    setSelectedItemId(itemId);
    const item = items.find((i) => i.id === itemId);
    if (!item) return;
    dragRef.current = {
      itemId, mode,
      startMouseX: e.clientX,
      startMouseY: e.clientY,
      startItemX: item.x,
      startItemY: item.y,
      startItemW: item.width,
      startItemH: item.height,
    };
    (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
  }, [items]);

  const onPointerMove = useCallback((e: React.PointerEvent) => {
    const d = dragRef.current;
    if (!d || !canvasRef.current) return;
    const rect = canvasRef.current.getBoundingClientRect();
    const dx = ((e.clientX - d.startMouseX) / rect.width) * 100;
    const dy = ((e.clientY - d.startMouseY) / rect.height) * 100;

    setItems((prev) => prev.map((item) => {
      if (item.id !== d.itemId) return item;
      if (d.mode === 'move') {
        return {
          ...item,
          x: Math.max(0, Math.min(100 - item.width, d.startItemX + dx)),
          y: Math.max(0, Math.min(100 - item.height, d.startItemY + dy)),
        };
      } else {
        const newW = Math.max(8, Math.min(100 - d.startItemX, d.startItemW + dx));
        const newH = Math.max(8, Math.min(100 - d.startItemY, d.startItemH + dy));
        return { ...item, width: newW, height: newH };
      }
    }));
  }, []);

  const onPointerUp = useCallback(() => { dragRef.current = null; }, []);

  function addItemSlot() {
    if (items.length >= 3) return;
    const newItem: ShowoffItem = {
      id: uid(),
      name: '',
      imageUrl: '',
      x: 10 + items.length * 30,
      y: 20,
      width: 25,
      height: 60,
    };
    setItems((prev) => [...prev, newItem]);
    setSelectedItemId(newItem.id);
  }

  function removeItem(id: string) {
    setItems((prev) => prev.filter((i) => i.id !== id));
    if (selectedItemId === id) setSelectedItemId(null);
  }

  function updateItemName(id: string, name: string) {
    setItems((prev) => prev.map((i) => i.id === id ? { ...i, name } : i));
  }

  function handleImageUpload(e: React.ChangeEvent<HTMLInputElement>, itemId: string) {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (ev) => {
      const url = ev.target?.result as string;
      setItems((prev) => prev.map((i) => i.id === itemId ? { ...i, imageUrl: url } : i));
    };
    reader.readAsDataURL(file);
  }

  function handleBgImageUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (ev) => { setBgImage(ev.target?.result as string); };
    reader.readAsDataURL(file);
  }

  function validate() {
    const errs: string[] = [];
    if (!name.trim()) errs.push('Showoff name is required.');
    if (name.length > 40) errs.push('Name must be 40 characters or fewer.');
    return errs;
  }

  function handleSave() {
    const errs = validate();
    if (errs.length) { setErrors(errs); return; }
    const showoff: Showoff = {
      id: editShowoff?.id ?? uid(),
      name: name.trim(),
      description: description.trim(),
      bgColor,
      bgImage,
      orientation,
      items,
      isPublic,
      isDisabled: editShowoff?.isDisabled ?? false,
      tags,
      ownerId: user.id,
      ownerName: user.username,
      ownerAvatar: user.avatar,
      likes: editShowoff?.likes ?? 0,
      likedBy: editShowoff?.likedBy ?? [],
      views: editShowoff?.views ?? 0,
      createdAt: editShowoff?.createdAt ?? new Date().toISOString().split('T')[0],
      updatedAt: new Date().toISOString().split('T')[0],
      comments: editShowoff?.comments ?? [],
      reports: editShowoff?.reports ?? [],
    };
    saveShowoff(showoff);
    onSave();
  }

  const canvasWidth = orientation === 'landscape' ? 480 : 270;
  const canvasHeight = orientation === 'landscape' ? 270 : 480;

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto', padding: '32px 24px' }}>
      <div style={{ marginBottom: 24 }}>
        <h1 style={{ margin: '0 0 4px', fontSize: 24, fontWeight: 700, color: '#e6edf3' }}>
          {isEdit ? 'Edit Showoff' : 'Create Showoff'}
        </h1>
        <p style={{ margin: 0, fontSize: 14, color: '#7d8590' }}>
          {isEdit ? 'Update your showoff display.' : 'Design a visual display for your collection items.'}
        </p>
      </div>

      {errors.length > 0 && (
        <div style={{ background: '#2a1f1d', border: '1px solid #5a2030', borderRadius: 8, padding: '12px 16px', marginBottom: 20 }}>
          {errors.map((e, i) => <p key={i} style={{ margin: i < errors.length - 1 ? '0 0 4px' : 0, fontSize: 13, color: '#f85149' }}>{e}</p>)}
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 32, alignItems: 'start' }}>
        {/* Left: form */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          {/* Name */}
          <div>
            <label style={labelStyle}>Showoff Name</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter showoff name"
              maxLength={40}
              style={inputStyle}
            />
            <p style={{ margin: '4px 0 0', fontSize: 11, color: '#484f58' }}>{name.length}/40</p>
          </div>

          {/* Description */}
          <div>
            <label style={labelStyle}>Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe your showoff..."
              rows={3}
              style={{ ...inputStyle, resize: 'none' }}
            />
          </div>

          {/* Background */}
          <div>
            <label style={labelStyle}>Background</label>
            <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <input type="color" value={bgColor} onChange={(e) => { setBgColor(e.target.value); setBgImage(undefined); }}
                  style={{ width: 36, height: 36, border: '1px solid #30363d', borderRadius: 6, cursor: 'pointer', background: 'none', padding: 2 }} />
                <code style={{ fontSize: 12, color: '#7d8590', fontFamily: 'monospace' }}>{bgColor}</code>
              </div>
              <span style={{ fontSize: 12, color: '#484f58' }}>or</span>
              <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer', fontSize: 12, color: '#58a6ff' }}>
                <input type="file" accept="image/*" onChange={handleBgImageUpload} style={{ display: 'none' }} />
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                Upload image
              </label>
              {bgImage && (
                <button onClick={() => setBgImage(undefined)} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: 12, color: '#f85149', fontFamily: 'inherit' }}>
                  Remove
                </button>
              )}
            </div>
          </div>

          {/* Visibility */}
          <div>
            <label style={labelStyle}>Visibility</label>
            <div style={{ display: 'flex', gap: 16 }}>
              {(['public', 'private'] as const).map((v) => (
                <label key={v} style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontSize: 14, color: '#e6edf3' }}>
                  <input
                    type="radio"
                    checked={(v === 'public') === isPublic}
                    onChange={() => setIsPublic(v === 'public')}
                    style={{ accentColor: '#2dd4bf' }}
                  />
                  {v.charAt(0).toUpperCase() + v.slice(1)}
                </label>
              ))}
            </div>
          </div>

          {/* Tags / Categories — search bar with bubbles */}
          <div>
            <label style={labelStyle}>Categories</label>
            {/* Selected bubbles */}
            {tags.length > 0 && (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginBottom: 8 }}>
                {tags.map((cat) => (
                  <span key={cat} style={{ display: 'flex', alignItems: 'center', gap: 5, background: '#1a3a2a', border: '1px solid #3fb950', borderRadius: 20, padding: '3px 10px 3px 12px', fontSize: 12, color: '#3fb950' }}>
                    {cat}
                    <button
                      type="button"
                      onClick={() => setTags((prev) => prev.filter((t) => t !== cat))}
                      style={{ background: 'none', border: 'none', cursor: 'pointer', padding: 0, display: 'flex', alignItems: 'center', color: '#3fb950', lineHeight: 1 }}
                    >
                      <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M18 6L6 18M6 6l12 12"/></svg>
                    </button>
                  </span>
                ))}
              </div>
            )}
            {/* Search input */}
            <div style={{ position: 'relative' }}>
              <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="#484f58" strokeWidth="2" style={{ position: 'absolute', left: 12, pointerEvents: 'none' }}>
                  <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
                </svg>
                <input
                  type="text"
                  placeholder={tags.length < SITE_CATEGORIES.length ? 'Search categories…' : 'All categories selected'}
                  value={catQuery}
                  onChange={(e) => { setCatQuery(e.target.value); setCatOpen(true); }}
                  onFocus={() => setCatOpen(true)}
                  onBlur={() => setTimeout(() => setCatOpen(false), 150)}
                  disabled={tags.length >= SITE_CATEGORIES.length}
                  style={{ ...inputStyle, paddingLeft: 34 }}
                />
              </div>
              {catOpen && (
                <div style={{ position: 'absolute', top: '100%', left: 0, right: 0, zIndex: 50, marginTop: 4, background: '#161b22', border: '1px solid #30363d', borderRadius: 8, maxHeight: 200, overflowY: 'auto', boxShadow: '0 8px 24px rgba(0,0,0,0.5)' }}>
                  {SITE_CATEGORIES
                    .filter((c) => c.toLowerCase().includes(catQuery.toLowerCase()) && !tags.includes(c))
                    .map((c) => (
                      <button
                        key={c}
                        type="button"
                        onMouseDown={() => { setTags((prev) => [...prev, c]); setCatQuery(''); }}
                        style={{ display: 'block', width: '100%', background: 'none', border: 'none', padding: '9px 14px', textAlign: 'left', fontSize: 13, color: '#c9d1d9', cursor: 'pointer', fontFamily: 'inherit' }}
                        onMouseEnter={(e) => (e.currentTarget.style.background = '#21262d')}
                        onMouseLeave={(e) => (e.currentTarget.style.background = 'none')}
                      >
                        {c}
                      </button>
                    ))}
                  {SITE_CATEGORIES.filter((c) => c.toLowerCase().includes(catQuery.toLowerCase()) && !tags.includes(c)).length === 0 && (
                    <p style={{ margin: 0, padding: '10px 14px', fontSize: 12, color: '#484f58' }}>No categories found</p>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Items */}
          <div>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 10 }}>
              <label style={{ ...labelStyle, marginBottom: 0 }}>Items (max 3)</label>
              {items.length < 3 && (
                <button onClick={addItemSlot} style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 10px', fontSize: 12, color: '#2dd4bf', cursor: 'pointer', fontFamily: 'inherit' }}>
                  + Add item
                </button>
              )}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {items.map((item, idx) => (
                <div key={item.id} style={{ background: '#21262d', border: `1px solid ${selectedItemId === item.id ? '#2dd4bf' : '#30363d'}`, borderRadius: 8, padding: 12 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    {/* Clickable image icon — triggers file upload */}
                    <label style={{ width: 40, height: 40, borderRadius: 6, overflow: 'hidden', background: '#161b22', flexShrink: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', border: '1px dashed #30363d' }} title="Click to upload image">
                      <input
                        ref={(el) => { fileInputRefs.current[idx] = el; }}
                        type="file" accept="image/*"
                        onChange={(e) => handleImageUpload(e, item.id)}
                        style={{ display: 'none' }}
                      />
                      {item.imageUrl ? (
                        <img src={item.imageUrl} alt="" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                      ) : (
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#484f58" strokeWidth="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                      )}
                    </label>
                    <div style={{ flex: 1 }}>
                      <input
                        type="text"
                        value={item.name}
                        onChange={(e) => updateItemName(item.id, e.target.value)}
                        placeholder={`Item ${idx + 1} name`}
                        onClick={() => setSelectedItemId(item.id)}
                        style={{ ...inputStyle, padding: '6px 10px', fontSize: 12, width: '100%' }}
                      />
                    </div>
                    <button onClick={() => removeItem(item.id)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#f85149', padding: 4 }}>
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M18 6L6 18M6 6l12 12" /></svg>
                    </button>
                  </div>
                </div>
              ))}
              {items.length === 0 && (
                <p style={{ fontSize: 13, color: '#484f58', margin: 0 }}>No items added yet. Click "+ Add item" to begin.</p>
              )}
            </div>
          </div>

          {/* Action buttons */}
          <div style={{ display: 'flex', gap: 10, paddingTop: 8 }}>
            <button onClick={onCancel} style={{ flex: 1, background: 'none', border: '1px solid #30363d', borderRadius: 8, padding: '10px 0', fontSize: 14, color: '#7d8590', cursor: 'pointer', fontFamily: 'inherit' }}>
              Cancel
            </button>
            <button onClick={handleSave} style={{ flex: 2, background: '#2dd4bf', border: 'none', borderRadius: 8, padding: '10px 0', fontSize: 14, fontWeight: 700, color: '#0d1117', cursor: 'pointer', fontFamily: 'inherit' }}>
              {isEdit ? 'Save Changes' : 'Create Showoff'}
            </button>
          </div>
        </div>

        {/* Right: canvas preview */}
        <div style={{ position: 'sticky', top: 80 }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
            <label style={labelStyle}>Preview</label>
            <div style={{ display: 'flex', gap: 8 }}>
              <button
                onClick={() => setOrientation((o) => o === 'landscape' ? 'portrait' : 'landscape')}
                title={`Switch to ${orientation === 'landscape' ? 'portrait' : 'landscape'}`}
                style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 10px', fontSize: 11, color: '#c9d1d9', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 6, fontFamily: 'inherit' }}
              >
                {orientation === 'landscape' ? (
                  <svg width="14" height="10" viewBox="0 0 14 10" fill="none" stroke="currentColor" strokeWidth="1.5"><rect x="0.75" y="0.75" width="12.5" height="8.5" rx="1.5"/></svg>
                ) : (
                  <svg width="10" height="14" viewBox="0 0 10 14" fill="none" stroke="currentColor" strokeWidth="1.5"><rect x="0.75" y="0.75" width="8.5" height="12.5" rx="1.5"/></svg>
                )}
                {orientation === 'landscape' ? 'Landscape' : 'Portrait'}
              </button>
              <button
                onClick={() => setExpanded(true)}
                title="Expand canvas"
                style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 8px', cursor: 'pointer', display: 'flex', alignItems: 'center', color: '#c9d1d9' }}
              >
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7"/></svg>
              </button>
            </div>
          </div>

          {/* Inline (small) canvas — click to expand */}
          <div style={{ display: 'flex', justifyContent: 'center' }}>
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 8 }}>
              <div
                onClick={() => setExpanded(true)}
                style={{
                  width: canvasWidth,
                  height: canvasHeight,
                  background: bgImage ? `url(${bgImage}) center/cover` : bgColor,
                  borderRadius: 10,
                  position: 'relative',
                  overflow: 'hidden',
                  cursor: 'pointer',
                  boxShadow: '0 8px 32px rgba(0,0,0,0.5)',
                  border: '2px solid #30363d',
                  userSelect: 'none',
                }}
              >
                {items.map((item) => (
                  <div
                    key={item.id}
                    style={{ position: 'absolute', left: `${item.x}%`, top: `${item.y}%`, width: `${item.width}%`, height: `${item.height}%`, boxSizing: 'border-box' }}
                  >
                    {item.imageUrl ? (
                      <img src={item.imageUrl} alt={item.name} style={{ width: '100%', height: '100%', objectFit: 'contain', pointerEvents: 'none', display: 'block' }} draggable={false} />
                    ) : (
                      <div style={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(255,255,255,0.05)' }}>
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.3)" strokeWidth="1.5"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                      </div>
                    )}
                  </div>
                ))}
                {/* Expand hint overlay */}
                <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0)', display: 'flex', alignItems: 'center', justifyContent: 'center', transition: 'background 0.15s' }}
                  onMouseEnter={(e) => (e.currentTarget.style.background = 'rgba(0,0,0,0.35)')}
                  onMouseLeave={(e) => (e.currentTarget.style.background = 'rgba(0,0,0,0)')}
                >
                  <div style={{ opacity: 0, transition: 'opacity 0.15s', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6, color: '#fff' }}
                    onMouseEnter={(e) => (e.currentTarget.style.opacity = '1')}
                    onMouseLeave={(e) => (e.currentTarget.style.opacity = '0')}
                  >
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7"/></svg>
                    <span style={{ fontSize: 12, fontWeight: 600 }}>Click to edit</span>
                  </div>
                </div>
              </div>

              {/* Name below canvas */}
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span style={{ fontSize: 14, fontWeight: 600, color: '#e6edf3' }}>{name || 'Showoff Name'}</span>
                <span style={{ fontSize: 11, color: isPublic ? '#3fb950' : '#7d8590', background: isPublic ? '#122016' : '#21262d', padding: '2px 8px', borderRadius: 4, fontWeight: 600 }}>
                  {isPublic ? 'Public' : 'Private'}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Expanded fullscreen canvas overlay */}
      {expanded && (
        <div style={{ position: 'fixed', inset: 0, zIndex: 300, background: 'rgba(1,4,9,0.92)', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 16, padding: 32 }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', maxWidth: 900, flexShrink: 0 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              <span style={{ fontSize: 15, fontWeight: 600, color: '#e6edf3' }}>{name || 'Showoff Name'}</span>
              <button
                onClick={() => setOrientation((o) => o === 'landscape' ? 'portrait' : 'landscape')}
                style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 10px', fontSize: 11, color: '#c9d1d9', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 6, fontFamily: 'inherit' }}
              >
                {orientation === 'landscape' ? (
                  <svg width="13" height="9" viewBox="0 0 14 10" fill="none" stroke="currentColor" strokeWidth="1.5"><rect x="0.75" y="0.75" width="12.5" height="8.5" rx="1.5"/></svg>
                ) : (
                  <svg width="9" height="13" viewBox="0 0 10 14" fill="none" stroke="currentColor" strokeWidth="1.5"><rect x="0.75" y="0.75" width="8.5" height="12.5" rx="1.5"/></svg>
                )}
                {orientation === 'landscape' ? 'Landscape' : 'Portrait'}
              </button>
              {items.length < 3 && (
                <button onClick={addItemSlot} style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 6, padding: '5px 10px', fontSize: 12, color: '#2dd4bf', cursor: 'pointer', fontFamily: 'inherit' }}>
                  + Add item
                </button>
              )}
            </div>
            <button
              onClick={() => { setExpanded(false); setSelectedItemId(null); }}
              style={{ background: '#21262d', border: '1px solid #30363d', borderRadius: 7, padding: '7px 16px', fontSize: 13, color: '#e6edf3', cursor: 'pointer', fontFamily: 'inherit', display: 'flex', alignItems: 'center', gap: 6 }}
            >
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M8 3v3a2 2 0 0 1-2 2H3m18 0h-3a2 2 0 0 1-2-2V3m0 18v-3a2 2 0 0 1 2-2h3M3 16h3a2 2 0 0 1 2 2v3"/></svg>
              Done
            </button>
          </div>

          {/* Large canvas */}
          <div
            ref={canvasRef}
            onPointerMove={onPointerMove}
            onPointerUp={onPointerUp}
            onPointerLeave={onPointerUp}
            onClick={(e) => { if (e.target === e.currentTarget) setSelectedItemId(null); }}
            style={orientation === 'landscape' ? {
              width: 'min(860px, 90vw)',
              aspectRatio: '16/9',
              background: bgImage ? `url(${bgImage}) center/cover` : bgColor,
              borderRadius: 12, position: 'relative', overflow: 'hidden',
              cursor: 'default', boxShadow: '0 16px 64px rgba(0,0,0,0.7)',
              border: '2px solid #30363d', userSelect: 'none', flexShrink: 0,
            } : {
              height: 'min(calc(100vh - 220px), 560px)',
              aspectRatio: '9/16',
              background: bgImage ? `url(${bgImage}) center/cover` : bgColor,
              borderRadius: 12, position: 'relative', overflow: 'hidden',
              cursor: 'default', boxShadow: '0 16px 64px rgba(0,0,0,0.7)',
              border: '2px solid #30363d', userSelect: 'none', flexShrink: 0,
            }}
          >
            {items.map((item) => {
              const isSelected = selectedItemId === item.id;
              return (
                <div
                  key={item.id}
                  style={{
                    position: 'absolute',
                    left: `${item.x}%`,
                    top: `${item.y}%`,
                    width: `${item.width}%`,
                    height: `${item.height}%`,
                    outline: isSelected ? '2px solid #2dd4bf' : '1px dashed rgba(45,212,191,0.3)',
                    outlineOffset: 1,
                    cursor: 'move',
                    boxSizing: 'border-box',
                  }}
                  onPointerDown={(e) => startDrag(e, item.id, 'move')}
                >
                  {item.imageUrl ? (
                    <img src={item.imageUrl} alt={item.name} style={{ width: '100%', height: '100%', objectFit: 'contain', pointerEvents: 'none', display: 'block' }} draggable={false} />
                  ) : (
                    <div style={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(255,255,255,0.05)' }}>
                      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.3)" strokeWidth="1.5"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                    </div>
                  )}
                  {isSelected && (
                    <div
                      style={{ position: 'absolute', bottom: -5, right: -5, width: 14, height: 14, background: '#2dd4bf', borderRadius: 3, cursor: 'nwse-resize' }}
                      onPointerDown={(e) => { e.stopPropagation(); startDrag(e, item.id, 'resize'); }}
                    />
                  )}
                </div>
              );
            })}
          </div>

          {/* Item strip at the bottom */}
          <div style={{ display: 'flex', gap: 10, flexShrink: 0, flexWrap: 'wrap', justifyContent: 'center' }}>
            {items.map((item, idx) => {
              const isSelected = selectedItemId === item.id;
              return (
                <div
                  key={item.id}
                  style={{
                    display: 'flex', alignItems: 'center', gap: 8,
                    background: isSelected ? '#1a3a3a' : '#21262d',
                    border: `1px solid ${isSelected ? '#2dd4bf' : '#30363d'}`,
                    borderRadius: 10, padding: '8px 10px',
                  }}
                >
                  {/* Clickable image icon */}
                  <label style={{ width: 36, height: 36, borderRadius: 6, overflow: 'hidden', background: '#161b22', flexShrink: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', border: '1px dashed #484f58' }} title="Click to upload image">
                    <input type="file" accept="image/*" onChange={(e) => handleImageUpload(e, item.id)} style={{ display: 'none' }} />
                    {item.imageUrl ? (
                      <img src={item.imageUrl} alt="" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                    ) : (
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#484f58" strokeWidth="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                    )}
                  </label>
                  {/* Name input */}
                  <input
                    type="text"
                    value={item.name}
                    onChange={(e) => updateItemName(item.id, e.target.value)}
                    onFocus={() => setSelectedItemId(item.id)}
                    placeholder={`Item ${idx + 1} name`}
                    style={{ background: 'none', border: 'none', outline: 'none', fontSize: 12, color: isSelected ? '#2dd4bf' : '#c9d1d9', fontFamily: 'inherit', width: 110 }}
                  />
                  <button
                    onClick={() => setSelectedItemId(item.id === selectedItemId ? null : item.id)}
                    style={{ background: 'none', border: 'none', cursor: 'pointer', padding: 2, color: isSelected ? '#2dd4bf' : '#484f58' }}
                    title={isSelected ? 'Deselect' : 'Select on canvas'}
                  >
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  </button>
                  <button onClick={() => removeItem(item.id)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#f85149', padding: 2 }}>
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M18 6L6 18M6 6l12 12"/></svg>
                  </button>
                </div>
              );
            })}
            {items.length < 3 && (
              <button onClick={addItemSlot} style={{ display: 'flex', alignItems: 'center', gap: 6, background: '#21262d', border: '1px dashed #30363d', borderRadius: 10, padding: '8px 14px', fontSize: 12, color: '#2dd4bf', cursor: 'pointer', fontFamily: 'inherit' }}>
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
                Add item
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

const labelStyle: React.CSSProperties = {
  display: 'block',
  fontSize: 13,
  fontWeight: 600,
  color: '#c9d1d9',
  marginBottom: 6,
};

const inputStyle: React.CSSProperties = {
  width: '100%',
  background: '#161b22',
  border: '1px solid #30363d',
  borderRadius: 7,
  padding: '9px 12px',
  fontSize: 14,
  color: '#e6edf3',
  outline: 'none',
  fontFamily: 'inherit',
  boxSizing: 'border-box',
};
