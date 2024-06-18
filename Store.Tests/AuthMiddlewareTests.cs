using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StoreAPI.Middleware;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Store.Tests
{
    public class AuthMiddlewareTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public AuthMiddlewareTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                        {"Jwt:Key", "bCvicteAbPqsrbW2c0hpqfHgTclxEkIknLurkGB38TK7c41jcNvCw4P5Ej76uy38"},
                        {"Jwt:Issuer", "test"},
                        {"Jwt:Audience", "test"}
                        })
                        .Build());
                })
                .Configure(app =>
                {
                    app.UseMiddleware<AuthMiddleware>();
                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("Test response");
                    });
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Middleware_WithoutAuthorizationHeader_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Missing Authorization Header", responseString);
        }

        [Fact]
        public async Task Middleware_WithInvalidToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalidtoken");
            var response = await _client.GetAsync("/");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Unauthorized: Invalid Token", responseString);
        }

        [Fact]
        public async Task Middleware_WithValidToken_ReturnsOk()
        {
            var token = GenerateJwtToken();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync("/");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("Test response", responseString);
        }

        private string GenerateJwtToken()
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.ASCII.GetBytes("bCvicteAbPqsrbW2c0hpqfHgTclxEkIknLurkGB38TK7c41jcNvCw4P5Ej76uy38"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: "ExampleIssuer",
                audience: "ExampleAudience",
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
