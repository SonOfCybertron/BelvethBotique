using System.Linq;
using System.Web.Mvc;
using BelvethBotique.Data;
using BelvethBotique.Models;
using BCrypt.Net;

namespace BelvethBotique.Controllers
{
    public class AccountController : Controller
    {
        private BelvethContext db = new BelvethContext();
        public ActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                Session["UserID"] = user.UserId;
                Session["Username"] = user.Username;
                Session["Role"] = user.Role;

                if (user.Role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public ActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.Users.Any(u => u.Username == model.Username))
            {
                ViewBag.Error = "Username already taken.";
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Role = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
