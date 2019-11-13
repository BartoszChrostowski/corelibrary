using System;
using System.Collections.Immutable;

namespace LeanCode.CQRS.Validation
{
    public abstract class DictionaryTranslator<TAppContext, TCommand> : ICommandExceptionTranslator<TAppContext, TCommand>
        where TAppContext : notnull
        where TCommand : ICommand
    {
        protected abstract ImmutableDictionary<Type, int> Translations { get; }

        public ValidationError? TryTranslate(Exception exception)
        {
            if (Translations.TryGetValue(exception.GetType(), out var errorCode))
            {
                return new ValidationError(string.Empty, string.Empty, errorCode);
            }
            else
            {
                return null;
            }
        }

        public sealed class Builder
        {
            private readonly ImmutableDictionary<Type, int>.Builder inner = ImmutableDictionary.CreateBuilder<Type, int>();

            public ImmutableDictionary<Type, int> Build() => inner.ToImmutable();

            public Builder Translate<TException>(int errorCode)
            {
                inner.Add(typeof(TException), errorCode);
                return this;
            }
        }
    }
}
