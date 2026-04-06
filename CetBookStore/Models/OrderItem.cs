using System.ComponentModel.DataAnnotations;

namespace CetBookStore.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int BookId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = "";

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public virtual Order? Order { get; set; }
    }
}
