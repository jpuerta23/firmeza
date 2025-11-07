using AdminRazer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AdminRazer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la relación opcional entre Cliente.IdentityUserId y AspNetUsers(Id)
            // Usamos DeleteBehavior.SetNull para que si se borra el usuario no se elimine el cliente.
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasIndex(e => e.IdentityUserId).HasDatabaseName("IX_Clientes_IdentityUserId");

                entity.HasOne<IdentityUser>()
                      .WithMany()
                      .HasForeignKey(e => e.IdentityUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}