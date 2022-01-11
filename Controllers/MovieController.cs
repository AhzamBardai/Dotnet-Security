using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecurityFinal.Data;
using SecurityFinal.Models;
using SecurityFinal.Utils;

namespace SecurityFinal.Controllers
{
    [Authorize]
    public class MovieController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IServiceScopeFactory _scope;

        public MovieController( IServiceScopeFactory scope, AppDbContext context )
        {
            _db = context;
            _scope = scope;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // populate data if this is the first time running
            if (!await Start()) TempData[SD.Error] = "There was a problem populating the new database with hard coded account. Please reload or try again later.";

            // gets all movies and check which the user is watching
            var movies = await _db.Movies.ToListAsync();
            if (HttpContext.User.Identity.IsAuthenticated) {
                var user = _db.AppUsers.FirstOrDefault(x => x.Id == User.Claims.First().Value);
                ViewBag.MovieId = user.MovieId != null ? user.MovieId : 0;
            }
            return View(movies);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData[SD.Error] = "Movie Not Found";
            }

            var movie = await _db.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                TempData[SD.Error] = "Movie Not Found";
            }

            return View(movie);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Released")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _db.Add(movie);
                await _db.SaveChangesAsync();
                TempData[SD.Success] = "Movie Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData[SD.Error] = "Movie Not Found";
            }

            var movie = await _db.Movies.FindAsync(id);
            if (movie == null)
            {
                TempData[SD.Error] = "Movie Not Found";
            }
            return View(movie);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Released")] Movie movie)
        {
            if (id != movie.Id)
            {
                TempData[SD.Error] = "Movie Not Found";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(movie);
                    await _db.SaveChangesAsync();
                    TempData[SD.Success] = "Movie Updated Successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Movies.Any(e => e.Id == movie.Id))
                    {
                        TempData[SD.Error] = "Movie Not Found";
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _db.Movies.FindAsync(id);
            if(movie == null) {
                TempData[SD.Error] = "Movie Not Found";
            }
            else {
                _db.Movies.Remove(movie);
                await _db.SaveChangesAsync();
                TempData[SD.Success] = "Movie Deleted Successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Watch(int? id ) {

            if (id == null) {
                TempData[SD.Error] = "Invalid Movie id";
            }
            var movie = await _db.Movies.FindAsync(id);
            var user = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == User.Claims.First().Value);
            
            // if movie has ended already 
            if (DateTime.Now > user.EndTime) {
                user.MovieId = null;
            }

            // if no current movie
            if (user.MovieId == null ) {
                var now = DateTime.Now;
                user.MovieId = movie.Id;
                user.StartTime = now;
                user.EndTime = now.AddHours(movie.Hours).AddMinutes(movie.Minutes);
                UserMovie userMovie = new UserMovie() { MovieId= movie.Id, UserId= user.Id, StartTime= now, Title= movie.Title};
                _db.UserMovies.Add(userMovie);
                TempData[SD.Success] = "Movie started successfully. Enjoy!";
            }
            else {
                TempData[SD.Error] = "You are already watching a movie. Please Finish or End that movie before starting another.";
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> End(int? id) {
            if (id == null) {
                TempData[SD.Error] = "Invalid Movie id";
            }
            var movie = await _db.Movies.FindAsync(id);
            var user = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == User.Claims.First().Value);
            if (user.MovieId != null) {
                user.MovieId = null;
                user.StartTime = DateTime.Now;
                user.EndTime = DateTime.Now;
                TempData[SD.Success] = "Movie ended successfully";
            }
            else {
                TempData[SD.Error] = "No movie started.";
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> ListWatched() {
            var user = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == User.Claims.First().Value);
            var userMovies = await _db.UserMovies.Where(x => x.UserId == user.Id).ToListAsync();
            return View(userMovies);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<bool> Start() {
            // Making scoped instances of services required for only this method
            using var scope = _scope.CreateScope();
            var _dbScoped = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var _mvScoped = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
            var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var _config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Add new admin role for new database
            if (!await _roleManager.RoleExistsAsync("ADMIN")) {
                // create role
                var result = await _roleManager.CreateAsync(new IdentityRole("ADMIN"));
                if (!result.Succeeded)
                    return false;
            }

            // Add manager account for new databases
            if (!await _dbScoped.AppUsers.AnyAsync()) {
                var config = _config.GetSection("AdminAccount").AsEnumerable(true).Select(x => x.Value).ToList();
                var user = new AppUser { UserName = config[2], Email = config[2], Name = config[1] };
                var result1 = await _userManager.CreateAsync(user, password: config[0]);
                var result2 = await _userManager.AddClaimAsync(user, new Claim("Name", user.Name));
                var result3 = await _userManager.AddToRoleAsync(user, "ADMIN");
                if (!result1.Succeeded && !result2.Succeeded && !result3.Succeeded)
                    return false;
            }

            // Add movies for new db
            if (!await _dbScoped.Movies.AnyAsync()) {
                try {
                    var moviesDefualt = await _mvScoped.Movies.ToListAsync();
                    moviesDefualt.ForEach(async x =>
                        await _dbScoped.Movies.AddAsync(new Movie { Hours = x.Hours, Title = x.Title, Released = x.Released, Minutes = x.Minutes }));
                    _dbScoped.SaveChanges();
                }
                catch (Exception) {
                    return false;
                }
            }
            return true;
        }

    }
}
