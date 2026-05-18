using Moq;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;
using LogisticsManagementSystem.Services;   // Make sure this line is correct

namespace LogisticsManagementSystem.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetUsdToZarRateAsync_ReturnsValidRate()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        Rates = new Dictionary<string, decimal> { { "ZAR", 18.45m } }
                    }))
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new CurrencyService(httpClient);

            // Act
            var rate = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.True(rate > 0);
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_ReturnsFallbackRate_WhenApiFails()
        {
            // Arrange: Mock HttpClient to throw an exception (simulating API failure)
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API unavailable"));

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new CurrencyService(httpClient);

            // Act
            var rate = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.Equal(18.20m, rate);
        }
    }
}