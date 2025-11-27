using ApiClientLibrary.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApiClientLibrary.Services
{
    /// <summary>
    /// Servicio para operaciones CRUD de órdenes de fabricación vía API REST.
    /// </summary>
    public class OrdenFabricacionService
    {
        // Lista global de tipos de contrato válidos
        private static readonly HashSet<string> TiposContratoValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "ContratoCFE", "ContratoCFEConGarantia", "ContratoParticular"
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F2_PreparacionFabricacion/";
        private readonly string _mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\OrdenFabricacion\\";

        public OrdenFabricacionService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Crea una nueva orden de fabricación usando un archivo JSON mock si no se pasa una orden.
        /// </summary>
        public async Task<List<OrdenFabricacionDTO>> CrearOrdenFabricacionAsync(OrdenFabricacionDTO orden = null)
        {
            if (orden == null)
            {
                var file = Path.Combine(_mockPath, "Create.json");
                if (File.Exists(file))
                {
                    var jsonMock = File.ReadAllText(file);
                    orden = JsonSerializer.Deserialize<OrdenFabricacionDTO>(jsonMock, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }

            // Validaciones
            if (orden == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó una orden de fabricación válida.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(orden.ClaveOrdenFabricacion))
            {
                Console.WriteLine("[VALIDACIÓN] La clave de fabricación es requerida.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(orden.LoteFabricacion))
            {
                Console.WriteLine("[VALIDACIÓN] El lote de fabricación es requerido.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(orden.IdProducto))
            {
                Console.WriteLine("[VALIDACIÓN] El producto es requerido.");
                return null;
            }
            if (orden.DetalleFabricacion == null || orden.DetalleFabricacion.Count == 0)
            {
                Console.WriteLine("[VALIDACIÓN] Debe existir al menos un detalle de fabricación.");
                return null;
            }

            foreach (var detalle in orden.DetalleFabricacion)
            {
                if (detalle.CantidadAFabricar <= 0)
                {
                    Console.WriteLine("[VALIDACIÓN] La cantidad a fabricar debe ser mayor a cero.");
                    return null;
                }
                if (string.IsNullOrWhiteSpace(detalle.TipoContrato) || !TiposContratoValidos.Contains(detalle.TipoContrato))
                {
                    Console.WriteLine("[VALIDACIÓN] Tipo de contrato inválido. Valores permitidos: ContratoCFE, ContratoCFEConGarantia, ContratoParticular.");
                    return null;
                }
            }

            var json = JsonSerializer.Serialize(orden);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("OrdenFabricacion", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var ordenes = JsonSerializer.Deserialize<List<OrdenFabricacionDTO>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return ordenes;
        }

        /// <summary>
        /// Actualiza una orden de fabricación existente usando un archivo JSON mock si no se pasa una orden.
        /// </summary>
        public async Task<List<OrdenFabricacionDTO>> ActualizarOrdenFabricacionAsync(OrdenFabricacionDTO orden = null)
        {
            if (orden == null)
            {
                var file = Path.Combine(_mockPath, "Edit.json");
                if (File.Exists(file))
                {
                    var jsonMock = File.ReadAllText(file);
                    orden = JsonSerializer.Deserialize<OrdenFabricacionDTO>(jsonMock, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }

            // Validaciones
            if (orden == null || string.IsNullOrWhiteSpace(orden.Id))
            {
                Console.WriteLine("[VALIDACIÓN] La orden debe tener un Id válido.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(orden.ClaveOrdenFabricacion))
            {
                Console.WriteLine("[VALIDACIÓN] La clave de fabricación es requerida.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(orden.LoteFabricacion))
            {
                Console.WriteLine("[VALIDACIÓN] El lote de fabricación es requerido.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(orden.IdProducto))
            {
                Console.WriteLine("[VALIDACIÓN] El producto es requerido.");
                return null;
            }
            if (orden.DetalleFabricacion == null || orden.DetalleFabricacion.Count == 0)
            {
                Console.WriteLine("[VALIDACIÓN] Debe existir al menos un detalle de fabricación.");
                return null;
            }

            foreach (var detalle in orden.DetalleFabricacion)
            {
                if (detalle.CantidadAFabricar <= 0)
                {
                    Console.WriteLine("[VALIDACIÓN] La cantidad a fabricar debe ser mayor a cero.");
                    return null;
                }
                if (string.IsNullOrWhiteSpace(detalle.TipoContrato) || !TiposContratoValidos.Contains(detalle.TipoContrato))
                {
                    Console.WriteLine("[VALIDACIÓN] Tipo de contrato inválido. Valores permitidos: ContratoCFE, ContratoCFEConGarantia, ContratoParticular.");
                    return null;
                }
            }

            var json = JsonSerializer.Serialize(orden);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("OrdenFabricacion", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var ordenes = JsonSerializer.Deserialize<List<OrdenFabricacionDTO>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return ordenes;
        }

        /// <summary>
        /// Obtiene una lista de órdenes de fabricación por Id (puede ser una o varias).
        /// </summary>
        public async Task<List<OrdenFabricacionDTO>> ObtenerOrdenFabricacionAsync(string ordenId)
        {
            if (string.IsNullOrWhiteSpace(ordenId))
                return null;

            var response = await _httpClient.GetAsync($"OrdenFabricacion/{ordenId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var ordenes = JsonSerializer.Deserialize<List<OrdenFabricacionDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return ordenes;
        }

        /// <summary>
        /// Valida si la orden de fabricación está completa (integridad).
        /// </summary>
        public async Task<bool> ValidarOrdenCompletaAsync(string ordenId)
        {
            var ordenes = await ObtenerOrdenFabricacionAsync(ordenId);
            if (ordenes == null || ordenes.Count == 0)
                return false;

            foreach (var orden in ordenes)
            {
                if (string.IsNullOrWhiteSpace(orden.Id) ||
                    string.IsNullOrWhiteSpace(orden.ClaveOrdenFabricacion) ||
                    string.IsNullOrWhiteSpace(orden.LoteFabricacion) ||
                    string.IsNullOrWhiteSpace(orden.IdProducto) ||
                    orden.DetalleFabricacion == null || orden.DetalleFabricacion.Count == 0)
                    return false;
            }
            return true;
        }
    }
}