using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Validation.Tests
{
    public class ValidationElementTests
    {
        private readonly ICommandValidatorResolver<AppContext> resolver;
        private readonly ICommandValidatorWrapper validator;

        public ValidationElementTests()
        {
            resolver = Substitute.For<ICommandValidatorResolver<AppContext>>();
            validator = Substitute.For<ICommandValidatorWrapper>();

            resolver.FindCommandValidator(typeof(Command)).Returns(validator);
            validator.ValidateAsync(default!, default!).ReturnsForAnyArgs(new ValidationResult(new ValidationError[0]));
        }

        [Fact]
        public async Task Correctly_short_circuits_next_execution_if_validator_is_not_available()
        {
            WithoutValidator();

            var cmd = new Command();
            var ctx = new AppContext();
            var nextResult = new CommandResult(new ValidationError[0]);

            var res = await ExecuteAsync(ctx, cmd, nextResult);

            Assert.True(res.NextCalled);
            Assert.Equal(ctx, res.Context);
            Assert.Equal(cmd, res.Command);
            Assert.Equal(nextResult, res.Result);
        }

        [Fact]
        public async Task Passes_the_context_and_command_to_the_validator()
        {
            var cmd = new Command();
            var ctx = new AppContext();

            await ExecuteAsync(ctx, cmd, CommandResult.Success);

            _ = validator.Received(1).ValidateAsync(ctx, cmd);
        }

        [Fact]
        public async Task If_the_validation_fails_does_not_call_next()
        {
            var err1 = new ValidationError("Property", "Msg", 1);
            InvalidCommand(err1);

            var res = await ExecuteAsync();

            Assert.False(res.NextCalled);
        }

        [Fact]
        public async Task If_the_validation_fails_correctly_wraps_the_result()
        {
            var err1 = new ValidationError("Property", "Msg", 1);
            InvalidCommand(err1);

            var res = await ExecuteAsync();

            Assert.NotNull(res.Result);
            Assert.False(res.Result!.WasSuccessful);
            var e = Assert.Single(res.Result!.ValidationErrors);
            Assert.Equal(e, err1);
        }

        [Fact]
        public async Task If_validation_succeeds_the_next_is_called_correctly()
        {
            var cmd = new Command();
            var ctx = new AppContext();
            var nextResult = new CommandResult(new ValidationError[0]);

            var res = await ExecuteAsync(ctx, cmd, nextResult);

            Assert.True(res.NextCalled);
            Assert.Equal(ctx, res.Context);
            Assert.Equal(cmd, res.Command);
            Assert.Equal(nextResult, res.Result);
        }

        private void WithoutValidator()
        {
            resolver.FindCommandValidator(default!).ReturnsForAnyArgs((ICommandValidatorWrapper?)null);
        }

        private void InvalidCommand(params ValidationError[] errors)
        {
            validator.ValidateAsync(default!, default!).ReturnsForAnyArgs(new ValidationResult(errors));
        }

        private Task<ExecutionResult> ExecuteAsync() => ExecuteAsync(CommandResult.Success);

        private Task<ExecutionResult> ExecuteAsync(CommandResult nextResult)
        {
            return ExecuteAsync(new AppContext(), new Command(), nextResult);
        }

        private async Task<ExecutionResult> ExecuteAsync(AppContext context, Command cmd, CommandResult nextResult)
        {
            var call = new ExecutionResult();
            var element = new ValidationElement<AppContext>(resolver);
            call.Result = await element.ExecuteAsync(
                context,
                cmd,
                (ctx2, cmd2) =>
                {
                    call.Context = ctx2;
                    call.Command = cmd2;
                    call.NextCalled = true;
                    return Task.FromResult(nextResult);
                });
            return call;
        }

        private class ExecutionResult
        {
            public bool NextCalled { get; set; }

            public AppContext? Context { get; set; }
            public ICommand? Command { get; set; }

            public CommandResult? Result { get; set; }
        }
    }
}
