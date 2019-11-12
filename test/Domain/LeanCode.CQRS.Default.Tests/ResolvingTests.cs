using System.Threading.Tasks;
using Autofac;
using LeanCode.Cache;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Default.Tests.Types;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.EventsExecution.Simple;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests
{
    public class ResolvingTests
    {
        private readonly IContainer container;

        public ResolvingTests()
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
        public void Resolves_command_handler()
        {
            var handler = container.Resolve<ICommandHandler<AppContext, Command>>();

            Assert.IsType<CommandHandler>(handler);
        }

        [Fact]
        public void Resolves_query_handler()
        {
            var handler = container.Resolve<IQueryHandler<AppContext, Query, Result>>();

            Assert.IsType<QueryHandler>(handler);
        }

        [Fact]
        public async Task Resolves_fluent_command_validators()
        {
            // Adapters come into play here, so just run it
            var validator = container.Resolve<ICommandValidator<AppContext, Command>>();

            var result = await validator.ValidateAsync(new AppContext(), new Command { FailValidation = true });

            var err = Assert.Single(result.Errors);
            Assert.Equal(1, err.ErrorCode);
        }

        [Fact]
        public void Resolves_exception_translator()
        {
            var translator = container.Resolve<ICommandExceptionTranslator<AppContext, Command>>();

            Assert.IsType<ExceptionTranslator>(translator);
        }

        [Fact]
        public void Resolves_security_element()
        {
            container.Resolve<CQRSSecurityElement<AppContext, ICommand, CommandResult>>();
            container.Resolve<CQRSSecurityElement<AppContext, IQuery, object>>();
            container.Resolve<RoleRegistry>();
            container.Resolve<DefaultPermissionAuthorizer>();
        }

        [Fact]
        public void Resolves_validation_element()
        {
            container.Resolve<ValidationElement<AppContext>>();
        }

        [Fact]
        public void Resolves_exception_translator_element()
        {
            container.Resolve<ExceptionTranslatorElement<AppContext>>();
        }

        [Fact]
        public void Resolves_cache_element()
        {
            container.Resolve<CacheElement<AppContext>>();
        }

        [Fact]
        public void Resolves_event_related_elements()
        {
            container.Resolve<EventsInterceptorElement<AppContext, ICommand, CommandResult>>();
            container.Resolve<EventsExecutorElement<AppContext, ICommand, CommandResult>>();
            container.Resolve<AsyncEventsInterceptor>();

            container.Resolve<SimpleEventsExecutor>();
        }

        [Fact]
        public void Resolves_executors()
        {
            container.Resolve<ICommandExecutor<AppContext>>();
            container.Resolve<IQueryExecutor<AppContext>>();
        }
    }
}
