using System;

namespace ApiClientLibrary.Models
{
    public class NormaDTO
    {
        public string Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Edicion { get; set; }
        public string Estatus { get; set; }
        public bool EsCFE { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}