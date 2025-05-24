using FluentAssertions;
using FluentValidation.TestHelper;
using GestMantIA.API.Validators;
using GestMantIA.Core.Models;
using Xunit;

namespace GestMantIA.UnitTests.Validators
{
    public class UsuarioValidatorTests
    {
        private readonly UsuarioValidator _validator;

        public UsuarioValidatorTests()
        {
            _validator = new UsuarioValidator();
        }

        [Fact]
        public void ValidUser_ShouldPass_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = "usuario1@example.com",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyUsername_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "",
                Email = "usuario1@example.com",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.NombreUsuario);
        }

        [Fact]
        public void ShortUsername_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "ab",
                Email = "usuario1@example.com",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.NombreUsuario);
        }

        [Fact]
        public void LongUsername_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = new string('a', 51),
                Email = "usuario1@example.com",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.NombreUsuario);
        }

        [Fact]
        public void EmptyEmail_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = "",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Email);
        }

        [Fact]
        public void InvalidEmail_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = "correo_invalido",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Email);
        }

        [Fact]
        public void LongEmail_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = new string('a', 95) + "@test.com",
                Contrasena = "Contraseña123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Email);
        }

        [Fact]
        public void EmptyPassword_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = "usuario1@example.com",
                Contrasena = ""
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Contrasena);
        }

        [Fact]
        public void ShortPassword_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = "usuario1@example.com",
                Contrasena = "123"
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Contrasena);
        }

        [Fact]
        public void LongPassword_ShouldFail_Validation()
        {
            // Arrange
            var usuario = new Usuario
            {
                NombreUsuario = "usuario1",
                Email = "usuario1@example.com",
                Contrasena = new string('1', 101)
            };

            // Act
            var result = _validator.TestValidate(usuario);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Contrasena);
        }
    }
}
