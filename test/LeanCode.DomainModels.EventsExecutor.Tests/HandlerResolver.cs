using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecutor.Tests
{
    class HandlerResolver : IDomainEventHandlerResolver
    {
        private readonly object[] handlers;

        public HandlerResolver(params object[] handlers)
        {
            this.handlers = handlers;
        }

        public IReadOnlyList<IDomainEventHandlerWrapper> FindEventHandlers(Type eventType)
        {
            var dehType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var wrapperCtor = typeof(Wrapper<>).MakeGenericType(eventType).GetConstructors()[0];

            return handlers
                .Where(h => dehType.IsAssignableFrom(h.GetType()))
                .Select(h => new object[] { h })
                .Select(h => (IDomainEventHandlerWrapper)wrapperCtor.Invoke(h))
                .ToList();
        }

        class Wrapper<TEvent> : IDomainEventHandlerWrapper
            where TEvent : IDomainEvent
        {
            private readonly IDomainEventHandler<TEvent> handler;

            public Type UnderlyingHandler { get; }

            public Wrapper(IDomainEventHandler<TEvent> handler)
            {
                this.handler = handler;
                UnderlyingHandler = handler.GetType();
            }

            public Task HandleAsync(IDomainEvent domainEvent)
            {
                return handler.HandleAsync((TEvent)domainEvent);
            }
        }
    }
}