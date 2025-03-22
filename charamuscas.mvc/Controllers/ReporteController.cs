using charamuscas.entities.Entities;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace charamuscas.mvc.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ReporteController : Controller
    {
        private readonly Contexto _db;

        public ReporteController(Contexto db)
        {
            _db = db;
        }

        public async Task<JsonResult> Ventas()
        {
            try
            {
                //ventas del mes
                var ventasMes = await _db.venta.Where(x => x.fecha_hora.Month == DateTime.Now.Month).ToListAsync();

                var ventasTotalMes = ventasMes.Sum(x => x.total);

                //utilidad bruta del mes(ganancias sobre productos)
                List<vw_venta_detalle> ventaDetalleMes = new List<vw_venta_detalle>();

                foreach (var item in ventasMes)
                {
                    var productos = await _db.vw_venta_detalle.Where(x => x.FK_venta == item.PK_codigo).ToListAsync();
                    ventaDetalleMes.AddRange(productos);
                }

                //utilidad bruta
                var gananciasMes = ventaDetalleMes.Sum(x => (x.subtotal - (x.cantidad_vendida * x.costo_unitario)));

                //margen de ganancias del mes
                var margenGananciasMes = (gananciasMes / ventasTotalMes);

                //Json de retorno
                var response = new
                {
                    ventas = ventasTotalMes,
                    ganancias = gananciasMes,
                    margenGanancias = margenGananciasMes,
                };

                return Json(response);
            }
            catch(Exception ex)
            {
                return Json(ex);
            }
        }

        public async Task<JsonResult> Ganancias()
        {
            try
            {
                // Agrupar y sumar las ventas por mes
                var ventasDelMes = await _db.venta
                .Where(x => x.fecha_hora.Year == DateTime.Now.Year)
                .GroupBy(x => x.fecha_hora.Month)
                .Select(g => new {
                    Mes = g.Key,
                }) // Asumiendo que 'total' es el campo que contiene el valor de la venta
                .ToListAsync();

                // Inicializar un arreglo para las ganancias
                List<decimal> ganancia = new List<decimal>(); // Para los 12 meses del año
                List<string> mes = new List<string>();

                //sacar utilidad neta de cada mes que se hicieron gastos en la empresa
                foreach (var item in ventasDelMes)
                {
                    //venta de ese mes
                    var ventaMes = await _db.venta.Where(x => x.fecha_hora.Month == item.Mes).Select(x => new { PK_codigo = x.PK_codigo}).ToListAsync();

                    //gastos de ese mes
                    var gastoDelMes = await _db.gastos_operativos.Where(x => x.fecha.Month == item.Mes).SumAsync(x => x.monto);

                    //inicializar total de $$ de ventas de ese mes
                    var ventaDetalleMes = 0.00m;
                    

                    foreach (var itemVenta in ventaMes)
                    {
                        //sacar utilidad bruta de ese mes
                        var utilidadBruta = await _db.vw_venta_detalle.Where(x => x.FK_venta == itemVenta.PK_codigo).SumAsync(x => (x.subtotal - (x.cantidad_vendida * x.costo_unitario)));                       
                        ventaDetalleMes += utilidadBruta;
                    }

                    //ganancias utilidad neta en el mes que se realizo
                    ganancia.Add(ventaDetalleMes - gastoDelMes);
                    DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
                    mes.Add(formatoFecha.GetMonthName(item.Mes));
                }

                var response = new
                {
                    gananciaTotal = ganancia,
                    mesGanancia = mes
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        public async Task<JsonResult> CategoriasMasVendidas()
        {
            try
            {
                var productos = await _db.vw_venta_detalle
                    .GroupBy(x => x.categoria)
                    .Select(x => new
                    {
                        categoria = x.Key,
                        cantidad = x.Sum(z => z.cantidad_vendida)
                    })
                    .OrderByDescending(x => x.cantidad)
                    .Take(3)
                    .ToListAsync();
                List<string> nombreCategorias = new List<string>();
                List<decimal> cantidadCategorias = new List<decimal>();

                foreach (var item in productos)
                {
                    nombreCategorias.Add(item.categoria);
                    cantidadCategorias.Add(item.cantidad);
                }

                var response = new
                {
                    nombre = nombreCategorias,
                    cantidad = cantidadCategorias,
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        public async Task<JsonResult> ProductosMasVendidos()
        {
            try
            {
                var productos = await _db.vw_venta_detalle
                    .GroupBy(x => x.producto)
                    .Select(x => new
                    {
                        producto = x.Key,
                        cantidad = x.Sum(z => z.cantidad_vendida)
                    })
                    .OrderByDescending(x => x.cantidad)
                    .Take(3)
                    .ToListAsync();
                List<string> nombreProductos = new List<string>();
                List<decimal> cantidadProductos = new List<decimal>();

                foreach (var item in productos)
                {
                    nombreProductos.Add(item.producto);
                    cantidadProductos.Add(item.cantidad);
                }

                var response = new
                {
                    nombre = nombreProductos,
                    cantidad = cantidadProductos,
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
    }
}
