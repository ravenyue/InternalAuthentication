using AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.Tests
{
    public class TestServerMiddleware
    {
        private readonly RequestDelegate _next;

        public TestServerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == new PathString("/oauth"))
            {
                if (context.User == null ||
                    context.User.Identity == null ||
                    !context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
               
                var identifier = context.User.FindFirst("UserId");
                if (identifier == null)
                {
                    context.Response.StatusCode = 500;
                    return;
                }

                await context.Response.WriteAsync(identifier.Value);
            }
            else if (context.Request.Path == new PathString("/name"))
            {
                await context.Response.WriteAsync(context.User.Identity.Name ?? "");
            }
            else if (context.Request.Path == new PathString("/roles"))
            {
                var rolesStr = "";
                var identities = context.User.Identities.ToList();
                if (identities[0] != null)
                {
                    var roles = identities[0].Claims
                        .Where(c => c.Type == identities[0].RoleClaimType)
                        .Select(c => c.Value)
                        .ToList();
                    rolesStr = string.Join(',', roles);
                }
                await context.Response.WriteAsync(rolesStr);
            }
            else if (context.Request.Path == new PathString("/custom"))
            {
                var claims = context.User.Claims.Select(c => new { c.Type, c.Value });

                var json = JsonSerializer.Serialize(claims);

                await context.Response.WriteAsync(json);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
