using System.Threading.Tasks;
using JoggingTracker.DataAccess.DbEntities;

namespace JoggingTracker.Core.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(UserDb user);
    }
}