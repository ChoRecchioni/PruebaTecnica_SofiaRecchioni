using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica_SofiaRecchioni.Data;
using PruebaTecnica_SofiaRecchioni.Models;

namespace PruebaTecnica_SofiaRecchioni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            try
            {
                return Ok(await _context.Productos.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener productos: {ex.Message}");
            }
        }

        // GET: api/productos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                    return NotFound($"No se encontró un producto con Id {id}");

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el producto: {ex.Message}");
            }
        }

        // POST: api/productos
        [HttpPost]
        public async Task<ActionResult<Producto>> CreateProducto(Producto producto)
        {
            try
            {
                if (producto == null)
                    return BadRequest("El producto no puede ser nulo");

                if (string.IsNullOrWhiteSpace(producto.Nombre))
                    return BadRequest("El nombre del producto es obligatorio");

                if (producto.Precio <= 0)
                    return BadRequest("El precio debe ser mayor que 0");

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear el producto: {ex.Message}");
            }
        }

        // PUT: api/productos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, Producto producto)
        {
            try
            {
                if (producto == null)
                    return BadRequest("El producto no puede ser nulo");

                if (string.IsNullOrWhiteSpace(producto.Nombre))
                    return BadRequest("El nombre del producto es obligatorio");

                if (producto.Precio <= 0)
                    return BadRequest("El precio debe ser mayor que 0");

                // Se fuerza a usar el ID de la URL
                producto.Id = id;

                if (!await _context.Productos.AnyAsync(p => p.Id == id))
                    return NotFound($"No existe un producto con Id {id}");

                _context.Entry(producto).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Error de concurrencia al actualizar el producto");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el producto: {ex.Message}");
            }
        }

        // DELETE: api/productos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    return NotFound($"No se encontró un producto con Id {id}");

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar el producto: {ex.Message}");
            }
        }
    }
}
