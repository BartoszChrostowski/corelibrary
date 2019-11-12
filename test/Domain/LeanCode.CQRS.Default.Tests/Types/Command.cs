using System;
using System.Threading.Tasks;
using FluentValidation;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.DomainModels.Model;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public class Command : ICommand
    {
        public bool FailValidation { get; set; }
        public bool FailTranslation { get; set; }
        public bool FailCommand { get; set; }
        public bool RaiseEvent { get; set; }
    }

    [AuthorizeWhen(typeof(IAuthorizer))]
    public class SecuredCommand : ICommand, IAuthorizerPayload
    { }

    public class FluentCommandValidator : AbstractValidator<Command>
    {
        public FluentCommandValidator()
        {
            RuleFor(c => c.FailValidation)
                .Equal(false)
                    .WithCode(1);
        }
    }

    public class ExceptionTranslator : ICommandExceptionTranslator<AppContext, Command>
    {
        public ValidationError? TryTranslate(Exception exception)
        {
            if (exception is IndexOutOfRangeException)
            {
                return new ValidationError(string.Empty, string.Empty, 2);
            }
            else
            {
                return null;
            }
        }
    }

    public class CommandHandler : ICommandHandler<AppContext, Command>, ICommandHandler<AppContext, SecuredCommand>
    {
        public Task ExecuteAsync(AppContext context, Command command)
        {
            if (command.FailCommand)
            {
                throw new InvalidOperationException();
            }
            else if (command.FailTranslation)
            {
                throw new IndexOutOfRangeException();
            }
            else if (command.RaiseEvent)
            {
                DomainEvents.Raise(new DomainEvent());
                return Task.CompletedTask;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task ExecuteAsync(AppContext context, SecuredCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
