using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Authentication.Internal.Tests
{
    public class InternalAuthenticationTests
    {
        [Fact]
        public async Task HeaderSourceAuthentication()
        {
            var userIdInHeader = "123";
            var userIdInQuery = "456";
            var server = await CreateServer(options =>
            {
                options.Source = ClaimSource.Header;
                options.AddSubjectMapping("UserId");
            });

            var header = new Dictionary<string, string>
            {
                {"UserId", userIdInHeader }
            };

            var url = $"http://example.com/auth?UserId={userIdInQuery}";
            var response = await SendAsync(server, url, header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(userIdInHeader, result);
        }

        [Fact]
        public async Task QuerySourceAuthentication()
        {
            var userIdInHeader = "123";
            var userIdInQuery = "456";
            var server = await CreateServer(options =>
            {
                options.Source = ClaimSource.Query;
                options.AddSubjectMapping("UserId");
            });

            var header = new Dictionary<string, string>
            {
                {"UserId", userIdInHeader }
            };

            var url = $"http://example.com/auth?UserId={userIdInQuery}";
            var response = await SendAsync(server, url, header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(userIdInQuery, result);
        }

        [Fact]
        public async Task NoHeaderAndQueryReceived()
        {
            var server = await CreateServer();
            var response = await SendAsync(server, "http://example.com/auth");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task NameInHeaderReceived()
        {
            var userId = "123";
            var userName = "zhangsan";
            var server = await CreateServer(options =>
            {
                options.Source = ClaimSource.HeaderAndQuery;
                options.AddSubjectMapping("UserId");
                options.AddNameMapping("UserName");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", userId },
                { "UserName", userName },
            };

            var response = await SendAsync(server, "http://example.com/name", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(userName, result);
        }

        [Fact]
        public async Task NameInQueryReceived()
        {
            var userId = "123";
            var userName = "zhangsan";
            var server = await CreateServer(options =>
            {
                options.Source = ClaimSource.HeaderAndQuery;
                options.AddSubjectMapping("UserId");
                options.AddNameMapping("UserName");
            });

            var url = $"http://example.com/name?UserId={userId}&UserName={userName}";
            var response = await SendAsync(server, url);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(userName, result);
        }

        [Fact]
        public async Task RolesReceived()
        {
            var userId = "123";
            var rolesInHeader = "admin";
            var rolesInQuery = "dev";
            var server = await CreateServer(options =>
            {
                options.AddSubjectMapping("UserId");
                options.AddRoleMapping("UserRole");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", "123" },
                { "UserRole", rolesInHeader },
            };

            var url = $"http://example.com/auth/admin,dev?UserId={userId}&UserRole={rolesInQuery}";
            var response = await SendAsync(server, url, header);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task NoRolesReceived()
        {
            var roles = "dev";
            var server = await CreateServer(options =>
            {
                options.AddSubjectMapping("UserId");
                options.AddRoleMapping("UserRole");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", "123" },
                { "UserRole", roles },
            };

            var response = await SendAsync(server, "http://example.com/auth/admin", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CustomFieldReceived()
        {
            var claimJson = "[{\"Type\":\"UserId\",\"Value\":\"123\"},{\"Type\":\"OrgId\",\"Value\":\"456\"}]";
            var server = await CreateServer(options =>
            {
                options.AddSubjectMapping("UserId");
                options.AddClaimMapping("OrgId");
            });

            var header = new Dictionary<string, string>
            {
                { "UserId", "123" },
                { "OrgId", "456" },
            };

            var response = await SendAsync(server, "http://example.com/claims", header);
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(claimJson, result);
        }

        private static async Task<TestServer> CreateServer(
            Action<InternalOptions> options = null)
        {
            var host = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddRouting();
                            services.AddAuthentication(InternalDefaults.AuthenticationScheme)
                                .AddInternal(options);
                        })
                        .Configure(app =>
                        {
                            app.UseRouting();
                            app.UseAuthentication();
                            app.UseTestEndpoints();
                        });

                }).Build();

            await host.StartAsync();
            return host.GetTestServer();
        }

        private static async Task<HttpResponseMessage> SendAsync(
            TestServer server,
            string uri,
            Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }
            var response = await server.CreateClient().SendAsync(request);

            return response;
        }
    }
}
