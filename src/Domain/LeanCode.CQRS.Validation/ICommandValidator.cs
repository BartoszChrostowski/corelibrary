using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidator<in TAppContext, TCommand>
        where TAppContext : notnull
        where TCommand : ICommand
    {
        Task<ValidationResult> ValidateAsync(TAppContext appContext, TCommand command);
    }
}
