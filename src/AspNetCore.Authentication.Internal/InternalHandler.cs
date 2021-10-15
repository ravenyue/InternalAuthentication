using AspNetCore.Authentication.Internal.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore.Authentication.Internal
{
    public class InternalHandler : AuthenticationHandler<InternalOptions>
    {
        public InternalHandler(IOptionsMonitor<InternalOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }
        protected new InternalEvents Events
        {
            get => (InternalEvents)base.Events;
            set => base.Events = value;
        }
        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new InternalEvents());
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>();
            if ((Options.Source & ClaimSource.Header) == ClaimSource.Header)
            {
                var headerClaims = GetClaimsFromHeader();
                claims.AddRange(headerClaims);
            }
            if ((Options.Source & ClaimSource.Query) == ClaimSource.Query)
            {
                var queryClaims = GetClaimsFromQuery();
                claims.AddRange(queryClaims);
            }

            if (!claims.Any(x => x.Type == Options.SubjectClaimType))
            {
                return AuthenticateResult.NoResult();
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name,
                Options.NameClaimType,
                Options.RoleClaimType));

            var authenticatedContext = new InternalAuthenticatedContext(Context, Scheme, Options)
            {
                Principal = principal
            };
            await Events.OnAuthenticated(authenticatedContext);

            if (authenticatedContext.Result != null)
            {
                return authenticatedContext.Result;
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        private List<Claim> GetClaimsFromHeader()
        {
            var claims = new List<Claim>();
            foreach (var claimMap in Options.ClaimMaps)
            {
                var fieldValues = Request.Headers[claimMap.Key];
                var fields = fieldValues.ToClaims(
                    claimMap.Value,
                    Options.Delimiter);
                claims.AddRange(fields);
            }

            return claims;
        }

        private List<Claim> GetClaimsFromQuery()
        {
            var claims = new List<Claim>();
            foreach (var claimMap in Options.ClaimMaps)
            {
                var fieldValues = Request.Query[claimMap.Key];
                var fields = fieldValues.ToClaims(
                    claimMap.Value,
                    Options.Delimiter);
                claims.AddRange(fields);
            }

            return claims;
        }
    }
}
