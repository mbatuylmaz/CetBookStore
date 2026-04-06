using CetBookStore.Data;
using CetBookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CetBookStore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "cetbookstore_dev.sqlite");
            var useSqlite = !OperatingSystem.IsWindows();

            if (useSqlite)
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite($"Data Source={sqlitePath}"));
            }
            else
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (useSqlite)
                {
                    await db.Database.EnsureCreatedAsync();
                    if (!await db.Books.AnyAsync())
                    {
                        var c1 = new Category { Name = "Computer", IsVisibleInMenu = true };
                        var c2 = new Category { Name = "Novel", IsVisibleInMenu = true };
                        var c3 = new Category { Name = "Book", IsVisibleInMenu = true };
                        db.Categories.AddRange(c1, c2, c3);
                        await db.SaveChangesAsync();

                        var b1 = new Book
                        {
                            Title = "Demo Kitap 1",
                            Description = "Örnek açıklama metni en az on karakter.",
                            Author = "Yazar A",
                            Publisher = "Yayın",
                            PageCount = 200,
                            Price = 99.90m,
                            IsInSale = false,
                            PreviousPrice = 0,
                            PublicationDate = DateTime.UtcNow.Date,
                            CreatedDate = DateTime.UtcNow,
                            CategoryId = c1.Id
                        };
                        var b2 = new Book
                        {
                            Title = "Demo Kitap 2",
                            Description = "İkinci kitap açıklaması burada yazıyor olmalı.",
                            Author = "Yazar B",
                            Publisher = "Yayın",
                            PageCount = 150,
                            Price = 45m,
                            IsInSale = true,
                            PreviousPrice = 60m,
                            PublicationDate = DateTime.UtcNow.Date,
                            CreatedDate = DateTime.UtcNow,
                            CategoryId = c2.Id
                        };
                        db.Books.AddRange(b1, b2);
                        await db.SaveChangesAsync();

                        db.Comments.Add(new Comment
                        {
                            UserName = "test",
                            Content = "Güzel kitap.",
                            CreatedDate = DateTime.UtcNow,
                            BookId = b1.Id
                        });
                        db.Comments.Add(new Comment
                        {
                            UserName = "anon",
                            Content = "Okudum, fena değil.",
                            CreatedDate = DateTime.UtcNow,
                            BookId = b1.Id
                        });
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    await db.Database.MigrateAsync();
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            await app.RunAsync();
        }
    }
}
