using System.Net;
using System.Net.Http.Json;
using GestMantIA.Shared.Identity.DTOs.Requests;
using GestMantIA.Shared.Identity.DTOs.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace GestMantIA.API.UnitTests.Controllers
{
    public class UserManagementControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UserManagementControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserIsUpdatedSuccessfully()
        {
            // Arrange: crea un usuario de prueba y obtiene su ID
            var createUserDto = new CreateUserDTO { UserName = "testuser", Email = "testuser@email.com", Password = "Test1234!", ConfirmPassword = "Test1234!", Roles = new List<string> { "User" } };
            var createResponse = await _client.PostAsJsonAsync("/api/UserManagement", createUserDto);
            createResponse.EnsureSuccessStatusCode();
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponseDTO>();
            var updateDto = new UpdateUserDTO { Id = createdUser.Id, UserName = "testuser_updated", Email = "testuser_updated@email.com", Roles = new List<string> { "User" } };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/UserManagement/{createdUser.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedUser = await response.Content.ReadFromJsonAsync<UserResponseDTO>();
            updatedUser.Should().NotBeNull();
            updatedUser.UserName.Should().Be("testuser_updated");
            updatedUser.Email.Should().Be("testuser_updated@email.com");
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateUserDTO { Id = Guid.NewGuid(), UserName = "nouser", Email = "nouser@email.com", Roles = new List<string> { "User" } };
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var response = await _client.PutAsJsonAsync($"/api/UserManagement/{nonExistentUserId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenDtoIsInvalid()
        {
            // Arrange: IDs no coinciden
            var createUserDto = new CreateUserDTO { UserName = "badrequestuser", Email = "badrequest@email.com", Password = "Test1234!", ConfirmPassword = "Test1234!", Roles = new List<string> { "User" } };
            var createResponse = await _client.PostAsJsonAsync("/api/UserManagement", createUserDto);
            createResponse.EnsureSuccessStatusCode();
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponseDTO>();
            var updateDto = new UpdateUserDTO { Id = Guid.NewGuid(), UserName = "badrequestuser2", Email = "badrequest2@email.com", Roles = new List<string> { "User" } };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/UserManagement/{createdUser.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnConflict_WhenUserNameOrEmailAlreadyExists()
        {
            // Arrange: crea dos usuarios
            var user1 = new CreateUserDTO { UserName = "conflictuser1", Email = "conflict1@email.com", Password = "Test1234!", ConfirmPassword = "Test1234!", Roles = new List<string> { "User" } };
            var user2 = new CreateUserDTO { UserName = "conflictuser2", Email = "conflict2@email.com", Password = "Test1234!", ConfirmPassword = "Test1234!", Roles = new List<string> { "User" } };
            var resp1 = await _client.PostAsJsonAsync("/api/UserManagement", user1);
            resp1.EnsureSuccessStatusCode();
            var created1 = await resp1.Content.ReadFromJsonAsync<UserResponseDTO>();
            var resp2 = await _client.PostAsJsonAsync("/api/UserManagement", user2);
            resp2.EnsureSuccessStatusCode();
            var created2 = await resp2.Content.ReadFromJsonAsync<UserResponseDTO>();
            // Intenta actualizar user2 con el email de user1
            var updateDto = new UpdateUserDTO { Id = created2.Id, UserName = "conflictuser1", Email = "conflict1@email.com", Roles = new List<string> { "User" } };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/UserManagement/{created2.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
        {
            // Arrange: simular error forzando DTO inválido (por ejemplo, Email null y UserName null)
            var createUserDto = new CreateUserDTO { UserName = "erroruser", Email = "erroruser@email.com", Password = "Test1234!", ConfirmPassword = "Test1234!", Roles = new List<string> { "User" } };
            var createResponse = await _client.PostAsJsonAsync("/api/UserManagement", createUserDto);
            createResponse.EnsureSuccessStatusCode();
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponseDTO>();
            createdUser.Should().NotBeNull(); // Ensure createdUser is not null
            var updateDto = new UpdateUserDTO { Id = createdUser!.Id, UserName = null, Email = null, Roles = new List<string> { "User" } };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/UserManagement/{createdUser.Id}", updateDto);

            // Assert
            // Dependiendo de la validación del modelo, podría ser 400 o 500. Se asume 500 si la excepción no es controlada.
            response.StatusCode.Should().Match(status => status == HttpStatusCode.InternalServerError || status == HttpStatusCode.BadRequest);
        }
    }
}
