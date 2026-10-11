using Claims.Core.Entities;
using Claims.Core.Interfaces;
using Claims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly ClaimsContext _context;

        public ClaimRepository(ClaimsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            return await _context.Claims.ToListAsync();
        }

        public async Task<Claim> GetClaimAsync(string id)
        {
            return await _context.Claims
                .Where(claim => claim.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task CreateClaimAsync(Claim item)
        {
            _context.Claims.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClaimAsync(string id)
        {
            var claim = await GetClaimAsync(id);
            if (claim is not null)
            {
                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();
            }
        }
    }
}
