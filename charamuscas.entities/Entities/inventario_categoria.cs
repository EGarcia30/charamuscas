using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class inventario_categoria
    {
        [Key]
        public int PK_codigo {  get; set; }
        public string nombre { get; set; }
    }
}
