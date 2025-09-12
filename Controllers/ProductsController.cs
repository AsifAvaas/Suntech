using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using SunTech.Models; // <-- Correct namespace
using System.Linq;
using System.Threading.Tasks;

namespace SunTech.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all products with optional search
        public async Task<IActionResult> Index(string search)
        {
            var productsQuery = _context.Products.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));
            }

            // Order by newest first
            var products = await productsQuery
                .OrderByDescending(p => p.ProductId) // assuming ProductId increments
                .ToListAsync();

            return View(products);
        }

        // Product details
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
