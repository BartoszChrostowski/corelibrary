using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public class SimpleEFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
          where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
          where TContext : DbContext
          where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task<TEntity> FindAsync(TIdentity id)
        {
            return dbSet.FindAsync(id);
        }
    }

    public class SimpleEFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
          where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
          where TContext : DbContext
          where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task<TEntity> FindAsync(Guid id)
        {
            return dbSet.FindAsync(id);
        }
    }
}