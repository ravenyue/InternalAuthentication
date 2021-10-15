using AspNetCore.QueryAuthentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Xunit;
using System.Net;

namespace AspNetCore.Tests
{
    public class QueryAuthenticationTests
    {
        [Fact]
        public async Task QueryValidation()
        {
            var userId = "123";
            var server = await CreateServer(options =>
            {
                options.SubjectQueryName = new ClaimMapping("UserId");
            });

            var response = await server.CreateClient()
                .GetAsync($"http://example.com/oauth?UserId={userId}");
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(userId, result);
        }

        [Fact]
        public async Task NoQueryReceived()
        {
            var server = await CreateServer();
            var response = await server.CreateClient()
                .GetAsync($"http://example.com/oauth");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task NameReceived()
        {
            var name = "zhangsan";
            var server = await CreateServer(options =>
            {
                options.SubjectQueryName = new ClaimMapping("UserId");
                options.UserNameQueryName = new ClaimMapping("UserName");
            });

            var response = await server.CreateClient()
                .GetAsync($"http://example.com/name?UserId=123&UserName={name}");
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
                options.SubjectQueryName = new ClaimMapping("UserId");
                options.UserRoleQueryName = new ClaimMapping("UserRole");
            });

            var response = await server.CreateClient()
                .GetAsync($"http://example.com/roles?UserId=123&UserRole={roles}");
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
                options.SubjectQueryName = new ClaimMapping("UserId", "UserId");
                options.AddClaimMapping("OrgId");
            });

            var response = await server.CreateClient()
                .GetAsync($"http://example.com/custom?UserId=123&OrgId=456");
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(claimJson, result);
        }

        private static async Task<TestServer> CreateServer(Action<QueryOptions> options = null)
        {
            var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services
                                .AddAuthentication(QueryDefaults.AuthenticationScheme)
                                .AddQuery(options);
                        })
                        .Configure(app =>
                        {
                            app.UseAuthentication();
                            app.UseMiddleware<TestServerMiddleware>();
                        });

                }).StartAsync();

            return host.GetTestServer();
        }
    }
}
