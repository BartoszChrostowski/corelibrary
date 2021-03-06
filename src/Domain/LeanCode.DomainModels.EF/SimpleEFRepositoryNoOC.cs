using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public sealed class SimpleEFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
        where TIdentity : notnull, IEquatable<TIdentity>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override Task<TEntity?> FindAsync(TIdentity id) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id))!; // TODO: remove ! when EF Core adds support for NRT
    }

    public sealed class SimpleEFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override Task<TEntity?> FindAsync(Guid id) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id == id)!; // TODO: remove ! when EF Core adds support for NRT
    }
}
