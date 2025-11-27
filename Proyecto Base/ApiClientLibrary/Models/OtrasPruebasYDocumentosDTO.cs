using System;

namespace ApiClientLibrary.Models
{
    public class OtrasPruebasYDocumentosDTO
    {
        public string Id { get; set; }
        public string TipoDocumento { get; set; }
        public string DescripcionDocumento { get; set; }
        public string UrlArchivo { get; set; }
        public string MD5 { get; set; }
        public string Estatus { get; set; }
        public DateTime Vigencia { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}