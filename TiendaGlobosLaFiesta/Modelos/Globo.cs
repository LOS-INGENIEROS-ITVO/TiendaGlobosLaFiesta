namespace TiendaGlobosLaFiesta.Models
{
    public class Globo
    {
        public string GloboId { get; set; }     // globoId
        public string Material { get; set; }    // material
        public string Unidad { get; set; }      // unidad
        public string Color { get; set; }       // color
        public int Stock { get; set; }          // stock
        public decimal Costo { get; set; }      // costo

        // Propiedades relacionadas con otras tablas
        public string Tamano { get; set; }      // de Globo_Tamanio
        public string Forma { get; set; }       // de Globo_Forma
        public string Tematica { get; set; }    // de Tematica

        public string Nombre => $"{Material} {Tamano} {Forma}".Trim();
    }
}
