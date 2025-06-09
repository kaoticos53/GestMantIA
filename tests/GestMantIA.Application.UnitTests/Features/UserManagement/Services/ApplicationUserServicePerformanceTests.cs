using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using GestMantIA.Application.UnitTests.TestHelpers;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using GestMantIA.Application.Features.UserManagement.Services;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Shared.Identity.DTOs.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AutoMapper;

namespace GestMantIA.Application.UnitTests.Features.UserManagement.Services
{
    public class ApplicationUserServicePerformanceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<ILogger<ApplicationUserService>> _mockLogger;
        private readonly ApplicationUserService _userService;
        private readonly Fixture _fixture;

        public ApplicationUserServicePerformanceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
                
            var roleStore = new Mock<IRoleStore<ApplicationRole>>();
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
                roleStore.Object,
                Array.Empty<IRoleValidator<ApplicationRole>>(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<ApplicationRole>>>().Object);
                
            _mockLogger = new Mock<ILogger<ApplicationUserService>>();
            
            var mockMapper = new Mock<IMapper>();
            var identityOptions = new IdentityOptions();
            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(identityOptions);
            
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            _userService = new ApplicationUserService(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                mockMapper.Object,
                _mockLogger.Object,
                mockOptions.Object,
                mockHttpContextAccessor.Object);
                
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetAllUsersAsync_WithLargeDataset_ReturnsInReasonableTime()
        {
            // Arrange
            var largeNumberOfUsers = 10000;
            var testUsers = _fixture
                .Build<ApplicationUser>()
                .Without(u => u.Id)
                .With(u => u.IsDeleted, false)
                .CreateMany(largeNumberOfUsers)
                .AsQueryable()
                .BuildMock();
            
            _mockUserManager.Setup(um => um.Users).Returns(testUsers);
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _userService.GetAllUsersAsync(1, 20);
            stopwatch.Stop();
            
            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(20);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
                "La consulta debería completarse en menos de 1 segundo");
        }

        [Fact]
        public async Task SearchUsersAsync_WithLargeDataset_ReturnsInReasonableTime()
        {
            // Arrange
            var largeNumberOfUsers = 10000;
            var testUsers = _fixture
                .Build<ApplicationUser>()
                .Without(u => u.Id)
                .With(u => u.IsDeleted, false)
                .With(u => u.UserName, (string userName) => $"user{userName}")
                .CreateMany(largeNumberOfUsers)
                .ToList();
                
            // Asegurar que al menos un usuario tenga un nombre de usuario conocido
            testUsers[0].UserName = "testuser";
            
            var mockUsers = testUsers.AsQueryable().BuildMock();
            _mockUserManager.Setup(um => um.Users).Returns(mockUsers);
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _userService.SearchUsersAsync("testuser", 1, 10);
            stopwatch.Stop();
            
            // Assert
            result.Should().NotBeNull();
            result.Items.Should().ContainSingle(u => u.UserName == "testuser");
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
                "La búsqueda debería completarse en menos de 500ms");
        }
    }
}
