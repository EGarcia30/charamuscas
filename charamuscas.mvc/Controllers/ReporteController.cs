using charamuscas.entities.Entities;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace charamuscas.mvc.Controllers
{
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
                var gananciasMes = ventaDetalleMes.Sum(x => (x.subtotal - x.costo_unitario));

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
                var ventasPorMes = await _db.venta
                    .Where(x => x.fecha_hora.Year == DateTime.Now.Year)
                    .GroupBy(x => x.fecha_hora.Month)
                    .Select(g => new { Mes = g.Key, TotalVentas = g.Sum(v => v.total) }) // Asumiendo que 'total' es el campo que contiene el valor de la venta
                    .ToListAsync();

                // Agrupar y sumar las compras por mes
                var comprasPorMes = await _db.compra
                    .Where(x => x.fecha_compra.Year == DateTime.Now.Year)
                    .GroupBy(x => x.fecha_compra.Month)
                    .Select(g => new { Mes = g.Key, TotalCompras = g.Sum(c => c.costo_total) }) // Asumiendo que 'monto' es el campo que contiene el valor de la compra
                    .ToListAsync();
                //var ventas = await _db.venta.GroupBy(x => x.fecha_hora.Month).ToListAsync();
                //var compras = await _db.compra.GroupBy(x => x.fecha_compra.Month).ToListAsync();

                // Inicializar un arreglo para las ganancias
                decimal[] ganancia = new decimal[12]; // Para los 12 meses del año
                string[] mes = new string[] {"Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"};

                for (var i = 1; i <= 12; i++)
                {
                    var totalVentas = ventasPorMes.FirstOrDefault(v => v.Mes == i)?.TotalVentas ?? 0;
                    var totalCompras = comprasPorMes.FirstOrDefault(c => c.Mes == i)?.TotalCompras ?? 0;                  

                    ganancia[i - 1] = totalVentas - totalCompras; // Guardar la ganancia en el índice correspondiente
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

        public async Task<JsonResult> ProductosMasVendidos()
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
                string[] nombreProductos = new string[3];
                decimal[] cantidadProductos = new decimal[3];
                int contador = 0;
                foreach (var item in productos)
                {
                    nombreProductos[contador] = item.categoria;
                    cantidadProductos[contador] = item.cantidad;
                    contador++;
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
