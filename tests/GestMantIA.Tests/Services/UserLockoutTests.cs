using System;
using System.Linq;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GestMantIA.Tests.Services
{
    public class UserLockoutTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _userService;
        private readonly Mock<IUserStore<ApplicationUser>> _mockUserStore;
        private readonly ApplicationUser _testUser;

        public UserLockoutTests()
        {
            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                _mockUserStore.Object, null, null, null, null, null, null, null, null);
            
            _mockLogger = new Mock<ILogger<UserService>>();
            var mockMapper = TestHelper.CreateMockMapper();
            var mockDbContext = TestHelper.CreateMockDbContext();
            
            _userService = new UserService(
                _mockUserManager.Object, 
                mockMapper, 
                _mockLogger.Object, 
                mockDbContext.Object);

            _testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com",
                IsActive = true
            };
        }

        [Fact]
        public async Task LockUserAsync_WithValidUserId_ShouldLockUser()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SetLockoutEnabledAsync(It.IsAny<ApplicationUser>(), true))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.LockUserAsync(_testUser.Id, TimeSpan.FromHours(1), "Test lock");

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.SetLockoutEnabledAsync(It.IsAny<ApplicationUser>(), true), Times.Once);
            _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        }

        [Fact]
        public async Task LockUserAsync_WithPermanentLock_ShouldSetMaxLockoutDate()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SetLockoutEnabledAsync(It.IsAny<ApplicationUser>(), true))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.LockUserAsync(_testUser.Id, reason: "Permanent lock");

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(
                x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), DateTimeOffset.MaxValue), 
                Times.Once);
        }

        [Fact]
        public async Task UnlockUserAsync_WithValidUserId_ShouldUnlockUser()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), null))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UnlockUserAsync(_testUser.Id);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), null), Times.Once);
            Assert.Null(_testUser.LockoutReason);
            Assert.Null(_testUser.LockoutDate);
        }

        [Fact]
        public async Task IsUserLockedOutAsync_WhenUserIsLocked_ShouldReturnTrue()
        {
            // Arrange
            _testUser.LockoutEnd = DateTimeOffset.UtcNow.AddDays(1);
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.IsUserLockedOutAsync(_testUser.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetUserLockoutInfoAsync_WhenUserExists_ShouldReturnLockoutInfo()
        {
            // Arrange
            var lockoutDate = DateTime.UtcNow.AddHours(-1);
            var lockoutEnd = DateTimeOffset.UtcNow.AddDays(1);
            
            _testUser.LockoutDate = lockoutDate;
            _testUser.LockoutEnd = lockoutEnd;
            _testUser.LockoutReason = "Test lockout";
            
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.GetUserLockoutInfoAsync(_testUser.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUser.Id, result.UserId);
            Assert.Equal(_testUser.UserName, result.UserName);
            Assert.True(result.IsLockedOut);
            Assert.Equal(lockoutEnd, result.LockoutEnd);
            Assert.Equal(lockoutDate, result.LockoutStart);
            Assert.Equal("Test lockout", result.Reason);
            Assert.False(result.IsPermanent);
        }

        [Fact]
        public async Task GetUserLockoutInfoAsync_WhenPermanentLock_ShouldSetIsPermanentTrue()
        {
            // Arrange
            _testUser.LockoutEnd = DateTimeOffset.MaxValue;
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.GetUserLockoutInfoAsync(_testUser.Id);

            // Assert
            Assert.True(result.IsPermanent);
        }

        [Fact]
        public async Task LockUserAsync_WhenUserNotFound_ShouldReturnFalse()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _userService.LockUserAsync("nonexistent-user");

            // Assert
            Assert.False(result);
        }

    }
}
