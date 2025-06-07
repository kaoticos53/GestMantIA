using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace GestMantIA.Web.HttpClients
{
    /// <summary>
    /// DelegatingHandler que agrega el token de autenticación a las solicitudes HTTP salientes.
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private const string AuthTokenKey = "authToken";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AuthHeaderHandler"/>
        /// </summary>
        /// <param name="localStorage">Servicio de almacenamiento local para obtener el token de autenticación.</param>
        public AuthHeaderHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Envía una solicitud HTTP al servidor con el token de autenticación.
        /// </summary>
        /// <param name="request">La solicitud HTTP a enviar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>La respuesta HTTP.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            // Obtener el token del almacenamiento local
            var token = await _localStorage.GetItemAsStringAsync(AuthTokenKey, cancellationToken);

            // Agregar el token a la cabecera de autorización si existe
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Eliminar comillas si existen (pueden ser agregadas por el almacenamiento local)
                token = token.Trim('"');
                
                // Agregar el token a la cabecera de autorización
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Agregar cabeceras adicionales si es necesario
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Continuar con el envío de la solicitud
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
