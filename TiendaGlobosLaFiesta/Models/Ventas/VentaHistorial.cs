using System.ComponentModel;
using System.Runtime.CompilerServices;
using TiendaGlobosLaFiesta.Models.Ventas;

public class VentaHistorial
{
    public string VentaId { get; set; }
    public string ClienteNombre { get; set; }
    public string ClienteId { get; set; } // <-- agregado
    public string NombreEmpleado { get; set; }
    public DateTime FechaVenta { get; set; }
    public string Estatus { get; set; }
    public decimal Total { get; set; }

    public VentaHistorial() { }

    public VentaHistorial(Venta venta, string clienteNombre, string empleadoNombre)
    {
        VentaId = venta.VentaId;
        ClienteNombre = clienteNombre;
        NombreEmpleado = empleadoNombre;
        FechaVenta = venta.FechaVenta;
        Estatus = venta.Estatus;
        Total = venta.ImporteTotal;
    }
}