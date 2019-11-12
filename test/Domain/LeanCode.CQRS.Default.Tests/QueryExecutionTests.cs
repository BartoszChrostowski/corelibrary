using System;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Cache;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Default.Tests.Types;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.CQRS.Validation.Fluent;
using NSubstitute;
using Xunit;

using AppContext = LeanCode.CQRS.Default.Tests.Types.AppContext;
using CacheItem = LeanCode.CQRS.Cache.CacheElement<LeanCode.CQRS.Default.Tests.Types.AppContext>.CacheItemWrapper;

namespace LeanCode.CQRS.Default.Tests
{
    public class QueryExecutionTests
    {
        private readonly IContainer container;
        private readonly ICacher cache;

        public QueryExecutionTests()
        {
            var types = TypesCatalog.Of<ResolvingTests>();

            var builder = new ContainerBuilder();

            cache = Substitute.For<ICacher>();
            builder.RegisterInstance(cache);

            builder.RegisterModule(new CQRSModule()
                .WithDefaultPipelines<AppContext>(types));

            // Register security
            builder.RegisterType<Authorizer>().AsImplementedInterfaces();

            container = builder.Build();
        }

        [Fact]
        public async Task Executes_pipeline()
        {
            await GetAsync(new Query());
        }

        [Fact]
        public async Task Default_pipeline_runs_custom_authorizers()
        {
            await Assert.ThrowsAsync<UnauthenticatedException>(() => GetAsync(new SecuredQuery()));
        }

        [Fact]
        public async Task Unhandled_exceptions_are_rethrown_to_the_top()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => GetAsync(new Query { FailExecution = true }));
        }

        [Fact]
        public async Task Default_pipeline_runs_the_query_through_cache()
        {
            var cachedRes = new Result { Value = int.MaxValue };
            var wrapped = new CacheItem(cachedRes);
            cache.Get<CacheItem>(Arg.Any<string>()).Returns(wrapped);
            cache.GetOrCreate(Arg.Any<string>(), Arg.Any<Func<CacheItem>>()).Returns(wrapped);
            cache.GetOrCreate(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<Func<CacheItem>>()).Returns(wrapped);
            cache.GetOrCreate(Arg.Any<string>(), Arg.Any<Func<Task<CacheItem>>>()).Returns(wrapped);
            cache.GetOrCreate(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<Func<Task<CacheItem>>>()).Returns(wrapped);

            var res = await GetAsync(new CachedQuery());

            Assert.Equal(cachedRes, res);
        }

        private async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
        {
            var exec = container.Resolve<IQueryExecutor<AppContext>>();

            return await exec.GetAsync(new AppContext(), query);
        }
    }
}
