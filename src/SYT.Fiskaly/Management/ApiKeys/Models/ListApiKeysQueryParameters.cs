using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.Management.ApiKeys.Models;

public sealed class ListApiKeysQueryParameters : ListQueryParametersBase
{
    public ApiKeySortField? OrderBy { get; set; }

    public SortDirection? Order { get; set; }

    public ApiKeyStatus? Status { get; set; }

    public override IEnumerable<KeyValuePair<string, string?>> ToQueryParameters()
    {
        List<KeyValuePair<string, string?>> parameters = new();

        if (OrderBy.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string?>("order_by", EnumApiValueProvider.GetApiName(OrderBy.Value)));
        }

        if (Order.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string?>("order", EnumApiValueProvider.GetApiName(Order.Value)));
        }

        if (Status.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string?>("status", EnumApiValueProvider.GetApiName(Status.Value)));
        }

        AddPaginationParameters(parameters);

        return parameters;
    }
}
