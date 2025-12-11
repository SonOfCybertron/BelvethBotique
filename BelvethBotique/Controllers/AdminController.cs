using BelvethBotique.Data;
using BelvethBotique.Models;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BelvethBotique.Controllers
{
    public class AdminController : Controller
    {
        private BelvethContext db = new BelvethContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["Role"]?.ToString() != "Admin")
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Dashboard()
        {
            var products = db.Products.ToList();
            return View(products);
        }


        public ActionResult Create()
        {
            return View();
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase imageFile)
        {
   
            if (imageFile == null || imageFile.ContentLength == 0)
            {
                ModelState.AddModelError("imageFile", "Image is required.");
            }

            if (ModelState.IsValid)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/jpg" };
                if (!allowedTypes.Contains(imageFile.ContentType))
                {
                    ModelState.AddModelError("imageFile", "Only JPG, PNG, or WEBP images are allowed.");
                    return View(product);
                }

                using (var reader = new BinaryReader(imageFile.InputStream))
                {
                    product.ImageData = reader.ReadBytes(imageFile.ContentLength);
                    product.ImageMimeType = imageFile.ContentType;
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Dashboard");
            }

            return View(product);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("Dashboard");

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Products.Find(product.ProductId);
                if (existing == null) return HttpNotFound();

                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Category = product.Category;
                existing.IsFeatured = product.IsFeatured;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/jpg" };
                    if (!allowedTypes.Contains(imageFile.ContentType))
                    {
                        ModelState.AddModelError("imageFile", "Only JPG, PNG, or WEBP images are allowed.");
                        return View(product);
                    }

                    using (var reader = new BinaryReader(imageFile.InputStream))
                    {
                        existing.ImageData = reader.ReadBytes(imageFile.ContentLength);
                        existing.ImageMimeType = imageFile.ContentType;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Dashboard");
            }

            return View(product);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return RedirectToAction("Dashboard");

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        public FileContentResult GetImage(int id)
        {
            var product = db.Products.Find(id);

            if (product == null || product.ImageData == null || string.IsNullOrEmpty(product.ImageMimeType))
            {
                string placeholderPath = Server.MapPath("~/Content/Images/placeholder.png");
                if (System.IO.File.Exists(placeholderPath))
                {
                    byte[] placeholderBytes = System.IO.File.ReadAllBytes(placeholderPath);
                    return File(placeholderBytes, "image/png");
                }
                else
                {
                    byte[] emptyImage = Convert.FromBase64String(
                        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2Nk+M/wHwAFgwJ/l5zN2gAAAABJRU5ErkJggg==");
                    return File(emptyImage, "image/png");
                }
            }

            return File(product.ImageData, product.ImageMimeType);
        }

        public ActionResult UserCart()
        {

            var cartItems = db.CartItems
                              .OrderByDescending(c => c.DateAdded)
                              .ToList();

            return View(cartItems);
        }

        public ActionResult CancelCartItem(int id)
        {
            var item = db.CartItems.Find(id);
            if (item != null)
            {
                item.Status = "Cancelled";
                db.SaveChanges();
            }
            return RedirectToAction("UserCart");
        }

        public ActionResult MarkForDelivery(int id)
        {
            var item = db.CartItems.Find(id);
            if (item != null)
            {
                item.Status = "For Delivery";
                db.SaveChanges();
            }
            return RedirectToAction("UserCart");
        }

        public ActionResult UserAcc()
        {
            var users = db.Users.ToList();
            return View(users);
        }

        public ActionResult EditUser(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User model, string newPassword)
        {
            var user = db.Users.Find(model.UserId);
            if (user == null) return HttpNotFound();

            user.Username = model.Username;
            user.Email = model.Email;
            user.Role = model.Role;

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            db.SaveChanges();
            return RedirectToAction("UserAcc");
        }

        public ActionResult DeleteUser(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("UserAcc");
        }

    }
}
