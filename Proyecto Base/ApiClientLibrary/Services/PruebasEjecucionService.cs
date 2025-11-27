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
    public class PruebasEjecucionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePathF3 = "F3_Pruebas/";
        private readonly string _basePathF4 = "F4_Liberacion/";

        public PruebasEjecucionService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> AgregarResultadoPruebaAsync(string expediente, string muestra, AgregaResultadoPruebaDTO resultado = null)
        {
            // Path al archivo JSON
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\ResultadoPrueba\\AgregarResultado.json";
            if (resultado == null && System.IO.File.Exists(mockPath))
            {
                var jsonMock = System.IO.File.ReadAllText(mockPath);
                resultado = JsonSerializer.Deserialize<AgregaResultadoPruebaDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (resultado == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó un resultado de prueba válido.");
                return false;
            }

            var json = JsonSerializer.Serialize(resultado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF3}AgregaResultadoPrueba?expediente={expediente}&muestra={muestra}";

            var response = await _httpClient.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }
            Console.WriteLine("[API SUCCESS] Respuesta: " + responseContent);
            return true;
        }

        public async Task<List<PruebaDTO>> ObtenerPruebasNoSatisfactoriasAsync(string expediente)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF3}PruebasNoSatisfactorias/{expediente}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {json}");
                return new List<PruebaDTO>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<PruebaDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<PruebaDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DESERIALIZACIÓN ERROR] {ex.Message}");
                return new List<PruebaDTO>();
            }
        }

        public async Task<ExpedienteInspeccionDTO> ValidarExpedienteAsync(string expediente)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF3}ValidacionExpediente/{expediente}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {json}");
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<ExpedienteInspeccionDTO>(json, new JsonSerializerOptions
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

        public async Task<bool> TerminarPruebasExpedienteAsync(string expediente)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF3}TerminarPruebasExpediente/{expediente}";
            var response = await _httpClient.PutAsync(url, null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }
            Console.WriteLine("[API SUCCESS] Respuesta: " + responseContent);
            return true;
        }

        public async Task<bool> CrearAvisoPruebaAsync(string expediente)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF4}CrearAvisoPrueba/{expediente}";
            var response = await _httpClient.PutAsync(url, null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }
            Console.WriteLine("[API SUCCESS] Respuesta: " + responseContent);
            return true;
        }

        public async Task<List<AvisoPruebaDTO>> ConsultarAvisosAsync(string expediente)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF4}ConsultaAvisos/{expediente}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {json}");
                return new List<AvisoPruebaDTO>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<AvisoPruebaDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<AvisoPruebaDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DESERIALIZACIÓN ERROR] {ex.Message}");
                return new List<AvisoPruebaDTO>();
            }
        }

        public async Task<bool> CerrarExpedienteAsync(string expediente, string resultado)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_basePathF4}CierreExpedientePruebas/{expediente}/{resultado}";
            var response = await _httpClient.PutAsync(url, null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return false;
            }
            Console.WriteLine("[API SUCCESS] Respuesta: " + responseContent);
            return true;
        }
    }
}