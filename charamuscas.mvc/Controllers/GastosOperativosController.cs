using charamuscas.entities.Entities;
using charamuscas.mvc.Helper;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace charamuscas.mvc.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class GastosOperativosController : Controller
    {
        private readonly Contexto _db;

        public GastosOperativosController(Contexto db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string search, int? numPag)
        {
            int cantidadRegistros = 6;
            if (!String.IsNullOrEmpty(search))
            {
                var gastosOperativosFiltrado = await _db.gastos_operativos.Where(x => x.descripcion.Contains(search))
                    .OrderByDescending(x => x.PK_codigo)
                    .ToListAsync();

                return View(Paginacion<gastos_operativos>.CrearPaginacion(gastosOperativosFiltrado.AsQueryable(), numPag ?? 1, cantidadRegistros));
            }

            var gastosOperativos = await _db.gastos_operativos.OrderByDescending(x => x.PK_codigo).ToListAsync();

            return View(Paginacion<gastos_operativos>.CrearPaginacion(gastosOperativos.AsQueryable(), numPag ?? 1, cantidadRegistros));
        }

        public async Task<IActionResult> Details(int id)
        {
            var gastoOperativo = await _db.gastos_operativos.FirstOrDefaultAsync(x => x.PK_codigo == id);
            return View(gastoOperativo);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if(id != 0)
            {
                var gastoOperativo = await _db.gastos_operativos.FirstOrDefaultAsync(x => x.PK_codigo == id);
                return View(gastoOperativo);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult>Edit(gastos_operativos value)
        {
            try
            {
                if (value != null)
                {
                    //Traer gasto operativo guardado en bd
                    var gastoBD = await _db.gastos_operativos.FirstOrDefaultAsync(x => x.PK_codigo == value.PK_codigo);

                    //hacer el cambio de los campos habilitados
                    gastoBD.descripcion = value.descripcion;
                    gastoBD.monto = value.monto;

                    //guardar cambios en bd
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details", "GastosOperativos", new { id = gastoBD.PK_codigo });
                }

                return View();
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(gastos_operativos value)
        {
            try
            {
                if(value != null){
                    //guardar nueva informacion en bd
                    value.fecha = DateTime.Now;
                    await _db.gastos_operativos.AddAsync(value);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details","GastosOperativos", new {id = value.PK_codigo});
                }
                return RedirectToAction("Create", "GastosOperativos");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Create", "GastosOperativos");
            }
        }


    }
}
