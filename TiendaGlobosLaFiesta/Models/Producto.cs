namespace TiendaGlobosLaFiesta.Models
{
    public class Producto
    {
        public string ProductoId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        private int unidad;
        public int Unidad
        {
            get => unidad;
            set
            {
                if (value <= 0) throw new ArgumentException("La unidad debe ser mayor que 0.");
                unidad = value;
            }
        }

        private decimal costo;
        public decimal Costo
        {
            get => costo;
            set
            {
                if (value < 0) throw new ArgumentException("El costo no puede ser negativo.");
                costo = value;
            }
        }

        private int stock;
        public int Stock
        {
            get => stock;
            set
            {
                if (value < 0) throw new ArgumentException("El stock no puede ser negativo.");
                stock = value;
            }
        }

        public string? ProveedorId { get; set; }
        public int? CategoriaId { get; set; }
        public bool Activo { get; set; } = true;
        public int VentasHoy { get; set; }

        public Producto Clone() => (Producto)this.MemberwiseClone();
    }
}