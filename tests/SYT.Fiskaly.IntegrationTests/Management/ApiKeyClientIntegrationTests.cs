using System.Net;
using AwesomeAssertions;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Exceptions;
using SYT.Fiskaly.IntegrationTests.Base;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.Management.ApiKeys.Responses;
using SYT.Fiskaly.Management.Common.Responses;
using SYT.Fiskaly.SignDE.Common;
using Xunit.Abstractions;

namespace SYT.Fiskaly.IntegrationTests.Management;

[Trait("Category", "Integration")]
[Trait("Feature", "ManagementAPI")]
[Trait("Priority", "Critical")]
[Collection("FiskalyClient collection")]
public class ApiKeyClientIntegrationTests
{
    private readonly FiskalyClientFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ApiKeyClientIntegrationTests(FiskalyClientFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _fixture.TestOutputHelper = output;
    }

    [Fact]
    public async Task CreateUpdateListGetAndDeleteApiKey_RoundTrips()
    {
        string suffix = Guid.NewGuid().ToString("N")[..8];
        OrganizationId managingOrganizationId = await ManagementApiTestHelpers.GetManagingOrganizationIdAsync(_fixture, _output);

        OrganizationId? organizationId = null;
        ApiKeyId? apiKeyId = null;

        try
        {
            organizationId = (await ManagementApiTestHelpers.CreateManagedOrganizationAsync(
                _fixture,
                managingOrganizationId,
                suffix,
                _output)).Id!.Value;

            CreateApiKeyRequest createRequest = new()
            {
                Name = $"sdk-runtime-{suffix}",
                Status = ApiKeyStatus.Enabled,
                Metadata = MetadataCollection.Empty
                    .Add("suite", "sdk-management")
                    .Add("suffix", suffix)
            };

            ApiKeyResponse createdApiKey = await _fixture.ApiKeyClient.CreateApiKeyAsync(organizationId.Value, createRequest);
            createdApiKey.Id.Should().NotBeNull();
            createdApiKey.Key.Should().NotBeNullOrWhiteSpace();
            createdApiKey.Secret.Should().NotBeNullOrWhiteSpace();
            createdApiKey.Status.Should().Be(ApiKeyStatus.Enabled);
            apiKeyId = createdApiKey.Id!.Value;

            ListApiKeyResponse listedApiKeys = await _fixture.ApiKeyClient.ListApiKeysAsync(organizationId.Value);
            listedApiKeys.Data.Should().Contain(apiKey => apiKey.Id == apiKeyId);

            ApiKeyResponse retrievedApiKey = await _fixture.ApiKeyClient.GetApiKeyAsync(organizationId.Value, apiKeyId.Value);
            retrievedApiKey.Id.Should().Be(apiKeyId.Value);
            retrievedApiKey.Name.Should().Be(createRequest.Name);

            UpdateApiKeyRequest updateRequest = new()
            {
                Status = ApiKeyStatus.Disabled,
                Metadata = MetadataCollection.Empty
                    .Add("suite", "sdk-management")
                    .Add("disabled", "true")
            };

            ApiKeyResponse updatedApiKey = await _fixture.ApiKeyClient.UpdateApiKeyAsync(
                organizationId.Value,
                apiKeyId.Value,
                updateRequest);

            updatedApiKey.Status.Should().Be(ApiKeyStatus.Disabled);
            updatedApiKey.Metadata.Should().NotBeNull();
            updatedApiKey.Metadata!["disabled"].Should().Be("true");

            StatusResponse deleteResponse = await _fixture.ApiKeyClient.DeleteApiKeyAsync(organizationId.Value, apiKeyId.Value);
            deleteResponse.Success.Should().BeTrue();
            apiKeyId = null;

            Func<Task> getDeletedApiKey = async () => await _fixture.ApiKeyClient.GetApiKeyAsync(organizationId.Value, createdApiKey.Id!.Value);
            FiskalyApiException deletedKeyException = await Assert.ThrowsAsync<FiskalyApiException>(getDeletedApiKey);
            deletedKeyException.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            if (organizationId is not null && apiKeyId is not null)
            {
                try
                {
                    await _fixture.ApiKeyClient.DeleteApiKeyAsync(organizationId.Value, apiKeyId.Value);
                }
                catch (FiskalyApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    _output.WriteLine($"API key already deleted: {apiKeyId}");
                }
            }

            await ManagementApiTestHelpers.TryDeleteOrganizationAsync(_fixture, organizationId, _output);
        }
    }
}
