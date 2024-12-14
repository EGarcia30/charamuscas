using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class compra
    {
        [Key]
        public int PK_codigo { get; set; }
        public string? nombre { get; set; }
        public decimal cantidad { get; set; }
        public decimal costo_total {  get; set; }
        public DateTime fecha_compra { get; set; }
    }
}
