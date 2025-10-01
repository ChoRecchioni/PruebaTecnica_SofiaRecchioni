using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica_SofiaRecchioni.Data;
using PruebaTecnica_SofiaRecchioni.Models;

namespace PruebaTecnica_SofiaRecchioni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdenesController(AppDbContext context)
        {
            _context = context;
        }

        // Modelos
        public class OrdenDto
        {
            public int Id { get; set; }
            public string Cliente { get; set; }
            public DateTime FechaCreacion { get; set; }
            public decimal Total { get; set; }
            public List<OrdenProductoDto> OrdenProductos { get; set; } = new();
        }

        public class OrdenProductoDto
        {
            public int ProductoId { get; set; }
            public string ProductoNombre { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
        }


        // Endpoints CRUD para Ordenes
        // GET: api/ordenes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orden>>> GetOrdenes()
        {
            try
            {
                return await _context.Ordenes
                             .Include(o => o.OrdenProductos)
                             .ThenInclude(op => op.Producto)
                             .ToListAsync();
            }
            catch (Exception ex)
            {
                string x = ex.Message.ToString();
                throw;
            }
        }

        // GET: api/ordenes/
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenDto>> GetOrden(int id)
        {
            var orden = await _context.Ordenes
                .Include(o => o.OrdenProductos)
                .ThenInclude(op => op.Producto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return NotFound();

            var dto = new OrdenDto
            {
                Id = orden.Id,
                Cliente = orden.Cliente,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total,
                OrdenProductos = orden.OrdenProductos.Select(op => new OrdenProductoDto
                {
                    ProductoId = op.ProductoId,
                    ProductoNombre = op.Producto?.Nombre,
                    Cantidad = op.Cantidad,
                    PrecioUnitario = op.PrecioUnitario
                }).ToList()
            };

            return dto;
        }


        // POST: api/ordenes
        [HttpPost]
        public async Task<ActionResult<Orden>> CreateOrden(Orden orden)
        {
            foreach (var op in orden.OrdenProductos)
            {
                var producto = await _context.Productos.FindAsync(op.ProductoId);
                if (producto == null) return BadRequest($"Producto con Id {op.ProductoId} no existe");

                op.PrecioUnitario = producto.Precio;
            }

            // Calcular total con descuento dinámico
            orden.Total = CalcularTotalConDescuento(orden);
            orden.FechaCreacion = DateTime.Now;

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrden), new { id = orden.Id }, orden);
        }


        // PUT: api/ordenes/
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrden(int id, Orden orden)
        {
            if (id != orden.Id)
                return BadRequest();

            // Validar existencia de la orden
            var ordenExistente = await _context.Ordenes
                .Include(o => o.OrdenProductos)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenExistente == null)
                return NotFound();

            ordenExistente.Cliente = orden.Cliente;

            // Actualizar detalles (OrdenProductos)
            // Eliminar los existentes para reemplazarlos con los nuevos
            _context.OrdenProductos.RemoveRange(ordenExistente.OrdenProductos);

            ordenExistente.OrdenProductos.Clear();

            foreach (var op in orden.OrdenProductos)
            {
                var producto = await _context.Productos.FindAsync(op.ProductoId);
                if (producto == null)
                    return BadRequest($"Producto con Id {op.ProductoId} no existe");

                ordenExistente.OrdenProductos.Add(new OrdenProducto
                {
                    ProductoId = op.ProductoId,
                    Cantidad = op.Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            // Calcular nuevo total con descuentos dinámicos
            ordenExistente.Total = CalcularTotalConDescuento(ordenExistente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ordenes.Any(o => o.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        // DELETE: api/ordenes/
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrden(int id)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden == null)
                return NotFound();

            _context.Ordenes.Remove(orden);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Métodos
        private decimal CalcularTotalConDescuento(Orden orden)
        {
            decimal subtotal = orden.OrdenProductos.Sum(op => op.Cantidad * op.PrecioUnitario);

            decimal descuento = 0;

            // Regla 1: total > 500 => 10% descuento
            if (subtotal > 500)
                descuento += 0.10m;

            // Regla 2: más de 5 productos distintos => 5% adicional
            int productosDistintos = orden.OrdenProductos
                .Select(op => op.ProductoId)
                .Distinct()
                .Count();

            if (productosDistintos > 5)
                descuento += 0.05m;

            decimal totalConDescuento = subtotal * (1 - descuento);
            return totalConDescuento;
        }

    }
}
