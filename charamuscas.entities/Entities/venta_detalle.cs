using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class venta_detalle
    {
        [Key]
        public int PK_codigo {  get; set; }
        public int FK_inventario { get; set; }
        public int FK_venta {  get; set; }
        public decimal cantidad_vendida {  get; set; }
        public decimal precio_venta { get; set; }
        public decimal subtotal { get; set; }
    }
}
