using charamuscas.entities.Entities;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace charamuscas.mvc.Controllers
{
    public class VentaController : Controller
    {
        private readonly Contexto _db;
        public VentaController(Contexto db)
        {
            _db = db;
        }

        // GET: VentaController
        public async Task<ActionResult> Index(string search)
        {
            //busqueda por filtro
            if (!String.IsNullOrEmpty(search))
            {
                var buscarVenta = await _db.venta
                    .Where(x => x.PK_codigo.ToString().Contains(search) || x.PK_hash.ToString().Contains(search) || x.total.ToString().Contains(search))
                    .OrderByDescending(x => x.PK_codigo)
                    .ToListAsync();
                return View(buscarVenta);
            }

            //todos los registros
            var venta = await _db.venta.OrderByDescending(x => x.PK_codigo).ToListAsync();
            return View(venta);
        }

        // GET: VentaController/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            var venta = await _db.venta.FirstOrDefaultAsync(x => x.PK_hash == id);
            ViewBag.venta_detalle = await _db.vw_venta_detalle.Where(x => x.FK_venta == venta.PK_codigo).OrderByDescending(x => x.PK_codigo).ToListAsync();
            return View(venta);
        }

        [HttpPost]
        public async Task<JsonResult> buscarVentaDetalle(int ventaId, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                var ventaDetalleFiltrado = await _db.vw_venta_detalle
                .Where(x => x.FK_venta == ventaId && (x.categoria.Contains(search) || x.producto.Contains(search))).OrderByDescending(x => x.PK_codigo).ToListAsync();

                return Json(ventaDetalleFiltrado);
            }
            
            var ventaDetalle = await _db.vw_venta_detalle
                .Where(x => x.FK_venta == ventaId).OrderByDescending(x => x.PK_codigo).ToListAsync();

            return Json(ventaDetalle);
        }

        [HttpPost]
        public async Task<ActionResult> _Modal_Productos()
        {
            var productos = await _db.vw_inventario.OrderByDescending(x => x.PK_codigo).ToListAsync();
            return PartialView(productos);
        }

        [HttpPost]
        public async Task<ActionResult> _Modal_Agregar_Producto(int id)
        {
            var producto = await _db.vw_inventario.FirstOrDefaultAsync(x => x.PK_codigo == id);
            return PartialView(producto);
        }
    }
}
