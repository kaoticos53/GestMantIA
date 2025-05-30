using FluentAssertions;
using GestMantIA.API.Controllers;
using GestMantIA.Core.Identity.Interfaces; // Para IUserService
using GestMantIA.Shared.Identity.DTOs.Responses; // Para UserResponseDTO
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GestMantIA.API.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserById_Should_Return_OkObjectResult_With_User_When_User_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserResponseDTO { Id = userId.ToString(), UserName = "Test User", Email = "test@example.com" };
        _mockUserService.Setup(s => s.GetUserProfileAsync(userId.ToString())).ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUserProfile(userId.ToString());

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserResponseDTO>().Subject;
        returnedUser.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetUserById_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetUserProfileAsync(userId.ToString())).ReturnsAsync((UserResponseDTO?)null);

        // Act
        var result = await _controller.GetUserProfile(userId.ToString());

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // TODO: Implementar más pruebas para UsersController (GetAllUsers, CreateUser, UpdateUser, DeleteUser, etc.)
    // Considerar el uso de Microsoft.AspNetCore.Mvc.Testing para pruebas de integración más completas si es necesario.
}
