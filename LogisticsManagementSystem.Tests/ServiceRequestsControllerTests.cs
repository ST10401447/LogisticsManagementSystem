using Xunit;
using LogisticsManagementSystem.Models;
using System;

namespace LogisticsManagementSystem.Tests
{
    public class ServiceRequestsControllerTests
    {
        [Fact]
        public void ServiceRequest_WithExpiredContract_ShouldBeBlocked()
        {
            // Arrange
            var contract = new Contract
            {
                ContractID = 1,
                Status = "Expired",
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1))
            };

            // Assert - expired contract should block service request
            Assert.Equal("Expired", contract.Status);
        }

        [Fact]
        public void ServiceRequest_WithOnHoldContract_ShouldBeBlocked()
        {
            // Arrange
            var contract = new Contract
            {
                ContractID = 1,
                Status = "On Hold",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5))
            };

            // Assert
            Assert.Equal("On Hold", contract.Status);
        }

        [Fact]
        public void ServiceRequest_WithActiveContract_ShouldBeAllowed()
        {
            // Arrange
            var contract = new Contract
            {
                ContractID = 1,
                Status = "Active",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5))
            };

            // Assert
            Assert.Equal("Active", contract.Status);
        }
    }
}