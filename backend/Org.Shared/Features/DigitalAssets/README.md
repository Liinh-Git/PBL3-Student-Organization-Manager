# DigitalAssets Module Contracts

## Module Purpose
Event file/asset management (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `DigitalAsset`

## Why Entity Exists in DB Foundation
DigitalAsset represents files/assets uploaded for events. The entity exists in DB v1 to preserve the file/asset domain model, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). File upload and asset management is a secondary feature that can be added in later phases.

## Possible Future DTOs
- `DigitalAssetDto`
- `UploadDigitalAssetRequest`
- `UpdateDigitalAssetRequest`

## Possible Future Endpoints
- `GET /api/events/{eventId}/assets` - List event assets
- `POST /api/events/{eventId}/assets` - Upload asset
- `DELETE /api/assets/{id}` - Delete asset

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. No upload API in base prototype.

---

**End of DigitalAssets README.md**
