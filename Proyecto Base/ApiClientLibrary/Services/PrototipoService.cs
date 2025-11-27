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
    /// Servicio para operaciones CRUD de prototipos vía API REST.
    /// </summary>
    public class PrototipoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        /// <summary>
        /// Inicializa el servicio con autenticación Bearer.
        /// </summary>
        public PrototipoService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de prototipos desde la API.
        /// Si el archivo Prototipos/Edit.json existe, lo deserializa y lo retorna como lista (modo mock).
        /// Si no, consulta la API.
        /// </summary>
        public async Task<List<PrototipoDTO>> ObtenerPrototiposAsync()
        {
            // Consulta real a la API
            var response = await _httpClient.GetAsync("Prototipo");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var prototipos = JsonSerializer.Deserialize<List<PrototipoDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return prototipos ?? new List<PrototipoDTO>();
        }

        /// <summary>
        /// Registra un nuevo prototipo en la API.
        /// Si no se pasa un prototipo, toma los datos de Prototipos/Create.json.
        /// Valida que la fecha de vencimiento sea mayor a la fecha de registro.
        /// </summary>
        public async Task<bool> RegistrarPrototipoAsync()
        {
            PrototipoDTO prototipo = new PrototipoDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Prototipos\\Create.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                prototipo = JsonSerializer.Deserialize<PrototipoDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            if (prototipo == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó un prototipo válido.");
                return false;
            }

            if (prototipo.FechaVencimiento <= prototipo.FechaRegistro)
            {
                Console.WriteLine("[VALIDACIÓN] La fecha de vencimiento debe ser mayor a la fecha de registro.");
                return false;
            }

            var json = JsonSerializer.Serialize(prototipo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Prototipo", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Actualiza un prototipo existente en la API.
        /// Si no se pasa un prototipo, toma los datos de Prototipos/Edit.json.
        /// Valida que la fecha de vencimiento sea mayor a la fecha de registro.
        /// </summary>
        public async Task<bool> ActualizarPrototipoAsync()
        {
            PrototipoDTO prototipo = new PrototipoDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Prototipos\\Edit.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                prototipo = JsonSerializer.Deserialize<PrototipoDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (prototipo == null || string.IsNullOrEmpty(prototipo.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El prototipo debe tener un Id válido.");
                return false;
            }

            if (prototipo.FechaVencimiento <= prototipo.FechaRegistro)
            {
                Console.WriteLine("[VALIDACIÓN] La fecha de vencimiento debe ser mayor a la fecha de registro.");
                return false;
            }

            var json = JsonSerializer.Serialize(prototipo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("Prototipo", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}