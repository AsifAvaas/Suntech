using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using SunTech.Models; // <-- Correct namespace
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
                // Add the new product to the database
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect back to the product list after creation
            }
            return View(product); // Return the form with validation errors if the model state is invalid
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
