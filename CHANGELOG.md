# Changelog

## [1.0.0-rc.6] - 2026-08-04

### Added
- `FiskalyExportNotReadyException`, raised when an export archive is requested before the export can be
  downloaded. Carries `State`, `ExportId`, the provider's `ExceptionCode`, and `IsTransient` so a caller can
  tell "keep polling" (PENDING/WORKING) from "trigger a new export" (ERROR).

### Fixed
- `DownloadExportAsync` refused a not-yet-COMPLETED export with a bare `InvalidOperationException`, which sits
  outside the `FiskalyException` hierarchy every caller catches. An ordinary race - the state is read
  immediately before the download and the export finishes in between - therefore escaped as an unhandled
  exception and reached consuming applications as an HTTP 500.

### Compatibility
- Source-compatible for callers that catch `FiskalyException`. Only code catching `InvalidOperationException`
  specifically around a download needs to change.

## [1.0.0-rc.5] - 2026-08-01

### Added
- `TxResponse.RawJson`: the exact body a transaction response was deserialized from, so a German fiscal
  signature can be shown to an auditor as the provider returned it rather than as this SDK read it. Captured
  on both the write and the read path; provider errors are not captured, because the error handler raises
  before the body is read.

## [1.0.0-rc.4] - 2026-04-12

### Added
- Added `Fiskaly:AllowHttpForPublicHosts` for explicit opt-in public HTTP endpoints, intended for controlled proxy deployments.

### Changed
- Updated package metadata, README, and release notes for the new public HTTP validation flag.
- Hardened NuGet packaging with symbol package output, XML documentation, deterministic build settings, and published repository metadata.

### Fixed
- Validation now permits public `http://` `BaseUrl` and `ManagementBaseUrl` only when `AllowHttpForPublicHosts=true`.

## [1.0.0-rc.3] - 2026-04-11

### Added
- Added `FiskalyCredentialsNotConfiguredException` for calls that execute without default or scoped credentials.

### Changed
- Default `Fiskaly:ApiKey` and `Fiskaly:ApiSecret` are now optional as a pair, which allows startup with runtime-scoped credentials only.
- Updated package metadata, package README, and repository README for the runtime-scoped credential flow.

### Fixed
- Validation now rejects only partially configured default credentials instead of requiring a global key pair at startup.
- Authentication now fails at execution time when no default or scoped credentials are available, instead of failing during service registration.

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
