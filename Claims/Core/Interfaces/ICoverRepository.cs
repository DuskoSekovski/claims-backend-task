using Claims.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Core.Interfaces
{
    public interface ICoverRepository
    {
        Task<IEnumerable<Cover>> GetCoverAsync();
        Task<Cover> GetCoverAsync(string id);
        Task CreateCoverAsync(Cover cover);
        Task DeleteCoverAsync(string id);
    }
}
