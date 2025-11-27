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
    /// Servicio para operaciones CRUD de pruebas y gestión de otras pruebas/documentos vía API REST.
    /// </summary>
    public class PruebaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        // Definición única de los valores válidos
        private static readonly string[] TiposPruebaValidos = { "RUTINA", "ACEPTACION" };
        private static readonly string[] TiposResultadoValidos = { "VALOR_REFERENCIA", "PASA/NO-PASA" };
        private static readonly string[] EstatusValidos = { "ACTIVA", "INACTIVA" };
        private static readonly string[] TiposDocumentoValidos = { "CertificadoMaterial", "Prueba Rutina", "Otro" };

        public PruebaService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene la lista de pruebas desde la API.
        /// </summary>
        public async Task<List<PruebaDTO>> ObtenerPruebasAsync()
        {
            var response = await _httpClient.GetAsync("Prueba");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var pruebas = JsonSerializer.Deserialize<List<PruebaDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return pruebas ?? new List<PruebaDTO>();
        }

        /// <summary>
        /// Registra una nueva prueba en la API. 
        /// Toma los datos de Prueba/Create.json.
        /// Valida (Tipo prueba, Tipo resultado, Estatus)
        /// </summary>
        public async Task<bool> RegistrarPruebaAsync()
        {
            PruebaDTO prueba = new PruebaDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Prueba\\Create.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                prueba = JsonSerializer.Deserialize<PruebaDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (prueba == null)
            {
                Console.WriteLine("[VALIDACIÓN] No se proporcionó una prueba válida.");
                return false;
            }

            // Validaciones requeridas
            if (Array.IndexOf(TiposPruebaValidos, prueba.TipoPrueba?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de prueba inválido. Valores permitidos: RUTINA, ACEPTACION.");
                return false;
            }

            if (Array.IndexOf(TiposResultadoValidos, prueba.TipoResultado?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de resultado inválido. Valores permitidos: VALOR_REFERENCIA, PASA/NO-PASA.");
                return false;
            }

            if (Array.IndexOf(EstatusValidos, prueba.Estatus?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Estatus inválido. Valores permitidos: ACTIVA, INACTIVA.");
                return false;
            }

            var json = JsonSerializer.Serialize(prueba);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Prueba", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Actualiza una prueba existente en la API. 
        /// Toma los datos de Prueba/Edit.json.
        /// Valida (Tipo prueba, Tipo resultado, Estatus)
        /// </summary>
        public async Task<bool> ActualizarPruebaAsync()
        {
            PruebaDTO prueba = new PruebaDTO();
            
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\Prueba\\Edit.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                prueba = JsonSerializer.Deserialize<PruebaDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (prueba == null || string.IsNullOrEmpty(prueba.Id))
            {
                Console.WriteLine("[VALIDACIÓN] La prueba debe tener un Id válido.");
                return false;
            }

            // Validaciones requeridas
            if (Array.IndexOf(TiposPruebaValidos, prueba.TipoPrueba?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de prueba inválido. Valores permitidos: RUTINA, ACEPTACION.");
                return false;
            }

            if (Array.IndexOf(TiposResultadoValidos, prueba.TipoResultado?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Tipo de resultado inválido. Valores permitidos: VALOR_REFERENCIA, PASA/NO-PASA.");
                return false;
            }

            if (Array.IndexOf(EstatusValidos, prueba.Estatus?.ToUpperInvariant()) == -1)
            {
                Console.WriteLine("[VALIDACIÓN] Estatus inválido. Valores permitidos: ACTIVA, INACTIVA.");
                return false;
            }

            var json = JsonSerializer.Serialize(prueba);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("Prueba", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Registra otra prueba o documento en la API.
        /// Valida el tipo de documento.
        /// </summary>
        public async Task<bool> RegistrarOtraPruebaAsync()
        {
            OtrasPruebasYDocumentosDTO documento = new OtrasPruebasYDocumentosDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\OtrasPruebasYDocumentos\\Edit.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                documento = JsonSerializer.Deserialize<OtrasPruebasYDocumentosDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (documento == null || string.IsNullOrEmpty(documento.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El documento debe tener un Id válido.");
                return false;
            }

            // Validación de tipo de documento
            if (Array.IndexOf(TiposDocumentoValidos, documento.TipoDocumento) == -1)
            {
                Console.WriteLine($"[VALIDACIÓN] El tipo de documento {documento.TipoDocumento} no es válido. Solo se aceptan: \"CertificadoMaterial\",\"Prueba Rutina\" y \"Otro\"");
                return false;
            }

            var json = JsonSerializer.Serialize(documento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("OtrasPruebasYDocumentos", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Obtiene la lista de otras pruebas o documentos desde un JSON o la API.
        /// </summary>
        public async Task<List<OtrasPruebasYDocumentosDTO>> ObtenerOtrasPruebasYDocumentosAsync()
        {
            var response = await _httpClient.GetAsync("OtrasPruebasYDocumentos");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var documentos = JsonSerializer.Deserialize<List<OtrasPruebasYDocumentosDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return documentos ?? new List<OtrasPruebasYDocumentosDTO>();
        }

        /// <summary>
        /// Actualiza una otra prueba o documento en la API. Si no se pasa el objeto, toma los datos de OtrasPruebasYDocumentos/Edit.json.
        /// Valida el tipo de documento.
        /// </summary>
        public async Task<bool> ActualizarOtrasPruebasYDocumentosAsync()
        {
            OtrasPruebasYDocumentosDTO documento = new OtrasPruebasYDocumentosDTO();
            var mockPath = "C:\\devel\\PracticaCursoSID\\Proyecto Base\\ApiClientLibrary\\Information\\OtrasPruebasYDocumentos\\Edit.json";
            if (File.Exists(mockPath))
            {
                var jsonMock = File.ReadAllText(mockPath);
                documento = JsonSerializer.Deserialize<OtrasPruebasYDocumentosDTO>(jsonMock, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (documento == null || string.IsNullOrEmpty(documento.Id))
            {
                Console.WriteLine("[VALIDACIÓN] El documento debe tener un Id válido.");
                return false;
            }

            // Validación de tipo de documento
            if (Array.IndexOf(TiposDocumentoValidos, documento.TipoDocumento) == -1)
            {
                Console.WriteLine($"[VALIDACIÓN] El tipo de documento {documento.TipoDocumento} no es válido. Solo se aceptan: \"CertificadoMaterial\",\"Prueba Rutina\" y \"Otro\"");
                return false;
            }

            var json = JsonSerializer.Serialize(documento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("OtrasPruebasYDocumentos", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API ERROR] Código: {(int)response.StatusCode} - Mensaje: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}