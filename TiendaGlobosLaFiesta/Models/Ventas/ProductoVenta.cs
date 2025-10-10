using System;
using System.Collections.ObjectModel;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class ProductoVenta : ItemVenta
    {
        public string ProductoId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty; // string consistente
    }
}
