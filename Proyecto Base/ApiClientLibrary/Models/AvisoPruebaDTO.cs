using System;
using System.Collections.Generic;

namespace ApiClientLibrary.Models
{
    public class AvisoPruebaDTO
    {
        public string Id { get; set; }
        public int Aviso { get; set; }
        public string NumApProv { get; set; }
        public string Familia_ID { get; set; }
        public string Unidad_ID { get; set; }
        public string Pedido { get; set; }
        public int Partida { get; set; }
        public int Cantidad { get; set; }
        public double Costo { get; set; }
        public string Moneda { get; set; }
        public double Iva { get; set; }
        public string Lugar_Destino { get; set; }
        public string Observaciones { get; set; }
        public string Descripcion { get; set; }
        public string Descripcion_Corta { get; set; }
        public string Norma { get; set; }
        public string DescripNS { get; set; }
        public string Tipo_Aviso { get; set; }
        public List<NumeroSerieOLoteDTO> NumerosSerieOLote { get; set; }
        public string Penalizaciones { get; set; }
        public CreadoDTO Creado { get; set; }
        public string Empresa { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<object> Actualizaciones { get; set; }
    }

    public class NumeroSerieOLoteDTO
    {
        public int Aviso { get; set; }
        public string No_Del { get; set; }
        public string No_Al { get; set; }
        public string Del2 { get; set; }
        public string Al2 { get; set; }
        public string PatIni { get; set; }
        public string PatFin { get; set; }
        public string PatMed { get; set; }
        public string DescSerie { get; set; }
        public int Cant { get; set; }
    }

    public class CreadoDTO
    {
        public string Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Empresa { get; set; }
        public string Evento { get; set; }
    }
}