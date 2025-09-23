using System;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;

// Este modelo puede estar en la raíz o en la carpeta Models.
// Si está en Models, su namespace debería ser TiendaGlobosLaFiesta.Models
public class Venta
{
    public string VentaId { get; set; }
    public string ClienteId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaVenta { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Estatus { get; set; }

    public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
    public ObservableCollection<GloboVenta> Globos { get; set; } = new();
}