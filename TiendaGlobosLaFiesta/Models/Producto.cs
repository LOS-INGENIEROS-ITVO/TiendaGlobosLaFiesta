namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Unidad { get; set; }
        public decimal Costo { get; set; }
        public int Stock { get; set; }

        // 🔹 PROPIEDADES AÑADIDAS 🔹
        public string ProveedorId { get; set; }
        public int? CategoriaId { get; set; } // Puede ser nulo
        public bool Activo { get; set; } = true;

        // Propiedad extra para reportes (no va en la BD)
        public int VentasHoy { get; set; }

        public Producto Clone()
        {
            return (Producto)this.MemberwiseClone();
        }
    }
}