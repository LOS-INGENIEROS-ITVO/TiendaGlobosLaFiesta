namespace TiendaGlobosLaFiesta.Models.Pedidos
{
    public class DetallePedidoGlobo
    {
        public int DetallePedidoGloboId { get; set; }
        public int PedidoId { get; set; }
        public string GloboId { get; set; }
        public int CantidadSolicitada { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}