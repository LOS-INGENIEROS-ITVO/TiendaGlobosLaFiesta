using System;
using System.Collections.Generic;

namespace TiendaGlobosLaFiesta.Models.Dashboard
{
    public class DashboardEmpleadoData
    {
        public decimal MisVentasHoy { get; set; }
        public int TicketsHoy { get; set; }
        public decimal TicketPromedioHoy { get; set; }
        public int ClientesAtendidosHoy { get; set; }
        public Dictionary<DateTime, decimal> VentasDiarias7Dias { get; set; } = new Dictionary<DateTime, decimal>();
    }
}