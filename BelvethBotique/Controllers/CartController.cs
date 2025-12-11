using BelvethBotique.Data;
using BelvethBotique.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BelvethBotique.Controllers
{
    public class CartController : Controller
    {
        private BelvethContext db = new BelvethContext();

        private List<SessionCartItem> GetCart()
        {
            if (Session["Cart"] == null)
                Session["Cart"] = new List<SessionCartItem>();

            return (List<SessionCartItem>)Session["Cart"];
        }

        public ActionResult AddToCart(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == id);

            if (existing == null)
            {
                cart.Add(new SessionCartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageBase64 = product.ImageData != null
                        ? "data:" + product.ImageMimeType + ";base64," + Convert.ToBase64String(product.ImageData)
                        : null
                });
            }
            else
            {
                existing.Quantity++;
            }

            Session["Cart"] = cart;
            return RedirectToAction("Cart");
        }

        public ActionResult Cart()
        {
            var cart = GetCart();
            return View("Index", cart);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int id, int qty)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
            {
                if (qty <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = qty;
            }

            Session["Cart"] = cart;
            return RedirectToAction("Cart");
        }

        public ActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
                cart.Remove(item);

            Session["Cart"] = cart;
            return RedirectToAction("Cart");
        }
        public ActionResult Clear()
        {
            Session["Cart"] = new List<SessionCartItem>();
            return RedirectToAction("Cart");
        }

        private void SaveSessionCartToDatabase(int userId)
        {
            var user = db.Users.Find(userId);
            if (user == null) throw new Exception("User not found");

            var cart = GetCart();

            foreach (var item in cart)
            {
                var product = db.Products.Find(item.ProductId);
                if (product == null) continue;

                db.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    DateAdded = DateTime.Now,
                    Status = "Pending",
                    User = user,
                    Product = product
                });
            }

            db.SaveChanges();
            Session["Cart"] = new List<SessionCartItem>();
        }

        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);
            if (user == null)
            {
                Session["UserID"] = null;
                TempData["ErrorMessage"] = "Your session is invalid. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["InfoMessage"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            SaveSessionCartToDatabase(userId);

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cart.Sum(x => x.Price * x.Quantity)
            };
            db.Orders.Add(order);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Checkout successful!";
            return View("CheckoutSuccess");
        }

        public ActionResult UserCart()
        {
            var cartItems = db.CartItems
                .Include(c => c.User)
                .Include(c => c.Product)
                .OrderByDescending(c => c.DateAdded)
                .ToList();

            return View(cartItems);
        }

        public ActionResult UpdateStatus(int cartItemId, string status)
        {
            var cartItem = db.CartItems.Find(cartItemId);
            if (cartItem == null) return HttpNotFound();

            cartItem.Status = status;
            db.SaveChanges();

            return RedirectToAction("UserCart");
        }

        public ActionResult Index()
        {
            return RedirectToAction("Cart");
        }
    }
}
