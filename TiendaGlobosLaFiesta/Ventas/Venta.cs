using System;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;

public class Venta
{
    public string VentaId { get; set; }
    public string ClienteId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaVenta { get; set; }
    public ObservableCollection<ProductoVenta> Productos { get; set; }
    public ObservableCollection<GloboVenta> Globos { get; set; }
    public decimal ImporteTotal { get; set; }
}
