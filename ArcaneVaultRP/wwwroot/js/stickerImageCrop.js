// ════════════════════════════════════════════════
// SHARED — Sticker image upload + resize (Create/Edit Sticker)
// Output is capped at 512×512 and exported as PNG (not JPEG) so
// transparent backgrounds survive.
// ════════════════════════════════════════════════

const STICKER_MAX = 512;
let stickerCropperInstance = null;

function openStickerUploadModal() {
    document.getElementById('stickerUploadError').style.display = 'none';
    new bootstrap.Modal(document.getElementById('stickerUploadModal')).show();
}

function onStickerFileDrop(e) {
    e.preventDefault();
    document.getElementById('stickerDropZone').style.borderColor = '#dee2e6';
    const file = e.dataTransfer.files[0];
    if (file) processStickerFile(file);
}

function onStickerFileSelected(input) {
    const file = input.files[0];
    if (file) processStickerFile(file);
    input.value = '';
}

async function processStickerFile(file) {
    const errEl = document.getElementById('stickerUploadError');
    errEl.style.display = 'none';

    if (!['image/jpeg', 'image/png', 'image/gif'].includes(file.type)) {
        errEl.textContent = 'Only JPG, PNG, and GIF files are allowed.';
        errEl.style.display = 'block'; return;
    }
    if (file.size > 5 * 1024 * 1024) {
        errEl.textContent = 'File size must not exceed 5MB.';
        errEl.style.display = 'block'; return;
    }

    // Animated GIFs can't go through the canvas-based resize step without
    // flattening to a single static frame — upload directly instead.
    if (file.type === 'image/gif') {
        await uploadStickerFileDirect(file);
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        bootstrap.Modal.getInstance(document.getElementById('stickerUploadModal'))?.hide();

        const cropImage = document.getElementById('stickerCropImage');
        cropImage.src = e.target.result;

        if (stickerCropperInstance) {
            stickerCropperInstance.destroy();
            stickerCropperInstance = null;
        }

        setTimeout(() => {
            const cropModal = new bootstrap.Modal(document.getElementById('stickerCropModal'));
            cropModal.show();

            document.getElementById('stickerCropModal').addEventListener('shown.bs.modal', function () {
                stickerCropperInstance = new Cropper(cropImage, {
                    aspectRatio: NaN,
                    viewMode: 1,
                    dragMode: 'move',
                    autoCropArea: 0.85,
                    cropBoxResizable: true,
                    cropBoxMovable: true,
                    guides: true,
                    center: true,
                    highlight: true,
                    background: true,
                    movable: true,
                    zoomable: true,
                    zoomOnTouch: true,
                    zoomOnWheel: true,
                    scalable: false,

                    crop(event) {
                        updateStickerCropBars(event.detail.width, event.detail.height);
                    }
                });
            }, { once: true });
        }, 300);
    };
    reader.readAsDataURL(file);
}

function updateStickerCropBars(w, h) {
    const clampedW = Math.min(Math.round(w), STICKER_MAX);
    const clampedH = Math.min(Math.round(h), STICKER_MAX);

    document.getElementById('stickerCropWidthBar').style.width = Math.round((clampedW / STICKER_MAX) * 100) + '%';
    document.getElementById('stickerCropHeightBar').style.width = Math.round((clampedH / STICKER_MAX) * 100) + '%';
    document.getElementById('stickerCropWidthLabel').textContent = clampedW + ' / ' + STICKER_MAX + ' px';
    document.getElementById('stickerCropHeightLabel').textContent = clampedH + ' / ' + STICKER_MAX + ' px';
}

function cancelStickerCrop() {
    if (stickerCropperInstance) {
        stickerCropperInstance.destroy();
        stickerCropperInstance = null;
    }
    bootstrap.Modal.getInstance(document.getElementById('stickerCropModal'))?.hide();
    setTimeout(() => openStickerUploadModal(), 300);
}

async function saveStickerCrop() {
    if (!stickerCropperInstance) return;

    const saveBtn = document.getElementById('stickerCropSaveBtn');
    saveBtn.disabled = true;
    saveBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>Uploading...`;

    const cropData = stickerCropperInstance.getData();
    const outW = Math.min(Math.round(cropData.width), STICKER_MAX);
    const outH = Math.min(Math.round(cropData.height), STICKER_MAX);

    const canvas = stickerCropperInstance.getCroppedCanvas({
        width: outW,
        height: outH,
        imageSmoothingEnabled: true,
        imageSmoothingQuality: 'high'
    });

    canvas.toBlob(async (blob) => {
        const formData = new FormData();
        formData.append('file', blob, 'sticker.png');

        const apiBase = document.getElementById('apiBaseInput')?.value || 'https://localhost:7040/api';
        const uploadRes = await fetch(`${apiBase}/ImageUpload`, { method: 'POST', body: formData });

        if (!uploadRes.ok) {
            saveBtn.disabled = false;
            saveBtn.innerHTML = 'Use this image';
            alert('Upload failed. Please try again.'); return;
        }

        const data = await uploadRes.json();
        const imageUrl = data.imagePath || data.ImagePath;
        const imagePublicId = data.imagePublicId || data.ImagePublicId;

        await deleteOldStickerImageIfReplacing(imagePublicId);
        applyStickerImage(imageUrl, imagePublicId);

        stickerCropperInstance.destroy();
        stickerCropperInstance = null;
        bootstrap.Modal.getInstance(document.getElementById('stickerCropModal'))?.hide();

        saveBtn.disabled = false;
        saveBtn.innerHTML = 'Use this image';
    }, 'image/png');
}

async function uploadStickerFileDirect(file) {
    const apiBase = document.getElementById('apiBaseInput')?.value || 'https://localhost:7040/api';
    const formData = new FormData();
    formData.append('file', file);

    const uploadRes = await fetch(`${apiBase}/ImageUpload`, { method: 'POST', body: formData });
    if (!uploadRes.ok) { alert('Image upload failed.'); return; }

    const data = await uploadRes.json();
    const imageUrl = data.imagePath || data.ImagePath;
    const imagePublicId = data.imagePublicId || data.ImagePublicId;

    await deleteOldStickerImageIfReplacing(imagePublicId);
    applyStickerImage(imageUrl, imagePublicId);
    bootstrap.Modal.getInstance(document.getElementById('stickerUploadModal'))?.hide();
}

async function deleteOldStickerImageIfReplacing(newPublicId) {
    const apiBase = document.getElementById('apiBaseInput')?.value || 'https://localhost:7040/api';
    const oldPublicId = document.getElementById('stickerImagePublicId')?.value;
    if (oldPublicId && oldPublicId !== newPublicId) {
        await fetch(`${apiBase}/ImageUpload`, {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(oldPublicId)
        });
    }
}

function applyStickerImage(imageUrl, imagePublicId) {
    document.getElementById('stickerImageUrl').value = imageUrl;
    document.getElementById('stickerImagePublicId').value = imagePublicId;

    const preview = document.getElementById('stickerImagePreview');
    const placeholder = document.getElementById('stickerImagePlaceholder');
    const removeBtn = document.getElementById('stickerRemoveImageBtn');

    if (preview) { preview.src = imageUrl; preview.style.display = 'block'; }
    if (placeholder) { placeholder.style.display = 'none'; }
    if (removeBtn) { removeBtn.style.display = 'flex'; }
}

function removeStickerImage(e) {
    e.stopPropagation();
    document.getElementById('stickerImageUrl').value = '';
    document.getElementById('stickerImagePublicId').value = '';

    const preview = document.getElementById('stickerImagePreview');
    const placeholder = document.getElementById('stickerImagePlaceholder');
    const removeBtn = document.getElementById('stickerRemoveImageBtn');

    if (preview) { preview.src = ''; preview.style.display = 'none'; }
    if (placeholder) { placeholder.style.display = 'block'; }
    if (removeBtn) { removeBtn.style.display = 'none'; }
}
