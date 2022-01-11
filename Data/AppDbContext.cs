using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecurityFinal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityFinal.Data {
    public class AppDbContext : IdentityDbContext {
        public AppDbContext( DbContextOptions<AppDbContext> options )
            : base(options) {
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<UserMovie> UserMovies { get; set; }

    }
}
