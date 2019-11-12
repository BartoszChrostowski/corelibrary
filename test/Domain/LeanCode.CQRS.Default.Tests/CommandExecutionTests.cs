using System;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Cache;
using LeanCode.Components;
using LeanCode.CQRS.Default.Tests.Types;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.CQRS.Validation.Fluent;
using NSubstitute;
using Xunit;

using AppContext = LeanCode.CQRS.Default.Tests.Types.AppContext;

namespace LeanCode.CQRS.Default.Tests
{
    public class CommandExecutionTests
    {
        private readonly IContainer container;
        private readonly DomainEventHandler eventHandler;

        public CommandExecutionTests()
        {
            var types = TypesCatalog.Of<ResolvingTests>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(Substitute.For<ICacher>());

            builder.RegisterModule(new CQRSModule()
                .WithDefaultPipelines<AppContext>(types));
            builder.RegisterModule(new FluentValidationModule(types));

            // Register security
            builder.RegisterType<Authorizer>().AsImplementedInterfaces();

            // Override the handler lifetime
            eventHandler = new DomainEventHandler();
            builder.RegisterInstance(eventHandler)
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            container = builder.Build();
        }

        [Fact]
        public async Task Executes_pipeline()
        {
            await RunAsync(new Command());
        }

        [Fact]
        public async Task Default_pipeline_runs_validation_on_commands()
        {
            var res = await RunAsync(new Command { FailValidation = true });

            var err = Assert.Single(res.ValidationErrors);
            Assert.Equal(1, err.ErrorCode);
        }

        [Fact]
        public async Task Default_pipeline_runs_exception_translation_on_commands()
        {
            var res = await RunAsync(new Command { FailTranslation = true });

            var err = Assert.Single(res.ValidationErrors);
            Assert.Equal(2, err.ErrorCode);
        }

        [Fact]
        public async Task Default_pipeline_correctly_catches_events()
        {
            await RunAsync(new Command { RaiseEvent = true });

            Assert.True(eventHandler.Handled);
        }

        [Fact]
        public async Task Default_pipeline_runs_custom_authorizers()
        {
            await Assert.ThrowsAsync<UnauthenticatedException>(() => RunAsync(new SecuredCommand()));
        }

        [Fact]
        public async Task Unhandled_exceptions_are_rethrown_to_the_top()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(new Command { FailCommand = true }));
        }

        private async Task<CommandResult> RunAsync<T>(T cmd)
            where T : ICommand
        {
            var exec = container.Resolve<ICommandExecutor<AppContext>>();

            return await exec.RunAsync(new AppContext(), cmd);
        }
    }
}
