namespace ApiClientLibrary.Models
{
    public class ExpedienteInspeccionDTO
    {
        public string Id { get; set; }
        public string ClaveExpediente { get; set; }
        public string OrdenFabricacion { get; set; }
        public int CantidadMuestras { get; set; }
        public int MaximoRechazos { get; set; }
        public List<string> Muestras { get; set; }
    }
    public class ExpedienteInspeccionPaginationDTO
    {
        public int Total { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
        public List<ExpedienteInspeccionCompleteDTO> Expedientes { get; set; }
    }
    public class ExpedienteInspeccionCompleteDTO
    {
        public string Id { get; set; }
        public string ClaveExpediente { get; set; }
        public List<MuestraDTO> MuestrasExpediente { get; set; }
        public int TamanioMuestra { get; set; }
        public int MaximoRechazos { get; set; }
        public string TipoMuestreo { get; set; }
        public string ResultadosPruebas { get; set; }
        public OrdenFabricacionDTO OrdenFabricacion { get; set; }
        public List<string> AvisosPrueba { get; set; }
        public string EstatusPruebas { get; set; }
        public string ResultadoExpediente { get; set; }
        public DateTime InicioPruebas { get; set; }
        public DateTime FinPruebas { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
    public class MuestraDTO
    {
        public string Identificador { get; set; }
        public string Estatus { get; set; }
        public List<ResultadoPruebasDTO> ResultadosPruebas { get; set; }

    }
    public class ResultadoPruebasDTO
    {
        public PruebaDTO Prueba { get; set; }
        public ValorReferenciaDTO ValorReferencia { get; set; }
        public DateTime FechaPrueba { get; set; }
        public InstrumentoDTO InstrumentoMedicion { get; set; }
        public int valorMedido { get; set; }
        public string Resultado {  get; set; }
        public int NumeroIntento { get; set; }
    }
}