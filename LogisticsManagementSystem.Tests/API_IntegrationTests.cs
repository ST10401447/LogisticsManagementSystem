using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using LogisticsManagementSystem.Api;
using LogisticsManagementSystem.Api.Data;
using Xunit;

namespace LogisticsManagementSystem.Tests
{
    public class API_IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public API_IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LogisticDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    services.AddDbContext<LogisticDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            }).CreateClient();
        }

        // Test 1: Check that the Contracts endpoint responds
        [Fact]
        public async Task GetContracts_ShouldRespond()
        {
            var response = await _client.GetAsync("/api/Contracts");
            Assert.NotNull(response);
        }

        // Test 2: Check that the Contracts response body is not null
        [Fact]
        public async Task GetContracts_ShouldReturnData()
        {
            var response = await _client.GetAsync("/api/Contracts");
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }

        // Test 3: Check that the Clients endpoint responds
        [Fact]
        public async Task GetClients_ShouldRespond()
        {
            var response = await _client.GetAsync("/api/Clients");
            Assert.NotNull(response);
        }

        // Test 4: Check that the Clients response body is not null
        [Fact]
        public async Task GetClients_ShouldReturnData()
        {
            var response = await _client.GetAsync("/api/Clients");
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }

        // Test 5: Check that the ServiceRequests endpoint responds
        [Fact]
        public async Task GetServiceRequests_ShouldRespond()
        {
            var response = await _client.GetAsync("/api/ServiceRequests");
            Assert.NotNull(response);
        }

        // Test 6: Check that the ServiceRequests response body is not null
        [Fact]
        public async Task GetServiceRequests_ShouldReturnData()
        {
            var response = await _client.GetAsync("/api/ServiceRequests");
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }

        // Test 7: Check that a contract that does not exist returns 404
        [Fact]
        public async Task GetContract_WhenNotFound_ShouldReturn404()
        {
            var response = await _client.GetAsync("/api/Contracts/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test 8: Check that a client that does not exist returns 404
        [Fact]
        public async Task GetClient_WhenNotFound_ShouldReturn404()
        {
            var response = await _client.GetAsync("/api/Clients/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}