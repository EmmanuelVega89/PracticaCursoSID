using ApiClientLibrary.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var services = new ServiceCollection();
services.AddMediatR(options => options.RegisterServicesFromAssemblyContaining<AuthService.Handler>());
var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

AnsiConsole.MarkupLine("[bold yellow]Autenticación requerida[/]");

var usuario = AnsiConsole.Ask<string>("Ingrese su [green]usuario[/]:");
var password = AnsiConsole.Prompt(
    new TextPrompt<string>("Ingrese su [red]contraseña[/]:")
        .PromptStyle("red")
        .Secret());
var command = new AuthService.Command(usuario, password);
var responseAuth = await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .SpinnerStyle(Style.Parse("green"))
    .StartAsync("Validando credenciales...", async ctx =>
    {
        return await mediator.Send(command);
    });

if (responseAuth.IsSuccess)
{
    var token = responseAuth.Value.Token;

    var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient<EstadoBackgroundService>((provider, client) =>
        {
            client.BaseAddress = new Uri("https://lapem.cfe.gob.mx/sid_capacitacion/");
        });

        services.AddSingleton(provider =>
        {
            var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(EstadoBackgroundService));
            var configuration = provider.GetRequiredService<IConfiguration>();
            return new EstadoBackgroundService(httpClient, configuration, token);
        });

        services.AddHostedService<EstadoBackgroundService>(provider =>
            provider.GetRequiredService<EstadoBackgroundService>());
    });

    var app = builder.Build();
    await app.RunAsync();
}
else
{
    AnsiConsole.MarkupLine($"[bold red]Error de autenticación:[/] {responseAuth.Errors[0].Message}");
}
//var servicio = new F1_ConfiguracionInicial();
//bool autenticado = await servicio.AutenticarYObtenerTokenAsync(
//    "emmanuel.vegaledezma@gmail.com",
//    "8977@LAPEM"
//);

//if (!autenticado)
//{
//    Console.WriteLine("No se pudo autenticar.");
//    return;
//}

//// Ahora puedes usar el servicio normalmente, por ejemplo:
//var estado = new ApiClientLibrary.Models.EstadoSIDDTO { Estado = "EN_PRUEBAS" };
//var respuesta = await servicio.RegistrarEstadoSID(estado);
//Console.WriteLine($"Código de respuesta: {respuesta.StatusCode}");

// Crea un handler personalizado: DelegatingHandler