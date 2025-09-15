using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using SunTech.Models;
using static SunTech.Models.DatabaseModel;

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

            if (!string.IsNullOrWhiteSpace(search))
            {
                string lowerSearch = search.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(lowerSearch) ||
                    (p.Category != null && p.Category.ToLower().Contains(lowerSearch)) ||
                    (p.Supplier != null && p.Supplier.ToLower().Contains(lowerSearch)));
            }

            var products = await productsQuery
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();

            ViewData["CurrentFilter"] = search;

            return View(products);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Category,Quantity,Supplier,Price,ExpiryDate")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
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
