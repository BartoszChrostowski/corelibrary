using System;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandExceptionTranslator<in TAppContext, TCommand>
        where TAppContext : notnull
        where TCommand : ICommand
    {
        ValidationError? TryTranslate(Exception exception);
    }
}
