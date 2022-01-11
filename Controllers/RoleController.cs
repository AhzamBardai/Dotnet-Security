using SecurityFinal.Data;
using SecurityFinal.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Controllers {
    [Authorize(Roles = "ADMIN")]
    public class RoleController : Controller {

        private readonly AppDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RoleController( AppDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager ) {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index() {

            var roles = _db.Roles.ToList();

            return View(roles);
        }

        [HttpGet]
        public IActionResult Upsert( string id ) {
            if (string.IsNullOrEmpty(id)) {
                return View();
            }
            else {
                var role = _db.Roles.FirstOrDefault(x => x.Id == id);
                return View(role);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert( IdentityRole model ) {
            if (await _roleManager.RoleExistsAsync(model.Name)) {
                // error
                TempData[SD.Error] = "Role Already Exists";
            }
            else {
                if (string.IsNullOrEmpty(model.Id)) {
                    // create
                    await _roleManager.CreateAsync(new IdentityRole { Name = model.Name });
                    TempData[SD.Success] = "Role Created Successfully";
                }
                else {
                    // update
                    var role = _db.Roles.FirstOrDefault(x => x.Id == model.Id);
                    if (role == null) {
                        TempData[SD.Error] = "Role Not Found";
                        return RedirectToAction(nameof(Index));

                    }
                    role.Name = model.Name;
                    role.NormalizedName = model.Name.ToUpper();
                    var result = await _roleManager.UpdateAsync(role);
                    TempData[SD.Success] = "Role Updated Successfully";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete( string id ) {
            var role = _db.Roles.FirstOrDefault(y => y.Id == id);
            var userWithRole = _db.UserRoles.Where(u => u.RoleId == id).Count();
            if (userWithRole > 0) {
                TempData[SD.Error] = "Cannot delete role with active users assigned to it.";
                return RedirectToAction(nameof(Index));
            }
            if (role == null) {
                TempData[SD.Success] = "Role Note Found";
                return RedirectToAction(nameof(Index));
            }
            await _roleManager.DeleteAsync(role);
            TempData[SD.Success] = "Role Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
