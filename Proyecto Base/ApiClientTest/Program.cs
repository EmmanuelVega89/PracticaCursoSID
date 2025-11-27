using ApiClientLibrary.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();
//**************************************************************************************************************
//***[Práctica 1: Autenticación y Configuración Inicial]********************************************************
//**************************************************************************************************************
// Configuración de servicios y Mediator
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddMediatR(options => options.RegisterServicesFromAssemblyContaining<AuthService.Handler>());
var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

AnsiConsole.MarkupLine("[bold yellow]Autenticación requerida[/]");

// Solicita credenciales y realiza autenticación
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

    // Configura e inicia el background service
    // Este servicio se ejecuta cada 60s configurado desde appsettings.json
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
            return new EstadoBackgroundService(httpClient, configuration, token);
        });

        services.AddHostedService<EstadoBackgroundService>(provider =>
            provider.GetRequiredService<EstadoBackgroundService>());
    });

    var app = builder.Build();
    var hostTask = app.RunAsync();
    //**************************************************************************************************************
    //**************************************************************************************************************
    // Menú principal de la aplicación
    bool salir = false;
    while (!salir)
    {
        var opcion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Seleccione una opción:[/]")
                .AddChoices(new[]
                {
                    "Instrumentos",
                    "Descripciones cortas",
                    "Prototipos",
                    "Pruebas",
                    "Normas",
                    "Producto",
                    "Valores de Referencia",
                    "Contratos",
                    "Órdenes de Fabricación",
                    "Expedientes de Pruebas",
                    "Terminar ejecución"
                }
            )
        );

        switch (opcion)
        {
            //**************************************************************************************************************
            //***[Práctica 2: Gestión de Instrumentos de Medición]**********************************************************
            //**************************************************************************************************************
            case "Instrumentos":
                {
                    bool salirInstrumentos = false;
                    while (!salirInstrumentos)
                    {
                        var opcionInstrumento = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Instrumentos:[/]")
                                .AddChoices(new[]
                                    {
                                        "Consultar instrumento",
                                        "Agregar instrumento (Create.json)",
                                        "Modificar instrumento (Edit.json)",
                                        "Regresar"
                                    }
                                )
                            );

                        switch (opcionInstrumento)
                        {
                            case "Consultar instrumento":
                                {
                                    using var httpClient = new HttpClient();
                                    var instrumentoService = new InstrumentoService(httpClient, configuration, token);

                                    try
                                    {
                                        var instrumentos = await instrumentoService.ObtenerInstrumentosAsync();
                                        if (instrumentos.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron instrumentos.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]INSTRUMENTOS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Nombre");
                                            table.AddColumn("Número Serie");
                                            table.AddColumn("Fecha Calibración");
                                            table.AddColumn("Fecha Vencimiento Calibración");
                                            table.AddColumn("Url Archivo");
                                            table.AddColumn("MD5");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var inst in instrumentos)
                                            {
                                                table.AddRow(
                                                    inst.Id ?? "",
                                                    inst.Nombre ?? "",
                                                    inst.NumeroSerie ?? "",
                                                    inst.FechaCalibracion.ToString("u"),
                                                    inst.FechaVencimientoCalibracion.ToString("u"),
                                                    inst.UrlArchivo ?? "",
                                                    inst.MD5 ?? "",
                                                    inst.Estatus ?? "",
                                                    inst.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener instrumentos: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar instrumento (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var instrumentoService = new InstrumentoService(httpClient, configuration, token);
                                        var exito = await instrumentoService.RegistrarInstrumentoAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Instrumento agregado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar el instrumento.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar instrumento: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar instrumento (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var instrumentoService = new InstrumentoService(httpClient, configuration, token);
                                        var exito = await instrumentoService.ActualizarInstrumentoAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Instrumento modificado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar el instrumento.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar instrumento: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirInstrumentos = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Descripciones cortas]*************************************************************************************
            //**************************************************************************************************************
            case "Descripciones cortas":
                {
                    using var httpClient = new HttpClient();
                    var descripcionesService = new DescripcionesCortasService(httpClient, configuration, token);

                    string textoBusqueda;
                    do
                    {
                        textoBusqueda = AnsiConsole.Ask<string>("Ingrese el [green]texto de búsqueda[/]:");
                        if (string.IsNullOrWhiteSpace(textoBusqueda))
                            AnsiConsole.MarkupLine("[red]El texto de búsqueda es obligatorio.[/]");
                    } while (string.IsNullOrWhiteSpace(textoBusqueda));

                    var pageNumber = AnsiConsole.Ask<int>("Ingrese el [blue]número de página[/]:");
                    var pageSize = AnsiConsole.Ask<int>("Ingrese el [blue]tamaño de página[/]:");

                    try
                    {
                        var expedientes = await descripcionesService.ConsultarDescripcionesCortasAsync(textoBusqueda, pageNumber, pageSize);
                        if (expedientes.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]No se encontraron descripciones cortas.[/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold blue]DESCRIPCIONES CORTAS[/]");
                            var table = new Table();
                            table.AddColumn("Id");
                            table.AddColumn("Descripción Corta");
                            table.AddColumn("Norma");

                            foreach (var exp in expedientes)
                            {
                                table.AddRow(
                                    exp.Id ?? "",
                                    exp.DescripcionCorta ?? "",
                                    exp.Norma ?? ""
                                );
                            }
                            AnsiConsole.Write(table);
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error al consultar descripciones cortas: {ex.Message}[/]");
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 3: Administración de Prototipos]*****************************************************************
            //**************************************************************************************************************
            case "Prototipos":
                {
                    bool salirPrototipos = false;
                    while (!salirPrototipos)
                    {
                        var opcionPrototipo = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Prototipos:[/]")
                                .AddChoices(new[]
                                    {
                                        "Consultar prototipo",
                                        "Agregar prototipo (Create.json)",
                                        "Modificar prototipo (Edit.json)",
                                        "Regresar"
                                    }
                                )
                            );

                        switch (opcionPrototipo)
                        {
                            case "Consultar prototipo":
                                {
                                    using var httpClient = new HttpClient();
                                    var prototipoService = new PrototipoService(httpClient, configuration, token);

                                    try
                                    {
                                        var prototipos = await prototipoService.ObtenerPrototiposAsync();
                                        if (prototipos.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron prototipos.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]PROTOTIPOS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Número");
                                            table.AddColumn("Fecha Emisión");
                                            table.AddColumn("Fecha Vencimiento");
                                            table.AddColumn("Url Archivo");
                                            table.AddColumn("MD5");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var prot in prototipos)
                                            {
                                                table.AddRow(
                                                    prot.Id ?? "",
                                                    prot.Numero ?? "",
                                                    prot.FechaEmision.ToString("u"),
                                                    prot.FechaVencimiento.ToString("u"),
                                                    prot.UrlArchivo ?? "",
                                                    prot.MD5 ?? "",
                                                    prot.Estatus ?? "",
                                                    prot.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener prototipos: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar prototipo (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var prototipoService = new PrototipoService(httpClient, configuration, token);
                                        var exito = await prototipoService.RegistrarPrototipoAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Prototipo agregado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar el prototipo.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar prototipo: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar prototipo (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var prototipoService = new PrototipoService(httpClient, configuration, token);
                                        var exito = await prototipoService.ActualizarPrototipoAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Prototipo modificado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar el prototipo.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar prototipo: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirPrototipos = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 4: Gestión de Valores de Referencia]*************************************************************
            //**************************************************************************************************************
            case "Valores de Referencia":
                {
                    bool salirValores = false;
                    while (!salirValores)
                    {
                        var opcionValor = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Valores de Referencia:[/]")
                                .AddChoices(new[]
                                    {
                                        "Consultar valores de referencia",
                                        "Agregar valor de referencia (Create.json)",
                                        "Modificar valor de referencia (Edit.json)",
                                        "Regresar"
                                    }
                                )
                            );

                        switch (opcionValor)
                        {
                            case "Consultar valores de referencia":
                                {
                                    using var httpClient = new HttpClient();
                                    var valorReferenciaService = new ValorReferenciaService(httpClient, configuration, token);

                                    try
                                    {
                                        var valores = await valorReferenciaService.ObtenerValoresReferenciaAsync();
                                        if (valores.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron valores de referencia.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]VALORES DE REFERENCIA[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Id Producto");
                                            table.AddColumn("Id Prueba");
                                            table.AddColumn("Valor");
                                            table.AddColumn("Valor2");
                                            table.AddColumn("Unidad");
                                            table.AddColumn("Comparación");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var v in valores)
                                            {
                                                table.AddRow(
                                                    v.Id ?? "",
                                                    v.IdProducto ?? "",
                                                    v.IdPrueba ?? "",
                                                    v.Valor.ToString(),
                                                    v.Valor2.ToString(),
                                                    v.Unidad ?? "",
                                                    v.Comparacion ?? "",
                                                    v.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener valores de referencia: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar valor de referencia (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var valorReferenciaService = new ValorReferenciaService(httpClient, configuration, token);
                                        var exito = await valorReferenciaService.RegistrarValorReferenciaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Valor de referencia agregado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar el valor de referencia.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar valor de referencia: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar valor de referencia (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var valorReferenciaService = new ValorReferenciaService(httpClient, configuration, token);
                                        var exito = await valorReferenciaService.ActualizarValorReferenciaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Valor de referencia modificado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar el valor de referencia.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar valor de referencia: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirValores = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 5: Administración de Productos]******************************************************************
            //**************************************************************************************************************
            case "Producto":
                {
                    bool salirProducto = false;
                    while (!salirProducto)
                    {
                        var opcionProducto = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Producto:[/]")
                                .AddChoices(new[]
                                    {
                                        "Consultar productos",
                                        "Agregar producto (Create.json)",
                                        "Modificar producto (Edit.json)",
                                        "Asociar pruebas a producto",
                                        "Regresar"
                                    }
                                )
                            );

                        switch (opcionProducto)
                        {
                            case "Consultar productos":
                                {
                                    using var httpClient = new HttpClient();
                                    var productoService = new ProductoService(httpClient, configuration, token);

                                    try
                                    {
                                        var productos = await productoService.ObtenerProductosAsync();
                                        if (productos.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron productos.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]PRODUCTOS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Código Fabricante");
                                            table.AddColumn("Descripción");
                                            table.AddColumn("Descripción Corta");
                                            table.AddColumn("Tipo Fabricación");
                                            table.AddColumn("Unidad");
                                            table.AddColumn("Norma");
                                            table.AddColumn("Prototipo");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Fecha Registro");
                                            table.AddColumn("Pruebas");

                                            foreach (var p in productos)
                                            {
                                                table.AddRow(
                                                    p.Id ?? "",
                                                    p.CodigoFabricante ?? "",
                                                    p.Descripcion ?? "",
                                                    p.DescripcionCorta ?? "",
                                                    p.TipoFabricacion ?? "",
                                                    p.Unidad ?? "",
                                                    p.Norma != null ? p.Norma.Nombre : "",
                                                    p.Prototipo != null ? p.Prototipo.Numero : "",
                                                    p.Estatus ?? "",
                                                    p.FechaRegistro.ToString("u"),
                                                    p.Pruebas != null ? string.Join(",", p.Pruebas.Select(x => x.Nombre)) : ""
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener productos: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar producto (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var productoService = new ProductoService(httpClient, configuration, token);
                                        var exito = await productoService.RegistrarProductoAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Producto agregado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar el producto.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar producto: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar producto (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var productoService = new ProductoService(httpClient, configuration, token);
                                        var exito = await productoService.ActualizarProductoAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Producto modificado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar el producto.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar producto: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Asociar pruebas a producto":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var productoService = new ProductoService(httpClient, configuration, token);

                                        var productoId = AnsiConsole.Ask<string>("Ingrese el [green]Id del producto[/]:");
                                        var pruebasInput = AnsiConsole.Ask<string>("Ingrese los [blue]Ids de las pruebas[/] separados por coma:");
                                        var pruebasIds = new List<string>();
                                        if (!string.IsNullOrWhiteSpace(pruebasInput))
                                        {
                                            pruebasIds = new List<string>(pruebasInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                                        }

                                        var exito = await productoService.AsociarPruebasAProductoAsync(productoId, pruebasIds);

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Pruebas asociadas correctamente al producto.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudieron asociar las pruebas al producto.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al asociar pruebas al producto: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirProducto = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 6: Gestión de Normas]****************************************************************************
            //**************************************************************************************************************
            case "Normas":
                {
                    bool salirNormas = false;
                    while (!salirNormas)
                    {
                        var opcionNorma = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Normas:[/]")
                                .AddChoices(new[]
                                    {
                                        "Consultar normas",
                                        "Consultar normas CFE",
                                        "Agregar norma (Create.json)",
                                        "Modificar norma (Edit.json)",
                                        "Regresar"
                                    }
                                )
                            );

                        switch (opcionNorma)
                        {
                            case "Consultar normas":
                                {
                                    using var httpClient = new HttpClient();
                                    var normaService = new NormaService(httpClient, configuration, token);

                                    try
                                    {
                                        var normas = await normaService.ObtenerNormasAsync();
                                        if (normas.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron normas.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]NORMAS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Nombre");
                                            table.AddColumn("Edición");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Es CFE");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var norma in normas)
                                            {
                                                table.AddRow(
                                                    norma.Id ?? "",
                                                    norma.Clave ?? "",
                                                    norma.Nombre ?? "",
                                                    norma.Edicion ?? "",
                                                    norma.Estatus ?? "",
                                                    norma.EsCFE ? "Sí" : "No",
                                                    norma.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener normas: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Consultar normas CFE":
                                {
                                    using var httpClient = new HttpClient();
                                    var normaService = new NormaService(httpClient, configuration, token);

                                    try
                                    {
                                        var normasCfe = await normaService.ObtenerNormasCfeAsync();
                                        if (normasCfe.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron normas CFE.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]NORMAS CFE[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Nombre");
                                            table.AddColumn("Edición");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Es CFE");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var norma in normasCfe)
                                            {
                                                table.AddRow(
                                                    norma.Id ?? "",
                                                    norma.Clave ?? "",
                                                    norma.Nombre ?? "",
                                                    norma.Edicion ?? "",
                                                    norma.Estatus ?? "",
                                                    norma.EsCFE ? "Sí" : "No",
                                                    norma.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener normas CFE: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar norma (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var normaService = new NormaService(httpClient, configuration, token);
                                        var exito = await normaService.RegistrarNormaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Norma agregada correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar la norma.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar norma: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar norma (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var normaService = new NormaService(httpClient, configuration, token);
                                        var exito = await normaService.ActualizarNormaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Norma modificada correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar la norma.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar norma: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirNormas = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 7: Configuración de Pruebas]*********************************************************************
            //**************************************************************************************************************
            case "Pruebas":
                {
                    bool salirPruebas = false;
                    while (!salirPruebas)
                    {
                        var opcionPrueba = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Pruebas:[/]")
                                .AddChoices(new[]
                                    {
                                        "Consultar pruebas",
                                        "Agregar prueba (Create.json)",
                                        "Modificar prueba (Edit.json)",
                                        "Consultar otras pruebas/documentos",
                                        "Agregar otra prueba/documento (Create.json)",
                                        "Modificar otra prueba/documento (Edit.json)",
                                        "Regresar"
                                    }
                                )
                            );

                        switch (opcionPrueba)
                        {
                            case "Consultar pruebas":
                                {
                                    using var httpClient = new HttpClient();
                                    var pruebaService = new PruebaService(httpClient, configuration, token);

                                    try
                                    {
                                        var pruebas = await pruebaService.ObtenerPruebasAsync();
                                        if (pruebas.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron pruebas.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]PRUEBAS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Nombre");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Tipo Prueba");
                                            table.AddColumn("Tipo Resultado");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var p in pruebas)
                                            {
                                                table.AddRow(
                                                    p.Id ?? "",
                                                    p.Nombre ?? "",
                                                    p.Estatus ?? "",
                                                    p.TipoPrueba ?? "",
                                                    p.TipoResultado ?? "",
                                                    p.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener pruebas: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar prueba (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var pruebaService = new PruebaService(httpClient, configuration, token);
                                        var exito = await pruebaService.RegistrarPruebaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Prueba agregada correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar la prueba.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar prueba: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar prueba (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var pruebaService = new PruebaService(httpClient, configuration, token);
                                        var exito = await pruebaService.ActualizarPruebaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Prueba modificada correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar la prueba.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar prueba: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Consultar otras pruebas/documentos":
                                {
                                    using var httpClient = new HttpClient();
                                    var pruebaService = new PruebaService(httpClient, configuration, token);

                                    try
                                    {
                                        var docs = await pruebaService.ObtenerOtrasPruebasYDocumentosAsync();
                                        if (docs.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron otras pruebas/documentos.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]OTRAS PRUEBAS/DOCUMENTOS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Tipo Documento");
                                            table.AddColumn("Descripción");
                                            table.AddColumn("Url Archivo");
                                            table.AddColumn("MD5");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Vigencia");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var d in docs)
                                            {
                                                table.AddRow(
                                                    d.Id ?? "",
                                                    d.TipoDocumento ?? "",
                                                    d.DescripcionDocumento ?? "",
                                                    d.UrlArchivo ?? "",
                                                    d.MD5 ?? "",
                                                    d.Estatus ?? "",
                                                    d.Vigencia.ToString("u"),
                                                    d.FechaRegistro.ToString("u")
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al obtener otras pruebas/documentos: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar otra prueba/documento (Create.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var pruebaService = new PruebaService(httpClient, configuration, token);
                                        var exito = await pruebaService.RegistrarOtraPruebaAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Documento agregado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar el documento.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar documento: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar otra prueba/documento (Edit.json)":
                                {
                                    try
                                    {
                                        using var httpClient = new HttpClient();
                                        var pruebaService = new PruebaService(httpClient, configuration, token);
                                        var exito = await pruebaService.ActualizarOtrasPruebasYDocumentosAsync();

                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Documento modificado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar el documento.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar documento: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirPruebas = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 8: Gestión de Contratos CFE]*********************************************************************
            //**************************************************************************************************************
            case "Contratos":
                {
                    bool salirContratos = false;
                    while (!salirContratos)
                    {
                        var opcionContrato = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Contratos:[/]")
                                .AddChoices(new[]
                                {
                    "Consultar contratos (paginado)",
                    "Agregar contrato (Create.json)",
                    "Modificar contrato (Edit.json)",
                    "Cambiar estatus de contrato",
                    "Regresar"
                                })
                        );

                        switch (opcionContrato)
                        {
                            case "Consultar contratos (paginado)":
                                {
                                    using var httpClient = new HttpClient();
                                    var contratoService = new ContratoService(httpClient, configuration, token);

                                    var pageNumber = AnsiConsole.Ask<int>("Ingrese el [blue]número de página[/]:");
                                    var pageSize = AnsiConsole.Ask<int>("Ingrese el [blue]tamaño de página[/]:");

                                    try
                                    {
                                        var contratos = await contratoService.ObtenerContratosAsync(pageNumber, pageSize);
                                        if (contratos.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron contratos.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]CONTRATOS[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Tipo Contrato");
                                            table.AddColumn("No. Contrato");
                                            table.AddColumn("Estatus");
                                            table.AddColumn("Detalle");
                                            table.AddColumn("Url Archivo");
                                            table.AddColumn("MD5");
                                            table.AddColumn("Fecha Entrega CFE");
                                            table.AddColumn("Pérdidas Vacio");
                                            table.AddColumn("Pérdidas Carga");

                                            foreach (var c in contratos)
                                            {
                                                table.AddRow(
                                                    c.Id ?? "",
                                                    c.TipoContrato ?? "",
                                                    c.NoContrato ?? "",
                                                    c.Estatus ?? "",
                                                    c.DetalleContrato != null
                                                        ? string.Join(" | ", c.DetalleContrato.Select(d =>
                                                            $"Partida: {d.PartidaContrato}, Desc: {d.DescripcionAviso}, Area: {d.AreaDestinoCFE}, Cant: {d.Cantidad}, Unidad: {d.Unidad}, Importe: {d.ImporteTotal}"))
                                                        : "",
                                                    c.UrlArchivo ?? "",
                                                    c.MD5 ?? "",
                                                    c.FechaEntregaCFE?.ToString("u") ?? "",
                                                    c.PerdidasGarantizadasVacio?.ToString() ?? "",
                                                    c.PerdidasGarantizadasCarga?.ToString() ?? ""
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al consultar contratos: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar contrato (Create.json)":
                                {
                                    using var httpClient = new HttpClient();
                                    var contratoService = new ContratoService(httpClient, configuration, token);

                                    try
                                    {
                                        var exito = await contratoService.RegistrarContratoAsync();
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Contrato agregado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar el contrato.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar contrato: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Modificar contrato (Edit.json)":
                                {
                                    using var httpClient = new HttpClient();
                                    var contratoService = new ContratoService(httpClient, configuration, token);

                                    try
                                    {
                                        var exito = await contratoService.ActualizarContratoAsync();
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Contrato modificado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo modificar el contrato.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al modificar contrato: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Cambiar estatus de contrato":
                                {
                                    using var httpClient = new HttpClient();
                                    var contratoService = new ContratoService(httpClient, configuration, token);

                                    var contratoId = AnsiConsole.Ask<string>("Ingrese el [green]Id del contrato[/]:");
                                    var nuevoEstatus = AnsiConsole.Ask<string>("Ingrese el [blue]nuevo estatus[/]:");

                                    try
                                    {
                                        var exito = await contratoService.CambiarEstatusContratoAsync(contratoId, nuevoEstatus);
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Estatus de contrato actualizado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo actualizar el estatus del contrato.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al cambiar estatus del contrato: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirContratos = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 9: Órdenes de Fabricación]***********************************************************************
            //**************************************************************************************************************
            case "Órdenes de Fabricación":
                {
                    bool salirOrdenes = false;
                    while (!salirOrdenes)
                    {
                        var opcionOrden = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Órdenes de Fabricación:[/]")
                                .AddChoices(new[]
                                {
                                    "Crear orden (Create.json)",
                                    "Actualizar orden (Edit.json)",
                                    "Consultar orden por ID",
                                    "Validar integridad de orden",
                                    "Regresar"
                                })
                        );

                        switch (opcionOrden)
                        {
                            case "Crear orden (Create.json)":
                                {
                                    using var httpClient = new HttpClient();
                                    var ordenService = new OrdenFabricacionService(httpClient, configuration, token);

                                    try
                                    {
                                        var ordenes = await ordenService.CrearOrdenFabricacionAsync();
                                        if (ordenes != null && ordenes.Count > 0)
                                        {
                                            AnsiConsole.MarkupLine("[green]Orden de fabricación creada correctamente.[/]");
                                            // Mostrar la lista de órdenes creadas
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Lote");
                                            table.AddColumn("Id Producto");
                                            table.AddColumn("Detalles");

                                            foreach (var orden in ordenes)
                                            {
                                                table.AddRow(
                                                    orden.Id ?? "",
                                                    orden.ClaveOrdenFabricacion ?? "",
                                                    orden.LoteFabricacion ?? "",
                                                    orden.IdProducto ?? "",
                                                    orden.DetalleFabricacion != null
                                                        ? string.Join("\n", orden.DetalleFabricacion.Select(d =>
                                                            $"Contrato: {d.ContratoId}, Tipo: {d.TipoContrato}, Partida: {d.PartidaContratoId}, Desc: {d.DescripcionPartida}, Unidad: {d.Unidad}, Cant. Contrato: {d.CantidadOriginalContrato}, Cant. Fabricar: {d.CantidadAFabricar}"))
                                                        : ""
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[red]No se pudo crear la orden de fabricación.[/]");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al crear la orden de fabricación: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Actualizar orden (Edit.json)":
                                {
                                    using var httpClient = new HttpClient();
                                    var ordenService = new OrdenFabricacionService(httpClient, configuration, token);

                                    try
                                    {
                                        var ordenes = await ordenService.ActualizarOrdenFabricacionAsync();
                                        if (ordenes != null && ordenes.Count > 0)
                                        {
                                            AnsiConsole.MarkupLine("[green]Orden de fabricación actualizada correctamente.[/]");
                                            // Mostrar la lista de órdenes actualizadas
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Lote");
                                            table.AddColumn("Id Producto");
                                            table.AddColumn("Detalles");

                                            foreach (var orden in ordenes)
                                            {
                                                table.AddRow(
                                                    orden.Id ?? "",
                                                    orden.ClaveOrdenFabricacion ?? "",
                                                    orden.LoteFabricacion ?? "",
                                                    orden.IdProducto ?? "",
                                                    orden.DetalleFabricacion != null
                                                        ? string.Join("\n", orden.DetalleFabricacion.Select(d =>
                                                            $"Contrato: {d.ContratoId}, Tipo: {d.TipoContrato}, Partida: {d.PartidaContratoId}, Desc: {d.DescripcionPartida}, Unidad: {d.Unidad}, Cant. Contrato: {d.CantidadOriginalContrato}, Cant. Fabricar: {d.CantidadAFabricar}"))
                                                        : ""
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[red]No se pudo actualizar la orden de fabricación.[/]");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al actualizar la orden de fabricación: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Consultar orden por ID":
                                {
                                    using var httpClient = new HttpClient();
                                    var ordenService = new OrdenFabricacionService(httpClient, configuration, token);

                                    var ordenId = AnsiConsole.Ask<string>("Ingrese el [green]Id de la orden de fabricación[/]:");
                                    try
                                    {
                                        var ordenes = await ordenService.ObtenerOrdenFabricacionAsync(ordenId);
                                        if (ordenes == null)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontró la orden de fabricación.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]ORDEN DE FABRICACIÓN[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Lote");
                                            table.AddColumn("Id Producto");
                                            table.AddColumn("Detalles");

                                            foreach (var orden in ordenes)
                                            {
                                                table.AddRow(
                                                    orden.Id ?? "",
                                                    orden.ClaveOrdenFabricacion ?? "",
                                                    orden.LoteFabricacion ?? "",
                                                    orden.IdProducto ?? "",
                                                    orden.DetalleFabricacion != null
                                                        ? string.Join("\n", orden.DetalleFabricacion.Select(d =>
                                                            $"Contrato: {d.ContratoId}, Tipo: {d.TipoContrato}, Partida: {d.PartidaContratoId}, Desc: {d.DescripcionPartida}, Unidad: {d.Unidad}, Cant. Contrato: {d.CantidadOriginalContrato}, Cant. Fabricar: {d.CantidadAFabricar}"))
                                                        : ""
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al consultar la orden de fabricación: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Validar integridad de orden":
                                {
                                    using var httpClient = new HttpClient();
                                    var ordenService = new OrdenFabricacionService(httpClient, configuration, token);

                                    var ordenId = AnsiConsole.Ask<string>("Ingrese el [green]Id de la orden de fabricación[/]:");
                                    try
                                    {
                                        var exito = await ordenService.ValidarOrdenCompletaAsync(ordenId);
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]La orden de fabricación está completa.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]La orden de fabricación NO está completa o no existe.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al validar la orden de fabricación: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirOrdenes = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Práctica 10: Expedientes de Pruebas]**********************************************************************
            //**************************************************************************************************************
            case "Expedientes de Pruebas":
                {
                    bool salirExpedientes = false;
                    while (!salirExpedientes)
                    {
                        var opcionExpediente = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[bold]Seleccione una opción de Expedientes:[/]")
                                .AddChoices(new[]
                                {
                                    "Consultar expedientes (paginado)",
                                    "Consultar expediente por clave",
                                    "Crear expediente (Create.json)",
                                    "Actualizar expediente (Edit.json)",
                                    "Agregar muestra a expediente",
                                    "Quitar muestra de expediente",
                                    "Regresar"
                                })
                        );

                        switch (opcionExpediente)
                        {
                            case "Consultar expedientes (paginado)":
                                {
                                    using var httpClient = new HttpClient();
                                    var expedienteService = new ExpedienteService(httpClient, configuration, token);

                                    var pageNumber = AnsiConsole.Ask<int>("Ingrese el [blue]número de página[/]:");
                                    var pageSize = AnsiConsole.Ask<int>("Ingrese el [blue]tamaño de página[/]:");

                                    try
                                    {
                                        var expedientes = await expedienteService.ObtenerExpedientesAsync(pageNumber, pageSize);
                                        if (expedientes.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontraron expedientes.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]EXPEDIENTES[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Tamaño Muestra");
                                            table.AddColumn("Max. Rechazos");
                                            table.AddColumn("Tipo Muestreo");
                                            table.AddColumn("Estatus Pruebas");
                                            table.AddColumn("Resultado Expediente");
                                            table.AddColumn("OrdenFabricacion Ids");
                                            table.AddColumn("Inicio Pruebas");
                                            table.AddColumn("Fin Pruebas");
                                            table.AddColumn("Fecha Registro");

                                            foreach (var exp in expedientes)
                                            {
                                                table.AddRow(
                                                    exp.Id ?? "",
                                                    exp.ClaveExpediente ?? "",
                                                    exp.TamanioMuestra.ToString(),
                                                    exp.MaximoRechazos.ToString(),
                                                    exp.TipoMuestreo ?? "",
                                                    exp.EstatusPruebas ?? "",
                                                    exp.ResultadoExpediente ?? "",
                                                    exp.OrdenFabricacion != null
                                                        ? $"Id: {exp.OrdenFabricacion.Id}, Clave: {exp.OrdenFabricacion.ClaveOrdenFabricacion}, Lote: {exp.OrdenFabricacion.LoteFabricacion}, IdProducto: {exp.OrdenFabricacion.IdProducto}"
                                                        : "",
                                                    exp.InicioPruebas != DateTime.MinValue ? exp.InicioPruebas.ToString("u") : "",
                                                    exp.FinPruebas != DateTime.MinValue ? exp.FinPruebas.ToString("u") : "",
                                                    exp.FechaRegistro != DateTime.MinValue ? exp.FechaRegistro.ToString("u") : ""
                                                );
                                                if (exp.MuestrasExpediente != null && exp.MuestrasExpediente.Count > 0)
                                                {
                                                    var muestraTable = new Table();
                                                    muestraTable.AddColumn("Id_Expediente");
                                                    muestraTable.AddColumn("Identificador");
                                                    muestraTable.AddColumn("Estatus");
                                                    muestraTable.AddColumn("Resultados de Pruebas (IDs)");

                                                    foreach (var muestra in exp.MuestrasExpediente)
                                                    {
                                                        // Tabla de resultados de pruebas (solo IDs)
                                                        var resultados = "";
                                                        if (muestra.ResultadosPruebas != null && muestra.ResultadosPruebas.Count > 0)
                                                        {
                                                            foreach (var res in muestra.ResultadosPruebas)
                                                            {
                                                                resultados +=
                                                                    $"PruebaId: {res.Prueba?.Id ?? ""}, " +
                                                                    $"\nValorReferenciaId: {res.ValorReferencia?.Id ?? ""}, " +
                                                                    $"\nInstrumentoId: {res.InstrumentoMedicion?.Id ?? ""}\n";
                                                            }
                                                        }
                                                        muestraTable.AddRow(
                                                            exp.Id ?? "",
                                                            muestra.Identificador ?? "",
                                                            muestra.Estatus ?? "",
                                                            resultados.TrimEnd('\n')
                                                        );
                                                    }
                                                    AnsiConsole.MarkupLine("[bold]MUESTRAS DEL EXPEDIENTE:[/]");
                                                    AnsiConsole.Write(muestraTable);
                                                }
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al consultar expedientes: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Consultar expediente por clave":
                                {
                                    using var httpClient = new HttpClient();
                                    var expedienteService = new ExpedienteService(httpClient, configuration, token);

                                    var clave = AnsiConsole.Ask<string>("Ingrese la [green]clave del expediente[/]:");
                                    try
                                    {
                                        var expedientes = await expedienteService.ObtenerExpedientePorIdAsync(clave);
                                        if (expedientes == null || expedientes.Count == 0)
                                        {
                                            AnsiConsole.MarkupLine("[yellow]No se encontró el expediente.[/]");
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("[bold blue]EXPEDIENTE[/]");
                                            var table = new Table();
                                            table.AddColumn("Id");
                                            table.AddColumn("Clave");
                                            table.AddColumn("Tamaño Muestra");
                                            table.AddColumn("Max. Rechazos");
                                            table.AddColumn("Tipo Muestreo");
                                            table.AddColumn("Estatus Pruebas");
                                            table.AddColumn("Resultado Expediente");

                                            foreach (var exp in expedientes)
                                            {
                                                table.AddRow(
                                                    exp.Id ?? "",
                                                    exp.ClaveExpediente ?? "",
                                                    exp.TamanioMuestra.ToString(),
                                                    exp.MaximoRechazos.ToString(),
                                                    exp.TipoMuestreo ?? "",
                                                    exp.EstatusPruebas ?? "",
                                                    exp.ResultadoExpediente ?? ""
                                                );
                                            }
                                            AnsiConsole.Write(table);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al consultar expediente: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Crear expediente (Create.json)":
                                {
                                    using var httpClient = new HttpClient();
                                    var expedienteService = new ExpedienteService(httpClient, configuration, token);

                                    try
                                    {
                                        var exito = await expedienteService.CrearExpedienteAsync();
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Expediente creado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo crear el expediente.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al crear expediente: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Actualizar expediente (Edit.json)":
                                {
                                    using var httpClient = new HttpClient();
                                    var expedienteService = new ExpedienteService(httpClient, configuration, token);

                                    try
                                    {
                                        var exito = await expedienteService.ActualizarExpedienteAsync();
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Expediente actualizado correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo actualizar el expediente.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al actualizar expediente: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Agregar muestra a expediente":
                                {
                                    using var httpClient = new HttpClient();
                                    var expedienteService = new ExpedienteService(httpClient, configuration, token);

                                    var clave = AnsiConsole.Ask<string>("Ingrese la [green]clave del expediente[/]:");
                                    var muestra = AnsiConsole.Ask<string>("Ingrese la [blue]muestra a agregar[/]:");

                                    try
                                    {
                                        var exito = await expedienteService.AgregarMuestraAsync(clave, muestra);
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Muestra agregada correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo agregar la muestra.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al agregar muestra: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Quitar muestra de expediente":
                                {
                                    using var httpClient = new HttpClient();
                                    var expedienteService = new ExpedienteService(httpClient, configuration, token);

                                    var clave = AnsiConsole.Ask<string>("Ingrese la [green]clave del expediente[/]:");
                                    var muestra = AnsiConsole.Ask<string>("Ingrese la [blue]muestra a quitar[/]:");

                                    try
                                    {
                                        var exito = await expedienteService.QuitarMuestraAsync(clave, muestra);
                                        if (exito)
                                            AnsiConsole.MarkupLine("[green]Muestra quitada correctamente.[/]");
                                        else
                                            AnsiConsole.MarkupLine("[red]No se pudo quitar la muestra.[/]");
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.MarkupLine($"[red]Error al quitar muestra: {ex.Message}[/]");
                                    }
                                    break;
                                }
                            case "Regresar":
                                salirExpedientes = true;
                                break;
                        }
                    }
                    break;
                }
            //**************************************************************************************************************
            //***[Salir de la aplicación]***********************************************************************************
            //**************************************************************************************************************
            case "Terminar ejecución":
                salir = true;
                AnsiConsole.MarkupLine("[yellow]Ejecución finalizada.[/]");
                break;
            //**************************************************************************************************************
            //**************************************************************************************************************
        }
    }

    // Finaliza el background service
    await app.StopAsync();
    await hostTask;
}
else
{
    AnsiConsole.MarkupLine($"[bold red]Error de autenticación:[/] {responseAuth.Errors[0].Message}");
}