using AutoMapper;
using FluentAssertions;
using GestMantIA.Application.Features.UserManagement.Services;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Shared.Identity.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GestMantIA.Application.UnitTests.Features.UserManagement.Services
{
    public class ApplicationUserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ApplicationUserService>> _mockLogger;
        private readonly Mock<IOptions<IdentityOptions>> _mockIdentityOptions;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ApplicationUserService _userService;

        public ApplicationUserServiceTests()
        {
            // UserManager mock
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), // IUserStore<ApplicationUser> store
                null!, // IOptions<IdentityOptions> optionsAccessor
                null!, // IPasswordHasher<ApplicationUser> passwordHasher
                null!, // IEnumerable<IUserValidator<ApplicationUser>> userValidators
                null!, // IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators
                null!, // ILookupNormalizer keyNormalizer
                null!, // IdentityErrorDescriber errors
                null!, // IServiceProvider services
                null!  // ILogger<UserManager<ApplicationUser>> logger
            );

            // RoleManager mock
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
                Mock.Of<IRoleStore<ApplicationRole>>(), // IRoleStore<ApplicationRole> store
                null!, // IEnumerable<IRoleValidator<ApplicationRole>> roleValidators
                null!, // ILookupNormalizer keyNormalizer
                null!, // IdentityErrorDescriber errors
                null!  // ILogger<RoleManager<ApplicationRole>> logger
            );

            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ApplicationUserService>>();
            _mockIdentityOptions = new Mock<IOptions<IdentityOptions>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Es importante configurar el .Value para IdentityOptions si el servicio lo accede.
            _mockIdentityOptions.Setup(io => io.Value).Returns(new IdentityOptions());

            _userService = new ApplicationUserService(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockMapper.Object,
                _mockLogger.Object,
                _mockIdentityOptions.Object,
                _mockHttpContextAccessor.Object);
        }

        // Prueba de marcador de posición para asegurar que la configuración compila y se ejecuta.
        [Fact]
        public void Constructor_Should_Initialize_Service_Without_Errors()
        {
            // Assert
            _userService.Should().NotBeNull();
            // Esta prueba verifica principalmente que el constructor se complete y los mocks se pasen correctamente.
        }

        // Aquí se añadirán más pruebas.

        [Fact]
        public async Task GetUserProfileAsync_Should_Return_UserResponseDTO_When_User_Exists_And_Not_Deleted()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var applicationUser = new ApplicationUser { Id = Guid.Parse(userId), UserName = "testuser", Email = "test@example.com", IsDeleted = false };
            var userRoles = new List<string> { "User" };
            var expectedDto = new UserResponseDTO { Id = userId, UserName = "testuser", Email = "test@example.com", Roles = userRoles };

            _mockUserManager.Setup(x => x.FindByIdAsync(It.Is<string>(s => s == userId))).ReturnsAsync(applicationUser);
            _mockUserManager.Setup(um => um.GetRolesAsync(applicationUser)).ReturnsAsync(userRoles);
            _mockMapper.Setup(m => m.Map<UserResponseDTO>(applicationUser)).Returns(expectedDto);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDto);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Never);
        }
        [Fact]
        public async Task GetUserProfileAsync_Should_Return_Null_When_User_Is_Deleted()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var applicationUser = new ApplicationUser { Id = Guid.Parse(userId), UserName = "testuser", Email = "test@example.com", IsDeleted = true };

            _mockUserManager.Setup(x => x.FindByIdAsync(It.Is<string>(s => s == userId))).ReturnsAsync(applicationUser);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockMapper.Verify(m => m.Map<UserResponseDTO>(It.IsAny<ApplicationUser>()), Times.Never); // No debería intentar mapear
            _mockUserManager.Verify(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never); // No debería intentar obtener roles
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"No se encontró el usuario con ID '{userId}' o está marcado como eliminado.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
        [Fact]
        public async Task GetUserProfileAsync_Should_Return_Null_When_User_Does_Not_Exist()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _mockUserManager.Setup(x => x.FindByIdAsync(It.Is<string>(s => s == userId))).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockMapper.Verify(m => m.Map<UserResponseDTO>(It.IsAny<ApplicationUser>()), Times.Never);
            _mockUserManager.Verify(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"No se encontró el usuario con ID '{userId}' o está marcado como eliminado.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
        [Fact]
        public async Task GetUserProfileAsync_Should_Return_Null_When_UserId_Is_Invalid_Guid()
        {
            // Arrange
            var invalidUserId = "not-a-guid";

            // Act
            var result = await _userService.GetUserProfileAsync(invalidUserId);

            // Assert
            result.Should().BeNull();
            _mockUserManager.Verify(um => um.FindByIdAsync(It.IsAny<string>()), Times.Never); // No debería intentar buscar
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"El ID de usuario '{invalidUserId}' no es un GUID válido.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
        [Fact]
        public async Task GetUserProfileAsync_Should_Return_Null_And_Log_Error_When_Exception_Occurs()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var exceptionMessage = "Database connection failed";
            _mockUserManager.Setup(um => um.FindByIdAsync(It.Is<string>(s => s == userId))).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"Error al obtener el perfil del usuario con ID '{userId}'.")),
                    It.Is<Exception>(ex => ex != null && ex.Message == exceptionMessage),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}