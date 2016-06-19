using StockMan.Service.Rds;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Web.RestService.Filters
{
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {

            UserService service = new UserService();
            var user = service.Find(userName);

            if (user == null)
            {
                // No user with userName/password exists.
                return null;
            }
            if (user.password != password)
            {
                return null;
            }

            IList<Claim> claimCollection = new List<Claim>
                    {
                    new Claim(ClaimTypes.Name,userName), 
                    new Claim(ClaimTypes.Role, "user")
                    };


            ClaimsIdentity identity = new ClaimsIdentity(claimCollection,"Basic");
            return await Task.FromResult<IPrincipal>(new ClaimsPrincipal(identity));
        }

    }
}