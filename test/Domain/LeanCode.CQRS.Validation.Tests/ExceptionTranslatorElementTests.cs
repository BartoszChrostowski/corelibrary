using System;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Validation.Tests
{
    public class ExceptionTranslatorElementTests
    {
        private readonly ICommandExceptionTranslatorResolver<AppContext> resolver;
        private readonly ICommandExceptionTranslatorWrapper translator;

        public ExceptionTranslatorElementTests()
        {
            resolver = Substitute.For<ICommandExceptionTranslatorResolver<AppContext>>();
            translator = Substitute.For<ICommandExceptionTranslatorWrapper>();

            resolver.FindCommandExceptionTranslator(typeof(Command)).Returns(translator);
            translator.TryTranslate(default!).ReturnsForAnyArgs((ValidationError?)null);
        }

        [Fact]
        public async Task Correctly_short_circuits_next_execution_if_translator_is_not_available()
        {
            WithoutTranslator();

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
        public async Task Passes_the_context_and_command_to_the_rest_of_the_pipeline_if_the_translator_is_available()
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

        [Fact]
        public async Task If_the_rest_of_the_pipeline_executes_successfully_The_translator_is_not_called()
        {
            await ExecuteAsync();

            translator.DidNotReceiveWithAnyArgs().TryTranslate(default!);
        }

        [Fact]
        public async Task If_the_pipeline_throws_the_translator_gets_called_with_correct_exception()
        {
            var ex = new Exception();

            await Assert.ThrowsAsync<Exception>(() => ExecuteAsync(ex));

            translator.Received(1).TryTranslate(ex);
        }

        [Fact]
        public async Task If_the_rest_of_the_pipeline_throws_known_exception_it_is_translated_to_correct_result()
        {
            var error = new ValidationError(string.Empty, "Handler failed", 1);
            TranslateTo<CustomException>(error);

            var res = await ExecuteAsync(new CustomException());

            Assert.NotNull(res.Result);
            Assert.False(res.Result!.WasSuccessful);
            var e = Assert.Single(res.Result!.ValidationErrors);
            Assert.Equal(error, e);
        }

        [Fact]
        public async Task If_the_exception_is_not_known_it_is_rethrown()
        {
            await Assert.ThrowsAsync<Exception>(() => ExecuteAsync(new Exception()));
        }

        private void WithoutTranslator()
        {
            resolver.FindCommandExceptionTranslator(default!).ReturnsForAnyArgs((ICommandExceptionTranslatorWrapper?)null);
        }

        private void TranslateTo<TException>(ValidationError error)
        {
            translator.TryTranslate(Arg.Is<Exception>(e => e.GetType() == typeof(TException))).Returns(error);
        }

        private Task<ExecutionResult> ExecuteAsync() => ExecuteAsync(CommandResult.Success);

        private Task<ExecutionResult> ExecuteAsync(CommandResult nextResult)
        {
            return ExecuteAsync(new AppContext(), new Command(), nextResult);
        }

        private Task<ExecutionResult> ExecuteAsync(Exception nextResult)
        {
            return ExecuteAsync(new AppContext(), new Command(), nextResult);
        }

        private async Task<ExecutionResult> ExecuteAsync(AppContext context, Command cmd, CommandResult nextResult)
        {
            var call = new ExecutionResult();
            var element = new ExceptionTranslatorElement<AppContext>(resolver);
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

        private async Task<ExecutionResult> ExecuteAsync(AppContext context, Command cmd, Exception nextResult)
        {
            var call = new ExecutionResult();
            var element = new ExceptionTranslatorElement<AppContext>(resolver);
            call.Result = await element.ExecuteAsync(
                context,
                cmd,
                (ctx2, cmd2) => throw nextResult);
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

    public class CustomException : Exception { }
}
