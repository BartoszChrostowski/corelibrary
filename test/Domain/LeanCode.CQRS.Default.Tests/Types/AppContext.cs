using System;
using System.Collections.Generic;
using System.Security.Claims;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public class AppContext : IPipelineContext, ISecurityContext, IEventsContext
    {
        public IPipelineScope? Scope { get; set; }

        public ClaimsPrincipal User { get; } = null!;

        public List<IDomainEvent> SavedEvents { get; set; } = null!;
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; } = null!;
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; } = null!;
    }
}
