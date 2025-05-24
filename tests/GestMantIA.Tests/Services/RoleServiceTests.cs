using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GestMantIA.Tests.Services
{
    public class RoleServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<ILogger<RoleService>> _loggerMock;
        private readonly IRoleService _roleService;

        public RoleServiceTests()
        {
            // Configurar base de datos en memoria para pruebas
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            
            // Configurar RoleManager mock
            var store = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
                store.Object, null, null, null, null);
                
            _loggerMock = new Mock<ILogger<RoleService>>();
            
            _roleService = new RoleService(
                _roleManagerMock.Object,
                _loggerMock.Object,
                _context);
        }

        [Fact]
        public async Task CreateRoleAsync_ValidRole_ReturnsTrue()
        {
            // Arrange
            var roleDto = new RoleDTO { Name = "Admin", Description = "Administrator role" };
            
            _roleManagerMock.Setup(x => x.RoleExistsAsync(roleDto.Name))
                .ReturnsAsync(false);
                
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.CreateRoleAsync(roleDto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetRoleByIdAsync_RoleExists_ReturnsRole()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var role = new ApplicationRole { Id = roleId, Name = "Admin", Description = "Admin role" };
            
            _roleManagerMock.Setup(x => x.FindByIdAsync(roleId))
                .ReturnsAsync(role);

            // Act
            var result = await _roleService.GetRoleByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(role.Name, result.Name);
            Assert.Equal(role.Description, result.Description);
        }

        [Fact]
        public async Task UpdateRoleAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var roleDto = new RoleDTO { Id = roleId, Name = "UpdatedRole", Description = "Updated Description" };
            var existingRole = new ApplicationRole { Id = roleId, Name = "OldName", Description = "Old Description" };
            
            _roleManagerMock.Setup(x => x.FindByIdAsync(roleId))
                .ReturnsAsync(existingRole);
                
            _roleManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.UpdateRoleAsync(roleDto);

            // Assert
            Assert.True(result.Success);
            _roleManagerMock.Verify(x => x.UpdateAsync(It.Is<ApplicationRole>(r => 
                r.Name == roleDto.Name && r.Description == roleDto.Description)), 
                Times.Once);
        }

        [Fact]
        public async Task DeleteRoleAsync_RoleExists_ReturnsTrue()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var role = new ApplicationRole { Id = roleId, Name = "ToDelete" };
            
            _roleManagerMock.Setup(x => x.FindByIdAsync(roleId))
                .ReturnsAsync(role);
                
            _roleManagerMock.Setup(x => x.DeleteAsync(role))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.DeleteRoleAsync(roleId);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetUsersInRoleAsync_ReturnsUsers()
        {
            // Arrange
            var roleName = "Admin";
            var users = new List<ApplicationUser>
            {
                new() { Id = "1", UserName = "user1", Email = "user1@example.com" },
                new() { Id = "2", UserName = "user2", Email = "user2@example.com" }
            };
            
            _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
                .ReturnsAsync(new ApplicationRole { Name = roleName });
                
            _roleManagerMock.Setup(x => x.GetUsersInRoleAsync(roleName))
                .ReturnsAsync(users);

            // Act
            var result = await _roleService.GetUsersInRoleAsync(roleName);

            // Assert
            Assert.Equal(2, result.Count());
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
