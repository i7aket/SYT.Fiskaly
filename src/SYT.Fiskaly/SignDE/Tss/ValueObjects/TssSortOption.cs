using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Tss.Enums;

namespace SYT.Fiskaly.SignDE.Tss.ValueObjects;

public readonly record struct TssSortOption
{
    private readonly SortOption<TssSortField> _inner;

    public TssSortOption(TssSortField field, SortDirection direction)
    {
        _inner = new SortOption<TssSortField>(field, direction, "TSS sort field");
    }

    public TssSortField Field => _inner.Field;

    public SortDirection Direction => _inner.Direction;

    public static TssSortOption By(TssSortField field, SortDirection direction = SortDirection.Ascending) =>
        new(field, direction);

    public override string ToString() => _inner.ToString();
}
