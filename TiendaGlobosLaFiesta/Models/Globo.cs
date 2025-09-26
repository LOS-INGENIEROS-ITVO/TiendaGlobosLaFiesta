using System.Collections.Generic;
using System.Linq;

namespace TiendaGlobosLaFiesta.Models
{
    public class Globo
    {
        public string GloboId { get; set; }
        public string Material { get; set; }
        public string Unidad { get; set; }
        public string Color { get; set; }
        public int Stock { get; set; }
        public decimal Costo { get; set; }
        public string ProveedorId { get; set; }
        public bool Activo { get; set; } = true;

        // Propiedades de características (se mantienen)
        public List<string> Tamanos { get; set; } = new List<string>();
        public List<string> Formas { get; set; } = new List<string>();
        public List<string> Tematicas { get; set; } = new List<string>();

        // Propiedades calculadas (se mantienen)
        public string Tamano => string.Join(", ", Tamanos);
        public string Forma => string.Join(", ", Formas);
        public string Tematica => string.Join(", ", Tematicas);
        public string Nombre => $"{Material} {Tamano} {Forma}".Trim();

        // 🔹 PROPIEDAD AÑADIDA PARA CONSISTENCIA 🔹
        public int VentasHoy { get; set; }

        public Globo Clone()
        {
            var clon = (Globo)this.MemberwiseClone();
            clon.Tamanos = new List<string>(this.Tamanos);
            clon.Formas = new List<string>(this.Formas);
            clon.Tematicas = new List<string>(this.Tematicas);
            return clon;
        }
    }
}