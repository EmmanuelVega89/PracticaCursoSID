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
    /// Servicio para operaciones CRUD de valores de referencia vía API REST.
    /// </summary>
    public class ValorReferenciaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        // Definición global de los valores válidos para tipo de comparación
        private static readonly string[] ComparacionesValidas = { "VALOR_MINIMO", "VALOR_MAXIMO", "RANGO", "NO_COMPARAR" };

        public ValorReferenciaService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de valores de referencia desde la API.
        /// </summary>
        public async Task<List<ValorReferenciaDTO>> ObtenerValoresReferenciaAsync()
        {
            var response = await _httpClient.GetAsync("ValorReferencia");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var valores = JsonSerializer.Deserialize<List<ValorReferenciaDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return valores ?? new List<ValorReferenciaDTO>();
        }

        /// <summary>
        /// Registra un nuevo valor de referencia en la API usando un archivo JSON mock si no se pasa un valor.
        /// </summary>
        public async Task<bool> RegistrarValorReferenciaAsync()
        {
            ValorReferenciaDTO valor = new ValorReferenciaDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\ValorReferencia\\Create.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                valor = JsonSerializer.Deserialize<ValorReferenciaDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (valor == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó un valor de referencia válido.");
                return false;
            }

            // Validaciones de comparación
            if (Array.IndexOf(ComparacionesValidas, valor.Comparacion?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de comparación inválida. Valores permitidos: VALOR_MINIMO, VALOR_MAXIMO, RANGO, NO_COMPARAR.");
                return false;
            }

            // Validación de rangos numéricos
            if (valor.Comparacion?.ToUpperInvariant() == "RANGO" && valor.Valor2 <= valor.Valor)
            {
                Console.WriteLine("[VALIDACIÓN] Para comparación RANGO, valor2 debe ser mayor o igual a valor.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(valor.Unidad))
            {
                Console.WriteLine("[VALIDACIÓN] La unidad es requerida.");
                return false;
            }

            var json = JsonSerializer.Serialize(valor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("ValorReferencia", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Actualiza un valor de referencia existente en la API usando un archivo JSON mock si no se pasa un valor.
        /// </summary>
        public async Task<bool> ActualizarValorReferenciaAsync(ValorReferenciaDTO valor = null)
        {
            if (valor == null)
            {
                var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\ValorReferencia\\Edit.json";
                if (File.Exists(mockPath))
                {
                    var jsonMock = File.ReadAllText(mockPath);
                    valor = JsonSerializer.Deserialize<ValorReferenciaDTO>(jsonMock, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }

            if (valor == null || string.IsNullOrEmpty(valor.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El valor de referencia debe tener un Id válido.");
                return false;
            }

            // Validaciones de comparación
            if (Array.IndexOf(ComparacionesValidas, valor.Comparacion?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de comparación inválida. Valores permitidos: VALOR_MINIMO, VALOR_MAXIMO, RANGO, NO_COMPARAR.");
                return false;
            }

            // Validación de rangos numéricos
            if (valor.Comparacion?.ToUpperInvariant() == "RANGO" && valor.Valor2 <= valor.Valor)
            {
                Console.WriteLine("[VALIDACIÓN] Para comparación RANGO, valor2 debe ser mayor o igual a valor.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(valor.Unidad))
            {
                Console.WriteLine("[VALIDACIÓN] La unidad es requerida.");
                return false;
            }

            var json = JsonSerializer.Serialize(valor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("ValorReferencia", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}