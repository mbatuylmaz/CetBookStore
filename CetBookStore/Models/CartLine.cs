namespace CetBookStore.Models
{
    public class CartLine
    {
        public int BookId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
