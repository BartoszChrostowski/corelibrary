using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation
{
    public class ExceptionTranslatorElement<TAppContext> : IPipelineElement<TAppContext, ICommand, CommandResult>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ExceptionTranslatorElement<TAppContext>>();

        private readonly ICommandExceptionTranslatorResolver<TAppContext> resolver;

        public ExceptionTranslatorElement(ICommandExceptionTranslatorResolver<TAppContext> resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            TAppContext appContext,
            ICommand payload,
            Func<TAppContext, ICommand, Task<CommandResult>> next)
        {
            var commandType = payload.GetType();
            var translator = resolver.FindCommandExceptionTranslator(commandType);

            if (translator is null)
            {
                return await next(appContext, payload);
            }
            else
            {
                try
                {
                    return await next(appContext, payload);
                }
                catch (Exception ex) when (translator.TryTranslate(ex) is ValidationError err)
                {
                    logger.Information(
                        "Command {@Command} failed execution with exception that has been translated to {@Error}",
                        commandType, err);
                    return CommandResult.NotValid(new ValidationResult(new[] { err }));
                }
            }
        }
    }

    public static partial class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TAppContext, ICommand, CommandResult> TranslateExceptions<TAppContext>(
            this PipelineBuilder<TAppContext, ICommand, CommandResult> builder)
            where TAppContext : IPipelineContext
        {
            return builder.Use<ExceptionTranslatorElement<TAppContext>>();
        }
    }
}
