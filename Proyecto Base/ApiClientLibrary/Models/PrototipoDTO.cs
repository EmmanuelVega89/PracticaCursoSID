using System;

namespace ApiClientLibrary.Models
{
    public class PrototipoDTO
    {
        public string Id { get; set; }
        public string Numero { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string UrlArchivo { get; set; }
        public string MD5 { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}