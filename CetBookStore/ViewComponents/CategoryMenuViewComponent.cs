using CetBookStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CetBookStore.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public CategoryMenuViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var list = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(list);
        }
    }
}
