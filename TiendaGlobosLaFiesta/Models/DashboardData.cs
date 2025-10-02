namespace TiendaGlobosLaFiesta.Models
{
    public class DashboardData
    {
        // KPIs Numéricos Principales
        public decimal VentasHoy { get; set; }
        public decimal Ventas7Dias { get; set; }
        public decimal VentasMes { get; set; }
        public decimal TicketPromedioHoy { get; set; }
        public int TotalStockCritico { get; set; }
        public int TotalClientesFrecuentes { get; set; }

        public List<string> NombresStockCritico { get; set; } = new List<string>();
        public List<string> NombresClientesFrecuentes { get; set; } = new List<string>();

        // Detalles del Top Cliente
        public string NombreTopCliente { get; set; }
        public decimal TotalTopCliente { get; set; }

        // Detalles de Productos Más Vendidos
        public string ProductoMasVendidoDia { get; set; }
        public string ProductoMasVendidoSemana { get; set; }
        public string ProductoMasVendidoMes { get; set; }

        // Datos para la Gráfica de Ventas Semanales
        public Dictionary<DateTime, decimal> VentasDiarias7Dias { get; set; } = new Dictionary<DateTime, decimal>();
    }
}