using System;
using System.Linq;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacValidatorResolver<TAppContext> : ICommandValidatorResolver<TAppContext>
    {
        private static readonly Type AppContextType = typeof(TAppContext);

        private static readonly Type ValidatorBase = typeof(ICommandValidator<,>);
        private static readonly Type WrapperBase = typeof(CommandValidatorWrapper<,>);

        private static readonly TypesCache typesCache = new TypesCache(GetTypes, ValidatorBase, WrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacValidatorResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandValidatorWrapper FindCommandValidator(Type commandType)
        {
            var (handlerType, constructor) = typesCache.Get(commandType);
            if (componentContext.TryResolve(handlerType, out var handler))
            {
                var cv = constructor.Invoke(new[] { handler });
                return (ICommandValidatorWrapper)cv;
            }
            else
            {
                return null;
            }
        }

        private static Type[] GetTypes(Type commandType)
        {
            return new[] { AppContextType, commandType };
        }
    }
}
