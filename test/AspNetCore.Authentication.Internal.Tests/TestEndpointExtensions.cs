using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.Authentication.Internal.Tests
{
    public static class TestEndpointExtensions
    {
        public static IApplicationBuilder UseTestEndpoints(this IApplicationBuilder builder)
        {
            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/auth", async context =>
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
                });

                endpoints.MapGet("/name", async context =>
                {
                    await context.Response.WriteAsync(context.User.Identity.Name ?? "");
                });

                endpoints.MapGet("/auth/{roles}", context =>
                {
                    var roles = context.Request.RouteValues["roles"].ToString();
                    foreach (var role in roles.Split(','))
                    {
                        if (!context.User.IsInRole(role))
                        {
                            context.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                    }
                    context.Response.StatusCode = 200;
                    return Task.CompletedTask;
                });

                endpoints.MapGet("/claims", async context =>
                {
                    var claims = context.User.Claims.Select(c => new { c.Type, c.Value });
                    var json = JsonSerializer.Serialize(claims);
                    await context.Response.WriteAsync(json);
                });
            });

            return builder;
        }
    }
}
