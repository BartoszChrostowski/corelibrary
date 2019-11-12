using System;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public class DomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime DateOccurred { get; }
    }

    public class DomainEventHandler : IDomainEventHandler<DomainEvent>
    {
        public bool Handled { get; private set; }

        public Task HandleAsync(DomainEvent domainEvent)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }
}
