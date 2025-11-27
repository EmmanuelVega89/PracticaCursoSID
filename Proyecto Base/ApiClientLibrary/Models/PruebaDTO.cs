using System;

namespace ApiClientLibrary.Models
{
    public class PruebaDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Estatus { get; set; }
        public string TipoPrueba { get; set; }
        public string TipoResultado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}