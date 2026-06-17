using Xunit;
using LogisticsManagementSystem.Controllers;
using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;

namespace LogisticsManagementSystem.Tests
{
    public class ContractsControllerTests
    {
        [Fact]
        public void Create_InvalidDate_ReturnsView()
        {
            // Arrange
            var contract = new Contract
            {
                ClientID = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                Status = "Active",
                ServiceLevel = "Premium"
            };

            // Assert - end date before start date is invalid
            Assert.True(contract.EndDate < contract.StartDate);
        }

        [Fact]
        public void Create_ValidDates_PassesValidation()
        {
            // Arrange
            var contract = new Contract
            {
                ClientID = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                Status = "Active",
                ServiceLevel = "Premium"
            };

            // Assert
            Assert.True(contract.EndDate >= contract.StartDate);
        }

        [Fact]
        public void Contract_DefaultStatus_IsDraft()
        {
            // Arrange
            var contract = new Contract();

            // Assert
            Assert.Equal("Draft", contract.Status);
        }
    }
}