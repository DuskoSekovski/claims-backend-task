using Claims.Auditing;
using Claims.Controllers;
using Claims.Core.Entities;
using Claims.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests : BaseTests
    {
        private readonly ClaimsController _controller;
        public ClaimsControllerTests()
        {
            _controller = new ClaimsController(NullLogger<ClaimsController>.Instance, MockClaimRepo.Object, MockAuditer.Object, MockCoverRepo.Object);
        }

        #region GET
        [Fact]
        public async Task GetAsync_ShouldReturnOkWithAllClaims()
        {
            // ARRANGE
            var claim1 = ValidClaimBlueprint;

            var claim2 = ValidClaimBlueprint;
            claim2.Id = coverGuid;
            claim2.Name = "Valid Test Claim 2";

            var fakeClaimsList = new List<Claim> { claim1, claim2 };

            MockClaimRepo.Setup(repo => repo.GetClaimsAsync())
                .ReturnsAsync(fakeClaimsList);

            // ACT
            var response = await _controller.GetAsync();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var claims = Assert.IsAssignableFrom<IEnumerable<Claim>>(okResult.Value);

            Assert.Equal(2, claims.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnOkWithCorrectClaim_WhenIdExists()
        {
            // ARRANGE
            var expectedClaim = ValidClaimBlueprint;
            expectedClaim.Id = claimGuid;

            MockClaimRepo.Setup(repo => repo.GetClaimAsync(claimGuid))
                .ReturnsAsync(expectedClaim);

            // ACT
            var response = await _controller.GetAsync(claimGuid);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var claim = Assert.IsType<Claim>(okResult.Value);

            Assert.Equal(claimGuid, claim.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // ARRANGE
            var nonExistentId = Guid.NewGuid().ToString();

            MockClaimRepo.Setup(repo => repo.GetClaimAsync(nonExistentId))
                .ReturnsAsync((Claim)null);

            // ACT
            var response = await _controller.GetAsync(nonExistentId);

            // ASSERT
            Assert.IsType<NotFoundResult>(response.Result);
        }
        #endregion

        #region CREATE
        [Fact]
        public async Task CreateAsync_ShouldReturnCreated_WhenClaimIsValidAndWithinCoverPeriod()
        {
            // ARRANGE
            var fakeCover = ValidCoverBlueprint;
            var validClaim = ValidClaimBlueprint;

            MockCoverRepo.Setup(repo => repo.GetCoverAsync(coverGuid))
                .ReturnsAsync(fakeCover);

            // ACT
            var response = await _controller.CreateAsync(validClaim);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtRouteResult>(response.Result);
            var returnedClaim = Assert.IsType<Claim>(createdResult.Value);

            Assert.Equal(validClaim.Id, returnedClaim.Id);

            MockClaimRepo.Verify(repo => repo.CreateClaimAsync(validClaim), Times.Once);
            MockAuditer.Verify(auditer => auditer.AuditClaim(validClaim.Id, "POST"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenRelatedCoverDoesNotExist()
        {
            // ARRANGE
            var invalidClaim = ValidClaimBlueprint;
            invalidClaim.CoverId = Guid.NewGuid().ToString();

            MockCoverRepo.Setup(repo => repo.GetCoverAsync(invalidClaim.CoverId))
                .ReturnsAsync((Cover)null);

            // ACT
            var response = await _controller.CreateAsync(invalidClaim);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("does not exist", badRequestResult.Value.ToString());

            MockClaimRepo.Verify(repo => repo.CreateClaimAsync(It.IsAny<Claim>()), Times.Never);
            MockAuditer.Verify(auditer => auditer.AuditClaim(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenClaimCreatedDateIsBeforeCoverStartDate()
        {
            // ARRANGE
            var fakeCover = ValidCoverBlueprint;
            fakeCover.StartDate = DateTime.UtcNow.AddMonths(1);

            var invalidClaim = ValidClaimBlueprint;

            MockCoverRepo.Setup(repo => repo.GetCoverAsync(coverGuid))
                .ReturnsAsync(fakeCover);

            // ACT
            var response = await _controller.CreateAsync(invalidClaim);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("must be between the related cover's period", badRequestResult.Value.ToString());

            MockClaimRepo.Verify(repo => repo.CreateClaimAsync(It.IsAny<Claim>()), Times.Never);
            MockAuditer.Verify(auditer => auditer.AuditClaim(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenClaimCreatedDateIsAfterCoverEndDate()
        {
            // ARRANGE
            var fakeCover = ValidCoverBlueprint;

            var invalidClaim = ValidClaimBlueprint;
            invalidClaim.Created = DateTime.UtcNow.AddMonths(4);

            MockCoverRepo.Setup(repo => repo.GetCoverAsync(coverGuid))
                .ReturnsAsync(fakeCover);

            // ACT
            var response = await _controller.CreateAsync(invalidClaim);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("must be between the related cover's period", badRequestResult.Value.ToString());

            MockClaimRepo.Verify(repo => repo.CreateClaimAsync(It.IsAny<Claim>()), Times.Never);
            MockAuditer.Verify(auditer => auditer.AuditClaim(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region DELETE
        [Fact]
        public async Task DeleteAsync_ShouldReturnNoContent_WhenClaimIsDeletedSuccessfully()
        {
            // ARRANGE
            var targetClaimId = claimGuid;

            // ACT
            var response = await _controller.DeleteAsync(targetClaimId);

            // ASSERT
            Assert.IsType<NoContentResult>(response);

            MockClaimRepo.Verify(repo => repo.DeleteClaimAsync(targetClaimId), Times.Once);
            MockAuditer.Verify(auditer => auditer.AuditClaim(targetClaimId, "DELETE"), Times.Once);
        }

        #endregion

    }
}
