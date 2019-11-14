using System;

namespace LeanCode.CQRS.Validation
{
    /// <summary>
    /// Translates exceptions thrown by corresponding command handler into a
    /// <see cref="ValidationError" />, if applicable.
    /// </summary>
    /// <remarks>
    /// Exception translators are a last-resort option and their use should be tightly controlled.
    /// There are times when validation logic differs from actual logic in only "save" calls and
    /// extracting domain service responsible for the validation from the actual logic is not
    /// feasible because it would result in substantial code duplication or the performance penalty
    /// would be too high - that's the only reason why you should use exception translators.
    ///
    /// There are times when you need to consume some data first and there can be only one act of
    /// consumption of that data (e.g. OAuth2 code authorization or other single-usage tokens). One
    /// could be tempted to just throw when that happens and just translate the exception. This is
    /// wrong approach (@Fiołek: and I'm guilty of doing just that) - you don't really have
    /// `command` there - it does not fit in the CQRS world. The solution to that is to use other
    /// mechanisms (casual API with backing app/domain services), not exception-driven development.
    /// </remarks>
    public interface ICommandExceptionTranslator<in TAppContext, TCommand>
        where TAppContext : notnull
        where TCommand : ICommand
    {
        ValidationError? TryTranslate(Exception exception);
    }
}
