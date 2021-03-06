using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    // Source: https://github.com/IdentityServer/IdentityServer4.EntityFramework/blob/dev/src/IdentityServer4.EntityFramework/Stores/PersistedGrantStore.cs
    internal class PersistedGrantStore : IPersistedGrantStore
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PersistedGrantStore>();
        private readonly IPersistedGrantContext dbContext;

        public PersistedGrantStore(IPersistedGrantContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task StoreAsync(PersistedGrant token)
        {
            var existing = await dbContext.PersistedGrants
                .SingleOrDefaultAsync(x => x.Key == token.Key);

            if (existing is null)
            {
                logger.Debug("{PersistedGrantKey} not found in database", token.Key);

                dbContext.PersistedGrants.Add(PersistedGrantMapper.MapToEntity(token));
            }
            else
            {
                logger.Debug("{PersistedGrantKey} found in database", token.Key);

                PersistedGrantMapper.Map(existing, token);
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warning(ex, "Exception updating {PersistedGrantKey} persisted grant in database", token.Key);
            }
        }

        public async Task<PersistedGrant?> GetAsync(string key)
        {
            var persistedGrant = await dbContext.PersistedGrants
                .FirstOrDefaultAsync(x => x.Key == key);

            var model = PersistedGrantMapper.MapToModel(persistedGrant);

            logger.Debug(
                "{PersistedGrantKey} found in database: {PersistedGrantKeyFound}",
                key, model != null);

            return model;
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = await dbContext.PersistedGrants
                .Where(x => x.SubjectId == subjectId)
                .ToListAsync();

            var model = persistedGrants.Select(x => PersistedGrantMapper.MapToModel(x)).ToList();

            logger.Debug(
                "{PersistedGrantCount} persisted grants found for {SubjectId}",
                persistedGrants.Count, subjectId);

            return model;
        }

        public async Task RemoveAsync(string key)
        {
            var persistedGrant = await dbContext.PersistedGrants
                .FirstOrDefaultAsync(x => x.Key == key);

            if (persistedGrant is null)
            {
                logger.Debug("No {PersistedGrantKey} persisted grant found in database", key);
            }
            else
            {
                logger.Debug("Removing {PersistedGrantKey} persisted grant from database", key);

                dbContext.PersistedGrants.Remove(persistedGrant);

                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.Warning(ex, "Exception removing {PersistedGrantKey} persisted grant from database", key);
                }
            }
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            var persistedGrants = await dbContext.PersistedGrants
                .Where(x => x.SubjectId == subjectId && x.ClientId == clientId)
                .ToListAsync();

            logger.Debug(
                "Removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}",
                persistedGrants.Count, subjectId, clientId);

            dbContext.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warning(
                    ex,
                    "Exception removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}",
                    persistedGrants.Count, subjectId, clientId);
            }
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var persistedGrants = await dbContext.PersistedGrants
                .Where(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type)
                .ToListAsync();

            logger.Debug(
                "Removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}, grantType {PersistedGrantType}",
                persistedGrants.Count, subjectId, clientId, type);

            dbContext.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warning(
                    ex,
                    "Exception removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}, grantType {PersistedGrantType}",
                    persistedGrants.Count, subjectId, clientId, type);
            }
        }
    }
}
