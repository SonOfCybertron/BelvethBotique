using System;

namespace BelvethBotique.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        public int UserId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
        public string Status { get; set; } = "Pending";

    }
}
