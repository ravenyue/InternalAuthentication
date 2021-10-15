using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Authentication.Internal.Events
{
    public class InternalAuthenticatedContext : ResultContext<InternalOptions>
    {
        public InternalAuthenticatedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            InternalOptions options)
            : base(context, scheme, options) { }
    }
}