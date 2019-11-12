using System;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Wrappers
{
    internal class CommandExceptionTranslatorWrapper<TAppContext, TCommand> : ICommandExceptionTranslatorWrapper
        where TAppContext : notnull
        where TCommand : ICommand
    {
        private readonly ICommandExceptionTranslator<TAppContext, TCommand> translator;

        public CommandExceptionTranslatorWrapper(ICommandExceptionTranslator<TAppContext, TCommand> translator)
        {
            this.translator = translator;
        }

        public ValidationError? TryTranslate(Exception ex) => translator.TryTranslate(ex);
    }
}
