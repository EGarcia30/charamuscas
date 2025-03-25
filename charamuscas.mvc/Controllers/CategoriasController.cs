using charamuscas.entities.Entities;
using charamuscas.mvc.Helper;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace charamuscas.mvc.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly Contexto _db;

        public CategoriasController(Contexto db) {
            _db = db;
        }

        public async Task<IActionResult> Index(string search, int? numPag)
        {
            int cantidadRegistros = 6;

            if (!String.IsNullOrEmpty(search))
            {
                var categoriasFiltradas = await _db.inventario_categoria
                    .Where(x => x.nombre.Contains(search))
                    .OrderByDescending(x => x.PK_codigo)
                    .ToListAsync();

                return View(Paginacion<inventario_categoria>.CrearPaginacion(categoriasFiltradas.AsQueryable(), numPag ?? 1, cantidadRegistros));
            }

            var categorias = await _db.inventario_categoria.OrderByDescending(x => x.PK_codigo) .ToListAsync();

            return View(Paginacion<inventario_categoria>.CrearPaginacion(categorias.AsQueryable(), numPag ?? 1, cantidadRegistros));
        }

        public async Task<IActionResult> Details(int id)
        {
            var categoria = await _db.inventario_categoria.FirstOrDefaultAsync(x => x.PK_codigo == id);
            return View(categoria);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _db.inventario_categoria.FirstOrDefaultAsync(x => x.PK_codigo == id);
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(inventario_categoria value)
        {
            try
            {
                if(value != null)
                {
                    //traer categoria original en bd
                    var categoriaBD = await _db.inventario_categoria.FirstOrDefaultAsync(x => x.PK_codigo == value.PK_codigo);

                    //hacer los cambios requeridos
                    categoriaBD.nombre = value.nombre;

                    //guardar categoria a bd
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details", "Categorias", new {id = value.PK_codigo});
                }
                return RedirectToAction("Index", "Categorias");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Categorias");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(inventario_categoria value)
        {
            try
            {
                if (value != null)
                {
                    //guardar nueva categoria en bd
                    await _db.inventario_categoria.AddAsync(value);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details", "Categorias", new { id = value.PK_codigo });
                }
                return View(null);
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
    }
}
