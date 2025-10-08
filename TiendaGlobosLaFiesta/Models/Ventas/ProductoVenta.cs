using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class ProductoVenta : ItemVenta
    {
        public string ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Unidad { get; set; }
        public int Stock { get; set; }
        public decimal Costo { get; set; }

        private int _cantidad;
        public int Cantidad { get => _cantidad; set { _cantidad = value; OnPropertyChanged(nameof(Cantidad)); OnPropertyChanged(nameof(Importe)); } }

        public decimal Importe => Costo * Cantidad;

        public override string Id => ProductoId;
        public override string Nombre => NombreProducto;

        public ProductoVenta() { }

        public ProductoVenta(Producto producto)
        {
            ProductoId = producto.ProductoId;
            NombreProducto = producto.Nombre;
            Unidad = producto.Unidad;
            Stock = producto.Stock;
            Costo = producto.Costo;
            Cantidad = 0;
        }
    }
}