# Changelog

## [1.0.0-rc.3] - 2026-04-11

### Added
- Added `FiskalyCredentialsNotConfiguredException` for calls that execute without default or scoped credentials.

### Changed
- Default `Fiskaly:ApiKey` and `Fiskaly:ApiSecret` are now optional as a pair, which allows startup with runtime-scoped credentials only.
- Updated package metadata, package README, and repository README for the runtime-scoped credential flow.

### Fixed
- Validation now rejects only partially configured default credentials instead of requiring a global key pair at startup.
- Authentication now fails at execution time when no default or scoped credentials are available, instead of failing during service registration.
- NuGet publish validation now includes the deterministic `WireMock` integration subset before packaging.

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
