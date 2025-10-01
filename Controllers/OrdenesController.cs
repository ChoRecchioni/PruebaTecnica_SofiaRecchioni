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

        // DTOs para respuestas
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

        // GET: api/ordenes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orden>>> GetOrdenes()
        {
            try
            {
                var ordenes = await _context.Ordenes
                    .Include(o => o.OrdenProductos)
                    .ThenInclude(op => op.Producto)
                    .ToListAsync();

                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las órdenes: {ex.Message}");
            }
        }

        // GET: api/ordenes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenDto>> GetOrden(int id)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.OrdenProductos)
                    .ThenInclude(op => op.Producto)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (orden == null)
                    return NotFound($"No se encontró una orden con Id {id}");

                var dto = new OrdenDto
                {
                    Id = orden.Id,
                    Cliente = orden.Cliente,
                    FechaCreacion = orden.FechaCreacion,
                    Total = orden.Total,
                    OrdenProductos = orden.OrdenProductos.Select(op => new OrdenProductoDto
                    {
                        ProductoId = op.ProductoId,
                        ProductoNombre = op.Producto?.Nombre ?? "Desconocido",
                        Cantidad = op.Cantidad,
                        PrecioUnitario = op.PrecioUnitario
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la orden: {ex.Message}");
            }
        }

        // POST: api/ordenes
        [HttpPost]
        public async Task<ActionResult<Orden>> CreateOrden(Orden orden)
        {
            try
            {
                if (orden == null)
                    return BadRequest("La orden no puede ser nula");

                if (string.IsNullOrWhiteSpace(orden.Cliente))
                    return BadRequest("El nombre del cliente es obligatorio");

                if (orden.OrdenProductos == null || !orden.OrdenProductos.Any())
                    return BadRequest("La orden debe contener al menos un producto");

                foreach (var op in orden.OrdenProductos)
                {
                    var producto = await _context.Productos.FindAsync(op.ProductoId);
                    if (producto == null)
                        return BadRequest($"El producto con Id {op.ProductoId} no existe");

                    if (op.Cantidad <= 0)
                        return BadRequest("La cantidad debe ser mayor a 0");

                    op.PrecioUnitario = producto.Precio;
                }

                orden.Total = CalcularTotalConDescuento(orden);
                orden.FechaCreacion = DateTime.Now;

                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrden), new { id = orden.Id }, orden);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la orden: {ex.Message}");
            }
        }

        // PUT: api/ordenes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrden(int id, Orden orden)
        {
            try
            {
                if (orden == null)
                    return BadRequest("La orden no puede ser nula");

                if (string.IsNullOrWhiteSpace(orden.Cliente))
                    return BadRequest("El nombre del cliente es obligatorio");

                if (orden.OrdenProductos == null || !orden.OrdenProductos.Any())
                    return BadRequest("La orden debe contener al menos un producto");

                var ordenExistente = await _context.Ordenes
                    .Include(o => o.OrdenProductos)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ordenExistente == null)
                    return NotFound($"No se encontró una orden con Id {id}");

                ordenExistente.Cliente = orden.Cliente;

                _context.OrdenProductos.RemoveRange(ordenExistente.OrdenProductos);
                ordenExistente.OrdenProductos.Clear();

                foreach (var op in orden.OrdenProductos)
                {
                    var producto = await _context.Productos.FindAsync(op.ProductoId);
                    if (producto == null)
                        return BadRequest($"El producto con Id {op.ProductoId} no existe");

                    if (op.Cantidad <= 0)
                        return BadRequest("La cantidad debe ser mayor a 0");

                    ordenExistente.OrdenProductos.Add(new OrdenProducto
                    {
                        ProductoId = op.ProductoId,
                        Cantidad = op.Cantidad,
                        PrecioUnitario = producto.Precio
                    });
                }

                ordenExistente.Total = CalcularTotalConDescuento(ordenExistente);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Error de concurrencia al actualizar la orden");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la orden: {ex.Message}");
            }
        }

        // DELETE: api/ordenes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrden(int id)
        {
            try
            {
                var orden = await _context.Ordenes.FindAsync(id);
                if (orden == null)
                    return NotFound($"No se encontró una orden con Id {id}");

                _context.Ordenes.Remove(orden);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar la orden: {ex.Message}");
            }
        }

        // Métodos 
        private decimal CalcularTotalConDescuento(Orden orden)
        {
            decimal subtotal = orden.OrdenProductos.Sum(op => op.Cantidad * op.PrecioUnitario);

            decimal descuento = 0;

            if (subtotal > 500)
                descuento += 0.10m;

            int productosDistintos = orden.OrdenProductos
                .Select(op => op.ProductoId)
                .Distinct()
                .Count();

            if (productosDistintos > 5)
                descuento += 0.05m;

            return subtotal * (1 - descuento);
        }
    }
}
