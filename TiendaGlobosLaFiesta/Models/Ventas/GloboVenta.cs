using System.Linq;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class GloboVenta : ItemVenta
    {
        public string GloboId { get; set; } = string.Empty;
        public string Tamano { get; set; } = string.Empty;
        public string Forma { get; set; } = string.Empty;
        public string Tematica { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public override string Id => GloboId;

        public override string Nombre => string.Join(" ", new[] { Material, Color, Tamano, Forma, Tematica }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
