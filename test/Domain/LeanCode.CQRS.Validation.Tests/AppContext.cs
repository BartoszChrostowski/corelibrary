using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation.Tests
{
    public class AppContext : IPipelineContext
    {
        public IPipelineScope? Scope { get; set; }
    }
}
