using Autofac;
using Autofac.Features.Variance;
using LeanCode.Components;
using LeanCode.CQRS.Default.Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default
{
    internal class TypedCQRSModule<TAppContext> : Module
        where TAppContext : IPipelineContext
    {
        private readonly TypesCatalog catalog;
        private readonly CommandBuilder<TAppContext> cmdBuilder;
        private readonly QueryBuilder<TAppContext> queryBuilder;

        public TypedCQRSModule(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> cmdBuilder,
            QueryBuilder<TAppContext> queryBuilder)
        {
            this.catalog = catalog;
            this.cmdBuilder = cmdBuilder;
            this.queryBuilder = queryBuilder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(ICommandHandler<,>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IQueryHandler<,,>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IDomainEventHandler<>));

            builder.RegisterType<AutofacCommandHandlerResolver<TAppContext>>().As<ICommandHandlerResolver<TAppContext>>();
            builder.RegisterType<AutofacQueryHandlerResolver<TAppContext>>().As<IQueryHandlerResolver<TAppContext>>();
            builder.RegisterType<AutofacAuthorizerResolver<TAppContext>>().As<IAuthorizerResolver<TAppContext>>();
            builder.RegisterType<AutofacValidatorResolver<TAppContext>>().As<ICommandValidatorResolver<TAppContext>>();

            builder.Register(c =>
                new CommandExecutor<TAppContext>(
                    c.Resolve<IPipelineFactory>(),
                    cmdBuilder))
                .As<ICommandExecutor<TAppContext>>()
                .SingleInstance();

            builder.Register(c =>
                new QueryExecutor<TAppContext>(
                    c.Resolve<IPipelineFactory>(),
                    queryBuilder))
                .As<IQueryExecutor<TAppContext>>()
                .SingleInstance();
        }
    }
}
