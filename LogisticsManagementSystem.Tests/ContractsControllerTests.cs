using Xunit;
using LogisticsManagementSystem.Controllers;
using LogisticsManagementSystem.Data;
using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Moq;

using System;
using System.IO;
using System.Threading.Tasks;

namespace LogisticsManagementSystem.Tests
{
    public class ContractsControllerTests
    {
        private LogisticDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<LogisticDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LogisticDbContext(options);

            context.Clients.Add(new Client
            {
                ClientID = 1,
                Name = "Test Client",
                ContactNo = "+27123456789",
                Email = "test@example.com",
                Region = "Western Cape"
            });

            context.SaveChanges();

            return context;
        }

        private ContractsController GetController(LogisticDbContext context)
        {
            var fileValidationService = new FileValidationService();

            var webHostEnvironment = new Mock<IWebHostEnvironment>();

            webHostEnvironment.Setup(w => w.WebRootPath)
                .Returns("wwwroot");

            return new ContractsController(
                context,
                fileValidationService,
                webHostEnvironment.Object
            );
        }

        [Fact]
        public async Task Create_ValidContract_RedirectsToIndex()
        {
            // Arrange
            var context = GetDbContext();

            var controller = GetController(context);

            var contract = new Contract
            {
                ClientID = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                Status = "Active",
                ServiceLevel = "Premium"
            };

            var fileMock = new Mock<IFormFile>();
            var memoryStream = new MemoryStream();

            fileMock.Setup(f => f.FileName).Returns("agreement.pdf");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.Create(contract, fileMock.Object);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Create_InvalidDate_ReturnsView()
        {
            // Arrange
            var context = GetDbContext();

            var controller = GetController(context);

            var contract = new Contract
            {
                ClientID = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                Status = "Active",
                ServiceLevel = "Premium"
            };

            var fileMock = new Mock<IFormFile>();
            var memoryStream = new MemoryStream();

            fileMock.Setup(f => f.FileName).Returns("agreement.pdf");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await controller.Create(contract, fileMock.Object);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingContract_RedirectsToIndex()
        {
            // Arrange
            var context = GetDbContext();

            var contract = new Contract
            {
                ContractID = 1,
                ClientID = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                Status = "Active",
                ServiceLevel = "Premium"
            };

            context.Contracts.Add(contract);

            context.SaveChanges();

            var controller = GetController(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }
    }
}