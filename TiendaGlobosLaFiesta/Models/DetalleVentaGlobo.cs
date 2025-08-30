namespace TiendaGlobosLaFiesta.Models
{
    public class DetalleVentaGlobo
    {
        public int DetalleGloboId { get; set; }
        public string VentaId { get; set; }
        public string GloboId { get; set; }
        public int Cantidad { get; set; }
        public decimal Costo { get; set; }
        public decimal Importe { get; set; }
    }
}