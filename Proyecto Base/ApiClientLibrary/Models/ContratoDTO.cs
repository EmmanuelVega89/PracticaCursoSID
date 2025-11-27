using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiClientLibrary.Models
{
    /// <summary>
    /// DTO genérico para contratos CFE, CFE con garantía y Particular.
    /// </summary>
    public class ContratoDTO
    {
        [JsonPropertyName("$Tipo")]
        public string Tipo { get; set; }
        public string Id { get; set; }

        public string TipoContrato { get; set; }
        public string NoContrato { get; set; }
        public string Estatus { get; set; }
        public List<DetalleContratoDTO> DetalleContrato { get; set; }

        // Solo para ContratoCFE y ContratoCFEConGarantia
        public string UrlArchivo { get; set; }
        public string MD5 { get; set; }
        public DateTime? FechaEntregaCFE { get; set; }

        // Solo para ContratoCFEConGarantia
        public double? PerdidasGarantizadasVacio { get; set; }
        public double? PerdidasGarantizadasCarga { get; set; }
    }

    /// <summary>
    /// DTO para el detalle de contrato.
    /// </summary>
    public class DetalleContratoDTO
    {
        public string PartidaContrato { get; set; }
        public string DescripcionAviso { get; set; }
        // Solo para contrato de CFE
        public string AreaDestinoCFE { get; set; } 
        public int Cantidad { get; set; }
        public string Unidad { get; set; }
        public double ImporteTotal { get; set; }
    }
}