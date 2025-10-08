using System;
using System.Collections.ObjectModel;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class VentaHistorial
    {
        public string VentaId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string Estatus { get; set; } = "Completada";

        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();
    }
}
