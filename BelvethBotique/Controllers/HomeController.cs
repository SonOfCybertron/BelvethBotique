using System;
using System.Linq;
using System.Web.Mvc;
using BelvethBotique.Data;
using BelvethBotique.Models;
using System.IO;

namespace BelvethBotique.Controllers
{
    public class HomeController : Controller
    {
        private readonly BelvethContext db = new BelvethContext();

        public ActionResult Index()
        {
            var featuredProducts = db.Products
                                      .Where(p => p.IsFeatured)
                                      .Take(3)
                                      .ToList();
            return View(featuredProducts);
        }

        public ActionResult Shop(string category, string search)
        {
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(term));
            }

            return View(products.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Shop");
            }

            var product = db.Products.Find(id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
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

                byte[] emptyImage = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2Nk+M/wHwAFgwJ/l5zN2gAAAABJRU5ErkJggg==");
                return File(emptyImage, "image/png");
            }

            return File(product.ImageData, product.ImageMimeType);
        }
    }
}
