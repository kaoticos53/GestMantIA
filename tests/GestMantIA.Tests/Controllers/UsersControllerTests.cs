using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestMantIA.API.Controllers;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GestMantIA.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<UsersController>> _loggerMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            
            _controller = new UsersController(_userServiceMock.Object, _loggerMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task GetUserProfile_UserExists_ReturnsOkWithUserProfile()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var expectedProfile = new UserResponseDTO
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                Roles = new List<string>()
            };

            _userServiceMock.Setup(s => s.GetUserProfileAsync(userId))
                .ReturnsAsync(expectedProfile);

            // Act
            var result = await _controller.GetUserProfile(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserResponseDTO>(okResult.Value);
            Assert.Equal(expectedProfile.Id, returnValue.Id);
            Assert.Equal(expectedProfile.UserName, returnValue.UserName);
        }

        [Fact]
        public async Task GetUserProfile_UserNotExists_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _userServiceMock.Setup(s => s.GetUserProfileAsync(userId))
                .ReturnsAsync((UserResponseDTO)null);

            // Act
            var result = await _controller.GetUserProfile(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        
        [Fact]
        public async Task SearchUsers_WithSearchTerm_ReturnsPagedResults()
        {
            // Arrange
            var searchTerm = "test";
            var pageNumber = 1;
            var pageSize = 10;
            
            var expectedResults = new PagedResult<UserResponseDTO>
            {
                Items = new List<UserResponseDTO>
                {
                    new() { Id = "1", Username = "testuser1", Email = "test1@example.com", Roles = new List<string>() },
                    new() { Id = "2", Username = "testuser2", Email = "test2@example.com", Roles = new List<string>() }
                },
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 2
            };

            _userServiceMock.Setup(s => s.SearchUsersAsync(searchTerm, pageNumber, pageSize))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.SearchUsers(searchTerm, pageNumber, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PagedResult<UserResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.TotalCount);
            Assert.Equal(2, returnValue.Items.Count());
        }

        [Fact]
        public async Task UpdateUserProfile_ValidData_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var updateDto = new UpdateProfileDTO
            {
                FirstName = "Updated",
                LastName = "User",
                PhoneNumber = "123456789"
            };

            _userServiceMock.Setup(s => s.UpdateUserProfileAsync(userId, updateDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUserProfile(userId, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }


        [Fact]
        public async Task UpdateUserProfile_UserNotExists_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var updateDto = new UpdateProfileDTO();

            _userServiceMock.Setup(s => s.UpdateUserProfileAsync(userId, updateDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUserProfile(userId, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
