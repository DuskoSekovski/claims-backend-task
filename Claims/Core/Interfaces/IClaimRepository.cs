using Claims.Core.Entities;

namespace Claims.Core.Interfaces
{
    public interface IClaimRepository
    {
        Task<IEnumerable<Claim>> GetClaimsAsync();
        Task<Claim> GetClaimAsync(string id);
        Task CreateClaimAsync(Claim item);
        Task DeleteClaimAsync(string id);
    }
}
