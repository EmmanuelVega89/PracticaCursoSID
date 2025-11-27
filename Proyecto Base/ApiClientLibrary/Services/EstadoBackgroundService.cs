using ApiClientLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ApiClientLibrary.Services
{
    public class EstadoBackgroundService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _basePath = "F1_ConfiguracionInicial/";

        public EstadoBackgroundService(HttpClient httpClient, IConfiguration configuration, string token)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{_configuration["ApiSettings:BaseUrl"]}{_basePath}");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int intervalo = _configuration.GetValue<int>("StateService:time", 60);

            while (!stoppingToken.IsCancellationRequested)
            {
                await EnviarEstadoSidAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(intervalo), stoppingToken);
            }
        }

        private async Task EnviarEstadoSidAsync(CancellationToken cancellationToken)
        {
            var estado = new EstadoSIDDTO { Estado = "EN_PRUEBAS" };

            // Validación de estado permitido
            var estadosValidos = new List<string> { "EN_ESPERA", "EN_PRUEBAS" };
            if (string.IsNullOrEmpty(estado.Estado) || !estadosValidos.Contains(estado.Estado))
                throw new IndexOutOfRangeException($"El estado {estado.Estado} no es válido.");

            var json = JsonSerializer.Serialize(estado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("EstadoSID", content, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    AnsiConsole.MarkupLine("[bold green]Servicio ONLINE[/]");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                         response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[bold red]Servicio OFFLINE[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]Estado desconocido: {response.StatusCode}[/]");
                }
            }
            catch (TaskCanceledException)
            {
                AnsiConsole.MarkupLine("[bold red]Servicio OFFLINE (Timeout)[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Servicio OFFLINE (Error: {ex.Message})[/]");
            }
        }
    }
}