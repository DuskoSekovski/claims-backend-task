using Claims.Core.Entities;
using Claims.Core.Interfaces;
using Claims.Core.Services;
using Claims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Repositories
{
    public class CoverRepository : ICoverRepository
    {
        private readonly ClaimsContext _context;

        public CoverRepository(ClaimsContext context)
        {
            _context = context;
        }

        public async Task CreateCoverAsync(Cover cover)
        {
            _context.Covers.Add(cover);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCoverAsync(string id)
        {
            var cover = await GetCoverAsync(id);
            if (cover is not null)
            {
                _context.Covers.Remove(cover);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Cover>> GetCoverAsync()
        {
            return await _context.Covers.ToListAsync();
        }

        public async Task<Cover> GetCoverAsync(string id)
        {
            return await _context.Covers
                .Where(claim => claim.Id == id)
                .SingleOrDefaultAsync();
        }
    }
}
