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
    public class ExpedienteService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F2_PreparacionFabricacion/";
        private readonly string _expedienteMockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Expediente\\";

        public ExpedienteService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ExpedienteInspeccionCompleteDTO>> ObtenerExpedientesAsync(int pageNumber = 1, int pageSize = 20)
        {
            var response = await _httpClient.GetAsync($"ExpedientePruebas?pageNumber={pageNumber}&pageSize={pageSize}");
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {json}");
                return new List<ExpedienteInspeccionCompleteDTO>();
            }

            try
            {
                var expedientes = JsonSerializer.Deserialize<ExpedienteInspeccionPaginationDTO>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return expedientes.Expedientes ?? new List<ExpedienteInspeccionCompleteDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DESERIALIZACIÓN ERROR] {ex.Message}");
                return new List<ExpedienteInspeccionCompleteDTO>();
            }
        }

        public async Task<List<ExpedienteInspeccionCompleteDTO>> ObtenerExpedientePorIdAsync(string claveExpediente)
        {
            var response = await _httpClient.GetAsync($"ExpedientePruebas/{claveExpediente}");
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {json}");
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<List<ExpedienteInspeccionCompleteDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DESERIALIZACIÓN ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CrearExpedienteAsync()
        {
            ExpedienteInspeccionDTO expediente = null;
            var file = Path.Combine(_expedienteMockPath, "Create.json");
            if (File.Exists(file))
            {
                var jsonMock = File.ReadAllText(file);
                expediente = JsonSerializer.Deserialize<ExpedienteInspeccionDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (expediente == null || string.IsNullOrWhiteSpace(expediente.ClaveExpediente))
            {
                Console.WriteLine("[VALIDACIÓN] El expediente debe tener una clave única.");
                return false;
            }

            var json = JsonSerializer.Serialize(expediente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("ExpedientePruebas", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }

            Console.WriteLine("[API SUCCESS] Respuesta: " + responseContent);
            return true;
        }

        public async Task<bool> ActualizarExpedienteAsync()
        {
            ExpedienteInspeccionDTO expediente = null;
            var file = Path.Combine(_expedienteMockPath, "Edit.json");
            if (File.Exists(file))
            {
                var jsonMock = File.ReadAllText(file);
                expediente = JsonSerializer.Deserialize<ExpedienteInspeccionDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (expediente == null || string.IsNullOrWhiteSpace(expediente.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El expediente debe tener un Id válido.");
                return false;
            }

            var json = JsonSerializer.Serialize(expediente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("ExpedientePruebas", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }

            Console.WriteLine("[API SUCCESS] Respuesta: " + responseContent);
            return true;
        }

        public async Task<bool> AgregarMuestraAsync(string expedienteClave, string muestra)
        {
            var json = JsonSerializer.Serialize(muestra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"AgregaMuestraExpediente/{expedienteClave}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }
            return true;
        }

        public async Task<bool> QuitarMuestraAsync(string expedienteClave, string muestra)
        {
            var response = await _httpClient.PutAsync($"QuitarMuestraExpediente/{expedienteClave}/{muestra}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }
            return true;
        }
    }
}