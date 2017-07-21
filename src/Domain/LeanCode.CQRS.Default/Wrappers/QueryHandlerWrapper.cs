using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Wrappers
{
    class QueryHandlerWrapper<TQuery, TResult> : IQueryHandlerWrapper
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> handler;

        public QueryHandlerWrapper(IQueryHandler<TQuery, TResult> handler)
        {
            this.handler = handler;
        }

        public async Task<object> ExecuteAsync(IQuery query)
        {
            return await handler.ExecuteAsync((TQuery)query).ConfigureAwait(false);
        }
    }
}