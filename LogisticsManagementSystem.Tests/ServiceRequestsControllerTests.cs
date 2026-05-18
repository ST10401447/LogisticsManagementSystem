using Xunit;

using LogisticsManagementSystem.Controllers;
using LogisticsManagementSystem.Data;
using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LogisticsManagementSystem.Tests
{
    public class ServiceRequestsControllerTests
    {
        private LogisticDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<LogisticDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new LogisticDbContext(options);

            context.Contracts.Add(new Contract
            {
                ContractID = 1,
                Status = "Active",
                ServiceLevel = "Premium",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10))
            });

            context.SaveChanges();

            return context;
        }

        private ServiceRequestsController GetController(LogisticDbContext context)
        {
            var httpClient = new HttpClient();

            var currencyService = new CurrencyService(httpClient);

            return new ServiceRequestsController(
                context,
                currencyService
            );
        }

        [Fact]
        public async Task Create_ValidServiceRequest_RedirectsToIndex()
        {
            // Arrange
            var context = GetDbContext();

            var controller = GetController(context);

            var request = new ServiceRequest
            {
                ContractID = 1,
                Description = "Truck Delivery",
                Cost = "100",
                Status = "Pending"
            };

            // Act
            var result = await controller.Create(request);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Create_ExpiredContract_ReturnsView()
        {
            // Arrange
            var context = GetDbContext();

            var expiredContract = new Contract
            {
                ContractID = 2,
                Status = "Expired",
                ServiceLevel = "Basic",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5))
            };

            context.Contracts.Add(expiredContract);

            context.SaveChanges();

            var controller = GetController(context);

            var request = new ServiceRequest
            {
                ContractID = 2,
                Description = "Delivery",
                Cost = "100",
                Status = "Pending"
            };

            // Act
            var result = await controller.Create(request);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingRequest_RedirectsToIndex()
        {
            // Arrange
            var context = GetDbContext();

            var request = new ServiceRequest
            {
                ServiceID = 1,
                ContractID = 1,
                Description = "Truck Delivery",
                Cost = "100",
                Status = "Pending"
            };

            context.ServiceRequests.Add(request);

            context.SaveChanges();

            var controller = GetController(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }
    }
}