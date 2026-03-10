using AwesomeAssertions;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.IntegrationTests.Base;
using SYT.Fiskaly.Management.Common.Responses;
using SYT.Fiskaly.Management.Organizations.Requests;
using SYT.Fiskaly.Management.Organizations.Responses;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.Common.Enums;
using Xunit.Abstractions;

namespace SYT.Fiskaly.IntegrationTests.Management;

[Trait("Category", "Integration")]
[Trait("Feature", "ManagementAPI")]
[Trait("Priority", "Critical")]
[Collection("FiskalyClient collection")]
public class OrganizationClientManagementIntegrationTests
{
    private readonly FiskalyClientFixture _fixture;
    private readonly ITestOutputHelper _output;

    public OrganizationClientManagementIntegrationTests(FiskalyClientFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _fixture.TestOutputHelper = output;
    }

    [Fact]
    public async Task CreateUpdateEnableAndDeleteOrganization_RoundTrips()
    {
        string suffix = Guid.NewGuid().ToString("N")[..8];
        OrganizationId managingOrganizationId = await ManagementApiTestHelpers.GetManagingOrganizationIdAsync(_fixture, _output);

        OrganizationId? createdOrganizationId = null;

        try
        {
            OrganizationResponse createdOrganization = await ManagementApiTestHelpers.CreateManagedOrganizationAsync(
                _fixture,
                managingOrganizationId,
                suffix,
                _output);

            createdOrganizationId = createdOrganization.Id!.Value;
            createdOrganization.ManagedByOrganizationId.Should().Be(managingOrganizationId);
            _output.WriteLine($"Created org envs: {string.Join(", ", createdOrganization.Envs ?? [])}");

            UpdateOrganizationRequest updateRequest = new()
            {
                DisplayName = $"SDK Mgmt Updated {suffix}",
                AddressLine2 = "Updated Suite",
                Metadata = MetadataCollection.Empty
                    .Add("suite", "sdk-management")
                    .Add("updated", "true")
            };

            OrganizationResponse updatedOrganization = await _fixture.OrganizationClient.UpdateOrganizationAsync(
                createdOrganizationId.Value,
                updateRequest);

            updatedOrganization.DisplayName.Should().Be(updateRequest.DisplayName);
            updatedOrganization.AddressLine2.Should().Be(updateRequest.AddressLine2);
            updatedOrganization.Metadata.Should().NotBeNull();
            updatedOrganization.Metadata!["updated"].Should().Be("true");

            OrganizationResponse retrievedOrganization = await _fixture.OrganizationClient.GetOrganizationAsync(createdOrganizationId.Value);
            retrievedOrganization.Id.Should().Be(createdOrganizationId.Value);
            retrievedOrganization.DisplayName.Should().Be(updateRequest.DisplayName);
            retrievedOrganization.Metadata.Should().NotBeNull();
            retrievedOrganization.Metadata!["updated"].Should().Be("true");
            retrievedOrganization.Envs.Should().NotBeNull();
            retrievedOrganization.Envs.Should().Contain(Env.Test,
                "current fiskaly docs state that new units are created in TEST by default");

            StatusResponse deleteResponse = await _fixture.OrganizationClient.DeleteOrganizationAsync(createdOrganizationId.Value);
            deleteResponse.Success.Should().BeTrue();
            createdOrganizationId = null;
        }
        finally
        {
            await ManagementApiTestHelpers.TryDeleteOrganizationAsync(_fixture, createdOrganizationId, _output);
        }
    }
}
