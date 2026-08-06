#nullable enable

namespace SYT.Fiskaly.Exceptions;

/// <summary>
/// Fiskaly SIGN DE error codes based on the official SIGN DE OpenAPI specification.
/// Refer to the official error catalog for status codes and retry guidance.
/// </summary>
public enum FiskalyErrorCode
{
    Unknown = 0,
    E_TSS_NOT_FOUND,
    E_TSS_CONFLICT,
    E_TSS_DISABLED,
    E_TSS_NOT_INITIALIZED,
    E_TSS_DEFECTIVE,
    E_TSS_DELETED,
    E_TSS_EVICTED,
    E_TSS_LOCKED,
    E_TSS_LIMIT_REACHED,
    E_TSS_LIMIT_PER_DAY_REACHED,
    E_ILLEGAL_TSS_STATE_CHANGE,
    E_TSS_ILLEGAL_STATE_TO_PERFORM_EXPORT,
    E_TX_NOT_FOUND,
    E_TX_UPSERT,
    E_TX_NO_TYPE_DEFINED,
    E_TX_ILLEGAL_TYPE_CHANGE,
    E_TX_LIMIT_REACHED,
    E_TX_REVISION_NOT_FOUND,
    E_PENDING_TX_CONFLICT,
    E_CLIENT_NOT_FOUND,
    E_CLIENT_CONFLICT,
    E_CLIENT_DEREGISTERED,
    E_CLIENT_LIMIT_REACHED,
    E_ILLEGAL_CLIENT_SERIAL,
    E_LATEST_TX_REVISION_NOT_FOUND,
    E_EXPORT_NOT_FOUND,
    E_EXPORT_NOT_COMPLETED,
    E_EXPORT_FORBIDDEN,
    E_EXPORT_IN_PROGRESS,
    E_EXPORT_TEMPORARILY_UNAVAILABLE,
    E_DUPLICATE_EXPORT,
    E_EXPORT_DUPLICATE_RATE_LIMITED,
    E_TOO_MANY_EXPORTS,
    E_CANCEL_EXPORT,
    E_UNAUTHORIZED,
    E_ADMIN_LOGIN_FAILED,
    E_ADMIN_PIN_BLOCKED,
    E_CHANGE_ADMIN_PIN_FAILED,
    E_ACCESS_DENIED,
    E_SMAERS_GATEWAY_CAPACITIES_DEPLETED,
    E_BOOTSTRAP_FILE_NOT_AVAILABLE,
    E_USE_MIDDLEWARE,
    E_MIDDLEWARE_PENDING_REQUEST,
    E_AVAILABLE_ON_TEST_ONLY,
    E_FAILED_SCHEMA_VALIDATION,
    E_PARAMETER_MISMATCH,
    E_LOG_NOT_FOUND,
    E_CSPL_NOT_FOUND,

    /// <summary>
    /// 423 Locked. The TSS certificate has passed its validity window; fiskaly answer this to every operation
    /// on that TSS. Recovery is a replacement TSS with newly registered clients - nothing about the existing
    /// one can be repaired, so this must never be retried.
    /// </summary>
    E_CERTIFICATE_EXPIRED,

    /// <summary>
    /// Generic 404 from the middleware, observed live on paths that do not exist. Documented nowhere in the
    /// 2.1.35 specification, which is why it used to fall through to <see cref="Unknown"/>.
    /// </summary>
    E_NOT_FOUND,

    /// <summary>
    /// 502 from the SMAERS gateway: the security module still holds unexported logs. fiskaly's guidance is to
    /// retry.
    /// </summary>
    SMAERS_GATEWAY_ERROR_PRECONDITION_UNEXPORTED_LOGS,

    /// <summary>
    /// 502 from the SMAERS gateway: the ERS could not be identified for this request. fiskaly's guidance is to
    /// retry.
    /// </summary>
    ERROR_IDENTIFY_ERS
}
