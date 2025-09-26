using System;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Models 
{
    public class VentaHistorial
    {
        public string VentaId { get; set; }
        public string ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string NombreEmpleado { get; set; } 
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public string Estatus { get; set; } 

        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();
    }
}