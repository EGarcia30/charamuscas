using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class gastos_operativos
    {
        [Key]
        public int PK_codigo {  get; set; }
        public DateTime fecha { get; set; }
        public string descripcion { get; set; }
        public decimal monto { get; set; }
    }
}
