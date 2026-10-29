namespace Apps.Uniform.Models.Dtos.Compositions;

public class CompositionsDto<T>
{
    public List<T> Compositions { get; set; } = new();
    
    public int? TotalCount { get; set; }

    public Dictionary<string, Dictionary<string, int>> Facets { get; set; } = new();
}
