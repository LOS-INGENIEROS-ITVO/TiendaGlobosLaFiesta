namespace TiendaGlobosLaFiesta.Models
{
    public class DetallePedidoProducto
    {
        public int DetallePedidoProductoId { get; set; }
        public int PedidoId { get; set; }
        public string ProductoId { get; set; }
        public int CantidadSolicitada { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}