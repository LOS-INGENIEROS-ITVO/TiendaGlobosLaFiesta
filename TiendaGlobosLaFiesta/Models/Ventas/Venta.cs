using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TiendaGlobosLaFiesta.Models.Ventas;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class Venta
    {
        public string VentaId { get; set; }
        public string ClienteId { get; set; }
        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();
        public DateTime FechaVenta { get; set; }
        public string Estatus { get; set; }
        public decimal ImporteTotal { get; set; }
    }
}