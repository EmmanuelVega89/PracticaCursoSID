using ApiClientLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ApiClientLibrary.Services
{
    public class F1_ConfiguracionInicial
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";
        public F1_ConfiguracionInicial()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}")
            };

            //if (!string.IsNullOrEmpty(token))
            //{
            //    _httpClient.DefaultRequestHeaders.Authorization =
            //        new AuthenticationHeaderValue("Bearer", token);
            //}
        }

        /// <summary>
        /// Permite cambiar manualmente el token de autorización (ej. para pruebas).
        /// </summary>
        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<HttpResponseMessage> RegistrarEstadoSID(EstadoSIDDTO estado)
        {
            var json = JsonSerializer.Serialize(estado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("EstadoSID", content);
            return response;
        }

        public async Task<bool> AutenticarYObtenerTokenAsync(string username, string password)
        {
            // Construye la URL completa usando la base del appsettings
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var loginUrl = $"{baseUrl}F0_Acceso/Login";
            var loginBody = new
            {
                username,
                password
            };
            var json = JsonSerializer.Serialize(loginBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(loginUrl, content);

            if (!response.IsSuccessStatusCode)
                return false;

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                var token = tokenElement.GetString();
                SetToken(token);
                return true;
            }
            return false;
        }
    }
}
