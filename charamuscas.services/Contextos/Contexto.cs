using charamuscas.entities.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charamuscas.services.Contextos
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {
        
        }
        public DbSet<compra> compra {  get; set; }
        public DbSet<inventario> inventario { get; set; }
        public DbSet<venta> venta { get; set; }
        public DbSet<venta_detalle> venta_detalle { get; set; }
        public DbSet<vw_venta_detalle> vw_venta_detalle { get; set; }
        public DbSet<vw_inventario> vw_inventario { get; set; }
        public DbSet<inventario_categoria> inventario_categoria { get; set; }
        public DbSet<usuarios> usuarios { get; set; }
        public DbSet<vw_usuarios> vw_usuarios { get; set; }
        public DbSet<usuarios_rol> usuarios_rol { get; set; }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(20, 8);
        }
    }
}
