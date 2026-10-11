using Claims.Auditing;
using Claims.Controllers;
using Claims.Core.Entities;
using Claims.Core.Enums;
using Claims.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Claims.Tests
{
    public class CoversControllerTests : BaseTests
    {
        private readonly CoversController _controller;
        public CoversControllerTests()
        {
            _controller = new CoversController(MockCoverRepo.Object, MockCalculator.Object, MockAuditer.Object, NullLogger<CoversController>.Instance);
        }

        #region GET
        [Fact]
        public async Task GetAsync_ShouldReturnOkWithAllCovers()
        {
            // ARRANGE
            var cover1 = ValidCoverBlueprint;

            var cover2 = ValidCoverBlueprint;
            cover2.Id = claimGuid;
            cover2.Type = CoverType.Yacht;

            var fakeCoversList = new List<Cover> { cover1, cover2 };

            MockCoverRepo.Setup(repo => repo.GetCoverAsync())
                .ReturnsAsync(fakeCoversList);

            // ACT
            var response = await _controller.GetAsync();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var covers = Assert.IsAssignableFrom<IEnumerable<Cover>>(okResult.Value);

            Assert.Equal(2, covers.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnOkWithCorrectCover_WhenIdExists()
        {
            // ARRANGE
            var expectedCover = ValidCoverBlueprint;
            expectedCover.Id = coverGuid;

            MockCoverRepo.Setup(repo => repo.GetCoverAsync(coverGuid))
                .ReturnsAsync(expectedCover);

            // ACT
            var response = await _controller.GetAsync(coverGuid);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var cover = Assert.IsType<Cover>(okResult.Value);

            Assert.Equal(coverGuid, cover.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // ARRANGE
            var nonExistentId = Guid.NewGuid().ToString();

            MockCoverRepo.Setup(repo => repo.GetCoverAsync(nonExistentId))
                .ReturnsAsync((Cover)null);

            // ACT
            var response = await _controller.GetAsync(nonExistentId);

            // ASSERT
            Assert.IsType<NotFoundResult>(response.Result);
        }
        #endregion

        #region CREATE
        [Fact]
        public async Task CreateAsync_ShouldReturnCreated_WhenCoverIsValid()
        {
            // ARRANGE
            var validCover = ValidCoverBlueprint;

            // ACT
            var response = await _controller.CreateAsync(validCover);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtRouteResult>(response.Result);
            var returnedCover = Assert.IsType<Cover>(createdResult.Value);

            Assert.Equal(validCover.Id, returnedCover.Id);

            MockCoverRepo.Verify(repo => repo.CreateCoverAsync(validCover), Times.Once);
            MockAuditer.Verify(auditer => auditer.AuditCover(validCover.Id, "POST"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenStartDateIsIntoThePast()
        {
            // ARRANGE
            var invalidCover = ValidCoverBlueprint;
            invalidCover.StartDate = invalidCover.StartDate.AddMonths(-3);

            // ACT
            var response = await _controller.CreateAsync(invalidCover);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("cannot be in the past", badRequestResult.Value.ToString());

            MockCalculator.Verify(c => c.ComputePremium(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CoverType>()), Times.Never);
            MockCoverRepo.Verify(repo => repo.CreateCoverAsync(It.IsAny<Cover>()), Times.Never);
            MockAuditer.Verify(auditer => auditer.AuditCover(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenTotalPeriodExceedsOneYear()
        {
            // ARRANGE
            var invalidCover = ValidCoverBlueprint;
            invalidCover.EndDate = invalidCover.StartDate.AddYears(1).AddDays(25);

            // ACT
            var response = await _controller.CreateAsync(invalidCover);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("cannot exceed 1 year", badRequestResult.Value.ToString());

            MockCoverRepo.Verify(repo => repo.CreateCoverAsync(It.IsAny<Cover>()), Times.Never);
            MockAuditer.Verify(auditer => auditer.AuditCover(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenStartDateIsAfterOrEqualAndEndDate()
        {
            // ARRANGE
            var invalidCover = ValidCoverBlueprint;
            invalidCover.StartDate = DateTime.UtcNow.AddDays(10);
            invalidCover.EndDate = DateTime.UtcNow;

            // ACT
            var response = await _controller.CreateAsync(invalidCover);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("start date must be earlier", badRequestResult.Value.ToString());

            MockCoverRepo.Verify(repo => repo.CreateCoverAsync(It.IsAny<Cover>()), Times.Never);
        }
        #endregion

        #region DELETE
        [Fact]
        public async Task DeleteAsync_ShouldReturnNoContent_WhenCoverIsDeletedSuccessfully()
        {
            // ARRANGE
            var targetCoverId = coverGuid;

            // ACT
            var response = await _controller.DeleteAsync(targetCoverId);

            // ASSERT
            Assert.IsType<NoContentResult>(response);

            MockCoverRepo.Verify(repo => repo.DeleteCoverAsync(targetCoverId), Times.Once);
            MockAuditer.Verify(auditer => auditer.AuditCover(targetCoverId, "DELETE"), Times.Once);
        }

        #endregion
    }
}
