using System.Threading.Tasks;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public interface IAuthorizer { }

    public class Authorizer : CustomAuthorizer<AppContext, IAuthorizer>
    {
        protected override Task<bool> CheckIfAuthorizedAsync(AppContext appContext, IAuthorizer obj)
        {
            return Task.FromResult(true);
        }
    }
}
