using System;
using System.Collections.Generic;

namespace ApiClientLibrary.Models
{
    public class DescripcionCortaExpedienteDTO
    {
        public string Id { get; set; }
        public string DescripcionCorta { get; set; }
        public string Norma { get; set; }
    }

    public class DescripcionCortaResponseDTO
    {
        public int Total { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
        public List<DescripcionCortaExpedienteDTO> Expedientes { get; set; }
    }
}