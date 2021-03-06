using System;
using Autofac;
using GreenPipes;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class PipeContextExtensions
    {
        public static TService GetService<TService>(this PipeContext pipe)
        {
            if (pipe.TryGetPayload<ILifetimeScope>(out var scope))
            {
                return scope.Resolve<TService>();
            }
            else
            {
                throw new InvalidOperationException("Cannot resolve service. ILifetimeScope not available for pipe context.");
            }
        }
    }
}
