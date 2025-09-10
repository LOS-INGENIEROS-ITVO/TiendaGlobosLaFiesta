namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}
