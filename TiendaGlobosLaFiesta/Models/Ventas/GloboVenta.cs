using System;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class GloboVenta : ItemVenta
    {
        public string GloboId { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Tamano { get; set; } = string.Empty;
        public string Forma { get; set; } = string.Empty;
        public string Tematica { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;

        public string Nombre => $"{Material} {Color} {Tamano} {Forma} {Tematica}".Trim();
    }
}
