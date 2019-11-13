using System;
using System.Collections.Immutable;

namespace LeanCode.CQRS.Validation
{
    public abstract class DictionaryTranslator<TAppContext, TCommand> :
        ICommandExceptionTranslator<TAppContext, TCommand>
        where TAppContext : notnull
        where TCommand : ICommand
    {
        protected abstract ImmutableDictionary<Type, ErrorItem> Translations { get; }

        public ValidationError? TryTranslate(Exception exception)
        {
            if (Translations.TryGetValue(exception.GetType(), out var item))
            {
                return new ValidationError(string.Empty, item.Message, item.ErrorCode);
            }
            else
            {
                return null;
            }
        }

        protected sealed class Builder
        {
            private readonly ImmutableDictionary<Type, ErrorItem>.Builder inner = ImmutableDictionary.CreateBuilder<Type, ErrorItem>();

            public ImmutableDictionary<Type, ErrorItem> Build() => inner.ToImmutable();

            public Builder Translate<TException>(string msg, int errorCode)
            {
                inner.Add(typeof(TException), new ErrorItem(msg, errorCode));
                return this;
            }
        }

        protected struct ErrorItem
        {
            public string Message { get; }
            public int ErrorCode { get; }

            public ErrorItem(string message, int errorCode)
            {
                Message = message;
                ErrorCode = errorCode;
            }

            public override bool Equals(object? obj)
            {
                return obj is DictionaryTranslator<TAppContext, TCommand>.ErrorItem item &&
                       Message == item.Message &&
                       ErrorCode == item.ErrorCode;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Message, ErrorCode);
            }
        }
    }
}
