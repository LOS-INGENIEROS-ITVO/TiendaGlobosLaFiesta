namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; }   // productoId
        public string Nombre { get; set; }       // nombre
        public int Unidad { get; set; }          // unidad
        public int Stock { get; set; }           // stock
        public decimal Costo { get; set; }       // costo
    }
}
