using System;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.IntegrationTestHelpers;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    /// <remarks>
    /// Single intergration test checking if events from command handler and further
    /// event handlers (consumers) are raised
    /// </remarks>
    public class MassTransitIntegrationTest : IClassFixture<TestApp>
    {
        private readonly TestApp testApp;

        public MassTransitIntegrationTest(TestApp testApp)
        {
            this.testApp = testApp;
        }

        [Fact]
        public async Task Test_event_relay_and_handling()
        {
            await PublishEvents();
            await TestEventsFromCommandHandler();
            await TestEventsFromConsumers();
            await TestFailingHandlers();
        }

        private async Task PublishEvents()
        {
            var ctx = new Context { CorrelationId = testApp.CorrelationId };
            var cmd = new TestCommand();
            await testApp.Commands.RunAsync(ctx, cmd);
        }

        private async Task TestEventsFromCommandHandler()
        {
            await WaitForConsumers();
            var handled = testApp.HandledEvents<Event1>();

            var evt = Assert.Single(handled);
            AssertConsumed(evt, typeof(FirstEvent1Consumer));
        }

        private async Task TestEventsFromConsumers()
        {
            await WaitForConsumers();
            var handled = testApp.HandledEvents<Event2>();

            Assert.Collection(
                handled.OrderBy(x => x.ConsumerType.FullName),
                e => AssertConsumed(e, typeof(Event2FirstConsumer)),
                e => AssertConsumed(e, typeof(Event2SecondConsumer)));
        }

        private async Task TestFailingHandlers()
        {
            await Task.Delay(5_500);

            var handled = testApp.HandledEvents<Event3>();
            var evt = Assert.Single(handled);

            AssertConsumed(evt, typeof(Event3RetryingConsumer));
        }

        private void AssertConsumed(HandledEvent evt, Type consumerType)
        {
            Assert.Equal(consumerType, evt.ConsumerType);
            Assert.Equal(testApp.CorrelationId, evt.CorrelationId);
        }

        private Task WaitForConsumers() => Task.Delay(500);
    }
}
