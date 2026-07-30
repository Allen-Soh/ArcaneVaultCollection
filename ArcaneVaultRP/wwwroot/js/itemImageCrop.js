// ════════════════════════════════════════════════
// SHARED — Item image upload + crop
// ════════════════════════════════════════════════

const MAX_W = 1200;
const MAX_H = 800;
let itemCropperInstance = null;

// ── Open upload modal ──
function openItemUploadModal() {
    document.getElementById('itemUploadError').style.display = 'none';
    document.getElementById('itemDropZone').style.borderColor = '#dee2e6';
    new bootstrap.Modal(document.getElementById('itemUploadModal')).show();
}

// ── Drag + drop ──
function onItemFileDrop(e) {
    e.preventDefault();
    document.getElementById('itemDropZone').style.borderColor = '#dee2e6';
    const file = e.dataTransfer.files[0];
    if (file) processItemFile(file);
}

// ── File input change ──
function onItemFileSelected(input) {
    const file = input.files[0];
    if (file) processItemFile(file);
    input.value = '';
}

// ── Validate + open crop modal ──
function processItemFile(file) {
    const errEl = document.getElementById('itemUploadError');
    errEl.style.display = 'none';

    if (!['image/jpeg', 'image/png'].includes(file.type)) {
        errEl.textContent = 'Only JPG and PNG files are allowed.';
        errEl.style.display = 'block'; return;
    }
    if (file.size > 5 * 1024 * 1024) {
        errEl.textContent = 'File size must not exceed 5MB.';
        errEl.style.display = 'block'; return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        bootstrap.Modal.getInstance(
            document.getElementById('itemUploadModal'))?.hide();

        const cropImage = document.getElementById('itemCropImage');
        cropImage.src = e.target.result;

        if (itemCropperInstance) {
            itemCropperInstance.destroy();
            itemCropperInstance = null;
        }

        setTimeout(() => {
            const cropModal = new bootstrap.Modal(
                document.getElementById('itemCropModal'));
            cropModal.show();

            document.getElementById('itemCropModal').addEventListener(
                'shown.bs.modal', function () {
                    itemCropperInstance = new Cropper(cropImage, {
                        aspectRatio: NaN,     // free crop
                        viewMode: 1,
                        dragMode: 'move',
                        autoCropArea: 0.85,    // default crop covers 85%
                        cropBoxResizable: true,    // user can resize freely
                        cropBoxMovable: true,
                        guides: true,    // bar indicators (rule of thirds grid)
                        center: true,
                        highlight: true,
                        background: true,
                        movable: true,
                        zoomable: true,
                        zoomOnTouch: true,
                        zoomOnWheel: true,
                        scalable: false,

                        crop(event) {
                            updateCropBars(event.detail.width, event.detail.height);
                        }
                    });
                }, { once: true });
        }, 300);
    };
    reader.readAsDataURL(file);
}

// ── Update W/H bar indicators ──
function updateCropBars(w, h) {
    const clampedW = Math.min(Math.round(w), MAX_W);
    const clampedH = Math.min(Math.round(h), MAX_H);

    document.getElementById('cropWidthBar').style.width =
        Math.round((clampedW / MAX_W) * 100) + '%';
    document.getElementById('cropHeightBar').style.width =
        Math.round((clampedH / MAX_H) * 100) + '%';
    document.getElementById('cropWidthLabel').textContent =
        clampedW + ' / ' + MAX_W + ' px';
    document.getElementById('cropHeightLabel').textContent =
        clampedH + ' / ' + MAX_H + ' px';
}

// ── Cancel crop → reopen upload ──
function cancelItemCrop() {
    if (itemCropperInstance) {
        itemCropperInstance.destroy();
        itemCropperInstance = null;
    }
    bootstrap.Modal.getInstance(document.getElementById('itemCropModal'))?.hide();
    setTimeout(() => openItemUploadModal(), 300);
}

// ── Save crop → upload to Cloudinary ──
async function saveItemCrop() {
    if (!itemCropperInstance) return;

    const saveBtn = document.getElementById('itemCropSaveBtn');
    saveBtn.disabled = true;
    saveBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>Uploading...`;

    // Get cropped data — capped at MAX_W × MAX_H
    const cropData = itemCropperInstance.getData();
    const outW = Math.min(Math.round(cropData.width), MAX_W);
    const outH = Math.min(Math.round(cropData.height), MAX_H);

    const canvas = itemCropperInstance.getCroppedCanvas({
        width: outW,
        height: outH,
        imageSmoothingEnabled: true,
        imageSmoothingQuality: 'high'
    });

    canvas.toBlob(async (blob) => {
        const formData = new FormData();
        formData.append('file', blob, 'item.jpg');

        const apiBase = document.getElementById('apiBaseInput')?.value
            || 'https://localhost:7040/api';

        const uploadRes = await fetch(`${apiBase}/ImageUpload`, {
            method: 'POST', body: formData
        });

        if (!uploadRes.ok) {
            saveBtn.disabled = false;
            saveBtn.innerHTML = 'Use this photo';
            alert('Upload failed. Please try again.'); return;
        }

        const data = await uploadRes.json();
        const imageUrl = data.imagePath || data.ImagePath;
        const imagePublicId = data.imagePublicId || data.ImagePublicId;

        // Delete old image if replacing
        const oldPublicId = document.getElementById('itemImagePublicId')?.value;
        if (oldPublicId && oldPublicId !== imagePublicId) {
            await fetch(`${apiBase}/ImageUpload`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(oldPublicId)
            });
        }

        // Set hidden fields
        document.getElementById('itemImageUrl').value = imageUrl;
        document.getElementById('itemImagePublicId').value = imagePublicId;

        // Show preview
        const preview = document.getElementById('itemImagePreview');
        const placeholder = document.getElementById('itemImagePlaceholder');
        const removeBtn = document.getElementById('removeImageBtn');
        const container = document.getElementById('itemImageContainer');

        if (preview) { preview.src = imageUrl; preview.style.display = 'block'; }
        if (placeholder) { placeholder.style.display = 'none'; }
        if (removeBtn) { removeBtn.style.display = 'flex'; }
        if (container) { container.style.border = 'none'; }
        const dedDeleteBtn = document.getElementById('btnDeleteImage');
        if (dedDeleteBtn) dedDeleteBtn.style.display = 'flex';

        // Destroy cropper + close modal
        itemCropperInstance.destroy();
        itemCropperInstance = null;
        bootstrap.Modal.getInstance(document.getElementById('itemCropModal'))?.hide();

        saveBtn.disabled = false;
        saveBtn.innerHTML = 'Use this photo';
    }, 'image/jpeg', 0.92);
}

// ── Remove image ──
function removeItemImage(e) {
    e.stopPropagation();

    const preview = document.getElementById('itemImagePreview');
    const placeholder = document.getElementById('itemImagePlaceholder');
    const removeBtn = document.getElementById('removeImageBtn');
    const container = document.getElementById('itemImageContainer');
    const urlField = document.getElementById('itemImageUrl');
    const pidField = document.getElementById('itemImagePublicId');

    if (preview) { preview.src = ''; preview.style.display = 'none'; }
    if (placeholder) { placeholder.style.display = 'block'; }
    if (removeBtn) { removeBtn.style.display = 'none'; }
    if (container) { container.style.border = '2px dashed var(--border-color)'; }
    const dedDeleteBtn = document.getElementById('btnDeleteImage');
    if (dedDeleteBtn) dedDeleteBtn.style.display = 'none';

    // Mark for deletion — keep publicId in a separate field so server can delete
    if (pidField?.value) {
        const deleteField = document.getElementById('itemDeletePublicId');
        if (deleteField) deleteField.value = pidField.value;
    }

    if (urlField) urlField.value = '';
    if (pidField) pidField.value = '';
}