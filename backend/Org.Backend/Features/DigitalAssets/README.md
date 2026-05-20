# DigitalAssets Module

## Status
**DB_FOUNDATION_ONLY** - No working endpoint in base prototype

## Domain Entity Purpose
`DigitalAsset` represents files and assets uploaded for events (images, documents, videos, etc.).

## Why It Exists in Database Foundation
DigitalAsset is included in DB v1 to preserve the domain integrity of event file management. The entity and relationships are established now to support future file upload and asset management features.

## Why No Working Endpoint is Required in Base Prototype
The base prototype focuses on core event planning without file upload functionality. File upload, storage, and asset management require additional infrastructure (cloud storage, CDN, etc.) and are deferred to future phases.

## Possible Future Endpoints
- `GET /api/events/{eventId}/assets` - List event assets
- `POST /api/events/{eventId}/assets` - Upload asset
- `GET /api/assets/{id}` - Get asset details
- `DELETE /api/assets/{id}` - Delete asset

## Future Features
- File upload API (multipart/form-data)
- Cloud storage integration (AWS S3, Azure Blob, etc.)
- Image thumbnail generation
- File type validation
- Asset gallery UI

## EXPLICIT WARNING
**DO NOT IMPLEMENT NOW**

DigitalAsset is DB foundation only. No upload API, storage integration, or UI should be created in Phase 3C. Future implementation will require infrastructure planning and cloud storage setup.

## Related Domain Entities
- `DigitalAsset` (Domain/Entities/DigitalAsset.cs)
- `Event`, `User`
- Enums: `FileType`

## Cross-layer Notes
- No shared contract in Phase 3C
- No frontend service in Phase 3C
- No frontend adapter in Phase 3C
- No frontend component in Phase 3C
- Status: **DB_FOUNDATION_ONLY**
