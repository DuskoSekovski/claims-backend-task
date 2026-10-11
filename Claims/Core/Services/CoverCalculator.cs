using Claims.Core.Entities;
using Claims.Core.Enums;
using Claims.Core.Interfaces;

namespace Claims.Core.Services
{
    public class CoverCalculator : ICoverCalculator
    {
        private const decimal baseDayRate = 1250m;

        public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
        {
            if(startDate >= endDate)
            {
                return 0m;
            }

            decimal multiplier = coverType switch
            {
                CoverType.Yacht => 1.1m,
                CoverType.PassengerShip => 1.2m,
                CoverType.Tanker => 1.5m,
                _ => 1.3m
            };

            var premiumPerDay = baseDayRate * multiplier;

            var insuranceLength = (endDate - startDate).Days;
            var totalPremium = 0m;
            decimal discount = 0m;

            //First 30 days
            int packageOneDays = Math.Min(insuranceLength, 30);
            totalPremium += TotalPremium(premiumPerDay, packageOneDays, discount);
            insuranceLength -= packageOneDays;

            //Next 150 days
            if(insuranceLength > 0)
            {
                int packageTwoDays = Math.Min(insuranceLength, 150);
                discount = coverType == CoverType.Yacht ? 0.05m : 0.02m;
                totalPremium += TotalPremium(premiumPerDay, packageTwoDays, discount);
                insuranceLength -= packageTwoDays;
            }

            //Remaining days
            if (insuranceLength > 0)
            {
                discount = coverType == CoverType.Yacht ? 0.08m : 0.03m;
                totalPremium += TotalPremium(premiumPerDay, insuranceLength, discount);
            }

            return totalPremium;
        }

        private decimal TotalPremium(decimal premiumPerDay, int insuranceLength, decimal discount)
        {
            return insuranceLength * (premiumPerDay - premiumPerDay * discount);
        }
    }
}
