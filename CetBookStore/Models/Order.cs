using System.ComponentModel.DataAnnotations;

namespace CetBookStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public DateTime OrderDate { get; set; }

        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
