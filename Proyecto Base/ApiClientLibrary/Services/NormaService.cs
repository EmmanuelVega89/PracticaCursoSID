using ApiClientLibrary.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiClientLibrary.Services
{
    /// <summary>
    /// Servicio para operaciones CRUD de normas vía API REST.
    /// </summary>
    public class NormaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        /// <summary>
        /// Inicializa el servicio con autenticación Bearer.
        /// </summary>
        public NormaService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de normas desde la API.
        /// </summary>
        public async Task<List<NormaDTO>> ObtenerNormasAsync()
        {
            var response = await _httpClient.GetAsync("Norma");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var normas = JsonSerializer.Deserialize<List<NormaDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return normas ?? new List<NormaDTO>();
        }

        /// <summary>
        /// Obtiene la lista de normas CFE (esCFE = true).
        /// </summary>
        public async Task<List<NormaDTO>> ObtenerNormasCfeAsync()
        {
            var todas = await ObtenerNormasAsync();
            return todas.FindAll(n => n.EsCFE);
        }

        /// <summary>
        /// Registra una nueva norma en la API o usando un archivo JSON mock si no se pasa una norma.
        /// </summary>
        public async Task<bool> RegistrarNormaAsync()
        {
            NormaDTO norma = new NormaDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Normas\\Create.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                norma = JsonSerializer.Deserialize<NormaDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (norma == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó una norma válida.");
                return false;
            }

            var json = JsonSerializer.Serialize(norma);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Norma", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Actualiza una norma existente en la API o usando un archivo JSON mock si no se pasa una norma.
        /// </summary>
        public async Task<bool> ActualizarNormaAsync()
        {
            NormaDTO norma = new NormaDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Normas\\Edit.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                norma = JsonSerializer.Deserialize<NormaDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            if (norma == null || string.IsNullOrEmpty(norma.Id))
            {
                Console.WriteLine("[VALIDACIÓN] La norma debe tener un Id válido.");
                return false;
            }

            var json = JsonSerializer.Serialize(norma);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("Norma", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}