using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GestMantIA.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IUserService _userService;
        private readonly Mock<ILogger<UserService>> _loggerMock;

        public UserServiceTests()
        {
            // Configurar base de datos en memoria para pruebas
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            
            // Configurar UserManager mock
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
                
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UserService>>();
            
            _userService = new UserService(
                _userManagerMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _context);
        }

        [Fact]
        public async Task GetUserProfileAsync_UserExists_ReturnsUserProfile()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var expectedProfile = new UserResponseDTO
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Roles = new List<string>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
                
            _mapperMock.Setup(m => m.Map<UserResponseDTO>(user))
                .Returns(expectedProfile);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProfile.Id, result.Id);
            Assert.Equal(expectedProfile.UserName, result.UserName);
            Assert.Equal(expectedProfile.Email, result.Email);
        }

        [Fact]
        public async Task GetUserProfileAsync_UserNotExists_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            Assert.Null(result);
        }


        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
