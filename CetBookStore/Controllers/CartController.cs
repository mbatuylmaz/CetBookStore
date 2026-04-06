using System.Security.Claims;
using System.Text.Json;
using CetBookStore.Data;
using CetBookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CetBookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CartJson";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Add(int bookId, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.BookId == bookId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartLine
                {
                    BookId = book.Id,
                    ProductName = book.Title,
                    Quantity = quantity,
                    Price = book.Price
                });
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Remove(int bookId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.BookId == bookId);
            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Purchase()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now
            };

            foreach (var line in cart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    BookId = line.BookId,
                    ProductName = line.ProductName,
                    Quantity = line.Quantity,
                    Price = line.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("MyOrders", "Orders");
        }

        private List<CartLine> GetCart()
        {
            var s = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(s))
            {
                return new List<CartLine>();
            }
            try
            {
                return JsonSerializer.Deserialize<List<CartLine>>(s) ?? new List<CartLine>();
            }
            catch
            {
                return new List<CartLine>();
            }
        }

        private void SaveCart(List<CartLine> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }
    }
}
