# Changelog

## [1.0.0-rc.7] - 2026-08-06

### Fixed
- An unrecognised error code no longer decides that a response is permanent. Classification falls back to the
  HTTP status, which is the term the specification's own retry rules are written in: 5xx "can be considered
  temporary … may safely retry", 499 "can, and should, be retried", 429 "wait for Retry-After and retry". The
  responses least likely to carry a code the SDK knows - a bare 503, a gateway 502 - were exactly the ones it
  refused to retry, and the `Retry-After` the pipeline parses for a 429 was therefore dead code, because the
  retry predicate never fired for one. An unrecognised code now reports the status it arrived with instead of
  being flattened to 500.
- `Retry-After` is read from any response that carries it, not only from a 429; fiskaly send it with 503 as
  well, and an interval they name beats a backoff the SDK guesses. An interval that resolves into the past -
  a stale `Date`, clock skew - is floored at one second rather than meaning "retry immediately", which turned
  the one response asking for patience into a tighter loop.
- A rejected token is dropped from the cache. `IFiskalyAuthenticationService.InvalidateToken` is new, and
  `JwtAuthHandler` calls it on 401 before rethrowing. Until now the cache kept handing out a token the
  provider had already refused until its nominal expiry, so fiskaly's guidance for a 401 - "simply
  reauthorize" - could not happen.
- The specification carried in `Documentation/official-documentation` is refreshed from 2.1.35 to 2.2.2.
  Diffing the two shows the gap was narrow but real: identical operation set, and exactly one added error code
  - `E_CERTIFICATE_EXPIRED`. Every code the current specification names is now known to the SDK.

### Considered and rejected
- Moving `JwtAuthHandler` inside the resilience pipeline, so that each retry attaches a freshly fetched token
  and a 401 heals within the same call. It routes token acquisition through Polly, and the integration suite
  shows the price: a permanent 404 stops failing immediately and the circuit breaker stops opening on
  consecutive failures, because authentication exceptions feed both. Healing one call sooner is not worth
  changing how every other failure behaves.

### Added
- `E_CERTIFICATE_EXPIRED` (423, permanent, "create a replacement TSS and register its clients anew"),
  `E_NOT_FOUND`, `SMAERS_GATEWAY_ERROR_PRECONDITION_UNEXPORTED_LOGS` and `ERROR_IDENTIFY_ERS` (both 502 and
  retryable, as fiskaly ask). None of these appear in the 2.1.35 specification carried in this repository; the
  live API emits them, and until now each fell through to the unknown-code path.

### Compatibility
- Source-compatible. `IFiskalyAuthenticationService` gains a member, which is breaking only for a caller that
  implements the interface itself.
- Behavioural: requests that previously failed on the first 429/499/5xx without a known code are now retried
  under the configured policy. Callers relying on immediate failure for those should review their timeouts.

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
