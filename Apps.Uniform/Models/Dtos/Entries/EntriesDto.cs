namespace Apps.Uniform.Models.Dtos.Entries;

public class EntriesDto<T>
{
    public List<T> Entries { get; set; } = new();
}