// Shared showoff canvas renderer — used by the ViewShowoff modal and the Create/Edit
// editing canvases (both the main canvas and the fullscreen preview). Not used by the
// Feed grid card, which is server-rendered C# (see _ShowoffGrid.cshtml's own comment
// near its sticker loop for why that one duplication is unavoidable).
window.ShowoffCanvas = (function () {

    // slots: Array<{ imagePath, itemName } | null> — null entries render as a "+" placeholder
    // (used by Create/Edit's fixed-length builder slots; ViewShowoff never passes null).
    function renderItemCells(slots, orientation) {
        const dividerStyle = orientation === 'Portrait'
            ? 'border-bottom: 1px dashed rgba(166,177,225,0.15);'
            : 'border-right: 1px dashed rgba(166,177,225,0.15);';

        return slots.map((item, idx) => {
            const isLast = idx === slots.length - 1;
            const border = isLast ? '' : dividerStyle;

            if (!item) {
                return `
                    <div class="d-flex align-items-center justify-content-center" style="flex:1; ${border}">
                        <i class="bi bi-plus-circle text-muted fs-5 opacity-50"></i>
                    </div>`;
            }

            return `
                <div class="d-flex align-items-center justify-content-center p-2" style="flex:1; overflow:hidden; ${border}">
                    ${item.imagePath
                        ? `<img src="${item.imagePath}" style="max-width:100%; max-height:100%; object-fit:contain; border-radius:6px; filter: drop-shadow(0 4px 8px rgba(0,0,0,0.3));" />`
                        : `<span class="small text-muted text-center px-1" style="font-size:11px; word-break:break-all;">${item.itemName}</span>`}
                </div>`;
        }).join('');
    }

    function applyBackground(el, { backgroundColor, backgroundImagePath }) {
        if (backgroundImagePath) {
            el.style.backgroundImage = `url('${backgroundImagePath}')`;
            el.style.backgroundSize = 'cover';
            el.style.backgroundPosition = 'center';
            el.style.backgroundColor = '';
        } else {
            el.style.backgroundImage = '';
            el.style.backgroundColor = backgroundColor;
        }
    }

    // stickers: Array<{ x, y, scale, rotation, imagePath }> — caller normalizes field
    // casing (camelCase vs PascalCase) before calling in, since this function doesn't guess.
    // opts.editable:false renders plain non-interactive divs (ViewShowoff).
    // opts.editable:true adds pointer handlers + selection handles (Create/Edit), and
    // requires ShowoffCanvas.activeController to be set (see createStickerController below).
    function renderOverlay(frameImagePath, stickers, opts) {
        opts = opts || {};
        const editable = !!opts.editable;
        const selectedIdx = opts.selectedIdx;

        let html = '';
        if (frameImagePath) {
            html += `<img src="${frameImagePath}" style="position:absolute;inset:0;width:100%;height:100%;object-fit:fill;pointer-events:none;z-index:2;" />`;
        }

        (stickers || []).forEach((s, idx) => {
            const scale = s.scale ?? 1;
            const rotation = s.rotation ?? 0;

            if (!editable) {
                html += `
                    <div style="position:absolute; left:${s.x}%; top:${s.y}%; width:${15 * scale}%; transform: translate(-50%,-50%) rotate(${rotation}deg); z-index:20; pointer-events:none;">
                        <img src="${s.imagePath}" style="width:100%; display:block;" />
                    </div>`;
                return;
            }

            const selected = selectedIdx === idx;
            html += `
                <div class="showoff-sticker ${selected ? 'selected' : ''}" data-idx="${idx}"
                     style="position:absolute; left:${s.x}%; top:${s.y}%; width:${15 * scale}%; transform: translate(-50%,-50%) rotate(${rotation}deg); z-index:20;"
                     onpointerdown="ShowoffCanvas.activeController.onStickerPointerDown(event, ${idx})">
                    <img src="${s.imagePath}" style="width:100%; display:block; pointer-events:none;" draggable="false" />
                    ${selected ? `
                        <div class="sticker-handle sticker-delete" onpointerdown="event.stopPropagation()" onclick="ShowoffCanvas.activeController.removeSticker(${idx})" title="Delete">&times;</div>
                        <div class="sticker-handle sticker-rotate" onpointerdown="ShowoffCanvas.activeController.onRotateHandleDown(event, ${idx})" title="Rotate"><i class="bi bi-arrow-clockwise"></i></div>
                        <div class="sticker-handle sticker-resize" onpointerdown="ShowoffCanvas.activeController.onResizeHandleDown(event, ${idx})" title="Resize"></div>
                    ` : ''}
                </div>`;
        });

        return html;
    }

    // data: { items, orientation, backgroundColor, backgroundImagePath, frameImagePath,
    //         stickers, editable, selectedIdx }
    function renderCanvas(el, data) {
        applyBackground(el, data);
        el.innerHTML = renderItemCells(data.items, data.orientation)
                      + renderOverlay(data.frameImagePath, data.stickers, { editable: data.editable, selectedIdx: data.selectedIdx });
    }

    // store: { getStickers, getSelectedIdx, setSelectedIdx, rerender } — implemented by the
    // host page against its own placedStickers/selectedStickerIdx variables. Returns the
    // handler set that renderOverlay's generated HTML calls back into via
    // ShowoffCanvas.activeController — set that once per page (Create/Edit each only ever
    // need one controller instance for their whole lifetime).
    function createStickerController(store) {
        let dragState = null;

        function onStickerPointerDown(event, idx) {
            event.stopPropagation();
            store.setSelectedIdx(idx);
            const canvasEl = event.currentTarget.closest('.canvas-box');
            const rect = canvasEl.getBoundingClientRect();
            const stickers = store.getStickers();
            dragState = {
                type: 'move', idx,
                startPointerX: event.clientX, startPointerY: event.clientY,
                startX: stickers[idx].x, startY: stickers[idx].y,
                canvasRect: rect
            };
            document.addEventListener('pointermove', onDragPointerMove);
            document.addEventListener('pointerup', onDragPointerUp);
            store.rerender();
        }

        function onResizeHandleDown(event, idx) {
            event.stopPropagation();
            const canvasEl = event.currentTarget.closest('.canvas-box');
            const rect = canvasEl.getBoundingClientRect();
            const s = store.getStickers()[idx];
            const centerX = rect.left + (s.x / 100) * rect.width;
            const centerY = rect.top + (s.y / 100) * rect.height;
            dragState = {
                type: 'resize', idx, centerX, centerY,
                startDist: Math.max(1, Math.hypot(event.clientX - centerX, event.clientY - centerY)),
                startScale: s.scale
            };
            document.addEventListener('pointermove', onDragPointerMove);
            document.addEventListener('pointerup', onDragPointerUp);
        }

        function onRotateHandleDown(event, idx) {
            event.stopPropagation();
            const canvasEl = event.currentTarget.closest('.canvas-box');
            const rect = canvasEl.getBoundingClientRect();
            const s = store.getStickers()[idx];
            const centerX = rect.left + (s.x / 100) * rect.width;
            const centerY = rect.top + (s.y / 100) * rect.height;
            dragState = {
                type: 'rotate', idx, centerX, centerY,
                startAngle: Math.atan2(event.clientY - centerY, event.clientX - centerX) * 180 / Math.PI,
                startRotation: s.rotation
            };
            document.addEventListener('pointermove', onDragPointerMove);
            document.addEventListener('pointerup', onDragPointerUp);
        }

        function onDragPointerMove(event) {
            if (!dragState) return;
            const s = store.getStickers()[dragState.idx];
            if (!s) return;

            if (dragState.type === 'move') {
                const dxPct = ((event.clientX - dragState.startPointerX) / dragState.canvasRect.width) * 100;
                const dyPct = ((event.clientY - dragState.startPointerY) / dragState.canvasRect.height) * 100;
                s.x = Math.min(100, Math.max(0, dragState.startX + dxPct));
                s.y = Math.min(100, Math.max(0, dragState.startY + dyPct));
            } else if (dragState.type === 'resize') {
                const dist = Math.hypot(event.clientX - dragState.centerX, event.clientY - dragState.centerY);
                s.scale = Math.min(3, Math.max(0.3, dragState.startScale * (dist / dragState.startDist)));
            } else if (dragState.type === 'rotate') {
                const angle = Math.atan2(event.clientY - dragState.centerY, event.clientX - dragState.centerX) * 180 / Math.PI;
                s.rotation = dragState.startRotation + (angle - dragState.startAngle);
            }

            document.querySelectorAll(`.showoff-sticker[data-idx="${dragState.idx}"]`).forEach(el => {
                el.style.left = s.x + '%';
                el.style.top = s.y + '%';
                el.style.width = (15 * s.scale) + '%';
                el.style.transform = `translate(-50%,-50%) rotate(${s.rotation}deg)`;
            });
        }

        function onDragPointerUp() {
            dragState = null;
            document.removeEventListener('pointermove', onDragPointerMove);
            document.removeEventListener('pointerup', onDragPointerUp);
        }

        function removeSticker(idx) {
            store.getStickers().splice(idx, 1);
            store.setSelectedIdx(null);
            store.rerender();
        }

        // Deselect when clicking empty canvas (either the main canvas or fullscreen preview,
        // both carry the canvas-box class).
        document.addEventListener('pointerdown', (e) => {
            if (e.target.closest('.canvas-box') && !e.target.closest('.showoff-sticker')) {
                if (store.getSelectedIdx() !== null) {
                    store.setSelectedIdx(null);
                    store.rerender();
                }
            }
        });

        return { onStickerPointerDown, onResizeHandleDown, onRotateHandleDown, removeSticker };
    }

    return { renderItemCells, applyBackground, renderOverlay, renderCanvas, createStickerController, activeController: null };
})();
