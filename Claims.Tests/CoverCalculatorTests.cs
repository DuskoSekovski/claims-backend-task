using Claims.Core.Enums;
using Claims.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Claims.Tests
{
    public class CoverCalculatorTests
    {
        private readonly CoverCalculator _calculator;

        public CoverCalculatorTests()
        {
            _calculator = new CoverCalculator();
        }


        [Fact]
        public void ComputePremium_ShouldApplyNoDiscount_ForFirst30Days()
        {
            // ARRANGE: 10 days for a Yacht. Base: 1250 * 1.10 = 1375 per day. 10 days = 13750.
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(10);

            // ACT
            var premium = _calculator.ComputePremium(start, end, CoverType.Yacht);

            // ASSERT
            Assert.Equal(13750m, premium);
        }

        [Fact]
        public void ComputePremium_ShouldApplyTier2Discount_ForDaysBetween31And180()
        {
            // ARRANGE: 40 Days for a Yacht. 
            // First 30 days: 30 * 1375 = 41250
            // Next 10 days: 10 * (1375 - 5%) = 10 * 1306.25 = 13062.5
            // Total expected = 54312.5
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(40);

            // ACT
            var premium = _calculator.ComputePremium(start, end, CoverType.Yacht);

            // ASSERT
            Assert.Equal(54312.5m, premium);
        }
    }
}
