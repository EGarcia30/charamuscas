using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.entities.Entities
{
    public class vw_usuarios
    {
        [Key]
        public int PK_codigo { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string nombre_usuario { get; set; }
        public byte[] clave_hash { get; set; } = new byte[32];
        public byte[] clave_salt { get; set; } = new byte[32];
        public int FK_rol { get; set; }
        public string rol {  get; set; }
        public string correo { get; set; }
        public string celular { get; set; }
    }
}
