using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using SunTech.Models;
using System.Linq;
using System.Threading.Tasks;
using static SunTech.Models.DatabaseModel;

namespace SunTech.Controllers
{
    public class InflowsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InflowsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all inflows
        public async Task<IActionResult> Index()
        {
            var inflows = await _context.Inflows
                .Include(i => i.Product) // include product info
                .OrderByDescending(i => i.DateReceived) // newest first
                .ToListAsync();

            return View(inflows);
        }

        // Show form to add inflow
        public IActionResult Create(int? productId)
        {
            // Load all products dynamically
            ViewBag.Products = _context.Products
                .OrderByDescending(p => p.ProductId) // newest first
                .ToList();

            // Optional preselect a product
            ViewBag.SelectedProductId = productId;

            return View();
        }

        // Post action to save inflow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity,SupplierId")] Inflow inflow)
        {
            if (ModelState.IsValid)
            {
                inflow.DateReceived = System.DateTime.Now;

                // Update product stock
                var product = await _context.Products.FindAsync(inflow.ProductId);
                if (product != null)
                {
                    product.Quantity += inflow.Quantity;
                    _context.Update(product);
                }

                _context.Add(inflow);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // If invalid, reload products list
            ViewBag.Products = _context.Products
                .OrderByDescending(p => p.ProductId)
                .ToList();

            return View(inflow);
        }
    }
}
