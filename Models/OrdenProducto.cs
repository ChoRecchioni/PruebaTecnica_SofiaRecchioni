namespace PruebaTecnica_SofiaRecchioni.Models
{
    public class OrdenProducto
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int ProductoId { get; set; }

        public int Cantidad { get; set; } = 1;  
        public decimal PrecioUnitario { get; set; } 

        public Orden? Orden { get; set; }
        public Producto? Producto { get; set; }
    }
}
