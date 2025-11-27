using System;
using System.Collections.Generic;

namespace ApiClientLibrary.Models
{
    public class ProductoDTO
    {
        public string Id { get; set; }
        public string CodigoFabricante { get; set; }
        public string Descripcion { get; set; }
        public string DescripcionCorta { get; set; }
        public string TipoFabricacion { get; set; }
        public string Unidad { get; set; }
        public string Norma { get; set; }
        public string Prototipo { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<string> Pruebas { get; set; }
    }
    public class ProductoResponseDTO
    {
        public string Id { get; set; }
        public string CodigoFabricante { get; set; }
        public string Descripcion { get; set; }
        public string DescripcionCorta { get; set; }
        public string TipoFabricacion { get; set; }
        public string Unidad { get; set; }
        public NormaDTO Norma { get; set; }
        public PrototipoDTO Prototipo { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<PruebaDTO> Pruebas { get; set; }
    }
}