using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public sealed class QueryFinalizer<TContext>
        : IPipelineFinalizer<TContext, IQuery, object>
        where TContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<QueryFinalizer<TContext>>();
        private readonly IQueryHandlerResolver resolver;

        public QueryFinalizer(IQueryHandlerResolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task<object> ExecuteAsync(
            TContext ctx,
            IQuery query)
        {
            var queryType = query.GetType();
            var handler = resolver.FindQueryHandler(queryType);
            if (handler == null)
            {
                logger.Fatal("Cannot find a handler for query {@Query}", query);
                throw new QueryHandlerNotFoundException(query.GetType());
            }

            object result;
            try
            {
                result = await handler.ExecuteAsync(query).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex, "Cannot execute query {@Query} because of internal error",
                    query);
                throw;
            }
            logger.Debug("Query {@Query} executed successfuly", query);
            return result;
        }
    }
}