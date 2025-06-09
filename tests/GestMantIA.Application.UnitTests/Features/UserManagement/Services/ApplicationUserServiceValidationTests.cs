using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
    public class ApplicationUserServiceValidationTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<ILogger<ApplicationUserService>> _mockLogger;
        private readonly ApplicationUserService _userService;
        private readonly Mock<IValidator<CreateUserDTO>> _mockCreateUserValidator;
        private readonly Mock<IValidator<UpdateUserDTO>> _mockUpdateUserValidator;

        public ApplicationUserServiceValidationTests()
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
            
            _mockCreateUserValidator = new Mock<IValidator<CreateUserDTO>>();
            _mockUpdateUserValidator = new Mock<IValidator<UpdateUserDTO>>();
            
            // Configurar validadores mock para pruebas con un resultado válido por defecto
            _mockCreateUserValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                
            _mockUpdateUserValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                
            // Configurar validación síncrona para compatibilidad
            _mockCreateUserValidator
                .Setup(v => v.Validate(It.IsAny<CreateUserDTO>()))
                .Returns(new FluentValidation.Results.ValidationResult());
                
            _mockUpdateUserValidator
                .Setup(v => v.Validate(It.IsAny<UpdateUserDTO>()))
                .Returns(new FluentValidation.Results.ValidationResult());
            
            var mockMapper = new Mock<IMapper>();
            
            // Configurar opciones de identidad para validación de contraseña
            var identityOptions = new IdentityOptions
            {
                Password = {
                    RequireDigit = true,
                    RequiredLength = 8,
                    RequireLowercase = true,
                    RequireUppercase = true,
                    RequireNonAlphanumeric = true
                }
            };
            
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

        [Theory]
        [InlineData("short", "Short1!", false)] // Muy corta
        [InlineData("longpasswordwithoutuppercase1!", "longpasswordwithoutuppercase1!", false)] // Sin mayúsculas
        [InlineData("LONGWITHOUTLOWERCASE1!", "LONGWITHOUTLOWERCASE1!", false)] // Sin minúsculas
        [InlineData("NoSpecialChars1", "NoSpecialChars1", false)] // Sin caracteres especiales
        [InlineData("ValidPass1!", "ValidPass1!", true)] // Válida
        public async Task CreateUserAsync_PasswordValidation_WorksCorrectly(string password, string confirmPassword, bool isValid)
        {
            // Arrange
            var userDto = new CreateUserDTO
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = password,
                ConfirmPassword = confirmPassword,
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
        public async Task CreateUserAsync_WithInvalidEmail_ThrowsValidationException()
        {
            // Arrange
            var invalidUserDto = new CreateUserDTO
            {
                UserName = "testuser",
                Email = "invalid-email",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!",
                Roles = new List<string> { "User" }
            };
            
            // Configurar el validador para fallar
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Email", "El correo electrónico no es válido")
            };
            
            var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
            
            // Configurar el mock para devolver el resultado de validación fallido
            _mockCreateUserValidator.Invocations.Clear();
            _mockCreateUserValidator
                .Setup(v => v.Validate(It.IsAny<CreateUserDTO>()))
                .Returns(validationResult);
                
            _mockCreateUserValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            
            // Act & Assert
            await _userService.Invoking(s => s.CreateUserAsync(invalidUserDto))
                .Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("Error de validación al crear el usuario");
        }

        [Fact]
        public async Task UpdateUserAsync_WithInvalidData_ThrowsValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidUserDto = new UpdateUserDTO
            {
                Id = userId,
                UserName = "", // Nombre de usuario vacío
                Email = "invalid-email",
                Roles = new List<string> { "InvalidRole" }
            };
            
            var user = new ApplicationUser { Id = userId, UserName = "existinguser", Email = "existing@example.com" };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            
            // Configurar el validador para fallar
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("UserName", "El nombre de usuario es requerido"),
                new FluentValidation.Results.ValidationFailure("Email", "El correo electrónico no es válido"),
                new FluentValidation.Results.ValidationFailure("Roles", "Rol no válido: InvalidRole")
            };
            
            var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
            
            // Configurar el mock para devolver el resultado de validación fallido
            _mockUpdateUserValidator.Invocations.Clear();
            _mockUpdateUserValidator
                .Setup(v => v.Validate(It.IsAny<UpdateUserDTO>()))
                .Returns(validationResult);
                
            _mockUpdateUserValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            
            // Act & Assert
            await _userService.Invoking(s => s.UpdateUserAsync(userId, invalidUserDto))
                .Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("Error de validación al actualizar el usuario");
        }

        [Fact]
        public async Task CreateUserAsync_WithDuplicateEmail_ThrowsValidationException()
        {
            // Arrange
            var duplicateEmail = "duplicate@example.com";
            var userDto = new CreateUserDTO
            {
                UserName = "testuser",
                Email = duplicateEmail,
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!",
                Roles = new List<string> { "User" }
            };
            
            // Configurar el mock para simular que el correo ya existe
            _mockUserManager.Setup(um => um.FindByEmailAsync(duplicateEmail))
                .ReturnsAsync(new ApplicationUser { 
                    Id = Guid.NewGuid(),
                    Email = duplicateEmail,
                    UserName = "existinguser"
                });
                
            // Configurar el validador para pasar
            _mockCreateUserValidator.Invocations.Clear();
            _mockCreateUserValidator
                .Setup(v => v.Validate(It.IsAny<CreateUserDTO>()))
                .Returns(new FluentValidation.Results.ValidationResult());
                
            _mockCreateUserValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            // Act & Assert
            await _userService.Invoking(s => s.CreateUserAsync(userDto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Ya existe un usuario con el mismo correo electrónico");
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicateEmail_ThrowsValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var duplicateEmail = "duplicate@example.com";
            
            var updateDto = new UpdateUserDTO
            {
                Id = userId,
                UserName = "testuser",
                Email = duplicateEmail,
                Roles = new List<string> { "User" }
            };
            
            // Configurar el mock para devolver el usuario existente
            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(new ApplicationUser { 
                    Id = userId,
                    UserName = "existinguser",
                    Email = "old@example.com"
                });
                
            // Configurar el mock para simular que ya existe otro usuario con el mismo correo
            _mockUserManager.Setup(um => um.FindByEmailAsync(duplicateEmail))
                .ReturnsAsync(new ApplicationUser { 
                    Id = Guid.NewGuid(), // Diferente ID
                    Email = duplicateEmail,
                    UserName = "otheruser"
                });
                
            // Configurar el validador para pasar
            _mockUpdateUserValidator.Invocations.Clear();
            _mockUpdateUserValidator
                .Setup(v => v.Validate(It.IsAny<UpdateUserDTO>()))
                .Returns(new FluentValidation.Results.ValidationResult());
                
            _mockUpdateUserValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            // Act & Assert
            await _userService.Invoking(s => s.UpdateUserAsync(userId, updateDto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Ya existe otro usuario con el mismo correo electrónico");
        }

        public void Dispose()
        {
            // Limpieza si es necesaria
        }
    }
}
