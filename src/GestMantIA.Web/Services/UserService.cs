using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GestMantIA.Core.Shared;
using GestMantIA.Shared.Identity.DTOs.Responses;
using GestMantIA.Web.Models.Users;
using GestMantIA.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GestMantIA.Web.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserService> _logger;
        private const string BasePath = "api/UserManagement";

        public UserService(IHttpClientFactory httpClientFactory, ILogger<UserService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        private HttpClient CreateHttpClient()
        {
            return _httpClientFactory.CreateClient("API");
        }

        public async Task<PagedResult<UserListModel>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool activeOnly = true)
        {
            try
            {
                var queryParams = new List<string>();
                
                queryParams.Add($"pageNumber={Uri.EscapeDataString(pageNumber.ToString())}");
                queryParams.Add($"pageSize={Uri.EscapeDataString(pageSize.ToString())}");

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

                if (!activeOnly)
                    queryParams.Add("activeOnly=false");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"{BasePath}?{queryString}";
                
                // Asegurarse de que la ruta no tenga barras duplicadas
                if (endpoint.StartsWith("/"))
                {
                    endpoint = endpoint.TrimStart('/');
                }

                using var httpClient = CreateHttpClient();
                
                _logger.LogInformation("Solicitando lista de usuarios desde: {BaseAddress}{Endpoint}", httpClient.BaseAddress?.ToString() ?? "null", endpoint);
                
                // Verificar que el HttpClient tenga configurada una dirección base
                if (httpClient.BaseAddress == null)
                {
                    _logger.LogError("La dirección base del HttpClient no está configurada.");
                    throw new InvalidOperationException("La dirección base del HttpClient no está configurada.");
                }

                var response = await httpClient.GetAsync(endpoint).ConfigureAwait(false);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var errorMessage = $"Error al obtener la lista de usuarios. Código: {response.StatusCode}";
                    _logger.LogError("{ErrorMessage}, Respuesta: {Response}", 
                        errorMessage, errorContent);
                        
                    if (!string.IsNullOrEmpty(errorContent) && errorContent.Contains("message"))
                    {
                        try
                        {
                            var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                            if (errorObj.TryGetProperty("message", out var message))
                            {
                                errorMessage = message.GetString() ?? errorMessage;
                            }
                        }
                        catch (JsonException)
                        {
                            // Si no se puede deserializar, usar el mensaje de error original
                        }
                    }
                    
                    throw new ApplicationException(errorMessage);
                }
                
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogInformation("Respuesta recibida: {Content}", content);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                try
                {
                    // Primero intentamos deserializar directamente a PagedResult<UserListModel>
                    var result = JsonSerializer.Deserialize<PagedResult<UserListModel>>(content, options);
                    if (result != null && result.Items != null && result.Items.Any())
                    {
                        // Fix for CS1503: Argumento 2: no se puede convertir de 'string' a 'Microsoft.Extensions.Logging.EventId'
                        // The issue is that the second argument in LogInformation expects an EventId, not a string.
                        // To fix this, we can remove the EventId argument or use the correct overload of LogInformation.

                        _logger.LogInformation("Usuarios recibidos (mapeo directo): {Count} usuarios", result.Items.Count());
                        return result;
                    }

                    // Si no hay elementos, intentamos con el DTO
                    var dtoResult = JsonSerializer.Deserialize<PagedResult<UserResponseDTO>>(content, options);
                    if (dtoResult?.Items != null)
                    {
                        // Mapear manualmente los DTOs a los modelos
                        var mappedItems = dtoResult.Items.Select(dto => new UserListModel
                        {
                            Id = dto.Id,
                            UserName = dto.UserName,
                            Email = dto.Email,
                            FirstName = dto.FirstName ?? string.Empty,
                            LastName = dto.LastName ?? string.Empty,
                            // IsActive se asume true si no está bloqueado
                            // EmailConfirmed no se mapea ya que no existe en UserListModel
                            // LockoutEnabled y LockoutEnd no se mapean directamente
                            Roles = dto.Roles?.ToList() ?? new List<string>()
                        }).ToList();

                        var mappedResult = new PagedResult<UserListModel> 
                    { 
                        Items = mappedItems, 
                        TotalCount = dtoResult.TotalCount,
                        PageNumber = dtoResult.PageNumber,
                        PageSize = dtoResult.PageSize
                    };

                        _logger.LogInformation("Usuarios recibidos (mapeo manual): {Count}", mappedItems.Count);
                        return mappedResult;
                    }


                    _logger.LogError("No se pudo deserializar la respuesta de la API. Contenido: {Content}", content);
                    return new PagedResult<UserListModel> 
                    { 
                        Items = new List<UserListModel>(), 
                        TotalCount = 0,
                        PageNumber = 1,
                        PageSize = pageSize
                    };
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error al deserializar la respuesta de la API. Contenido: {Content}", content);
                    throw new ApplicationException("Error al procesar la respuesta del servidor.", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                throw new ApplicationException("No se pudo obtener la lista de usuarios. Por favor, intente nuevamente.", ex);
            }
        }

        public async Task<UserListModel> GetUserByIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID de usuario no puede estar vacío", nameof(userId));

            try
            {
                using var httpClient = CreateHttpClient();
                var response = await httpClient.GetAsync($"{BasePath}/{userId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                return JsonSerializer.Deserialize<UserListModel>(content, options) ??
                    throw new InvalidOperationException("La respuesta de la API no es válida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID {UserId}", userId);
                throw new ApplicationException($"No se pudo obtener el usuario con ID {userId}", ex);
            }
        }

        public async Task<ApiResponse> CreateUserAsync(CreateUserModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                using var httpClient = CreateHttpClient();
                var response = await httpClient.PostAsJsonAsync($"{BasePath}", model);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Usuario creado exitosamente" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear usuario. Código: {StatusCode}, Respuesta: {Response}", 
                        response.StatusCode, errorContent);
                    
                    return new ApiResponse 
                    { 
                        Success = false, 
                        Message = $"Error al crear usuario: {response.ReasonPhrase}" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear el usuario");
                return new ApiResponse 
                { 
                    Success = false, 
                    Message = $"Error inesperado al crear el usuario: {ex.Message}" 
                };
            }
        }

        public async Task<ApiResponse> UpdateUserAsync(Guid userId, UpdateUserModel model)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID de usuario no puede estar vacío", nameof(userId));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                using var httpClient = CreateHttpClient();
                var response = await httpClient.PutAsJsonAsync($"{BasePath}/{userId}", model);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Usuario actualizado exitosamente" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar usuario. Código: {StatusCode}, Respuesta: {Response}",
                        response.StatusCode, errorContent);

                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error al actualizar usuario: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID {UserId}", userId);
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado al actualizar el usuario: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID de usuario no puede estar vacío", nameof(userId));

            try
            {
                using var httpClient = CreateHttpClient();
                var response = await httpClient.DeleteAsync($"{BasePath}/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Usuario eliminado exitosamente" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al eliminar usuario. Código: {StatusCode}, Respuesta: {Response}",
                        response.StatusCode, errorContent);

                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error al eliminar usuario: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID {UserId}", userId);
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado al eliminar el usuario: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> AssignRolesAsync(Guid userId, IEnumerable<string> roles)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID de usuario no puede estar vacío", nameof(userId));

            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            try
            {
                using var httpClient = CreateHttpClient();
                var response = await httpClient.PostAsJsonAsync($"{BasePath}/{userId}/roles", roles);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true, Message = "Roles asignados exitosamente" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al asignar roles. Código: {StatusCode}, Respuesta: {Response}",
                        response.StatusCode, errorContent);

                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error al asignar roles: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles al usuario con ID {UserId}", userId);
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error inesperado al asignar roles: {ex.Message}"
                };
            }
        }
    }
}
