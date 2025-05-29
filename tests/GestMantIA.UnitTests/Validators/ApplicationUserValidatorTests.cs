using FluentAssertions;
using FluentValidation.TestHelper;
using GestMantIA.API.Validators;
using GestMantIA.Core.Identity.Entities;
using Xunit;

namespace GestMantIA.UnitTests.Validators
{
    public class ApplicationUserValidatorTests
    {
        private readonly ApplicationUserValidator _validator;

        public ApplicationUserValidatorTests()
        {
            _validator = new ApplicationUserValidator();
        }

        [Fact]
        public void ValidUser_ShouldPass_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "usuario1",
                Email = "usuario1@example.com"
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyUsername_ShouldFail_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "",
                Email = "usuario1@example.com"
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.UserName);
        }

        [Fact]
        public void ShortUsername_ShouldFail_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "ab",
                Email = "usuario1@example.com"
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.UserName)
                .WithErrorMessage("El nombre de usuario debe tener entre 3 y 50 caracteres");
        }

        [Fact]
        public void LongUsername_ShouldFail_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = new string('a', 51),
                Email = "usuario1@example.com"
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.UserName)
                .WithErrorMessage("El nombre de usuario debe tener entre 3 y 50 caracteres");
        }

        [Fact]
        public void EmptyEmail_ShouldFail_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "usuario1",
                Email = ""
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Email)
                .WithErrorMessage("El correo electrónico es obligatorio");
        }

        [Fact]
        public void InvalidEmail_ShouldFail_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "usuario1",
                Email = "correo_invalido"
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Email)
                .WithErrorMessage("El correo electrónico no tiene un formato válido");
        }

        [Fact]
        public void LongEmail_ShouldFail_Validation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "usuario1",
                Email = new string('a', 95) + "@test.com"
            };

            // Act
            var result = _validator.TestValidate(user);

            // Assert
            result.ShouldHaveValidationErrorFor(u => u.Email)
                .WithErrorMessage("El correo electrónico no puede tener más de 100 caracteres");
        }
    }
}
