using ApiClientLibrary.Services;

F1_ConfiguracionInicial apirest = new F1_ConfiguracionInicial();
var response = await apirest.RegistrarEstadoSID(new ApiClientLibrary.Models.EstadoSIDDTO
    {
        Estado = "EN_ESPERA"
    });

Console.WriteLine(response);