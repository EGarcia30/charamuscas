﻿using charamuscas.entities.Entities;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace charamuscas.mvc.Controllers
{
    public class CompraController : Controller
    {
        private readonly Contexto _db;
        public CompraController(Contexto db)
        {
            _db = db;
        }
        // GET: CompraController
        public async Task<ActionResult> Index()
        {
            var compras = await _db.compra.OrderByDescending(x => x.PK_codigo).Take(100).ToListAsync();

            return View(compras);
        }

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var compra = await _db.compra.FirstOrDefaultAsync(x => x.PK_codigo == id);
                return View(compra);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                if (id != 0)
                {
                    var compra = await _db.compra.FirstOrDefaultAsync(x => x.PK_codigo == id);
                    return View(compra);
                }
                return RedirectToAction("Index", "Compra");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Compra");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(compra value)
        {
            try
            {
                //validar que objeto no sea null
                if (value != null)
                {
                    //validar modelo
                    if (ModelState.IsValid)
                    {
                        //sacar registro de bd
                        var compra = await _db.compra.FirstOrDefaultAsync(x => x.PK_codigo == value.PK_codigo);

                        //editar registro
                        compra.nombre = value.nombre;
                        compra.cantidad = value.cantidad;
                        compra.costo_total = value.costo_total;

                        await _db.SaveChangesAsync();

                        return RedirectToAction("Details", "Compra", new { id = value.PK_codigo });
                    }
                }
                return RedirectToAction("Details", "Compra", new { id = value.PK_codigo });
            }
            catch (Exception ex)
            {

                return RedirectToAction("Details", "Compra", new { id = value.PK_codigo });
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(compra value)
        {
            try
            {
                if (value != null)
                {
                    //agregar hora
                    value.fecha_compra = DateTime.Now;

                    //agregar registro a bd
                    await _db.compra.AddAsync(value);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details", "Compra", new { id = value.PK_codigo });

                }

                return RedirectToAction("Create", "Compra");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Create", "Compra");
            }
        }
    }
}
