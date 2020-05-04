using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Inbox
{
    public interface IConsumedMessagesContext
    {
        DbSet<ConsumedMessage> ConsumedMessages { get; }
        Task SaveChangesAsync();
    }
}
