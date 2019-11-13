using System;
using System.Collections.Immutable;
using Xunit;

namespace LeanCode.CQRS.Validation.Tests
{
    public class DictionaryTranslatorTests
    {
        [Fact]
        public void If_it_knows_the_exception_type_It_is_translated_to_empty_error_with_proper_code()
        {
            var res = new Translator().TryTranslate(new Exception1());

            Assert.NotNull(res);
            Assert.Equal(string.Empty, res!.PropertyName);
            Assert.Equal(string.Empty, res!.ErrorMessage);
            Assert.Equal(1, res!.ErrorCode);
        }

        [Fact]
        public void If_it_does_not_know_the_exception_type_It_returns_null()
        {
            var res = new Translator().TryTranslate(new Exception());

            Assert.Null(res);
        }

        [Fact]
        public void If_it_knows_about_base_exception_It_is_ignored()
        {
            var res = new Translator().TryTranslate(new Exception2());

            Assert.Null(res);
        }

        private class Translator : DictionaryTranslator<AppContext, ICommand>
        {
            protected override ImmutableDictionary<Type, int> Translations { get; } = new Builder()
                .Translate<Exception1>(1)
                .Build();
        }

        private class Exception1 : Exception { }
        private class Exception2 : Exception1 { }
    }
}
