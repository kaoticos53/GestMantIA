using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using GestMantIA.Application.UnitTests.TestHelpers;
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
    public class ApplicationUserServiceSecurityTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<ILogger<ApplicationUserService>> _mockLogger;
        private readonly ApplicationUserService _userService;
        private readonly Fixture _fixture;

        public ApplicationUserServiceSecurityTests()
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

        [Theory]
        [InlineData("admin'; DROP TABLE Users;--")] // SQL Injection
        [InlineData("admin' OR '1'='1")]           // SQL Injection
        [InlineData("<script>alert('XSS')</script>")] // XSS
        [InlineData("test@example.com\nBCC:malicious@example.com")] // Header Injection
        public async Task SearchUsersAsync_WithMaliciousInput_HandlesSafely(string searchTerm)
        {
            // Arrange
            _mockUserManager.Setup(um => um.Users).Returns(new List<ApplicationUser>().AsQueryable().BuildMock());
            
            // Act
            var result = await _userService.SearchUsersAsync(searchTerm, 1, 10);
            
            // Assert
            result.Should().NotBeNull();
            // Verificar que no se lanzó ninguna excepción
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("test@.com", false)]
        [InlineData("@example.com", false)]
        [InlineData("test@example", false)]
        public async Task CreateUserAsync_EmailValidation_WorksCorrectly(string email, bool isValid)
        {
            // Arrange
            var userDto = new CreateUserDTO
            {
                UserName = "testuser",
                Email = email,
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!",
                Roles = new List<string> { "User" }
            };
            
            _mockUserManager.Setup(um => 
                um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
                
            _mockUserManager.Setup(um => 
                um.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            
            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(userDto);
            
            // Assert
            if (isValid)
            {
                await act.Should().NotThrowAsync();
            }
            else
            {
                await act.Should().ThrowAsync<ArgumentException>();
            }
        }

        [Fact]
        public async Task CreateUserAsync_PasswordComplexity_ValidatesCorrectly()
        {
            // Arrange
            var weakPasswordDto = new CreateUserDTO
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "weak",
                ConfirmPassword = "weak",
                Roles = new List<string> { "User" }
            };
            
            // Act & Assert
            await _userService.Invoking(s => s.CreateUserAsync(weakPasswordDto))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*La contraseña no cumple con los requisitos de complejidad*");
        }

        [Fact]
        public async Task UpdateUserAsync_PreventRoleEscalation_WhenNotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var currentUser = new ApplicationUser { Id = userId, UserName = "regularuser" };
            
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);
                
            var updateDto = new UpdateUserDTO
            {
                Id = userId,
                UserName = "regularuser",
                Email = "user@example.com",
                Roles = new List<string> { "Admin" } // Intentando agregar rol de administrador
            };
            
            // Act & Assert
            await _userService.Invoking(s => s.UpdateUserAsync(userId, updateDto))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("No tienes permiso para asignar roles de administrador");
        }

        public void Dispose()
        {
            // Limpieza si es necesaria
        }
    }
}
