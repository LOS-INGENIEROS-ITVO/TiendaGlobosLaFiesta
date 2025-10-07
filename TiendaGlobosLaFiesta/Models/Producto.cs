namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Unidad { get; set; } = "pieza"; // ahora string
        public decimal Costo { get; set; }
        public int Stock { get; set; }
        public string? ProveedorId { get; set; }
        public int? CategoriaId { get; set; }
        public bool Activo { get; set; } = true;
        public int VentasHoy { get; set; }

        public Producto Clone() => (Producto)this.MemberwiseClone();
    }
}
