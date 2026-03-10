# Changelog

## [1.0.0-rc.2] - 2026-03-10

### Added
- Added Management API write support for organizations and API keys.
- Added `IApiKeyClient` for API key lifecycle operations.
- Added `IFiskalyCredentialScopeFactory` for scoped per-organization credentials.
- Added typed `ApiKeyId` and typed query parameters for API key listing.

### Changed
- Moved `Env` to a neutral common namespace for reuse across SIGN DE and Management API layers.
- Aligned package metadata, documentation, and release notes with the expanded Management API surface.
- Simplified integration-test configuration so `FiskalyBase` can fall back to `Fiskaly`.

### Fixed
- Restored typed `ManagedByOrganizationId` handling with empty-string-to-null deserialization.
- Ensured API key clients use their own client configuration and resilience profile.
- Added a unit-test gate to the NuGet publish workflow.
