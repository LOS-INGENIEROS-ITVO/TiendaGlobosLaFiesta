using System.Collections.Generic;

namespace TiendaGlobosLaFiesta.Models
{
    public class Globo
    {
        public string GloboId { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Unidad { get; set; } = "pieza";
        public string Color { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal Costo { get; set; }
        public string ProveedorId { get; set; } = string.Empty;
        public string ProveedorNombre { get; set; } = "Sin proveedor";

        public bool Activo { get; set; } = true;

        public List<string> Tamanos { get; set; } = new();
        public List<string> Formas { get; set; } = new();
        public List<string> Tematicas { get; set; } = new();

        public string Tamano => Tamanos.Count > 0 ? string.Join(", ", Tamanos) : "---";
        public string Forma => Formas.Count > 0 ? string.Join(", ", Formas) : "---";
        public string Tematica => Tematicas.Count > 0 ? string.Join(", ", Tematicas) : "---";

        public string Nombre => $"{Material} {Tamano} {Forma}".Trim();
        public int VentasHoy { get; set; }

        public Globo Clone()
        {
            var clon = (Globo)this.MemberwiseClone();
            clon.Tamanos = new List<string>(Tamanos);
            clon.Formas = new List<string>(Formas);
            clon.Tematicas = new List<string>(Tematicas);
            return clon;
        }
    }
}