using FluentAssertions;
using GestMantIA.API.Controllers;
using GestMantIA.Core.Identity.Interfaces; // Para IAuthenticationService
using GestMantIA.Core.Identity.Results; // Para AuthenticationResult
using GestMantIA.Shared.Identity.DTOs; // Para LoginRequest, RegisterRequest, etc.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GestMantIA.API.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        // Setup HttpContextAccessor mock if GetIpAddress() relies on it heavily, for now basic mock
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object, _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task Login_Should_Return_OkObjectResult_With_Token_When_Credentials_Are_Valid()
    {
        // Arrange
        var loginRequest = new LoginRequest { UsernameOrEmail = "test@example.com", Password = "P@ssword1" }; // Asumiendo propiedades Email y Password
        var authResponse = AuthenticationResult.Success("fake_jwt_token", "fake_refresh_token", DateTime.UtcNow.AddHours(1), null); // Ajustar según constructor o factory method de AuthenticationResult
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<LoginRequest>(), It.IsAny<string>())).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        returnedResponse.Should().BeEquivalentTo(authResponse);
        returnedResponse.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_Should_Return_UnauthorizedObjectResult_When_Credentials_Are_Invalid()
    {
        // Arrange
        var loginRequest = new LoginRequest { UsernameOrEmail = "wrong@example.com", Password = "WrongP@ssword" };
        var authResponse = AuthenticationResult.Failure(new List<string> { "Error general" }, "Invalid credentials");
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<LoginRequest>(), It.IsAny<string>())).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var returnedResponse = unauthorizedResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        returnedResponse.Succeeded.Should().BeFalse();
        returnedResponse.AccessToken.Should().BeNull();
    }

    // TODO: Implementar más pruebas para AuthController (Register, RefreshToken, etc.)
}
