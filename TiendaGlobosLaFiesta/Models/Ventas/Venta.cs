using System;
using System.Collections.ObjectModel;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class Venta
    {
        public string VentaId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public int EmpleadoId { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public decimal ImporteTotal { get; set; }

        public string Estatus { get; set; } = "Completada";

        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();

        // Calcula el total automáticamente
        public void CalcularImporteTotal()
        {
            ImporteTotal = Productos.Sum(p => p.Importe) + Globos.Sum(g => g.Importe);
        }
    }
}
