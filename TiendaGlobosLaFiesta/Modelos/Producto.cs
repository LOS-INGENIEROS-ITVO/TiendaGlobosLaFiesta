namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Unidad { get; set; }
        public decimal Costo { get; set; }
        public int Stock { get; set; }

        // Nueva propiedad para reportes de ventas (no relacionada al inventario real)
        public int VentasHoy { get; set; }
    }
}
