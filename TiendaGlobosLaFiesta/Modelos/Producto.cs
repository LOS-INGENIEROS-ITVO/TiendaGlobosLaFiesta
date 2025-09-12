namespace TiendaGlobosLaFiesta.Modelos
{
    public class Producto
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Unidad { get; set; }
        public decimal Costo { get; set; }
        public int Stock { get; set; }
        public int VentasHoy { get; set; }

        // 🔹 ASEGÚRATE DE QUE ESTE MÉTODO EXISTA 🔹
        public Producto Clone()
        {
            return (Producto)this.MemberwiseClone();
        }
    }
}