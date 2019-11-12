using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public class Query : IQuery<Query> { }

    public class QueryHandler : IQueryHandler<AppContext, Query, Query>
    {
        public Task<Query> ExecuteAsync(AppContext context, Query query)
        {
            return Task.FromResult(query);
        }
    }
}
