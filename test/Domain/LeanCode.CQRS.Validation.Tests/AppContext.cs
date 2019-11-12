using System;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation.Tests
{
    public class AppContext : IPipelineContext
    {
        private IPipelineScope? scope;
        public IPipelineScope Scope
        {
            get => scope ?? throw new NullReferenceException();
            set => scope = value;
        }
    }
}
