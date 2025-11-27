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
    /// Servicio para operaciones CRUD de contratos vía API REST.
    /// </summary>
    public class ContratoService
    {
        // Lista global de unidades válidas para contratos
        private static readonly HashSet<string> UnidadesValidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "par", "kg", "m", "ton", "jgo", "l", "tr", "lt", "pz", "Car"
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F2_PreparacionFabricacion/";

        public ContratoService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de contratos con paginación.
        /// </summary>
        public async Task<List<ContratoDTO>> ObtenerContratosAsync(int pageNumber = 1, int pageSize = 20)
        {
            var response = await _httpClient.GetAsync($"Contratos?pageNumber={pageNumber}&pageSize={pageSize}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var paginado = JsonSerializer.Deserialize<ContratoPaginadoDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return paginado?.Contratos ?? new List<ContratoDTO>();
        }

        /// <summary>
        /// Registra un nuevo contrato usando el archivo Create.json.
        /// </summary>
        public async Task<bool> RegistrarContratoAsync()
        {
            ContratoDTO contrato = new ContratoDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Contrato\\Create.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                contrato = JsonSerializer.Deserialize<ContratoDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (contrato == null || string.IsNullOrWhiteSpace(contrato.NoContrato) || string.IsNullOrWhiteSpace(contrato.TipoContrato))
            {
                Console.WriteLine("[VALIDACIÓN] El contrato debe tener número y tipo.");
                return false;
            }

            // Validación de unidades
            if (contrato.DetalleContrato != null)
            {
                foreach (var detalle in contrato.DetalleContrato)
                {
                    if (string.IsNullOrWhiteSpace(detalle.Unidad) || !UnidadesValidas.Contains(detalle.Unidad))
                    {
                        Console.WriteLine($"[VALIDACIÓN] La unidad '{detalle.Unidad}' no es válida. Las unidades permitidas son: {string.Join(",", UnidadesValidas)}");
                        return false;
                    }
                }
            }

            var json = JsonSerializer.Serialize(contrato);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Contratos", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[API SUCCESS] Respuesta: {responseContent}");
            return true;
        }

        /// <summary>
        /// Actualiza un contrato existente usando el archivo Edit.json.
        /// </summary>
        public async Task<bool> ActualizarContratoAsync(ContratoDTO contrato = null)
        {
            if (contrato == null)
            {
                var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Contrato\\Edit.json";
                if (File.Exists(mockPath))
                {
                    var jsonMock = File.ReadAllText(mockPath);
                    contrato = JsonSerializer.Deserialize<ContratoDTO>(jsonMock, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }

            if (contrato == null || string.IsNullOrWhiteSpace(contrato.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El contrato debe tener un Id válido.");
                return false;
            }

            // Validación de unidades
            if (contrato.DetalleContrato != null)
            {
                foreach (var detalle in contrato.DetalleContrato)
                {
                    if (string.IsNullOrWhiteSpace(detalle.Unidad) || !UnidadesValidas.Contains(detalle.Unidad))
                    {
                        Console.WriteLine($"[VALIDACIÓN] La unidad '{detalle.Unidad}' no es válida. Las unidades permitidas son: {string.Join(",", UnidadesValidas)}");
                        return false;
                    }
                }
            }

            var json = JsonSerializer.Serialize(contrato);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"Contratos", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[API SUCCESS] Respuesta: {responseContent}");
            return true;
        }

        /// <summary>
        /// Cambia el estatus de un contrato.
        /// </summary>
        public async Task<bool> CambiarEstatusContratoAsync(string contratoId, string nuevoEstatus)
        {
            if (string.IsNullOrWhiteSpace(contratoId) || string.IsNullOrWhiteSpace(nuevoEstatus))
            {
                Console.WriteLine("[VALIDACIÓN] Se requiere Id de contrato y nuevo estatus.");
                return false;
            }

            // Buscar el contrato por Id usando la paginación (puedes ajustar el pageSize si tienes muchos contratos)
            var contratos = await ObtenerContratosAsync(1, 1000);
            var contrato = contratos.Find(c => c.Id == contratoId);

            if (contrato == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se encontró el contrato con el Id proporcionado.");
                return false;
            }

            // Asignar el nuevo estatus
            contrato.Estatus = nuevoEstatus;

            // Usar el método de actualización existente
            return await ActualizarContratoAsync(contrato);
        }
    }
}