using System;

namespace LeanCode.Pipelines
{
    public interface IPipelineContext
    {
        /// <summary>
        /// Managed by <see cref="PipelineExecutor{TContext, TInput, TOutput}" />.
        /// </summary>
        IPipelineScope Scope { get; set; }
    }
}
