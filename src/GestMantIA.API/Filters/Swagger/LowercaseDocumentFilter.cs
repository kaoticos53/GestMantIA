using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace GestMantIA.API.Filters.Swagger
{
    /// <summary>
    /// Filtro para convertir todas las rutas a minúsculas en la documentación de Swagger
    /// </summary>
    public class LowercaseDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths.ToDictionary(
                entry => string.Join("/", entry.Key.Split('/')
                    .Select(x => x.Contains('{', StringComparison.Ordinal) ? x : x.ToLowerInvariant())),
                entry => entry.Value);

            swaggerDoc.Paths = new OpenApiPaths();
            
            foreach (var path in paths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }
}
