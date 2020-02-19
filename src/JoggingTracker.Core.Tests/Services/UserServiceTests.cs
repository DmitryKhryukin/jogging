using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore3Mock;
using FluentAssertions;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Exceptions;
using JoggingTracker.Core.Mapping;
using JoggingTracker.Core.Services;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace JoggingTracker.Core.Tests.Services
{
    public class UserServiceTests
    {
        private readonly DbContextMock<JoggingTrackerDataContext> _mockDbContext;
        private readonly Mock<UserManager<UserDb>> _userManagerMock;
        private readonly Mock<SignInManager<UserDb>> _signInManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<ITokenService> _tokenManagerMock;
        private readonly TestUserSevice _userService;
        
        private static List<UserDb> SeedUsers =>
            new List<UserDb>()
            {
                new UserDb()
                {
                    Id = "id1",
                    UserName = "First User"
                },
                new UserDb()
                {
                    Id = "id2",
                    UserName = "Second User"
                },
                new UserDb()
                {
                    Id = "id3",
                    UserName = "Third User"
                }
            };
        
        public UserServiceTests()
        {
            DbContextOptions<JoggingTrackerDataContext> dummyOptions = new DbContextOptionsBuilder<JoggingTrackerDataContext>().Options;
            _mockDbContext = new DbContextMock<JoggingTrackerDataContext>(dummyOptions);
            var dbSetMock = new Mock<DbSet<UserDb>>();
            var dbContextMock = new Mock<JoggingTrackerDataContext>();
            dbContextMock.Setup(s => s.Set<UserDb>()).Returns(dbSetMock.Object);

            _userManagerMock = new Mock<UserManager<UserDb>>(
                new Mock<IUserStore<UserDb>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<UserDb>>().Object,
                new IUserValidator<UserDb>[0],
                new IPasswordValidator<UserDb>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<UserDb>>>().Object);

            _signInManagerMock = new Mock<SignInManager<UserDb>>(_userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<UserDb>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<UserDb>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);
            
            _tokenManagerMock = new Mock<ITokenService>();

            var autoMapperProfile = new AutoMapperProfile();
            var mapperConfiguration = new MapperConfiguration(x => x.AddProfile(autoMapperProfile));
            var mapper = new Mapper(mapperConfiguration);
            
            _userService = new TestUserSevice(_userManagerMock.Object, 
                _signInManagerMock.Object,
                _mockDbContext.Object,
                _roleManagerMock.Object,
                _tokenManagerMock.Object,
                mapper);
        }
        
        #region GetUsersAsync Tests
        [Fact]
        public void GetUsersAsync_AdminUser_NoFilter_ReturnsAllUsers()
        {
            // arrange
            var mockUsers = SeedUsers.AsQueryable().BuildMock();
            _userManagerMock.Setup(u => u.Users).Returns(mockUsers.Object);

            // act
            var result = _userService.GetUsersAsync(true, null).Result;

            // assert
            result.Total.Should().Be(SeedUsers.Count);
        }
        
        [Fact]
        public void GetUsersAsync_PageSizeAndPageNumberProvided_ReturnsPagedUsers()
        {
            // arrange
            var users = new List<UserDb>()
            {
                new UserDb()
                {
                    Id = "id1",
                    UserName = "First User"
                },
                new UserDb()
                {
                    Id = "id2",
                    UserName = "Second User"
                },
                new UserDb()
                {
                    Id = "id3",
                    UserName = "Third User"
                }
                ,
                new UserDb()
                {
                    Id = "id4",
                    UserName = "Third User"
                }
                ,
                new UserDb()
                {
                    Id = "id5",
                    UserName = "Third User"
                }
            };
            
            var mockUsers = users.AsQueryable().BuildMock();
            _userManagerMock.Setup(u => u.Users).Returns(mockUsers.Object);

            var pageSize = 2;
            var pageNumber = 2;
            // act
            var result = _userService.GetUsersAsync(true, null, pageNumber, pageSize).Result;

            // assert
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
            result.Total.Should().Be(users.Count);
            result.Items.Count.Should().Be(2);
            result.Items[0].Id.Should().Be(users[2].Id);
            result.Items[1].Id.Should().Be(users[3].Id);
        }
        
        [Fact]
        public void GetUsersAsync_IncorrectFilter_ThrowsException()
        {
            // arrange
            var mockUsers = SeedUsers.AsQueryable().BuildMock();
            _userManagerMock.Setup(u => u.Users).Returns(mockUsers.Object);

            var filter = "password = '1234'";
            
            // act
            var result = Assert.ThrowsAsync<JoggingTrackerBadRequestException>(() => _userService.GetUsersAsync(true, filter)).Result ;

            // assert
            result.Message.Should().StartWith(ErrorMessages.CouldntParseFilter);
        }
        
        [Fact]
        public void GetUsersAsync_Correct_ReturnFilteredUsers()
        {
            // arrange
            var mockUsers = SeedUsers.AsQueryable().BuildMock();
            _userManagerMock.Setup(u => u.Users).Returns(mockUsers.Object);

            var filter = "(username eq 'First User') or (id eq 'id3')";
            
            // act
            var result =  _userService.GetUsersAsync(true, filter).Result;

            // assert
            result.Total.Should().Be(2);
        }
        #endregion

        #region CreateUserAsync Tests
        [Fact]
        public void CreateUserAsync_UserIsCreated()
        {
            // arrange
            var createAsyncResult = IdentityResult.Success;
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<UserDb>(), It.IsAny<string>()))
                .Returns(Task.FromResult(createAsyncResult));
            
            // act
            _userService.CreateUserAsync(new UserRegisterRequest()).Wait();

            // assert
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserDb>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserDb>(), UserRoles.RegularUser), Times.Once());
        }
        
        [Fact]
        public void CreateUserAsync_InvalidRequest_ThrowsCustomException()
        {
            // arrange
            var createAsyncResult = IdentityResult.Failed();
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<UserDb>(), It.IsAny<string>()))
                .Returns(Task.FromResult(createAsyncResult));
            
            // act
            var exception = Assert.ThrowsAsync<JoggingTrackerBadRequestException>(() => _userService.CreateUserAsync(new UserRegisterRequest())).Result;

            // assert
            exception.Message.Should().StartWith(ErrorMessages.UserCantBeCreated);
            
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserDb>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public void CreateUserAsync_UsernameExists_ThrowsCustomException()
        {
            // arrange
            var request = new UserRegisterRequest()
            {
                UserName = "test"
            };
            
            _userManagerMock.Setup(u => u.FindByNameAsync(request.UserName))
                .Returns(Task.FromResult(new UserDb()));
            
            // act
            var exception = Assert.ThrowsAsync<JoggingTrackerBadRequestException>(() => _userService.CreateUserAsync(request)).Result;

            // assert
            exception.Message.Should().StartWith(ErrorMessages.UserNameExists);
            
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserDb>(), It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserDb>(), UserRoles.RegularUser), Times.Never());
        } 
        
        #endregion

        #region UpdateUserRoles

        [Fact]
        public void UpdateUserRolesAsync_RemovesNotPresentedRoles_AddNewRoles()
        {
            // arrange
            var role1 = "role1";
            var role2 = "role2";
            var role3 = "role3";

            IList<string> currentRoles = new List<string>() {role1, role2};

            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<UserDb>()))
                .Returns(Task.FromResult(currentRoles));

            var updateRoles = new List<string>() {role2, role3};

            var userDb = new UserDb();

            _userManagerMock.Setup(x => x.AddToRoleAsync(userDb, role3));
            _userManagerMock.Setup(x => x.RemoveFromRoleAsync(userDb, role1));
            
            // act
            _userService.UpdateUserRolesAsync(userDb, updateRoles).Wait();
            
            // assert
            _userManagerMock.Verify(x => x.AddToRoleAsync(userDb, It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.RemoveFromRoleAsync(userDb, It.IsAny<string>()), Times.Once);
        }

        #endregion
        
        #region ValidateAdminAccessAsync

        [Fact]
        public async void ValidateAdminAccessAsync_IsCurrentUserAdminIsNull_DontCheckIfEditedUserIsInRole()
        {
            bool? isCurrentUserAdmin = null;
            
            _userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>()));

            await _userService.ValidateAdminAccessAsync(isCurrentUserAdmin, new UserDb());
            
            _userManagerMock.Verify(x => x.IsInRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async void ValidateAdminAccessAsync_IsCurrentUserAdminTrue_DontCheckIfEditedUserIsInRole()
        {
            bool? isCurrentUserAdmin = true;
            
            _userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>()));

            await _userService.ValidateAdminAccessAsync(isCurrentUserAdmin, new UserDb());
            
            _userManagerMock.Verify(x => x.IsInRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async void ValidateAdminAccessAsync_IsCurrentUserAdminFalse_CheckIfEditedUserIsInAdminRole()
        {
            bool? isCurrentUserAdmin = false;
            
            _userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            await _userService.ValidateAdminAccessAsync(isCurrentUserAdmin, new UserDb());
            
            _userManagerMock.Verify(x => x.IsInRoleAsync(It.IsAny<UserDb>(), UserRoles.Admin), Times.Once);
        }
        
        [Fact]
        public void ValidateAdminAccessAsync_IsCurrentUserAdminFalse_EditedUserIsInAdminRole_ThrowFobiddenException()
        {
            bool? isCurrentUserAdmin = false;
            
            _userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<UserDb>(), It.IsAny<string>())).Returns(Task.FromResult(true));

           Assert.ThrowsAsync<JoggingTrackerForbiddenException>(() => _userService.ValidateAdminAccessAsync(isCurrentUserAdmin, new UserDb())); 

        }

        #endregion
    }

    public class TestUserSevice : UserService
    {
        public TestUserSevice(UserManager<UserDb> userManager, 
            SignInManager<UserDb> signInManager, 
            JoggingTrackerDataContext dbContext, 
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService, 
            IMapper mapper) : base(userManager, signInManager, dbContext, roleManager, tokenService, mapper)
        {
        }
        public new async Task UpdateUserRolesAsync(UserDb userDb, IEnumerable<string> userRoles)
        {
            await base.UpdateUserRolesAsync(userDb, userRoles);
        }

        public new async Task ValidateAdminAccessAsync(bool? isCurrentUserAdmin, UserDb editedUser)
        {
            await base.ValidateAdminAccessAsync(isCurrentUserAdmin, editedUser);
        }
    }
}