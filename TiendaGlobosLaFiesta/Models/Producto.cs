namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Unidad { get; set; }
        public decimal Costo { get; set; }
        public int Stock { get; set; }
        public string ProveedorId { get; set; }
        public int? CategoriaId { get; set; }
        public bool Activo { get; set; } = true;
        public int VentasHoy { get; set; }

        public Producto Clone()
        {
            return (Producto)this.MemberwiseClone();
        }
    }
}
