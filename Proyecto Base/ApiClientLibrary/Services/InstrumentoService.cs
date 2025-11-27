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
    /// Servicio para operaciones CRUD de instrumentos vía API REST.
    /// </summary>
    public class InstrumentoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        /// <summary>
        /// Inicializa el servicio con autenticación Bearer.
        /// </summary>
        public InstrumentoService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de instrumentos desde la API.
        /// Muestra mensaje de error en consola si la API responde con error.
        /// </summary>
        public async Task<List<InstrumentoDTO>> ObtenerInstrumentosAsync()
        {
            var response = await _httpClient.GetAsync("Instrumento");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var instrumentos = JsonSerializer.Deserialize<List<InstrumentoDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return instrumentos ?? new List<InstrumentoDTO>();
        }

        /// <summary>
        /// Registra un instrumento usando datos de Create.json.
        /// Muestra mensaje de error en consola si la API responde con error.
        /// </summary>
        public async Task<bool> RegistrarInstrumentoAsync()
        {
            var instrumentoJson = File.ReadAllText("C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Instrumentos\\Create.json");
            try
            {
                var instrumento = JsonSerializer.Deserialize<InstrumentoDTO>(instrumentoJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (instrumento == null)
                {
                    instrumento = new InstrumentoDTO
                    {
                        Id = "",
                        Nombre = "BALANZA",
                        NumeroSerie = "4535355",
                        FechaCalibracion = DateTime.Parse("2025-01-25T17:28:05.044Z"),
                        FechaVencimientoCalibracion = DateTime.Parse("2026-01-25T17:28:05.044Z"),
                        UrlArchivo = "https://google.com",
                        MD5 = "",
                        Estatus = "ACTIVO",
                        FechaRegistro = DateTime.Parse("2025-11-25T17:28:05.044Z")
                    };
                }

                var json = JsonSerializer.Serialize(instrumento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Instrumento", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al deserializar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza un instrumento usando datos de Edit.json.
        /// Muestra mensaje de error en consola si la API responde con error.
        /// </summary>
        public async Task<bool> ActualizarInstrumentoAsync()
        {
            var instrumentoJson = File.ReadAllText("C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Instrumentos\\Edit.json");
            var instrumento = JsonSerializer.Deserialize<InstrumentoDTO>(instrumentoJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (instrumento == null || string.IsNullOrEmpty(instrumento.Id))
                throw new ArgumentException("El instrumento debe tener un Id válido.");

            var json = JsonSerializer.Serialize(instrumento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"Instrumento", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}
