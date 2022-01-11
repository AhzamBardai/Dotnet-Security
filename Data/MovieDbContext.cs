using Microsoft.EntityFrameworkCore;
using SecurityFinal.Models;

namespace SecurityFinal.Data {
    public class MovieDbContext : DbContext {

        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
    }
}
