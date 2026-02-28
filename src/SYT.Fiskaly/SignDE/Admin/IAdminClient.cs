using SYT.Fiskaly.SignDE.Admin.Requests;
using SYT.Fiskaly.SignDE.Admin.Responses;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.SignDE.Admin;

/// <summary>
/// Typed access to SIGN DE admin endpoints (/api/v2/tss/{tss_id}/admin*).
/// </summary>
public interface IAdminClient
{
    /// <summary>
    /// Calls POST /api/v2/tss/{tss_id}/admin/auth to open an admin session.
    /// </summary>
    Task<AdminAuthenticationResponse> AuthenticateAdminAsync(
        TssId tssId,
        AdminAuthenticationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls POST /api/v2/tss/{tss_id}/admin/logout to end the admin session.
    /// </summary>
    Task LogoutAdminAsync(
        TssId tssId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls PATCH /api/v2/tss/{tss_id}/admin to change the admin PIN.
    /// </summary>
    Task ChangeAdminPinAsync(
        TssId tssId,
        ChangeAdminPinRequest request,
        CancellationToken cancellationToken = default);
}
