using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Uniform;

public class Application : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => 
        [
            ApplicationCategory.Cms,
            ApplicationCategory.FileManagementAndStorage
        ];
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}
