using System.Text.Json;
using AwesomeAssertions;
using SYT.Fiskaly.Management.ApiKeys.Responses;

namespace SYT.Fiskaly.UnitTests.Management.ApiKeys.Responses;

public class ListApiKeyResponseTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_WithCountAndData_ReturnsExpectedValues()
    {
        const string json = """
                            {
                              "_type": "API_KEY_LIST",
                              "count": 1,
                              "data": [
                                {
                                  "_id": "550e8400-e29b-41d4-a716-446655440000",
                                  "name": "runtime-key"
                                }
                              ]
                            }
                            """;

        ListApiKeyResponse? response = JsonSerializer.Deserialize<ListApiKeyResponse>(json);

        response.Should().NotBeNull();
        response!.Type.Should().Be("API_KEY_LIST");
        response.Count.Should().Be(1);
        response.Data.Should().ContainSingle();
        response.Data[0].Name.Should().Be("runtime-key");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_WithoutData_UsesEmptyCollection()
    {
        const string json = """
                            {
                              "_type": "API_KEY_LIST"
                            }
                            """;

        ListApiKeyResponse? response = JsonSerializer.Deserialize<ListApiKeyResponse>(json);

        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull();
        response.Data.Should().BeEmpty();
    }
}
