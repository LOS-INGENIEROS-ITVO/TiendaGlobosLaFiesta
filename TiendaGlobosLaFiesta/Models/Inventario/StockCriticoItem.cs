// TiendaGlobosLaFiesta\Models\StockCriticoItem.cs
namespace TiendaGlobosLaFiesta.Models.Inventario
{
    public class StockCriticoItem
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public int StockActual { get; set; }
        public decimal Precio { get; set; }  // si aplica, puede ser 0 o tomar costo de Producto/Globo
        public string Tipo { get; set; }     // "Producto" o "Globo"
        public string Unidad { get; set; }   // ej. "pieza", "pack"
        public string Color { get; set; }    // usado para globos (si aplica)

        public Producto Producto { get; set; }
        public Globo Globo { get; set; }

        // Emoji según tipo
        public string Emoji => Tipo == "Globo" ? "🎈" : "🛍️";
    }
}
