using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Models
{
    public class ProductoVenta : ItemVenta
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Unidad { get; set; }  // Cantidad por unidad
    }
}
