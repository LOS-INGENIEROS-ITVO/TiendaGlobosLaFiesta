using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public class GloboVenta : ItemVenta
    {
        public string GloboId { get; set; }
        public string Material { get; set; }
        public string Color { get; set; }
        public string Tamano { get; set; }
        public string Forma { get; set; }
        public string Tematica { get; set; }
        public string Unidad { get; set; }
        public int Stock { get; set; }
        public decimal Costo { get; set; }

        private int _cantidad;
        public int Cantidad { get => _cantidad; set { _cantidad = value; OnPropertyChanged(nameof(Cantidad)); OnPropertyChanged(nameof(Importe)); } }

        public decimal Importe => Costo * Cantidad;

        public override string Id => GloboId;
        public override string Nombre => $"{Material} {Color}";

        public GloboVenta() { }

        public GloboVenta(Globo globo)
        {
            GloboId = globo.GloboId;
            Material = globo.Material;
            Color = globo.Color;
            Tamano = globo.Tamano;
            Forma = globo.Forma;
            Tematica = globo.Tematica;
            Unidad = globo.Unidad;
            Stock = globo.Stock;
            Costo = globo.Costo;
            Cantidad = 0;
        }
    }
}