using System.Collections.ObjectModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class Pedido
    {
        public int PedidoId { get; set; }
        public string ProveedorId { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Estatus { get; set; }
        public decimal? Total { get; set; }

        public ObservableCollection<DetallePedidoProducto> Productos { get; set; } = new();
        public ObservableCollection<DetallePedidoGlobo> Globos { get; set; } = new();
    }
}