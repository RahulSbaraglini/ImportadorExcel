using Microsoft.EntityFrameworkCore;
using DesafioConcilig1.Models;

namespace DesafioConciliq1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}