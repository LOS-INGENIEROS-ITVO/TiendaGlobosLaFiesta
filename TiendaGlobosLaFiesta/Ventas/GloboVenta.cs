using TiendaGlobosLaFiesta.Models;

public class GloboVenta : ItemVenta
{
    public string GloboId { get; set; }
    public string Material { get; set; }
    public string Color { get; set; }
    public string Tamano { get; set; }
    public string Forma { get; set; }
    public string Tematica { get; set; }
    public string Unidad { get; set; }

    public string Nombre => $"{Material} {Color} {Tamano} {Forma} {Tematica}";
}