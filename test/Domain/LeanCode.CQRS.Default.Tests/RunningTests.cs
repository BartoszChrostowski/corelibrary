using System.Threading.Tasks;
using Autofac;
using LeanCode.Cache;
using LeanCode.Components;
using LeanCode.CQRS.Default.Tests.Types;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests
{
    public class RunningTests
    {
        private readonly IContainer container;

        public RunningTests()
        {
            var types = TypesCatalog.Of<ResolvingTests>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(Substitute.For<ICacher>());

            builder.RegisterModule(new CQRSModule()
                .WithDefaultPipelines<AppContext>(types));
            builder.RegisterModule(new FluentValidationModule(types));

            container = builder.Build();
        }

        [Fact]
        public async Task Running_command()
        {
            var exec = container.Resolve<ICommandExecutor<AppContext>>();

            await exec.RunAsync(new AppContext(), new Command());
        }

        [Fact]
        public async Task Running_queries()
        {
            var exec = container.Resolve<IQueryExecutor<AppContext>>();

            await exec.GetAsync(new AppContext(), new Query());
        }
    }
}
