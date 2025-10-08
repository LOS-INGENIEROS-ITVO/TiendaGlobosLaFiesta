namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class ProductoVenta : ItemVenta
    {
        public string ProductoId { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;

        public override string Id => ProductoId;
        public override string Nombre => NombreProducto;
    }
}
