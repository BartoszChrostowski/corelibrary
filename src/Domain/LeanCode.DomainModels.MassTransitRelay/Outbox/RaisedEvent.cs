using System;
using LeanCode.DomainModels.Model;
using LeanCode.IdentityProvider;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public class RaisedEvent
    {
        public const int MaxEventTypeLength = 500;

        public Guid Id { get; private set; }
        public Guid CorrelationId { get; private set; }
        public DateTime DateOcurred { get; private set; }
        public bool WasPublished { get; private set; }
        public string EventType { get; private set; }
        public string Payload { get; private set; }

        public void SetWasPublished(bool published)
        {
            WasPublished = published;
        }

        public static RaisedEvent Create(object evt, Guid correlationId, Func<object, string> evtSerializer)
        {
            var raisedEvt = new RaisedEvent
            {
                Payload = evtSerializer(evt),
                EventType = evt.GetType().FullName!,
                CorrelationId = correlationId,
            };

            if (evt is IDomainEvent domainEvent)
            {
                raisedEvt.Id = domainEvent.Id;
                raisedEvt.DateOcurred = domainEvent.DateOccurred;
            }
            else
            {
                raisedEvt.Id = Identity.NewId();
                raisedEvt.DateOcurred = Time.Now;
            }

            return raisedEvt;
        }

        private RaisedEvent()
        {
            EventType = null!;
            Payload = null!;
        }

        public static void Configure(ModelBuilder builder)
        {
            builder.Entity<RaisedEvent>(cfg =>
            {
                cfg.HasKey(e => e.Id).IsClustered(false);
                cfg.HasIndex(e => new { e.DateOcurred, e.WasPublished }).IsClustered(true);

                cfg.Property(e => e.Id)
                    .ValueGeneratedNever();

                cfg.Property(e => e.EventType)
                    .HasMaxLength(MaxEventTypeLength);
            });
        }
    }
}