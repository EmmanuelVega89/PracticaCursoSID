using System;

namespace ApiClientLibrary.Models
{
    public class AgregaResultadoPruebaDTO
    {
        public string IdPrueba { get; set; }
        public string IdValorReferencia { get; set; }
        public DateTime FechaPrueba { get; set; }
        public string OperadorPrueba { get; set; }
        public string IdInstrumentoMedicion { get; set; }
        public double ValorMedido { get; set; }
        public string Resultado { get; set; }
        public int NumeroIntento { get; set; }
    }
}