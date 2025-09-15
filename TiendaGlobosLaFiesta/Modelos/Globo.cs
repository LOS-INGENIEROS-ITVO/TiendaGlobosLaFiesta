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

        public string Tamano { get; set; }    
        public string Forma { get; set; }      
        public string Tematica { get; set; }   

        public string Nombre => $"{Material} {Tamano} {Forma}".Trim();
        public int VentasHoy { get; set; }
    }
}