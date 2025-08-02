namespace VetData.Client.Helpers;

public class ODataQueryBuilder
{
    private readonly List<string> _filters = new();
    private readonly List<string> _expands = new();
    private int? _skip;
    private int? _top;

    public void AddFilter(string filter) => _filters.Add(filter);
    public void AddExpand(string expand) => _expands.Add(expand);
    public void AddSkip(int skip) => _skip = skip;
    public void AddTop(int top) => _top = top;

    public string Build()
    {
        var parts = new List<string>();

        if (_filters.Any())
            parts.Add($"$filter={string.Join(" and ", _filters)}");
        
        if (_expands.Any())
            parts.Add($"$expand={string.Join(",", _expands)}");

        if (_skip.HasValue)
            parts.Add($"$skip={_skip.Value}");

        if (_top.HasValue)
            parts.Add($"$top={_top.Value}");

        return parts.Any() ? "?" + string.Join("&", parts) : string.Empty;
    }
}
