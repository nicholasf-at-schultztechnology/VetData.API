
namespace VetData.Client.Models.Responses;

public record ODataResponse<T>
{
    public IReadOnlyList<T> Value { get; init; } = Array.Empty<T>();
    public int? Count { get; init; }
}