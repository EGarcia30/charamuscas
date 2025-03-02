using charamuscas.entities.Entities;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(venta venta)
        {
            try
            {
                if(venta != null)
                {
                    venta.PK_hash = Guid.NewGuid();
                    venta.fecha_hora = DateTime.Now;

                    _db.venta.Add(venta);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details", "Venta", new { id = venta.PK_hash });
                }
                return RedirectToAction("Create", "Venta");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Create", "Venta");
            }
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

        [HttpPost]
        public async Task<JsonResult> AgregarProductoDetalle(vw_inventario inventario, int ventaId)
        {
            try
            {
                if(inventario != null)
                {
                    //Verificar que cantidad vendida no sea mayor a cantidad de stock
                    var inventarioDB = await _db.inventario.FirstOrDefaultAsync(x => x.PK_codigo == inventario.PK_codigo);
                    if (inventario.cantidad > inventarioDB.cantidad)
                    {
                        return Json(null);
                    }

                    //quitar cantidad disponible a inventario
                    inventarioDB.cantidad = (inventarioDB.cantidad - inventario.cantidad);

                    //sacar el subtotal del producto
                    var subtotal = (inventario.cantidad * inventario.precio_unitario);

                    //agregar el total a la venta
                    var ventaDB = await _db.venta.FirstOrDefaultAsync(x => x.PK_codigo == ventaId);
                    ventaDB.total += subtotal;

                    //crear objeto de venta detalle
                    venta_detalle ventaDetalle = new venta_detalle();
                    ventaDetalle.FK_inventario = inventario.PK_codigo;
                    ventaDetalle.FK_venta = ventaId;
                    ventaDetalle.cantidad_vendida = inventario.cantidad;
                    ventaDetalle.precio_venta = inventario.precio_unitario;
                    ventaDetalle.subtotal = subtotal;

                    //Guardar en base de datos
                    _db.venta_detalle.Add(ventaDetalle);
                    await _db.SaveChangesAsync();

                    //Todos los datos de la tabla venta detalle
                    var productos = await _db.vw_venta_detalle.Where(x => x.FK_venta == ventaId).OrderByDescending(x => x.PK_codigo).ToListAsync();

                    var response = new
                    {
                        productosDetalle = productos,
                        ventaTotal = ventaDB.total,
                        inventarioCantidad = inventarioDB.cantidad,
                    };

                    return Json(response);
                }
                return Json(null);
            }
            catch (Exception ex) {
                return Json(ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult> _Modal_Editar_Producto(int id, int ventaId)
        {
            var producto = await _db.vw_venta_detalle.FirstOrDefaultAsync(x => x.PK_codigo == id);
            ViewBag.ventaId = ventaId;
            return PartialView(producto);
        }

        public async Task<JsonResult> EditarProducto(vw_venta_detalle value, int ventaId)
        {
            try
            {
                if(value != null)
                {
                    //Traemos el objecto de venta detalle
                    var item = await _db.venta_detalle.FirstOrDefaultAsync(x => x.PK_codigo == value.PK_codigo);
                    //traemos el iventario(producto)
                    var producto = await _db.inventario.FirstOrDefaultAsync(x => x.PK_codigo == item.FK_inventario);

                    //Se regresan los productos comprados a mi inventario
                    producto.cantidad += item.cantidad_vendida;

                    //condicion de no vender menor a 0 productos en inventario o compra que supera el inventario
                    if(producto.cantidad <= 0 || producto.cantidad < value.cantidad_vendida)
                    {
                        return Json(null);
                    }

                    //asignamos nuevos valores
                    item.cantidad_vendida = value.cantidad_vendida;
                    item.precio_venta = value.precio_venta;
                    producto.cantidad -= value.cantidad_vendida;

                    //nuevo subtotal
                    var subtotal = (item.cantidad_vendida * value.precio_venta);
                    item.subtotal = subtotal;

                    //cambio en base de datos
                    await _db.SaveChangesAsync();

                    //sacar total
                    var total = await _db.vw_venta_detalle.Where(x => x.FK_venta == ventaId).SumAsync(x => x.subtotal);

                    //poner a venta el total
                    var venta = await _db.venta.FirstOrDefaultAsync(x => x.PK_codigo == ventaId);
                    venta.total = total;

                    _db.SaveChangesAsync();

                    var response = new
                    {
                        venta_detalle = item,
                        ventaTotal = total
                    };

                    return Json(response);
                }

                return Json(null);

            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult> _Modal_Eliminar_Producto(int id,int ventaId)
        {
            var producto = await _db.vw_venta_detalle.FirstOrDefaultAsync(x => x.PK_codigo == id);
            ViewBag.ventaId = ventaId;
            return PartialView(producto);
        }

        [HttpPost]
        public async Task<JsonResult> EliminarProducto(int id, int ventaId)
        {
            try
            {
                if (id != 0)
                {
                    //Traemos el objecto de venta detalle
                    var item = await _db.venta_detalle.FirstOrDefaultAsync(x => x.PK_codigo == id);

                    //cambios en base de datos
                    _db.venta_detalle.Remove(item);
                    await _db.SaveChangesAsync();

                    var venta = await _db.venta.FirstOrDefaultAsync(x => x.PK_codigo == ventaId);
                    var venta_detalle = await _db.vw_venta_detalle.Where(x => x.FK_venta == ventaId).OrderByDescending(x => x.PK_codigo).ToListAsync();

                    //sacar total a venta
                    var sumSubtotal = venta_detalle.Sum(x => x.subtotal);
                    venta.total = sumSubtotal;

                    await _db.SaveChangesAsync();
                    var response = new
                    {
                        productosDetalle = venta_detalle,
                        ventaTotal = sumSubtotal,
                    };

                    return Json(response);
                }
                return Json(null);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
    }
}
