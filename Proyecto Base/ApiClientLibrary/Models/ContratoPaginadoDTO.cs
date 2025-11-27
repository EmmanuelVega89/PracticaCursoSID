using System.Collections.Generic;

namespace ApiClientLibrary.Models
{
    public class ContratoPaginadoDTO
    {
        public int Total { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
        public List<ContratoDTO> Contratos { get; set; }
    }
}