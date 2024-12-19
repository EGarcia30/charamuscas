using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class vw_inventario
    {
        [Key]
        public int PK_codigo { get; set; }
        public Guid PK_hash { get; set; }
        public string? producto { get; set; }
        public decimal cantidad { get; set; }
        public decimal precio_unitario { get; set; }
        public int FK_categoria { get; set; }
        public string categoria { get; set; }
    }
}
