using ApiClientLibrary.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiClientLibrary.Services
{
    /// <summary>
    /// Servicio para operaciones CRUD de productos y asociación de pruebas vía API REST.
    /// </summary>
    public class ProductoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        // Definición global de los valores válidos para tipo de fabricación
        private static readonly string[] TiposFabricacionValidos = { "LOTE", "SERIE" };

        public ProductoService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de productos desde la API.
        /// </summary>
        public async Task<List<ProductoResponseDTO>> ObtenerProductosAsync()
        {
            var response = await _httpClient.GetAsync("Producto");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var productos = JsonSerializer.Deserialize<List<ProductoResponseDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return productos ?? new List<ProductoResponseDTO>();
        }

        /// <summary>
        /// Registra un nuevo producto en la API usando un archivo JSON mock si no se pasa un producto.
        /// </summary>
        public async Task<bool> RegistrarProductoAsync()
        {
            ProductoDTO producto = new ProductoDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Producto\\Create.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                producto = JsonSerializer.Deserialize<ProductoDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (producto == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó un producto válido.");
                return false;
            }

            // Validaciones requeridas
            if (Array.IndexOf(TiposFabricacionValidos, producto.TipoFabricacion?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de fabricación inválido. Valores permitidos: LOTE, SERIE.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(producto.Norma))
            {
                Console.WriteLine("[VALIDACIÓN] La norma es requerida.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(producto.Prototipo))
            {
                Console.WriteLine("[VALIDACIÓN] El prototipo es requerido.");
                return false;
            }

            if (producto.Pruebas == null || producto.Pruebas.Count == 0)
            {
                Console.WriteLine("[VALIDACIÓN] Debe definir al menos una prueba.");
                return false;
            }

            var json = JsonSerializer.Serialize(producto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Producto", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Actualiza un producto existente en la API usando un archivo JSON mock si no se pasa un producto.
        /// </summary>
        public async Task<bool> ActualizarProductoAsync(ProductoDTO producto = null)
        {
            if (producto == null)
            {
                var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Producto\\Edit.json";

                if (File.Exists(mockPath))
                {
                    var jsonMock = File.ReadAllText(mockPath);
                    producto = JsonSerializer.Deserialize<ProductoDTO>(jsonMock, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }

            if (producto == null || string.IsNullOrEmpty(producto.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El producto debe tener un Id válido.");
                return false;
            }

            // Validaciones requeridas
            if (Array.IndexOf(TiposFabricacionValidos, producto.TipoFabricacion?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de fabricación inválido. Valores permitidos: LOTE, SERIE.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(producto.Norma))
            {
                Console.WriteLine("[VALIDACIÓN] La norma es requerida.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(producto.Prototipo))
            {
                Console.WriteLine("[VALIDACIÓN] El prototipo es requerido.");
                return false;
            }

            if (producto.Pruebas == null || producto.Pruebas.Count == 0)
            {
                Console.WriteLine("[VALIDACIÓN] Debe definir al menos una prueba.");
                return false;
            }

            var json = JsonSerializer.Serialize(producto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("Producto", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Asocia una lista de pruebas a un producto, validando que los IDs existan en las pruebas registradas y actualizando el producto.
        /// </summary>
        public async Task<bool> AsociarPruebasAProductoAsync(string productoId, List<string> pruebasIds)
        {
            if (string.IsNullOrEmpty(productoId) || pruebasIds == null || pruebasIds.Count == 0)
            {
                Console.WriteLine("[VALIDACIÓN] Se requiere un productoId y al menos un id de prueba.");
                return false;
            }

            // Validar que los IDs de pruebas existan usando PruebaService
            var pruebaService = new PruebaService(_httpClient, _configuration,
                _httpClient.DefaultRequestHeaders.Authorization.Parameter);

            var pruebasRegistradas = await pruebaService.ObtenerPruebasAsync();
            var idsValidos = new HashSet<string>(pruebasRegistradas.ConvertAll(p => p.Id));

            var idsInvalidos = new List<string>();
            foreach (var id in pruebasIds)
            {
                if (!idsValidos.Contains(id))
                    idsInvalidos.Add(id);
            }

            if (idsInvalidos.Count > 0)
            {
                Console.WriteLine($"[VALIDACIÓN] Los siguientes IDs de prueba no existen: {string.Join(", ", idsInvalidos)}");
                return false;
            }

            // Buscar el producto por productoId
            var productos = await ObtenerProductosAsync();
            var producto = productos.Find(p => p.Id == productoId);

            if (producto == null)
            {
                Console.WriteLine($"[VALIDACIÓN] No existe un producto con el Id: {productoId}");
                return false;
            }

            // Mapear ProductoResponseDTO a ProductoDTO
            var productoDTO = new ProductoDTO
            {
                Id = producto.Id,
                CodigoFabricante = producto.CodigoFabricante,
                Descripcion = producto.Descripcion,
                DescripcionCorta = producto.DescripcionCorta,
                TipoFabricacion = producto.TipoFabricacion,
                Unidad = producto.Unidad,
                Norma = producto.Norma?.Id, 
                Prototipo = producto.Prototipo?.Id, 
                Estatus = producto.Estatus,
                FechaRegistro = producto.FechaRegistro,
                Pruebas = pruebasIds 
            };

            // Validaciones requeridas
            if (Array.IndexOf(TiposFabricacionValidos, productoDTO.TipoFabricacion?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de fabricación inválido. Valores permitidos: LOTE, SERIE.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(productoDTO.Norma))
            {
                Console.WriteLine("[VALIDACIÓN] La norma es requerida.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(productoDTO.Prototipo))
            {
                Console.WriteLine("[VALIDACIÓN] El prototipo es requerido.");
                return false;
            }

            if (productoDTO.Pruebas == null || productoDTO.Pruebas.Count == 0)
            {
                Console.WriteLine("[VALIDACIÓN] Debe definir al menos una prueba.");
                return false;
            }

            return await ActualizarProductoAsync(productoDTO);
        }
    }
}