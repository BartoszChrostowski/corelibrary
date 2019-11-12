using System.Threading.Tasks;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Tests.Types
{
    public interface IAuthorizerPayload { }
    public interface IAuthorizer { }

    public class Authorizer : CustomAuthorizer<AppContext, IAuthorizerPayload>, IAuthorizer
    {
        protected override Task<bool> CheckIfAuthorizedAsync(AppContext appContext, IAuthorizerPayload obj)
        {
            return Task.FromResult(false);
        }
    }
}
