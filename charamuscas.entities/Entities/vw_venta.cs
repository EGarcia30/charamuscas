using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class vw_venta
    {
        [Key]
        public int PK_codigo { get; set; }
        public DateTime fecha_hora { get; set; }
        public Guid PK_hash { get; set; }
        public int FK_inventario { get; set; }
        public string? producto { get; set; }
        public decimal cantidad_vendida { get; set; }
        public decimal precio_unitario { get; set; }
        public decimal total { get; set; }
    }
}
