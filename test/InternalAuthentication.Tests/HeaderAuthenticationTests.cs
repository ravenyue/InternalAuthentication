using AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace AspNetCore.Tests
{
    public class HeaderAuthenticationTests
    {
        [Fact]
        public async Task HeaderValidation()
        {
            var userId = "123";
            var server = await CreateServer(options =>
            {
                options.SubjectHeaderName = new ClaimMapping("UserId");
            });

            var header = new Dictionary<string, string>
            {
                {"UserId", userId }
            };

            var response = await SendAsync(server, "http://example.com/oauth", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(userId, result);
        }

        [Fact]
        public async Task NoHeaderReceived()
        {
            var server = await CreateServer();
            var response = await SendAsync(server, "http://example.com/oauth");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task NameReceived()
        {
            var name = "zhangsan";
            var server = await CreateServer(options =>
            {
                options.SubjectHeaderName = new ClaimMapping("UserId");
                options.UserNameHeaderName = new ClaimMapping("UserName");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", "123" },
                { "UserName", name },
            };

            var response = await SendAsync(server, "http://example.com/name", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(name, result);
        }

        [Fact]
        public async Task RolesReceived()
        {
            var roles = "admin,dev";
            var server = await CreateServer(options =>
            {
                options.SubjectHeaderName = new ClaimMapping("UserId");
                options.UserRoleHeaderName = new ClaimMapping("UserRole");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", "123" },
                { "UserRole", roles },
            };

            var response = await SendAsync(server, "http://example.com/roles", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(roles, result);
        }

        [Fact]
        public async Task CustomFieldReceived()
        {
            var claimJson = "[{\"Type\":\"UserId\",\"Value\":\"123\"},{\"Type\":\"OrgId\",\"Value\":\"456\"}]";
            var server = await CreateServer(options =>
            {
                options.SubjectHeaderName = new ClaimMapping("UserId", "UserId");
                options.AddClaimMapping("OrgId");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", "123" },
                { "OrgId", "456" },
            };

            var response = await SendAsync(server, "http://example.com/custom", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(claimJson, result);
        }

        private static async Task<TestServer> CreateServer(Action<HeaderOptions> options = null)
        {
            var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services
                                .AddAuthentication(HeaderDefaults.AuthenticationScheme)
                                .AddHeader(options);
                        })
                        .Configure(app =>
                        {
                            app.UseAuthentication();
                            app.UseMiddleware<TestServerMiddleware>();
                        });

                }).StartAsync();

            return host.GetTestServer();
        }

        private static async Task<HttpResponseMessage> SendAsync(TestServer server, string uri, Dictionary<string, string> authorizationHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (authorizationHeader != null)
            {
                foreach (var pair in authorizationHeader)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }
            var response = await server.CreateClient().SendAsync(request);

            return response;
        }
    }
}
