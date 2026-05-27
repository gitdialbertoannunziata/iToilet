using System.Net;
using System.Net.Http.Json;
using Dapr.Client;
using LocatorApi.Controllers;
using LocatorApi.Entities;
using LocatorApi.Models;
using LocatorApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocatorApi.Tests.Controllers
{
    public class LocatorControllerTest
    {
        [Fact]
        public async Task Around_ReturnsOk_WithReviews_WhenReviewServiceReturnsOk()
        {
            // Arrange
            var repositoryMock = new Mock<IToiletRepository>();
            var daprClientMock = new Mock<DaprClient>();
            var loggerMock = new Mock<ILogger<LocatorController>>();

            var city = "Milan";
            var lat = 45.4642;
            var lng = 9.1900;

            var id = Guid.NewGuid();
            var toilets = new List<Toilet>
            {
                new Toilet { Id = id, City = city, Name = "Toilet 1", Address = "Address 1", Photo = "photo1.jpg" }
            };

            var reviews = new List<Review> { new Review() };

            repositoryMock
                .Setup(r => r.GetNearestAsync(city, lat, lng, It.IsAny<CancellationToken>()))
                .ReturnsAsync(toilets);

            daprClientMock
                .Setup(d => d.InvokeMethodWithResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(reviews)
                });

            var sut = CreateSut(daprClientMock.Object, repositoryMock.Object, loggerMock.Object);

            // Act
            var result = await sut.Around(city, lat, lng);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<List<ToiletWithReview>>(ok.Value);
            var item = Assert.Single(payload);
            Assert.NotNull(item.Reviews);
            Assert.Single(item.Reviews!);
        }

        [Fact]
        public async Task Around_ReturnsOk_WithoutReviews_WhenReviewServiceReturnsNonOk()
        {
            // Arrange
            var repositoryMock = new Mock<IToiletRepository>();
            var daprClientMock = new Mock<DaprClient>();
            var loggerMock = new Mock<ILogger<LocatorController>>();

            var city = "Rome";
            var lat = 41.9028;
            var lng = 12.4964;

            var id = Guid.NewGuid();
            var toilets = new List<Toilet>
            {
                new Toilet { Id = id, City = city, Name = "Toilet 2", Address = "Address 2", Photo = "photo2.jpg" }
            };

            repositoryMock
                .Setup(r => r.GetNearestAsync(city, lat, lng, It.IsAny<CancellationToken>()))
                .ReturnsAsync(toilets);

            daprClientMock
                .Setup(d => d.InvokeMethodWithResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var sut = CreateSut(daprClientMock.Object, repositoryMock.Object, loggerMock.Object);

            // Act
            var result = await sut.Around(city, lat, lng);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<List<ToiletWithReview>>(ok.Value);
            var item = Assert.Single(payload);
            Assert.True(item.Reviews is null || !item.Reviews.Any());
        }

        [Fact]
        public async Task Around_ReturnsOk_EmptyList_WhenNoNearestToilets()
        {
            // Arrange
            var repositoryMock = new Mock<IToiletRepository>();
            var daprClientMock = new Mock<DaprClient>();
            var loggerMock = new Mock<ILogger<LocatorController>>();

            var city = "Turin";
            var lat = 45.0703;
            var lng = 7.6869;

            repositoryMock
                .Setup(r => r.GetNearestAsync(city, lat, lng, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Toilet>());

            var sut = CreateSut(daprClientMock.Object, repositoryMock.Object, loggerMock.Object);

            // Act
            var result = await sut.Around(city, lat, lng);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<List<ToiletWithReview>>(ok.Value);
            Assert.Empty(payload);

            daprClientMock.Verify(
                d => d.InvokeMethodWithResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static LocatorController CreateSut(
            DaprClient daprClient,
            IToiletRepository repository,
            ILogger<LocatorController> logger)
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            return new LocatorController(daprClient, repository, httpContextAccessor, logger);
        }
    }
}