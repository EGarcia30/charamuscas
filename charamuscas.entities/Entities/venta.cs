﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class venta
    {
        [Key]
        public int PK_codigo { get; set; }
        public Guid PK_hash { get; set; }
        public decimal total { get; set; }
        public DateTime fecha_hora { get; set; }
    }
}
