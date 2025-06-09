using System.Net;
using FluentAssertions;
using GestMantIA.API.Controllers;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Identity.Results;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using GestMantIA.Infrastructure.Services;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Headers;

namespace GestMantIA.API.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<ICookieService> _mockCookieService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockCookieService = new Mock<ICookieService>();
        
        // Configuración básica del mock de CookieService
        _mockCookieService.Setup(c => c.GetRefreshTokenFromCookie()).Returns(string.Empty);
        
        _controller = new AuthController(
            _mockAuthService.Object, 
            _mockLogger.Object, 
            _mockCookieService.Object);
            
        // Configurar el HttpContext para el controlador con encabezado Origin
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://test.com";
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        
        // Configurar RemoteIpAddress para pruebas de GetIpAddress
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
    }

    [Fact]
    public async Task Login_Should_Return_OkObjectResult_With_Token_When_Credentials_Are_Valid()
    {
        // Arrange
        var loginRequest = new LoginRequest { UsernameOrEmail = "test@example.com", Password = "P@ssword1" };
        var userInfo = new UserInfo 
        { 
            Id = "1",
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            FullName = "Test User"
        };
        var authResponse = AuthenticationResult.Success("fake_jwt_token", "fake_refresh_token", DateTime.UtcNow.AddHours(1), userInfo);
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<LoginRequest>(), It.IsAny<string>())).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var actionResult = result.Should().BeOfType<ActionResult<AuthenticationResult>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        returnedResponse.Should().BeEquivalentTo(authResponse);
        returnedResponse.AccessToken.Should().NotBeNullOrEmpty();
        
        // Verificar que se llamó a SetRefreshTokenCookie
        _mockCookieService.Verify(
            c => c.SetRefreshTokenCookie(authResponse.RefreshToken, It.IsAny<DateTime?>()), 
            Times.Once);
    }

    [Fact]
    public async Task Login_Should_Return_UnauthorizedObjectResult_When_Credentials_Are_Invalid()
    {
        // Arrange
        var loginRequest = new LoginRequest { UsernameOrEmail = "test@example.com", Password = "WrongPassword" };
        var authResponse = AuthenticationResult.Failure(new List<string> { "Credenciales inválidas" });
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<LoginRequest>(), It.IsAny<string>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var actionResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var returnedResponse = actionResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        returnedResponse.Succeeded.Should().BeFalse();
        returnedResponse.Message.Should().Be("Error de autenticación");
        
        // Verificar que no se llamó a SetRefreshTokenCookie
        _mockCookieService.Verify(
            c => c.SetRefreshTokenCookie(It.IsAny<string>(), It.IsAny<DateTime?>()), 
            Times.Never);
    }
    
    [Fact]
    public async Task RefreshToken_Should_Return_New_Tokens_When_Valid_RefreshToken()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";
        var userInfo = new UserInfo 
        { 
            Id = "1",
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            FullName = "Test User"
        };
        var newTokens = AuthenticationResult.Success("new_jwt_token", "new_refresh_token", DateTime.UtcNow.AddHours(1), userInfo);
        
        _mockCookieService.Setup(c => c.GetRefreshTokenFromCookie()).Returns(refreshToken);
        _mockAuthService.Setup(s => s.RefreshTokenAsync(refreshToken, It.IsAny<string>()))
            .ReturnsAsync(newTokens);
            
        // Act
        var result = await _controller.RefreshToken();
        
        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        returnedResponse.Should().BeEquivalentTo(newTokens);
        
        // Verificar que se llamó a SetRefreshTokenCookie con el nuevo token
        _mockCookieService.Verify(
            c => c.SetRefreshTokenCookie(newTokens.RefreshToken, It.IsAny<DateTime?>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task RefreshToken_Should_Return_BadRequest_When_No_RefreshToken_In_Cookie()
    {
        // Arrange
        _mockCookieService.Setup(c => c.GetRefreshTokenFromCookie()).Returns((string)null);
        
        // Act
        var result = await _controller.RefreshToken();
        
        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Token de actualización no proporcionado");
    }
    
    [Fact]
    public async Task RevokeToken_Should_Return_Ok_When_Token_Is_Revoked()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";
        _mockCookieService.Setup(c => c.GetRefreshTokenFromCookie()).Returns(refreshToken);
        _mockAuthService.Setup(s => s.RevokeTokenAsync(refreshToken, It.IsAny<string>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _controller.RevokeToken();
        
        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Token revocado exitosamente" });
        
        // Verificar que se llamó a DeleteRefreshTokenCookie
        _mockCookieService.Verify(c => c.DeleteRefreshTokenCookie(), Times.Once);
    }
    
    [Fact]
    public async Task RevokeToken_Should_Return_BadRequest_When_No_RefreshToken_In_Cookie()
    {
        // Arrange
        _mockCookieService.Setup(c => c.GetRefreshTokenFromCookie()).Returns((string)null);
        
        // Act
        var result = await _controller.RevokeToken();
        
        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Token de actualización no proporcionado");
    }
    
    [Fact]
    public async Task RevokeToken_Should_Return_BadRequest_When_Revoke_Fails()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";
        _mockCookieService.Setup(c => c.GetRefreshTokenFromCookie()).Returns(refreshToken);
        _mockAuthService.Setup(s => s.RevokeTokenAsync(refreshToken, It.IsAny<string>()))
            .ReturnsAsync(false);
            
        // Act
        var result = await _controller.RevokeToken();
        
        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("No se pudo revocar el token");
    }
    
    [Fact]
    public void GetOrigin_Should_Return_Origin_From_Request_Header()
    {
        // Arrange
        var expectedOrigin = "https://example.com";
        _controller.HttpContext.Request.Headers.Origin = expectedOrigin;
        
        // Act & Assert - Verificar que el controlador maneja correctamente el encabezado Origin
        var originHeader = _controller.HttpContext.Request.Headers.Origin.ToString();
        originHeader.Should().Be(expectedOrigin);
    }
    
    [Fact]
    public void GetOrigin_Should_Return_Default_When_No_Origin_Header()
    {
        // Arrange
        _controller.HttpContext.Request.Headers.Remove("Origin");
        
        // Act & Assert - Verificar que el controlador maneja correctamente la ausencia del encabezado Origin
        var originHeader = _controller.HttpContext.Request.Headers.Origin.ToString();
        originHeader.Should().BeEmpty();
    }
    
    [Fact]
    public void GetIpAddress_Should_Return_First_From_XForwardedFor()
    {
        // Arrange
        var expectedIp = "192.168.1.1";
        _controller.HttpContext.Request.Headers["X-Forwarded-For"] = $"{expectedIp}, 10.0.0.1";
        
        // Act & Assert - Verificar que podemos obtener la IP del encabezado X-Forwarded-For
        var xForwardedFor = _controller.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
        var firstIp = xForwardedFor?.Split(',').FirstOrDefault()?.Trim();
        firstIp.Should().Be(expectedIp);
    }
    
    [Fact]
    public void GetIpAddress_Should_Return_RemoteIpAddress_When_No_XForwardedFor()
    {
        // Arrange
        var expectedIp = "192.168.1.100";
        _controller.HttpContext.Request.Headers.Remove("X-Forwarded-For");
        _controller.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse(expectedIp);
        
        // Act & Assert - Verificar que podemos obtener la IP remota
        var remoteIp = _controller.HttpContext.Connection.RemoteIpAddress?.ToString();
        remoteIp.Should().Be(expectedIp);
    }
    
    [Fact]
    public async Task Register_Should_Return_Ok_When_Registration_Succeeds()
    {
        // Arrange
        var registerRequest = new RegisterRequest 
        { 
            UserName = "testuser",
            Email = "test@example.com", 
            Password = "P@ssword1", 
            ConfirmPassword = "P@ssword1",
            FullName = "Test User"
        };
        
        var userId = Guid.NewGuid();
        var registerResult = RegisterResult.Success(userId, false, "Usuario registrado exitosamente");
        _mockAuthService.Setup(s => s.RegisterAsync(It.Is<RegisterRequest>(r => 
            r.UserName == "testuser" && 
            r.Email == "test@example.com" &&
            r.Password == "P@ssword1" &&
            r.ConfirmPassword == "P@ssword1" &&
            r.FullName == "Test User"), 
            It.IsAny<string>()))
            .ReturnsAsync(registerResult);
            
        // Configurar el controlador para devolver un origen
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                Request = { Headers = { ["Origin"] = "https://test.com" } }
            }
        };
            
        // Act
        var result = await _controller.Register(registerRequest);
        
        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<RegisterResult>().Subject;
        returnedResponse.Succeeded.Should().BeTrue();
        returnedResponse.Message.Should().Be("Usuario registrado exitosamente");
    }
    
    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        var registerRequest = new RegisterRequest();
        _controller.ModelState.AddModelError("Email", "El correo electrónico es requerido");
        
        // Act
        var result = await _controller.Register(registerRequest);
        
        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeOfType<SerializableError>();
    }
    
    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Registration_Fails()
    {
        // Arrange
        var registerRequest = new RegisterRequest 
        { 
            UserName = "testuser",
            Email = "test@example.com", 
            Password = "P@ssword1", 
            ConfirmPassword = "P@ssword1",
            FullName = "Test User"
        };
        
        var errorMessage = "Error en el registro";
        var registerResult = RegisterResult.Failure(new List<string> { errorMessage });
        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<string>()))
            .ReturnsAsync(registerResult);
            
        // Configurar el controlador para devolver un origen
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                Request = { Headers = { ["Origin"] = "https://test.com" } }
            }
        };
            
        // Act
        var result = await _controller.Register(registerRequest);
        
        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedResponse = badRequestResult.Value.Should().BeOfType<RegisterResult>().Subject;
        returnedResponse.Succeeded.Should().BeFalse();
        returnedResponse.Message.Should().Be(errorMessage);
    }
    
    // TODO: Implementar más pruebas para AuthController (Register, RefreshToken, etc.)
}
