using Microsoft.AspNetCore.Mvc;
using SecurityFinal.Data;
using SecurityFinal.Models;
using SecurityFinal.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SecurityFinal.Controllers {
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller {

        private readonly AppDbContext _db;
        public UserManager<IdentityUser> _userManager;
        public UserController( AppDbContext db, UserManager<IdentityUser> userManager ) {
            _db = db;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index() {
            var users = _db.AppUsers.ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var user in users) {
                var currentUserRole = userRoles.FirstOrDefault(x => x.UserId == user.Id);
                if (currentUserRole == null) {
                    user.Role = "None";
                    user.RoleId = "None";
                }
                else {
                    var r = roles.FirstOrDefault(role => role.Id == currentUserRole.RoleId);
                    user.Role = r.Name;
                    user.RoleId = r.Id;
                }
            }
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id) {
            if (string.IsNullOrEmpty(id)) {
                TempData[SD.Error] = "Invalid Id Format";
                return RedirectToAction(nameof(Index));
            }
            var user = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) {
                TempData[SD.Error] = "User not found";
                return RedirectToAction(nameof(Index));
            }
            var currentMovie = await _db.Movies.FirstOrDefaultAsync(x => x.Id == user.MovieId);
            var userMovies = await _db.UserMovies.Where(x => x.UserId == user.Id).ToListAsync();
            UserDetailViewModel userDetailViewModel = new UserDetailViewModel() {
                User = user,
                CurrentMovie = currentMovie != null ? currentMovie : new Movie(),
                PastMovies = userMovies != null ? userMovies : new List<UserMovie>(),
            };
            return View(userDetailViewModel);
        }

        [HttpGet]
        public IActionResult Edit( string id ) {
            var user = _db.AppUsers.FirstOrDefault(x => x.Id == id);
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            if (user == null) {
                TempData[SD.Error] = "User not found";
                return RedirectToAction(nameof(Index));
            }
            var currentUserRole = userRoles.FirstOrDefault(x => x.UserId == user.Id);
            if (currentUserRole != null) {
                var roleData = roles.FirstOrDefault(x => x.Id == currentUserRole.RoleId);
                user.Role = roleData.Name;
                user.RoleId = roleData.Id;
            }
            user.RoleList = roles.Select(x => new SelectListItem {
                Text = x.Name,
                Value = x.Id
            });
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit( AppUser model ) {
            if (ModelState.IsValid) {
                var user = _db.AppUsers.FirstOrDefault(x => x.Id == model.Id);
                var currentUserRole = _db.UserRoles.FirstOrDefault(x => x.UserId == user.Id);
                var roles = _db.Roles.ToList();
                if (user == null) {
                    TempData[SD.Error] = "User not found";
                    return RedirectToAction(nameof(Index));
                }
                if (currentUserRole != null) {
                    var existingRole = _db.Roles.Where(x => x.Id == currentUserRole.RoleId)
                        .Select(x => x.Name).FirstOrDefault(); // when you have users with single roles
                      //.Select(x => x.Name).ToList(); When you have users with multiple roles
                    // Remove old role
                    await _userManager.RemoveFromRoleAsync(user, existingRole);
                }
                // Add new role
                await _userManager.AddToRoleAsync(user, _db.Roles.FirstOrDefault(x => x.Id == model.RoleId).Name);
                user.Name = model.Name;

                _db.SaveChanges();
                TempData[SD.Success] = "User data updated successfully";
                return RedirectToAction(nameof(Index));
            }
            model.RoleList = _db.Roles.Select(x => new SelectListItem {
                Text = x.Name,
                Value = x.Id
            });
            return View(model);
        }

        [HttpPost]
        public IActionResult LockToggle( string id ) {
            var user = _db.AppUsers.FirstOrDefault(x => x.Id == id);
            if (user == null) {
                TempData[SD.Error] = "User not found";
                return RedirectToAction(nameof(Index));
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now) {
                // user is locked until atleast today and this will unlock them
                user.LockoutEnd = DateTime.Now;
                TempData[SD.Success] = "User Unlocked successfully";
            }
            else {
                // user is not locked so we lock till the specified time
                user.LockoutEnd = DateTime.Now.AddDays(5);
                TempData[SD.Success] = "User locked successfully";
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete( string id ) {
            var user = _db.AppUsers.FirstOrDefault(x => x.Id == id);
            if (user == null) {
                TempData[SD.Error] = "User not found";
                return RedirectToAction(nameof(Index));
            }
            _db.AppUsers.Remove(user);
            _db.SaveChanges();
            TempData[SD.Success] = "User Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
