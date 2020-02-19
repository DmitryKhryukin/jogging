using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Exceptions;
using JoggingTracker.Core.Helpers;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JoggingTracker.Core.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserDb> _userManager;
        private readonly SignInManager<UserDb> _signInManager;
        private readonly JoggingTrackerDataContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public UserService(UserManager<UserDb> userManager,
            SignInManager<UserDb> signInManager,
            JoggingTrackerDataContext dbContext,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }
        
        public async Task<UserDto> CreateUserAsync(UserRegisterRequest request)
        {
            var newUser = new UserDb()
            {
                UserName =  request.UserName
            };
            
            var existingUser = await _userManager.FindByNameAsync(request.UserName);

            if (existingUser != null)
            {
                throw new JoggingTrackerBadRequestException(ErrorMessages.UserNameExists);
            }

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                var errorMessages = string.Join(",", result.Errors.Select(x => x.Description));

                throw new JoggingTrackerBadRequestException($"Can't create user: {errorMessages}");
            }
            
            await _userManager.AddToRoleAsync(newUser, UserRoles.RegularUser);

            return _mapper.Map<UserDto>(newUser);
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(bool isCurrentUserAdmin,
            string filter = null,
            int? pageNumber = null, 
            int? pageSize = null)
        {
            var predicate = ExpressionHelper.GetFilterPredicate<UserDb, UserDto>(filter);

            IQueryable<UserDb> query;
            
            if (isCurrentUserAdmin)
            {
                query = _userManager.Users
                    .AsNoTracking();
            }
            else
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);
                var adminIds = adminUsers?.Select(x => x.Id).ToList();
                    
                //filter admin users
                query = _userManager.Users.Where(x => !adminIds.Contains(x.Id))
                    .AsNoTracking();
            }

            query = query.Where(predicate);

            var result = await PaginationHelper.GetPagedResponseAsync<UserDb, UserDto>(
                query, pageNumber, pageSize, _mapper);
            

            return result;
        }

        public async Task<UserDto> GetCurrentUserAsync(string userId)
        {
            UserDto result = null;

            var userDb = await _userManager.FindByIdAsync(userId);

            if (userDb != null)
            {
                result = _mapper.Map<UserDb, UserDto>(userDb);
            }

            return result;
        }

        public async Task<UserWithRolesDto> UpdateUserAsync(string userId, 
            UserWithRolesUpdateRequest request,
            bool isCurrentUserAdmin)
        {
            if (!isCurrentUserAdmin && request.Roles != null && request.Roles.Contains(UserRoles.Admin))
            {
                throw new JoggingTrackerForbiddenException(ErrorMessages.Forbidden);
            }

            await UpdateUserAsync(userId, request, request.Roles);

            return await GetUserWithRolesAsync(userId, isCurrentUserAdmin);
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UserUpdateRequest request,
            IEnumerable<string> userRoles = null, bool? isCurrentUserAdmin = null)
        {
            var userDb = await GetUserDb(userId);
            
            await ValidateAdminAccessAsync(isCurrentUserAdmin, userDb);
            
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await UpdateUserNameAsync(userDb, request.UserName);

                    await UpdateUserPasswordAsync(userDb, request.NewPassword, request.OldPassword);

                    if (userRoles != null && userRoles.Any())
                    {
                        ValidateRoles(userRoles);
                        await UpdateUserRolesAsync(userDb, userRoles);
                    }

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();

                    var errorMessage = $"{ErrorMessages.UserCantBeUpdated} : {ex.Message}";
                    throw new JoggingTrackerInternalServerErrorException(errorMessage);
                }
            }

            return _mapper.Map<UserDb, UserDto>(userDb);
        }
        
        public async Task<UserWithRolesDto> GetUserWithRolesAsync(string userId, bool isCurrentUserAdmin)
        {
            UserWithRolesDto result = null;

            var userDb = await _userManager.FindByIdAsync(userId);

            await ValidateAdminAccessAsync(isCurrentUserAdmin, userDb);
            
            if (userDb != null)
            {
                result = _mapper.Map<UserDb, UserWithRolesDto>(userDb);
                result.Roles = await _userManager.GetRolesAsync(userDb);
            }

            return result;
        }

        public async Task DeleteUserAsync(string userId, bool? isCurrentUserAdmin)
        {
            var userDb = await _userManager.FindByIdAsync(userId);

            await ValidateAdminAccessAsync(isCurrentUserAdmin, userDb);
            
            if (userDb == null)
            {
                throw new JoggingTrackerNotFoundException(ErrorMessages.UserNotFound);
            }

            var userRoles = await _userManager.GetRolesAsync(userDb);
            var userRuns = _dbContext.Runs.Where(x => x.UserId == userDb.Id);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await _userManager.RemoveFromRolesAsync(userDb, userRoles);
                    _dbContext.Runs.RemoveRange(userRuns);
                    await _userManager.DeleteAsync(userDb);
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();

                    var errorMessage = $"{ErrorMessages.UserCantBeDeleted} : {ex.Message}";
                    throw new JoggingTrackerInternalServerErrorException(errorMessage);
                }
            }
        }

        public async Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request)
        {
            var result = new UserLoginResponse();

            var signInResult = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, false, false);

            if (signInResult.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(request.UserName);

                result.Token = await _tokenService.GenerateToken(user);
                result.UserId = user.Id;
            }

            return result;
        }

        protected async Task<UserDb> GetUserDb(string userId)
        {
            var userDb = await _userManager.FindByIdAsync(userId);

            if (userDb == null)
            {
                throw new JoggingTrackerNotFoundException(ErrorMessages.UserNotFound);
            }

            return userDb;
        }
        
        protected async Task UpdateUserPasswordAsync(UserDb userDb, string newPassword, string odlPassword)
        {
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword.Length < AppConstants.MinPasswordLength)
                {
                    throw new JoggingTrackerBadRequestException(ErrorMessages.PasswordShouldBe6CharLong);
                }

                var changePasswordResult =
                    await _userManager.ChangePasswordAsync(userDb, odlPassword, newPassword);

                if (!changePasswordResult.Succeeded)
                {
                    var errorMessages = changePasswordResult.Errors.Select(x => x.Description);
                    var customErrorMessage = $"{ErrorMessages.CantUpdatePassword} : {string.Join(",", errorMessages)}";

                    throw new JoggingTrackerInternalServerErrorException(customErrorMessage);
                }
            }
        }

        protected async Task UpdateUserNameAsync(UserDb userDb, string newUserName)
        {
            var isUsernameUpdate = newUserName != userDb.UserName;

            if (isUsernameUpdate)
            {
                var usernameExist = await _userManager.FindByNameAsync(newUserName) != null;

                if (usernameExist)
                {
                    throw new JoggingTrackerBadRequestException(ErrorMessages.UserNameExists);
                }

                userDb.UserName = newUserName;
                var result = await _userManager.UpdateAsync(userDb);

                if (!result.Succeeded)
                {
                    throw new JoggingTrackerBadRequestException(ErrorMessages.UserCantBeUpdated);
                }
            }
        }
        
        protected async Task UpdateUserRolesAsync(UserDb userDb, IEnumerable<string> userRoles)
        {
            var currentRoles = await _userManager.GetRolesAsync(userDb);

            var newRoles = userRoles.Except(currentRoles).Distinct();
            var rolesToDelete = currentRoles.Except(userRoles).Distinct();

            foreach (var newRole in newRoles)
            {
                await _userManager.AddToRoleAsync(userDb, newRole);
            }
            
            foreach (var roleToDelete in rolesToDelete)
            {
                await _userManager.RemoveFromRoleAsync(userDb, roleToDelete);
            }
        }
        
        protected void ValidateRoles(IEnumerable<string> userRoles)
        {
            if (userRoles.Any())
            {
                var allRoles = _roleManager.Roles.Select(x => x.Name).ToList();

                var notValidRoles = userRoles.Except(allRoles);

                if (notValidRoles.Any())
                {
                    var errorMessage = $"{ErrorMessages.InvalidUserRoles} : {string.Join(',', notValidRoles)}";
                    throw new JoggingTrackerBadRequestException(errorMessage);
                }
            }
        }

        
        protected async Task ValidateAdminAccessAsync(bool isCurrentUserAdmin, UserDb editedUser)
        { 
            await ValidateAdminAccessAsync((bool?) isCurrentUserAdmin, editedUser);
        }
        
        protected async Task ValidateAdminAccessAsync(bool? isCurrentUserAdmin, UserDb editedUser)
        {
            if (isCurrentUserAdmin.HasValue && !isCurrentUserAdmin.Value)
            {
                var editedUserIsAdmin = await _userManager.IsInRoleAsync(editedUser, UserRoles.Admin);

                if (editedUserIsAdmin)
                {
                    throw new JoggingTrackerForbiddenException(ErrorMessages.Forbidden);
                }
            }
        }
    }
}