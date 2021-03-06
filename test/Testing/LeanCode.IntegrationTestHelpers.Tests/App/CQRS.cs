using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    [AuthorizeWhenHasAnyOf("user")]
    public class SampleQuery : IRemoteQuery<SampleQuery.Result?>
    {
        public sealed class Result
        {
            public string? Data { get; set; }
        }
    }

    public class SampleQueryHandler : IQueryHandler<AppContext, SampleQuery, SampleQuery.Result?>
    {
        private readonly TestDbContext dbContext;

        public SampleQueryHandler(TestDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<SampleQuery.Result?> ExecuteAsync(AppContext context, SampleQuery query)
        {
            // This will return `null`, but the query will be executed
            var data = await dbContext.Entities.Select(l => l.Value).FirstOrDefaultAsync();
            return new SampleQuery.Result { Data = $"{context.UserId}-{data}" };
        }
    }

    public sealed class AppContext : IEventsContext, ISecurityContext
    {
        private IPipelineScope? scope;
        private ClaimsPrincipal? user;

        List<IDomainEvent> IEventsInterceptorContext.SavedEvents { get; set; } = new List<IDomainEvent>();
        List<(IDomainEvent Event, Type Handler)> IEventsExecutorContext.ExecutedHandlers { get; set; } = new List<(IDomainEvent Event, Type Handler)>();
        List<(IDomainEvent Event, Type Handler)> IEventsExecutorContext.FailedHandlers { get; set; } = new List<(IDomainEvent Event, Type Handler)>();

        IPipelineScope IPipelineContext.Scope
        {
            get => scope ?? throw new NullReferenceException();
            set => scope = value;
        }

        public ClaimsPrincipal User
        {
            get => user ?? throw new NullReferenceException();
            set => user = value;
        }

        public Guid UserId { get; }

        public AppContext(ClaimsPrincipal user, Guid userId)
        {
            User = user;
            UserId = userId;
        }

        public static AppContext FromHttp(HttpContext context)
        {
            Guid.TryParse(context.User?.FindFirst("sub")?.Value, out var uid);
            return new AppContext(context.User!, uid);
        }
    }
}
