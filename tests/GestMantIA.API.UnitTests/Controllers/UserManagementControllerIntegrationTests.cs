using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        private readonly string _testUserEmail = "admin@localhost";
        private readonly string _testUserPassword = "Admin1234!";

        public UserManagementControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                UsernameOrEmail = _testUserEmail,
                Password = _testUserPassword
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/Auth/login", content);
            response.EnsureSuccessStatusCode();
            // Pseudocódigo detallado:
            // 1. Leer el contenido de la respuesta como string.
            // 2. Intentar deserializar el string como un objeto JSON.
            // 3. Verificar si el JSON tiene la propiedad "AccessToken".
            // 4. Si falla, mostrar el contenido real para depuración.

            // Código para depuración:
            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                if (!doc.RootElement.TryGetProperty("AccessToken", out var accessTokenElement))
                {
                    throw new InvalidOperationException($"No se encontró 'AccessToken'. Respuesta: {responseContent}");
                }
                return accessTokenElement.GetString() ?? throw new InvalidOperationException("El token es nulo");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error al deserializar la respuesta: {responseContent}", ex);
            }
        }

        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var token = await GetAuthTokenAsync();
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserIsUpdatedSuccessfully()
        {
            // Arrange: crea un usuario de prueba y obtiene su ID
            var client = await CreateAuthenticatedClientAsync();
            
            var createUserDto = new CreateUserDTO 
            { 
                UserName = "testuser", 
                Email = "testuser@email.com", 
                Password = "Test1234!", 
                ConfirmPassword = "Test1234!", 
                Roles = new List<string> { "User" } 
            };
            
            var createResponse = await client.PostAsJsonAsync("/api/UserManagement", createUserDto);
            createResponse.EnsureSuccessStatusCode();
            
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponseDTO>();
            createdUser.Should().NotBeNull();
            createdUser.Id.Should().NotBe(Guid.Empty);
            
            // Prepara los datos de actualización
            var updateDto = new UpdateUserDTO 
            { 
                Id = createdUser.Id,
                UserName = "updateduser", 
                Email = "updateduser@email.com", 
                Roles = new List<string> { "User" } 
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/UserManagement/{createdUser.Id}", updateDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedUser = await response.Content.ReadFromJsonAsync<UserResponseDTO>();
            updatedUser.Should().NotBeNull();
            updatedUser.Id.Should().Be(createdUser.Id);
            updatedUser.UserName.Should().Be(updateDto.UserName);
            updatedUser.Email.Should().Be(updateDto.Email);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();
            
            var updateDto = new UpdateUserDTO 
            { 
                Id = Guid.NewGuid(), 
                UserName = "nouser", 
                Email = "nouser@email.com", 
                Roles = new List<string> { "User" } 
            };
            
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var response = await client.PutAsJsonAsync($"/api/UserManagement/{nonExistentUserId}", updateDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenDtoIsInvalid()
        {
            // Arrange: crea un usuario de prueba
            var client = await CreateAuthenticatedClientAsync();
            
            var createUserDto = new CreateUserDTO 
            { 
                UserName = "testuser2", 
                Email = "testuser2@email.com", 
                Password = "Test1234!", 
                ConfirmPassword = "Test1234!",
                Roles = new List<string> { "User" } 
            };
            
            var createResponse = await client.PostAsJsonAsync("/api/UserManagement", createUserDto);
            createResponse.EnsureSuccessStatusCode();
            
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponseDTO>();
            createdUser.Should().NotBeNull();
            createdUser.Id.Should().NotBe(Guid.Empty);
            
            var updateDto = new UpdateUserDTO 
            { 
                Id = Guid.NewGuid(), // ID diferente al de la URL
                UserName = "badrequestuser2", 
                Email = "badrequest2@email.com", 
                Roles = new List<string> { "User" } 
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/UserManagement/{createdUser.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
