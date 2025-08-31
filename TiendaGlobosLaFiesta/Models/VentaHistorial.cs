using System;

namespace TiendaGlobosLaFiesta.Models
{
    public class VentaHistorial
    {
        public string VentaId { get; set; }
        public string Cliente { get; set; }
        public string Empleado { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}
