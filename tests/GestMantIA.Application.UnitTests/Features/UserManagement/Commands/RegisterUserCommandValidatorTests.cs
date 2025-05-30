/*
using Xunit;
using GestMantIA.Application.Features.UserManagement.Commands.RegisterUser;
using FluentAssertions;
using Moq;

namespace GestMantIA.Application.UnitTests.Features.UserManagement.Commands;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = string.Empty, Password = "P@ssword1", ConfirmPassword = "P@ssword1" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    // TODO: Añadir más pruebas para RegisterUserCommandValidator
}
*/
