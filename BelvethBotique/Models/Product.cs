using System.ComponentModel.DataAnnotations;

namespace BelvethBotique.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageMimeType { get; set; }
        public bool IsFeatured { get; set; }
    }
}