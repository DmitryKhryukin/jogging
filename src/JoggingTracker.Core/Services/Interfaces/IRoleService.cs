using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoggingTracker.Core.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<string>> GetRolesAsync(bool isAdmin);
    }
}