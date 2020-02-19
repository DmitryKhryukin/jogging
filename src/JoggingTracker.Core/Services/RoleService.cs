using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JoggingTracker.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<string>> GetRolesAsync(bool isAdmin)
        {
            var result = new List<string>();
            
            var allRoles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();

            if (!isAdmin)
            {
                allRoles.Remove(UserRoles.Admin);
            }
            
            result = allRoles;

            return result;
        }
    }
}