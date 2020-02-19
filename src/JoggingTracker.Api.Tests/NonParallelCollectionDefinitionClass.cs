using Xunit;

namespace JoggingTracker.Api.Tests
{
    /// <summary>
    /// https://stackoverflow.com/a/59124800/775779
    /// </summary>
    [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
    public class NonParallelCollectionDefinitionClass
    {
    }
}