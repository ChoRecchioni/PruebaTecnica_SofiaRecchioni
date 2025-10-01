namespace PruebaTecnica_SofiaRecchioni.Models
{
    public class Orden
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = null!;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public decimal Total { get; set; }

        public List<OrdenProducto> OrdenProductos { get; set; } = new();
    }
}
