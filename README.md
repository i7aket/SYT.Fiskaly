# SYT.Fiskaly

`SYT.Fiskaly` is a production-oriented .NET SDK for Fiskaly SIGN DE v2 and the Fiskaly
Management API.

It provides strongly typed clients, resilient HTTP pipelines, typed identifiers, and
configuration validation for:
- TSS lifecycle via `ITssClient`
- Client lifecycle via `IClientManagementClient`
- Transactions via `ITransactionClient`
- Exports via `IExportClient`
- Management API organizations via `IOrganizationClient`
- Management API API keys via `IApiKeyClient`
- Authentication via `IFiskalyAuthenticationService`
- Scoped per-organization credentials via `IFiskalyCredentialScopeFactory`

## Package

- Package ID: `SYT.Fiskaly`
- Current channel: `1.0.0-rc.4`
- Target framework: `net10.0`
- License: `MIT`
- Repository: `https://github.com/i7aket/SYT.Fiskaly`
- NuGet debugging support: portable PDBs via `.snupkg`, XML docs, and Source Link-compatible repository metadata

## Installation

```bash
dotnet add package SYT.Fiskaly --prerelease
```

## Configuration

Minimum infrastructure settings:

```json
{
  "Fiskaly": {
    "BaseUrl": "https://kassensichv-middleware.fiskaly.com/api/v2/",
    "ManagementBaseUrl": "https://dashboard.fiskaly.com/api/v0/"
  }
}
```

Default `ApiKey` / `ApiSecret` are optional. Configure them only when the process should have a
global fallback credential pair. If credentials are supplied later via
`IFiskalyCredentialScopeFactory`, the SDK can start without default secrets.

Optional transport validation flags:
- `Fiskaly:AllowHttpForPrivateNetworks=true` allows `http://` for RFC1918/link-local/private LAN hosts.
- `Fiskaly:AllowHttpForPublicHosts=true` allows `http://` for public hosts. Keep this off unless you intentionally run an insecure proxy endpoint.

Optional per-client overrides are available under:
- `Fiskaly:AuthClient`
- `Fiskaly:AdminClient`
- `Fiskaly:TssClient`
- `Fiskaly:TransactionClient`
- `Fiskaly:ExportClient`
- `Fiskaly:ClientManagementClient`
- `Fiskaly:OrganizationClient`
- `Fiskaly:ApiKeyClient`

## Registration

```csharp
using SYT.Fiskaly;

builder.Services.AddFiskaly(builder.Configuration);
```

## Quick Start

```csharp
using SYT.Fiskaly.SignDE.Tss;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

ITssClient tssClient = app.Services.GetRequiredService<ITssClient>();
TssId tssId = TssId.New();

var created = await tssClient.CreateTssAsync(tssId, cancellationToken: ct);
Console.WriteLine($"Created TSS: {created.Id}");
```

## Control Plane Example

This is the recommended pattern for control-plane services that create managed organizations,
issue runtime API keys, and then operate in the scope of those runtime credentials.

```csharp
using SYT.Fiskaly.Authentication;
using SYT.Fiskaly.Authentication.Credentials;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Management.ApiKeys;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.Management.Organizations;

IOrganizationClient organizations = sp.GetRequiredService<IOrganizationClient>();
IApiKeyClient apiKeys = sp.GetRequiredService<IApiKeyClient>();
IFiskalyCredentialScopeFactory scopes = sp.GetRequiredService<IFiskalyCredentialScopeFactory>();

OrganizationId organizationId = OrganizationId.From("9b8ad703-b85c-4dec-882d-2dc7525ada3f");

var createdKey = await apiKeys.CreateApiKeyAsync(
    organizationId,
    new CreateApiKeyRequest
    {
        Name = "runtime-berlin-register-01"
    },
    ct);

using IDisposable scope = scopes.BeginScope(new ApiKeyCredentials(
    ApiKey.From(createdKey.Key ?? throw new InvalidOperationException("API key was not returned.")),
    ApiSecret.From(createdKey.Secret ?? throw new InvalidOperationException("API secret was not returned."))));

var runtimeOrganization = await organizations.GetOrganizationAsync(organizationId, ct);
Console.WriteLine($"Scoped organization lookup: {runtimeOrganization.Name}");
```

## SIGN DE Transaction Example

```csharp
using SYT.Fiskaly.SignDE.Clients.ValueObjects;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Transactions;
using SYT.Fiskaly.SignDE.Transactions.Enums;
using SYT.Fiskaly.SignDE.Transactions.Requests;
using SYT.Fiskaly.SignDE.Transactions.Schemas;
using SYT.Fiskaly.SignDE.Transactions.ValueObjects;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

ITransactionClient transactionClient = sp.GetRequiredService<ITransactionClient>();

TssId tssId = TssId.From("d9ee9052-fd45-4846-af24-818d10353cdb");
ClientId clientId = ClientId.From("0d918f2a-3c47-4662-a665-8e565d61109b");
TxId txId = TxId.New();

StartTransactionRequest start = new()
{
    ClientId = clientId,
    Metadata = MetadataCollection.Empty.Add("cashpoint", "POS-01")
};

_ = await transactionClient.StartTransactionAsync(tssId, txId, start, ct);

Receipt receipt = new()
{
    ReceiptType = ReceiptType.Receipt,
    AmountsPerVatRate =
    [
        new VatRateAmount
        {
            VatRate = VatRate.Normal,
            Amount = MoneyAmount.Create(45.00m, CurrencyCode.EUR)
        }
    ],
    AmountsPerPaymentType =
    [
        new PaymentTypeAmount
        {
            PaymentType = PaymentType.Cash,
            Amount = MoneyAmount.Create(45.00m, CurrencyCode.EUR)
        }
    ]
};

FinishTransactionRequest finish = FinishTransactionRequest.CreateReceipt(
    clientId,
    receipt,
    MetadataCollection.Empty.Add("cashpoint", "POS-01"));

var finished = await transactionClient.FinishTransactionAsync(tssId, txId, finish, ct);
Console.WriteLine($"Tx finished: {finished.Id}, qr={finished.QrCodeData}");
```

## TEST and LIVE Environments

- Fiskaly API keys are organization-scoped and environment-scoped.
- `TEST` and `LIVE` are separate operational environments.
- New managed organizations are typically provisioned and exercised in `TEST` first.
- Moving to `LIVE` is an explicit Management API and operational step. It is not an automatic `TEST -> LIVE` promotion.
- Runtime services should use credentials issued for the exact target organization and environment.

## Validation Rules

- `Fiskaly:ApiKey` and `Fiskaly:ApiSecret` are optional as a pair.
- If one is configured, the other must also be configured.
- When `Fiskaly:ApiSecret` is set, it must be exactly 43 alphanumeric characters.
- `BaseUrl` and `ManagementBaseUrl` must be absolute and end with `/`.
- HTTPS is required by default.
- HTTP is allowed only for localhost, or private networks when `AllowHttpForPrivateNetworks=true`.
- HTTP for public hosts is allowed only when `AllowHttpForPublicHosts=true`.

## Default Resilience Profiles

| Client | Timeout (s) | RetryCount | CircuitBreakerThreshold | CircuitBreakerDuration (s) |
|---|---:|---:|---:|---:|
| `AuthClient` | 10 | 2 | 0 | 10 |
| `AdminClient` | 10 | 1 | 5 | 30 |
| `TssClient` | 30 | 3 | 5 | 60 |
| `TransactionClient` | 45 | 5 | 10 | 90 |
| `ExportClient` | 120 | 2 | 3 | 240 |
| `ClientManagementClient` | 30 | 3 | 5 | 60 |
| `OrganizationClient` | 30 | 3 | 5 | 60 |
| `ApiKeyClient` | 30 | 3 | 5 | 60 |

## Runtime Override in Code

```csharp
using SYT.Fiskaly;
using SYT.Fiskaly.Configuration;

builder.Services.AddFiskaly(builder.Configuration, configure: cfg =>
{
    cfg.TransactionClient.UseHighResilience();
    cfg.ApiKeyClient.DisableResilience();
});
```

## Error Handling

Production usage typically maps SDK exceptions to domain-level failures:
- `FiskalyApiException`
- `FiskalyTimeoutException`
- `FiskalyCredentialsNotConfiguredException`
- `FiskalyException`

## Release Notes

See [CHANGELOG.md](CHANGELOG.md) for the current release history.
