using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.Management.ApiKeys.Models;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.UnitTests.Management.ApiKeys.Models;

public class ListApiKeysQueryParametersTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_CreatesEmptyParameters()
    {
        ListApiKeysQueryParameters parameters = new();

        Assert.Null(parameters.Limit);
        Assert.Null(parameters.Offset);
        Assert.Null(parameters.OrderBy);
        Assert.Null(parameters.Order);
        Assert.Null(parameters.Status);
        Assert.Null(parameters.ShowDeleted);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ToQueryParameters_WithTypedValues_ReturnsExpectedPairs()
    {
        ListApiKeysQueryParameters parameters = new()
        {
            OrderBy = ApiKeySortField.CreatedAt,
            Order = SortDirection.Ascending,
            Status = ApiKeyStatus.Disabled,
            Limit = 10,
            Offset = 5
        };

        List<KeyValuePair<string, string?>> queryParams = parameters.ToQueryParameters().ToList();

        Assert.Collection(queryParams,
            item =>
            {
                Assert.Equal("order_by", item.Key);
                Assert.Equal("created_at", item.Value);
            },
            item =>
            {
                Assert.Equal("order", item.Key);
                Assert.Equal("asc", item.Value);
            },
            item =>
            {
                Assert.Equal("status", item.Key);
                Assert.Equal("disabled", item.Value);
            },
            item =>
            {
                Assert.Equal("limit", item.Key);
                Assert.Equal("10", item.Value);
            },
            item =>
            {
                Assert.Equal("offset", item.Key);
                Assert.Equal("5", item.Value);
            });
    }
}
