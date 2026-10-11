using Claims.Core.Entities;
using Claims.Core.Enums;

namespace Claims.Core.Interfaces
{
    public interface ICoverCalculator
    {
        decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
    }
}
