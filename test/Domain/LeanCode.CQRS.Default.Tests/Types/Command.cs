using System;
using System.Threading.Tasks;
using FluentValidation;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;
using LeanCode.CQRS.Validation.Fluent;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public class Command : ICommand { public bool Fail { get; set; } }

    public class FluentCommandValidator : AbstractValidator<Command>
    {
        public FluentCommandValidator()
        {
            RuleFor(c => c.Fail)
                .Equal(false)
                    .WithCode(1);
        }
    }

    public class ExceptionTranslator : ICommandExceptionTranslator<AppContext, Command>
    {
        public ValidationError? TryTranslate(Exception exception)
        {
            return null;
        }
    }

    public class CommandHandler : ICommandHandler<AppContext, Command>
    {
        public Task ExecuteAsync(AppContext context, Command command)
        {
            return Task.CompletedTask;
        }
    }
}
