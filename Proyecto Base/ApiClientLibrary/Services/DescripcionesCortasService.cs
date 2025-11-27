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
    /// Servicio para consultar descripciones cortas vía API REST.
    /// </summary>
    public class DescripcionesCortasService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        /// <summary>
        /// Inicializa el servicio con autenticación Bearer.
        /// </summary>
        public DescripcionesCortasService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Consulta la API de descripciones cortas usando POST con parámetros en la query string.
        /// Devuelve una lista de expedientes deserializados.
        /// </summary>
        /// <param name="textoBusqueda">Texto a buscar.</param>
        /// <param name="pageNumber">Número de página.</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <returns>Lista de objetos DescripcionCortaExpedienteDTO.</returns>
        public async Task<List<DescripcionCortaExpedienteDTO>> ConsultarDescripcionesCortasAsync(string textoBusqueda, int pageNumber, int pageSize)
        {
            var query = $"DescripcionesCortas?textoBusqueda={Uri.EscapeDataString(textoBusqueda)}&pageNumber={pageNumber}&pageSize={pageSize}";
            var response = await _httpClient.PostAsync(query, null);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {responseContent}");
                return new List<DescripcionCortaExpedienteDTO>();
            }

            try
            {
                var dto = JsonSerializer.Deserialize<DescripcionCortaResponseDTO>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return dto?.Expedientes ?? new List<DescripcionCortaExpedienteDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DESERIALIZACIÓN ERROR] {ex.Message}");
                return new List<DescripcionCortaExpedienteDTO>();
            }
        }
    }
}