using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading;
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
    public class ApplicationUserServiceConcurrencyTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<ILogger<ApplicationUserService>> _mockLogger;
        private readonly ApplicationUserService _userService;

        public ApplicationUserServiceConcurrencyTests()
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
        }

        [Fact]
        public async Task UpdateUserAsync_WithConcurrentUpdates_HandlesConcurrency()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var originalUser = new ApplicationUser 
            { 
                Id = userId, 
                UserName = "user1", 
                Email = "user1@example.com", 
                ConcurrencyStamp = "1" 
            };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(originalUser);
                
            var identityError = new IdentityError { 
                Code = "ConcurrencyFailure", 
                Description = "Optimistic concurrency failure, object has been modified." 
            };
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(identityError));
            
            // Act & Assert
            await _userService.Invoking(s => s.UpdateUserAsync(userId, 
                new UpdateUserDTO { 
                    Id = userId,
                    UserName = "user1", 
                    Email = "updated@example.com" 
                }))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No se pudo actualizar el usuario debido a un error de concurrencia. Por favor, actualice la página e intente nuevamente.");
        }

        [Fact]
        public async Task DeleteUserAsync_WithConcurrentDeletion_HandlesConcurrency()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "user1" };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
                
            var identityError = new IdentityError { 
                Code = "ConcurrencyFailure", 
                Description = "Optimistic concurrency failure, object has been modified." 
            };
            _mockUserManager.Setup(um => um.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(identityError));
            
            // Act & Assert
            var result = await _userService.DeleteUserAsync(userId);
            
            // Assert
            result.Should().BeFalse();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error de concurrencia al eliminar al usuario")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateUserRolesAsync_WithConcurrentRoleUpdates_HandlesConcurrency()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "user1" };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
                
            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });
                
            _mockUserManager.Setup(um => um.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
                
            _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency exception"));
            
            // Act
            var result = await _userService.UpdateUserRolesAsync(userId, new List<string> { "Admin" });
            
            // Assert
            result.Should().BeFalse();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error de concurrencia al actualizar roles")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            // Limpieza si es necesaria
        }
    }
}
