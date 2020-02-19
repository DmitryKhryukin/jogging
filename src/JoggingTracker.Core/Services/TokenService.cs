using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace JoggingTracker.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserDb> _userManager;
        private readonly AppSettings _settings;

        public TokenService(UserManager<UserDb> userManager,
            AppSettings settings)
        {
            _userManager = userManager;
            _settings = settings;
        }

        public async Task<string> GenerateToken(UserDb user)
        {
            var userClaims = await GetUserClaims(user);

            return GenerateToken(_settings.Secret, _settings.LifetimeInDays, userClaims);
        }

        private string GenerateToken(string secret, int lifetimeInDays, IList<Claim> userClaims)
        {
            var secretKey = Encoding.ASCII.GetBytes(secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

            var tokeOptions = new JwtSecurityToken(
                claims: userClaims,
                expires: DateTime.UtcNow.AddDays(lifetimeInDays),
                signingCredentials: signingCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }

        private async Task<IList<Claim>> GetUserClaims(UserDb user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var roleName in roles)
            {
                var claim = new Claim(ClaimTypes.Role, roleName);
                userClaims.Add(claim);
            }
            
            // mapped by default to JwtRegisteredClaimNames.NameId
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id)); 
            userClaims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)); 
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)));

            return userClaims;
        }
    }
}