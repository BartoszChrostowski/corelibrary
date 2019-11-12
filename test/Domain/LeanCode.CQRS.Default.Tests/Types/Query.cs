using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public class Result
    {
        public int Value { get; set; }
    }

    public class Query : IQuery<Result>
    {
        public bool FailExecution { get; set; }
    }

    [AuthorizeWhen(typeof(IAuthorizer))]
    public class SecuredQuery : IQuery<Result>, IAuthorizerPayload { }
    [QueryCache(1)]
    public class CachedQuery : IQuery<Result> { }

    public class QueryHandler :
        IQueryHandler<AppContext, Query, Result>,
        IQueryHandler<AppContext, SecuredQuery, Result>,
        IQueryHandler<AppContext, CachedQuery, Result>
    {
        public Task<Result> ExecuteAsync(AppContext context, Query query)
        {
            if (query.FailExecution)
            {
                throw new InvalidOperationException();
            }
            else
            {
                return Task.FromResult(new Result { Value = 10 });
            }
        }

        public Task<Result> ExecuteAsync(AppContext context, SecuredQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<Result> ExecuteAsync(AppContext context, CachedQuery query)
        {
            return Task.FromResult(new Result { Value = 10 });
        }
    }
}
