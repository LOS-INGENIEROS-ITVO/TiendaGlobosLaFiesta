namespace TiendaGlobosLaFiesta.Models
{
    public class DetalleVentaProducto
    {
        public int DetalleProductoId { get; set; }
        public string VentaId { get; set; }
        public string ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Costo { get; set; }
        public decimal Importe { get; set; }
    }
}
