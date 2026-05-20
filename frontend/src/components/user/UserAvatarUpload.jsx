import { useEffect, useRef, useState } from "react";
import { useAuthContext } from "../../contexts/AuthContext.jsx";
import { uploadMyAvatar } from "../../services/userService.js";
import "./UserAvatarUpload.css";

const MAX_AVATAR_SIZE = 20 * 1024 * 1024;
const ACCEPTED_AVATAR_TYPES = ["image/jpeg", "image/png", "image/webp"];
const CROP_BOX_SIZE = 300;
const OUTPUT_SIZE = 512;
const MIN_ZOOM = 1;
const MAX_ZOOM = 3;

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  if (url.startsWith("/")) return `${origin}${url}`;
  return `${origin}/${url}`;
}

function getInitial(name, email) {
  const source = name || email || "U";
  return source.trim().charAt(0).toUpperCase();
}

function loadImage(src) {
  return new Promise((resolve, reject) => {
    const image = new Image();
    image.onload = () => resolve(image);
    image.onerror = reject;
    image.src = src;
  });
}

function normalizeRotation(rotation) {
  return ((rotation % 360) + 360) % 360;
}

function getCoverDisplaySize(naturalSize, rotation) {
  const isQuarterTurn = normalizeRotation(rotation) % 180 !== 0;
  const effectiveWidth = isQuarterTurn ? naturalSize.height : naturalSize.width;
  const effectiveHeight = isQuarterTurn ? naturalSize.width : naturalSize.height;
  const baseScale = Math.max(
    CROP_BOX_SIZE / effectiveWidth,
    CROP_BOX_SIZE / effectiveHeight,
  );

  return {
    width: naturalSize.width * baseScale,
    height: naturalSize.height * baseScale,
    rotatedWidth: effectiveWidth * baseScale,
    rotatedHeight: effectiveHeight * baseScale,
    baseScale,
  };
}

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}

function clampCrop(nextCrop, naturalSize) {
  const rotation = normalizeRotation(nextCrop.rotation || 0);
  const displaySize = getCoverDisplaySize(naturalSize, rotation);
  const zoom = clamp(nextCrop.scale, MIN_ZOOM, MAX_ZOOM);
  const maxX = Math.max(0, (displaySize.rotatedWidth * zoom - CROP_BOX_SIZE) / 2);
  const maxY = Math.max(0, (displaySize.rotatedHeight * zoom - CROP_BOX_SIZE) / 2);

  return {
    x: clamp(nextCrop.x, -maxX, maxX),
    y: clamp(nextCrop.y, -maxY, maxY),
    scale: zoom,
    rotation,
  };
}

async function createCroppedAvatarFile(imageUrl, crop, naturalSize) {
  const image = await loadImage(imageUrl);
  const canvas = document.createElement("canvas");
  canvas.width = OUTPUT_SIZE;
  canvas.height = OUTPUT_SIZE;

  const ctx = canvas.getContext("2d");
  const displaySize = getCoverDisplaySize(naturalSize, crop.rotation);
  const outputRatio = OUTPUT_SIZE / CROP_BOX_SIZE;
  const finalScale = displaySize.baseScale * crop.scale * outputRatio;

  ctx.fillStyle = "#ffffff";
  ctx.fillRect(0, 0, OUTPUT_SIZE, OUTPUT_SIZE);
  ctx.translate(
    OUTPUT_SIZE / 2 + crop.x * outputRatio,
    OUTPUT_SIZE / 2 + crop.y * outputRatio,
  );
  ctx.rotate((crop.rotation * Math.PI) / 180);
  ctx.scale(finalScale, finalScale);
  ctx.drawImage(image, -image.naturalWidth / 2, -image.naturalHeight / 2);

  const blob = await new Promise((resolve) =>
    canvas.toBlob(resolve, "image/jpeg", 0.92),
  );

  if (!blob) {
    throw new Error("Không thể xử lý ảnh đại diện.");
  }

  return new File([blob], "avatar.jpg", { type: "image/jpeg" });
}

function UserAvatarUpload({ userProfile, onUploaded }) {
  const { initAuth } = useAuthContext();
  const inputRef = useRef(null);
  const dragRef = useRef(null);
  const [previewUrl, setPreviewUrl] = useState("");
  const [cropImageUrl, setCropImageUrl] = useState("");
  const [cropNaturalSize, setCropNaturalSize] = useState({
    width: CROP_BOX_SIZE,
    height: CROP_BOX_SIZE,
  });
  const [crop, setCrop] = useState({
    x: 0,
    y: 0,
    scale: 1,
    rotation: 0,
  });
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    return () => {
      if (previewUrl) URL.revokeObjectURL(previewUrl);
      if (cropImageUrl) URL.revokeObjectURL(cropImageUrl);
    };
  }, [previewUrl, cropImageUrl]);

  const avatarSrc =
    previewUrl || toAbsoluteMediaUrl(userProfile?.avatarUrl || "");
  const initial = getInitial(userProfile?.fullName, userProfile?.email);
  const displaySize = getCoverDisplaySize(cropNaturalSize, crop.rotation);

  const setClampedCrop = (updater) => {
    setCrop((prev) => {
      const next = typeof updater === "function" ? updater(prev) : updater;
      return clampCrop(next, cropNaturalSize);
    });
  };

  const resetCropToFit = () => {
    setCrop({ x: 0, y: 0, scale: 1, rotation: 0 });
  };

  const handlePickFile = () => {
    if (!isUploading) inputRef.current?.click();
  };

  const handleFileSelected = async (event) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;

    if (!ACCEPTED_AVATAR_TYPES.includes(file.type)) {
      setError("Chỉ hỗ trợ ảnh JPG, PNG hoặc WEBP.");
      return;
    }

    if (file.size > MAX_AVATAR_SIZE) {
      setError("Dung lượng ảnh tối đa là 20MB.");
      return;
    }

    const localCropUrl = URL.createObjectURL(file);
    try {
      const image = await loadImage(localCropUrl);
      if (cropImageUrl) URL.revokeObjectURL(cropImageUrl);
      setCropImageUrl(localCropUrl);
      setCropNaturalSize({
        width: image.naturalWidth,
        height: image.naturalHeight,
      });
      setCrop({ x: 0, y: 0, scale: 1, rotation: 0 });
      setError("");
    } catch {
      URL.revokeObjectURL(localCropUrl);
      setError("Không thể đọc ảnh đã chọn.");
    }
  };

  const closeCropModal = () => {
    if (cropImageUrl) URL.revokeObjectURL(cropImageUrl);
    setCropImageUrl("");
    setCropNaturalSize({ width: CROP_BOX_SIZE, height: CROP_BOX_SIZE });
    setCrop({ x: 0, y: 0, scale: 1, rotation: 0 });
    dragRef.current = null;
  };

  const handleDragStart = (event) => {
    event.currentTarget.setPointerCapture?.(event.pointerId);
    dragRef.current = {
      pointerId: event.pointerId,
      startX: event.clientX,
      startY: event.clientY,
      cropX: crop.x,
      cropY: crop.y,
    };
  };

  const handleDragMove = (event) => {
    const drag = dragRef.current;
    if (!drag || drag.pointerId !== event.pointerId) return;
    setClampedCrop((prev) => ({
      ...prev,
      x: drag.cropX + event.clientX - drag.startX,
      y: drag.cropY + event.clientY - drag.startY,
    }));
  };

  const handleDragEnd = (event) => {
    if (dragRef.current?.pointerId === event.pointerId) {
      dragRef.current = null;
    }
  };

  const handleZoomChange = (event) => {
    const nextScale = Number(event.target.value);
    setClampedCrop((prev) => ({ ...prev, scale: nextScale }));
  };

  const handleRotate = () => {
    setClampedCrop((prev) => ({
      ...prev,
      rotation: normalizeRotation(prev.rotation + 90),
    }));
  };

  const handleConfirmCrop = async () => {
    if (!cropImageUrl || isUploading) return;

    setIsUploading(true);
    try {
      const croppedFile = await createCroppedAvatarFile(
        cropImageUrl,
        crop,
        cropNaturalSize,
      );
      const localPreview = URL.createObjectURL(croppedFile);
      if (previewUrl) URL.revokeObjectURL(previewUrl);
      setPreviewUrl(localPreview);

      const updatedProfile = await uploadMyAvatar(croppedFile);
      onUploaded?.(updatedProfile);
      await initAuth();
      setPreviewUrl("");
      closeCropModal();
    } catch (err) {
      setError(err.message || "Không thể tải ảnh đại diện.");
      setPreviewUrl("");
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <>
      <section className="user-avatar-upload" aria-label="Ảnh đại diện">
        <div className="user-avatar-upload__preview">
          {avatarSrc ? (
            <img src={avatarSrc} alt="Ảnh đại diện người dùng" />
          ) : (
            <span>{initial}</span>
          )}
        </div>

        <div className="user-avatar-upload__content">
          <div>
            <p className="user-avatar-upload__label">Ảnh đại diện</p>
            <p className="user-avatar-upload__hint">
              JPG, PNG hoặc WEBP. Kích thước tối đa 20MB.
            </p>
          </div>

          <div className="user-avatar-upload__actions">
            <button
              type="button"
              className="user-avatar-upload__button"
              onClick={handlePickFile}
              disabled={isUploading}
            >
              <svg
                width="17"
                height="17"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z" />
                <circle cx="12" cy="13" r="4" />
              </svg>
              {isUploading ? "Đang tải..." : "Tải ảnh lên"}
            </button>
            <input
              ref={inputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleFileSelected}
              className="user-avatar-upload__input"
            />
          </div>

          {error ? <p className="user-avatar-upload__error">{error}</p> : null}
        </div>
      </section>

      {cropImageUrl ? (
        <div className="avatar-crop-modal" role="dialog" aria-modal="true">
          <div className="avatar-crop-modal__panel">
            <div className="avatar-crop-modal__header">
              <h3>Căn chỉnh ảnh đại diện</h3>
              <button
                type="button"
                className="avatar-crop-modal__close"
                onClick={closeCropModal}
                aria-label="Đóng"
                disabled={isUploading}
              >
                x
              </button>
            </div>

            <div
              className="avatar-crop-modal__stage"
              onPointerMove={handleDragMove}
              onPointerUp={handleDragEnd}
              onPointerCancel={handleDragEnd}
            >
              <img
                src={cropImageUrl}
                alt="Ảnh đang căn chỉnh"
                className="avatar-crop-modal__image"
                draggable="false"
                onPointerDown={handleDragStart}
                style={{
                  width: `${displaySize.width}px`,
                  height: `${displaySize.height}px`,
                  transform: `translate(-50%, -50%) translate(${crop.x}px, ${crop.y}px) rotate(${crop.rotation}deg) scale(${crop.scale})`,
                }}
              />
              <div className="avatar-crop-modal__circle-frame" />
            </div>

            <label className="avatar-crop-modal__zoom">
              <span>Thu phóng</span>
              <input
                type="range"
                min={MIN_ZOOM}
                max={MAX_ZOOM}
                step="0.01"
                value={crop.scale}
                onChange={handleZoomChange}
                disabled={isUploading}
              />
            </label>

            <div className="avatar-crop-modal__tools">
              <button
                type="button"
                className="avatar-crop-modal__tool"
                onClick={resetCropToFit}
                disabled={isUploading}
              >
                Khớp ảnh
              </button>
              <button
                type="button"
                className="avatar-crop-modal__tool"
                onClick={handleRotate}
                disabled={isUploading}
              >
                Xoay 90 độ
              </button>
            </div>

            <div className="avatar-crop-modal__actions">
              <button
                type="button"
                className="avatar-crop-modal__button avatar-crop-modal__button--ghost"
                onClick={closeCropModal}
                disabled={isUploading}
              >
                Hủy
              </button>
              <button
                type="button"
                className="avatar-crop-modal__button avatar-crop-modal__button--primary"
                onClick={handleConfirmCrop}
                disabled={isUploading}
              >
                {isUploading ? "Đang lưu..." : "Cắt và Lưu"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </>
  );
}

export default UserAvatarUpload;
