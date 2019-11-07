using System;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandExceptionTranslatorResolver<in TAppContext>
        where TAppContext : notnull
    {
        ICommandExceptionTranslatorWrapper? FindCommandExceptionTranslator(Type commandType);
    }

    public interface ICommandExceptionTranslatorWrapper
    {
        ValidationError? TryTranslate(Exception ex);
    }
}
