using ApiClientLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using FluentResults;
using Newtonsoft.Json;
using MediatR;

namespace ApiClientLibrary.Services
{
    public static class AuthService
    {
        public class Command : IRequest<Result<AuthResult>>
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public Command(string username, string password)
            {
                Username = username;
                Password = password;
            }
        }
        public class Handler : IRequestHandler<Command, Result<AuthResult>>
        {
            private IConfiguration _configuration;
            public async Task<Result<AuthResult>> Handle(Command command, CancellationToken cancellationToken)
            {
                try
                {
                    var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    _configuration = builder.Build();
                    using var httpClient = new HttpClient();
                    var baseUrl = _configuration["ApiSettings:BaseUrl"];
                    var loginUrl = $"{baseUrl}F0_Acceso/Login";
                    //var url = "https://lapem.cfe.gob.mx/sid_evaluacion/F0_Acceso/Login";
                    var payload = new
                    {
                        username = command.Username,
                        password = command.Password
                    };
                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(loginUrl, content, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return Result.Fail<AuthResult>($"Error de autenticación: {response.StatusCode}");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<AuthResult>(responseBody);
                    if (result == null || string.IsNullOrEmpty(result.Token))
                    {
                        return Result.Fail<AuthResult>("No se encontró el token en la respuesta de autenticación.");
                    }
                    return Result.Ok(result);
                }
                catch (Exception ex)
                {
                    return Result.Fail<AuthResult>(ex.Message);
                }
            }
        }
    }
}
