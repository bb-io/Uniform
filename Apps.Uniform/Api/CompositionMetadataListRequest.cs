using RestSharp;

namespace Apps.Uniform.Api;

public class CompositionMetadataListRequest : RestRequest
{
    private const string NoFields = "";
    private const string NoSlots = "";

    public CompositionMetadataListRequest(string state, string orderBy, int limit, int offset) : base("/api/v1/canvas")
    {
        this.AddParameter("state", state);
        this.AddParameter("orderBy", orderBy);
        this.AddParameter("limit", limit);
        this.AddParameter("offset", offset);
        this.AddParameter("select.fields[only]", NoFields);
        this.AddParameter("select.slots[only]", NoSlots);
    }
}
