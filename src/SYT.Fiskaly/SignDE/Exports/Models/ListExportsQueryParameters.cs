using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Exports.Enums;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;

namespace SYT.Fiskaly.SignDE.Exports.Models;

public class ListExportsQueryParameters : ListQueryParametersBase
{
    public ExportSortOption? Sort { get; set; }

    public IReadOnlyCollection<ExportState>? States { get; set; }

    public override IEnumerable<KeyValuePair<string, string?>> ToQueryParameters()
    {
        List<KeyValuePair<string, string?>> items = new List<KeyValuePair<string, string?>>();

        if (Sort is { } sort)
        {
            items.Add(new KeyValuePair<string, string?>("order_by", EnumApiValueProvider.GetApiName(sort.Field)));
            items.Add(new KeyValuePair<string, string?>("order", EnumApiValueProvider.GetApiName(sort.Direction)));
        }

        if (States is { Count: > 0 } states)
        {
            int index = 0;
            foreach (ExportState state in states)
            {
                items.Add(new KeyValuePair<string, string?>($"states[{index++}]", EnumApiValueProvider.GetApiName(state)));
            }
        }

        AddPaginationParameters(items);

        return items;
    }

    internal IEnumerable<KeyValuePair<string, string?>> ToKeyValuePairs() => ToQueryParameters();
}
