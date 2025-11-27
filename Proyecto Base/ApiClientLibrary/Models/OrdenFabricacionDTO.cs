using System.Collections.Generic;

namespace ApiClientLibrary.Models
{
    /// <summary>
    /// DTO para la orden de fabricación.
    /// </summary>
    public class OrdenFabricacionDTO
    {
        public string Id { get; set; }
        public string ClaveOrdenFabricacion { get; set; }
        public string LoteFabricacion { get; set; }
        public string IdProducto { get; set; }
        public List<DetalleFabricacionDTO> DetalleFabricacion { get; set; }
    }

    /// <summary>
    /// DTO para el detalle de fabricación dentro de la orden.
    /// </summary>
    public class DetalleFabricacionDTO
    {
        public string ContratoId { get; set; }
        public string TipoContrato { get; set; }
        public string PartidaContratoId { get; set; }
        public string DescripcionPartida { get; set; }
        public string Unidad { get; set; }
        public int CantidadOriginalContrato { get; set; }
        public int CantidadAFabricar { get; set; }
    }
}