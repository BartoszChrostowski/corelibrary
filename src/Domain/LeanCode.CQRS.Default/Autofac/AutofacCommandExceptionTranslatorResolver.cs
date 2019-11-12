using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Autofac
{
    internal class AutofacCommandExceptionTranslatorResolver<TAppContext> : ICommandExceptionTranslatorResolver<TAppContext>
        where TAppContext : notnull
    {
        private static readonly Type AppContextType = typeof(TAppContext);

        private static readonly Type TranslatorBase = typeof(ICommandExceptionTranslator<,>);
        private static readonly Type TranslatorWrapperBase = typeof(CommandExceptionTranslatorWrapper<,>);
        private static readonly TypesCache TypesCache = new TypesCache(GetTypes, TranslatorBase, TranslatorWrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacCommandExceptionTranslatorResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandExceptionTranslatorWrapper? FindCommandExceptionTranslator(Type commandType)
        {
            var (handlerType, constructor) = TypesCache.Get(commandType);

            if (componentContext.TryResolve(handlerType, out var handler))
            {
                var wrapper = constructor.Invoke(new[] { handler });

                return (ICommandExceptionTranslatorWrapper)wrapper;
            }
            else
            {
                return null;
            }
        }

        private static Type[] GetTypes(Type commandType) =>
            new[] { AppContextType, commandType };
    }
}
