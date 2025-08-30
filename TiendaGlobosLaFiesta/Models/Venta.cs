using System;
using System.Collections.ObjectModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class Venta
    {
        public string VentaId { get; set; }
        public int EmpleadoId { get; set; }
        public string ClienteId { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal ImporteTotal { get; set; }

        public ObservableCollection<ProductoVenta> Productos { get; set; } = new ObservableCollection<ProductoVenta>();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new ObservableCollection<GloboVenta>();
    }
}
