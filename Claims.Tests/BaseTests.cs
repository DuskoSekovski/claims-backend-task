using Claims.Auditing;
using Claims.Core.Entities;
using Claims.Core.Enums;
using Claims.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Tests
{
    public class BaseTests
    {
        protected readonly Mock<IClaimRepository> MockClaimRepo;
        protected readonly Mock<ICoverRepository> MockCoverRepo;
        protected readonly Mock<IAuditer> MockAuditer;
        protected readonly Mock<ICoverCalculator> MockCalculator;

        protected BaseTests()
        {
            MockClaimRepo = new Mock<IClaimRepository>();
            MockCoverRepo = new Mock<ICoverRepository>();
            MockAuditer = new Mock<IAuditer>();
            MockCalculator = new Mock<ICoverCalculator>();
        }

        protected readonly string coverGuid = "11111111-1111-1111-1111-111111111111";
        protected readonly string claimGuid = "22222222-2222-2222-2222-222222222222";

        protected Cover ValidCoverBlueprint => new()
        {
            Id = coverGuid,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddMonths(3),
            Type = CoverType.ContainerShip,
            Premium = 500m
        };

        protected Claim ValidClaimBlueprint => new()
        {
            Id = claimGuid,
            CoverId = coverGuid,
            Created = DateTime.UtcNow.AddDays(5),
            Name = "Valid Test Claim",
            DamageCost = 1000m
        };
    }
}
