using charamuscas.entities.Entities;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace charamuscas.mvc.Controllers
{
    public class InventarioController : Controller
    {
        private readonly Contexto _db;
        public InventarioController(Contexto db)
        {
            _db = db;
        }
        // GET: InventarioController
        public async Task<ActionResult> Index(string search)
        {
            //busqueda por filtro
            if (!String.IsNullOrEmpty(search))
            {
                var buscarInventario = await _db.vw_inventario
                    .Where(x => x.producto.Contains(search) || x.categoria.Contains(search))
                    .OrderByDescending(x => x.PK_codigo)
                    .ToListAsync();
                return View(buscarInventario);
            }

            //todos los registros
            var inventario = await _db.vw_inventario.OrderByDescending(x => x.PK_codigo).ToListAsync();
            return View(inventario);
        }

        // GET: InventarioController/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            var inventario = await _db.vw_inventario.FirstOrDefaultAsync(x => x.PK_hash == id);
            return View(inventario);
        }

        // GET: InventarioController/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.categorias = await _db.inventario_categoria.ToListAsync();
            return View();
        }

        // POST: InventarioController/Create
        [HttpPost]
        public async Task<ActionResult> Create(inventario value)
        {
            try
            {
                if(value != null)
                {
                    value.PK_hash = Guid.NewGuid();
                    await _db.inventario.AddAsync(value);
                    await _db.SaveChangesAsync();
                    
                    return RedirectToAction("Details", "Inventario", new {id = value.PK_hash});

                }
                return RedirectToAction("Index", "Inventario");
            }
            catch
            {
                return RedirectToAction("Index", "Inventario");
            }
        }

        // GET: InventarioController/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            var inventario = await _db.vw_inventario.FirstOrDefaultAsync(x => x.PK_hash == id);
            ViewBag.categorias = await _db.inventario_categoria.ToListAsync();

            return View(inventario);
        }

        // POST: InventarioController/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(vw_inventario value)
        {
            try
            {
                if(value != null)
                {

                    var inventario = await _db.inventario.FirstOrDefaultAsync(x => x.PK_codigo == value.PK_codigo);
                    inventario.nombre = value.producto;
                    inventario.cantidad = value.cantidad;
                    inventario.precio_unitario = value.precio_unitario;

                    await _db.SaveChangesAsync();
                    return RedirectToAction("Details", "Inventario", new {id = inventario.PK_hash});
                }
                return RedirectToAction("Index", "Inventario");
            }
            catch
            {
                return RedirectToAction("Details", "Inventario", new { id = value.PK_hash });
            }
        }
    }
}
